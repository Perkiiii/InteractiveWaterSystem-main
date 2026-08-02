using UnityEngine;

namespace Water25D
{
    /// <summary>
    /// Fixed-capacity Rigidbody2D contact membership used by both water physics volumes.
    /// Collider references are sampled into bounded storage for aggregate bounds; the
    /// logical collider count remains separate so an oversized body is not released early.
    /// </summary>
    internal sealed class WaterLogicalBodyContactTracker
    {
        public const int CompileTimeMaximumLogicalBodies = 16;
        public const int CompileTimeMaximumSampledCollidersPerBody = 8;

        private readonly Rigidbody2D[] _bodies = new Rigidbody2D[CompileTimeMaximumLogicalBodies];
        private readonly Collider2D[] _sampledColliders = new Collider2D[
            CompileTimeMaximumLogicalBodies * CompileTimeMaximumSampledCollidersPerBody];
        private readonly int[] _colliderCounts = new int[CompileTimeMaximumLogicalBodies];
        private readonly int[] _sampledColliderCounts = new int[CompileTimeMaximumLogicalBodies];
        private readonly int[] _bodySequences = new int[CompileTimeMaximumLogicalBodies];

        private int _maximumLogicalBodies = 8;
        private int _logicalBodyCount;
        private int _nextBodySequence;
        private int _colliderOverflowCount;
        private int _droppedBodyCount;

        public WaterLogicalBodyContactTracker(int maximumLogicalBodies = 8)
        {
            Configure(maximumLogicalBodies);
        }

        public int MaximumLogicalBodies => _maximumLogicalBodies;
        public int LogicalBodyCount => _logicalBodyCount;
        public int ColliderOverflowCount => _colliderOverflowCount;
        public int DroppedBodyCount => _droppedBodyCount;

        public void Configure(int maximumLogicalBodies)
        {
            _maximumLogicalBodies = Mathf.Clamp(maximumLogicalBodies, 1, CompileTimeMaximumLogicalBodies);
        }

        public bool TryAdd(Collider2D collider)
        {
            return TryAdd(collider, out _);
        }

        public bool TryAdd(Collider2D collider, out bool bodyBecameActive)
        {
            bodyBecameActive = false;
            if (!IsValidCollider(collider))
            {
                return false;
            }

            var body = collider.attachedRigidbody;
            var bodyIndex = FindBodyIndex(body);
            if (bodyIndex >= 0)
            {
                if (ContainsSampledCollider(bodyIndex, collider))
                {
                    return false;
                }

                _colliderCounts[bodyIndex]++;
                var sampledCount = _sampledColliderCounts[bodyIndex];
                if (sampledCount < CompileTimeMaximumSampledCollidersPerBody)
                {
                    _sampledColliders[GetColliderStorageIndex(bodyIndex, sampledCount)] = collider;
                    _sampledColliderCounts[bodyIndex] = sampledCount + 1;
                }
                else
                {
                    _colliderOverflowCount++;
                }

                return true;
            }

            if (_logicalBodyCount >= _maximumLogicalBodies)
            {
                _droppedBodyCount++;
                return false;
            }

            bodyIndex = _logicalBodyCount++;
            _bodies[bodyIndex] = body;
            _colliderCounts[bodyIndex] = 1;
            _sampledColliderCounts[bodyIndex] = 1;
            _bodySequences[bodyIndex] = ++_nextBodySequence;
            _sampledColliders[GetColliderStorageIndex(bodyIndex, 0)] = collider;
            bodyBecameActive = true;
            return true;
        }

        public bool TryRemove(Collider2D collider)
        {
            return TryRemove(collider, out _);
        }

        public bool TryRemove(Collider2D collider, out bool bodyBecameInactive)
        {
            bodyBecameInactive = false;
            if (collider == null)
            {
                return false;
            }

            var bodyIndex = FindBodyIndex(collider.attachedRigidbody);
            if (bodyIndex < 0 || _colliderCounts[bodyIndex] <= 0)
            {
                return false;
            }

            var sampledIndex = FindSampledColliderIndex(bodyIndex, collider);
            if (sampledIndex >= 0)
            {
                var sampledCount = _sampledColliderCounts[bodyIndex];
                for (var i = sampledIndex; i < sampledCount - 1; i++)
                {
                    _sampledColliders[GetColliderStorageIndex(bodyIndex, i)] =
                        _sampledColliders[GetColliderStorageIndex(bodyIndex, i + 1)];
                }

                _sampledColliders[GetColliderStorageIndex(bodyIndex, sampledCount - 1)] = null;
                _sampledColliderCounts[bodyIndex] = sampledCount - 1;
            }

            _colliderCounts[bodyIndex]--;
            if (_colliderCounts[bodyIndex] == 0)
            {
                RemoveBodyAt(bodyIndex);
                bodyBecameInactive = true;
            }

            return true;
        }

        public bool ContainsBody(Rigidbody2D body)
        {
            return body != null && FindBodyIndex(body) >= 0;
        }

        public bool TryGetBodyAt(int index, out Rigidbody2D body)
        {
            if (index < 0 || index >= _logicalBodyCount)
            {
                body = null;
                return false;
            }

            body = _bodies[index];
            return body != null;
        }

        public bool TryGetSampleAt(int index, out WaterSurfaceContactSample sample)
        {
            sample = default;
            if (index < 0 || index >= _logicalBodyCount)
            {
                return false;
            }

            var body = _bodies[index];
            if (!IsValidBody(body))
            {
                return false;
            }

            var sampledCount = _sampledColliderCounts[index];
            var hasBounds = false;
            var representative = (Collider2D)null;
            var aggregateBounds = default(Bounds);
            for (var i = 0; i < sampledCount; i++)
            {
                var collider = _sampledColliders[GetColliderStorageIndex(index, i)];
                if (!IsValidCollider(collider))
                {
                    continue;
                }

                if (!hasBounds)
                {
                    aggregateBounds = collider.bounds;
                    representative = collider;
                    hasBounds = true;
                }
                else
                {
                    aggregateBounds.Encapsulate(collider.bounds);
                }
            }

            if (!hasBounds)
            {
                return false;
            }

            // Unity 6.0.5 deprecates GetInstanceID. EntityId remains a runtime-stable
            // identity for the same Rigidbody2D and avoids the obsolete API diagnostic.
            var bodyKey = body.GetEntityId().GetHashCode();
            sample = new WaterSurfaceContactSample
            {
                Body = body,
                RepresentativeCollider = representative,
                AggregateBounds = aggregateBounds,
                BodyKey = bodyKey,
                ColliderCount = _colliderCounts[index],
                SampledColliderCount = sampledCount
            };
            return IsFinite(aggregateBounds.min) && IsFinite(aggregateBounds.max);
        }

        public void CleanupInvalid()
        {
            for (var bodyIndex = _logicalBodyCount - 1; bodyIndex >= 0; bodyIndex--)
            {
                var body = _bodies[bodyIndex];
                if (!IsValidBody(body))
                {
                    RemoveBodyAt(bodyIndex);
                    continue;
                }

                var sampledCount = _sampledColliderCounts[bodyIndex];
                for (var colliderIndex = sampledCount - 1; colliderIndex >= 0; colliderIndex--)
                {
                    var collider = _sampledColliders[GetColliderStorageIndex(bodyIndex, colliderIndex)];
                    if (IsValidCollider(collider))
                    {
                        continue;
                    }

                    RemoveSampledColliderAt(bodyIndex, colliderIndex);
                    if (_colliderCounts[bodyIndex] > 0)
                    {
                        _colliderCounts[bodyIndex]--;
                    }
                }

                if (_colliderCounts[bodyIndex] <= 0)
                {
                    RemoveBodyAt(bodyIndex);
                }
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _bodies.Length; i++)
            {
                _bodies[i] = null;
                _colliderCounts[i] = 0;
                _sampledColliderCounts[i] = 0;
                _bodySequences[i] = 0;
            }

            for (var i = 0; i < _sampledColliders.Length; i++)
            {
                _sampledColliders[i] = null;
            }

            _logicalBodyCount = 0;
            _nextBodySequence = 0;
            _colliderOverflowCount = 0;
            _droppedBodyCount = 0;
        }

        private void RemoveSampledColliderAt(int bodyIndex, int colliderIndex)
        {
            var sampledCount = _sampledColliderCounts[bodyIndex];
            for (var i = colliderIndex; i < sampledCount - 1; i++)
            {
                _sampledColliders[GetColliderStorageIndex(bodyIndex, i)] =
                    _sampledColliders[GetColliderStorageIndex(bodyIndex, i + 1)];
            }

            _sampledColliders[GetColliderStorageIndex(bodyIndex, sampledCount - 1)] = null;
            _sampledColliderCounts[bodyIndex] = sampledCount - 1;
        }

        private void RemoveBodyAt(int bodyIndex)
        {
            var lastIndex = _logicalBodyCount - 1;
            for (var i = bodyIndex; i < lastIndex; i++)
            {
                _bodies[i] = _bodies[i + 1];
                _colliderCounts[i] = _colliderCounts[i + 1];
                _sampledColliderCounts[i] = _sampledColliderCounts[i + 1];
                _bodySequences[i] = _bodySequences[i + 1];
                for (var colliderIndex = 0; colliderIndex < CompileTimeMaximumSampledCollidersPerBody; colliderIndex++)
                {
                    _sampledColliders[GetColliderStorageIndex(i, colliderIndex)] =
                        _sampledColliders[GetColliderStorageIndex(i + 1, colliderIndex)];
                }
            }

            _bodies[lastIndex] = null;
            _colliderCounts[lastIndex] = 0;
            _sampledColliderCounts[lastIndex] = 0;
            _bodySequences[lastIndex] = 0;
            for (var colliderIndex = 0; colliderIndex < CompileTimeMaximumSampledCollidersPerBody; colliderIndex++)
            {
                _sampledColliders[GetColliderStorageIndex(lastIndex, colliderIndex)] = null;
            }

            _logicalBodyCount = lastIndex;
        }

        private int FindBodyIndex(Rigidbody2D body)
        {
            if (body == null)
            {
                return -1;
            }

            for (var i = 0; i < _logicalBodyCount; i++)
            {
                if (_bodies[i] == body)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool ContainsSampledCollider(int bodyIndex, Collider2D collider)
        {
            return FindSampledColliderIndex(bodyIndex, collider) >= 0;
        }

        private int FindSampledColliderIndex(int bodyIndex, Collider2D collider)
        {
            var sampledCount = _sampledColliderCounts[bodyIndex];
            for (var i = 0; i < sampledCount; i++)
            {
                if (_sampledColliders[GetColliderStorageIndex(bodyIndex, i)] == collider)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int GetColliderStorageIndex(int bodyIndex, int colliderIndex)
        {
            return bodyIndex * CompileTimeMaximumSampledCollidersPerBody + colliderIndex;
        }

        private static bool IsValidBody(Rigidbody2D body)
        {
            return body != null && body.gameObject != null && body.gameObject.activeInHierarchy && body.simulated;
        }

        private static bool IsValidCollider(Collider2D collider)
        {
            return collider != null && collider.enabled && collider.gameObject != null &&
                   collider.gameObject.activeInHierarchy && IsValidBody(collider.attachedRigidbody);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
