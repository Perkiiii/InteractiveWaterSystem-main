using UnityEngine;
using Water25D.Rendering;

namespace Water25D
{
    /// <summary>
    /// Thin surface-crossing trigger. Trigger callbacks only maintain logical membership;
    /// qualified crossings and contact-foam samples are evaluated on the physics clock.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WaterSurfaceInteraction2D : MonoBehaviour
    {
        private enum SurfaceSide
        {
            Above,
            Straddling,
            Below,
            Invalid
        }

        private struct SurfaceBodyState
        {
            public Rigidbody2D Body;
            public Collider2D RepresentativeCollider;
            public int BodyKey;
            public Bounds PreviousBounds;
            public Bounds CurrentBounds;
            public bool HasPreviousSample;
            public SurfaceSide PreviousSurfaceSide;
            public SurfaceSide CurrentSurfaceSide;
            public bool DownwardCrossingEmitted;
            public bool UpwardCrossingEmitted;
            public bool HasQualifiedSurfaceContact;
            public Vector2 CurrentVelocity;
        }

        private readonly WaterLogicalBodyContactTracker _tracker = new WaterLogicalBodyContactTracker();
        private readonly SurfaceBodyState[] _bodyStates = new SurfaceBodyState[WaterLogicalBodyContactTracker.CompileTimeMaximumLogicalBodies];
        private Water25DController _water;
        private LayerMask _solidInteractionLayers;
        private LayerMask _triggerInteractionLayers;
        private bool _includeTriggerColliders;
        private float _crossingEpsilon = 0.02f;
        private int _bodyStateCount;

        public int LogicalContactCount => _tracker.LogicalBodyCount;
        public int DroppedTrackedBodyCount => _tracker.DroppedBodyCount;
        public int ColliderSampleOverflowCount => _tracker.ColliderOverflowCount;
        public int MaximumTrackedBodies => _tracker.MaximumLogicalBodies;

        internal void Configure(
            Water25DController water,
            LayerMask solidInteractionLayers,
            LayerMask triggerInteractionLayers,
            bool includeTriggerColliders,
            int maximumTrackedBodies,
            float crossingEpsilon)
        {
            if (_water != water)
            {
                _tracker.Clear();
                ClearBodyStates();
            }

            _water = water;
            _solidInteractionLayers = solidInteractionLayers;
            _triggerInteractionLayers = triggerInteractionLayers;
            _includeTriggerColliders = includeTriggerColliders;
            _tracker.Configure(maximumTrackedBodies);
            _crossingEpsilon = Mathf.Clamp(crossingEpsilon, 0.001f, 0.25f);
        }

        private void Awake()
        {
            if (_water == null)
            {
                _water = GetComponentInParent<Water25DController>();
            }
        }

        private void FixedUpdate()
        {
            if (!isActiveAndEnabled || _water == null || !_water.isActiveAndEnabled)
            {
                return;
            }

            _tracker.CleanupInvalid();
            RemoveStatesForUntrackedBodies();
            for (var i = 0; i < _tracker.LogicalBodyCount; i++)
            {
                if (!_tracker.TryGetSampleAt(i, out var sample))
                {
                    continue;
                }

                var stateIndex = FindBodyState(sample.Body);
                if (stateIndex < 0)
                {
                    stateIndex = AddBodyState(sample);
                    if (stateIndex < 0)
                    {
                        continue;
                    }
                }

                EvaluateBodySample(stateIndex, sample);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!CanInteract(other))
            {
                return;
            }

            // Membership is deliberately the only callback-side effect. Crossing decisions
            // use aggregate bounds and Rigidbody2D velocity in FixedUpdate.
            _tracker.TryAdd(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!CanInteract(other))
            {
                return;
            }

            _tracker.TryRemove(other);
        }

        private void OnDisable()
        {
            ClearContacts();
        }

        internal void ClearContacts()
        {
            _tracker.Clear();
            ClearBodyStates();
        }

        private bool CanInteract(Collider2D other)
        {
            if (!isActiveAndEnabled || _water == null || !_water.isActiveAndEnabled ||
                other == null || other.attachedRigidbody == null)
            {
                return false;
            }

            if (other.isTrigger && !_includeTriggerColliders)
            {
                return false;
            }

            var mask = other.isTrigger ? _triggerInteractionLayers : _solidInteractionLayers;
            return (mask.value & (1 << other.gameObject.layer)) != 0;
        }

        private int AddBodyState(WaterSurfaceContactSample sample)
        {
            if (_bodyStateCount >= _bodyStates.Length)
            {
                return -1;
            }

            var velocity = GetFiniteVelocity(sample.Body);
            var estimatedPreviousBounds = sample.AggregateBounds;
            estimatedPreviousBounds.center -= new Vector3(
                velocity.x * Time.fixedDeltaTime,
                velocity.y * Time.fixedDeltaTime,
                0f);
            var stateIndex = _bodyStateCount++;
            _bodyStates[stateIndex] = new SurfaceBodyState
            {
                Body = sample.Body,
                RepresentativeCollider = sample.RepresentativeCollider,
                BodyKey = sample.BodyKey,
                PreviousBounds = estimatedPreviousBounds,
                CurrentBounds = sample.AggregateBounds,
                HasPreviousSample = true,
                PreviousSurfaceSide = ClassifySurfaceSide(estimatedPreviousBounds),
                CurrentSurfaceSide = ClassifySurfaceSide(sample.AggregateBounds),
                CurrentVelocity = velocity
            };

            // The bounded initial estimate may prove a moving edge crossed the line while the
            // callback was being delivered. A stationary body merely discovered straddling the
            // trigger never emits a synthetic crossing.
            EvaluateCrossing(stateIndex, estimatedPreviousBounds, sample.AggregateBounds, velocity, true, sample);
            return stateIndex;
        }

        private void EvaluateBodySample(int stateIndex, WaterSurfaceContactSample sample)
        {
            var state = _bodyStates[stateIndex];
            var previousBounds = state.CurrentBounds;
            var velocity = GetFiniteVelocity(sample.Body);
            state.PreviousBounds = previousBounds;
            state.CurrentBounds = sample.AggregateBounds;
            state.PreviousSurfaceSide = state.CurrentSurfaceSide;
            state.CurrentSurfaceSide = ClassifySurfaceSide(sample.AggregateBounds);
            state.RepresentativeCollider = sample.RepresentativeCollider;
            state.CurrentVelocity = velocity;
            state.HasPreviousSample = true;
            _bodyStates[stateIndex] = state;

            EvaluateCrossing(stateIndex, previousBounds, sample.AggregateBounds, velocity, false, sample);
            UpdateContactFoam(sample, velocity);
            state = _bodyStates[stateIndex];
            UpdateWake(sample, state);

            if (state.CurrentSurfaceSide == SurfaceSide.Above)
            {
                state.DownwardCrossingEmitted = false;
                state.UpwardCrossingEmitted = false;
                state.HasQualifiedSurfaceContact = false;
            }
            else if (state.CurrentSurfaceSide == SurfaceSide.Below)
            {
                state.UpwardCrossingEmitted = false;
                state.HasQualifiedSurfaceContact = false;
            }

            _bodyStates[stateIndex] = state;
        }

        private void EvaluateCrossing(
            int stateIndex,
            Bounds previousBounds,
            Bounds currentBounds,
            Vector2 velocity,
            bool isInitialEstimate,
            WaterSurfaceContactSample sample)
        {
            if (_water == null || !IsFinite(velocity) ||
                !IsFinite(previousBounds) || !IsFinite(currentBounds))
            {
                return;
            }

            var state = _bodyStates[stateIndex];
            var waterline = _water.WaterlineWorldY;
            var downward = previousBounds.min.y > waterline + _crossingEpsilon &&
                           currentBounds.min.y <= waterline + _crossingEpsilon &&
                           currentBounds.min.y < previousBounds.min.y &&
                           velocity.y < 0f &&
                           !state.DownwardCrossingEmitted;
            var upward = previousBounds.max.y < waterline - _crossingEpsilon &&
                         currentBounds.max.y >= waterline - _crossingEpsilon &&
                         currentBounds.max.y > previousBounds.max.y &&
                         velocity.y > 0f &&
                         !state.UpwardCrossingEmitted;

            if (isInitialEstimate)
            {
                var previousSide = ClassifySurfaceSide(previousBounds);
                var currentSide = ClassifySurfaceSide(currentBounds);
                if (previousSide == currentSide)
                {
                    downward = false;
                    upward = false;
                }
            }

            if (downward)
            {
                state.DownwardCrossingEmitted = true;
                state.HasQualifiedSurfaceContact = true;
                _bodyStates[stateIndex] = state;
                EmitQualifiedCrossing(sample, velocity, WaterInteractionEventType.SurfaceEnter);
            }
            else if (upward)
            {
                state.UpwardCrossingEmitted = true;
                state.HasQualifiedSurfaceContact = false;
                _bodyStates[stateIndex] = state;
                EmitQualifiedCrossing(sample, velocity, WaterInteractionEventType.SurfaceExit);
            }
        }

        private void EmitQualifiedCrossing(
            WaterSurfaceContactSample sample,
            Vector2 velocity,
            WaterInteractionEventType eventType)
        {
            if (_water == null)
            {
                return;
            }

            var strength = _water.CalculateImpactStrength(velocity);
            var eventPosition = new Vector2(sample.AggregateBounds.center.x, _water.WaterlineWorldY);
            var eventData = new WaterInteractionEvent(
                _water,
                sample.Body,
                sample.RepresentativeCollider,
                eventPosition,
                velocity,
                strength,
                eventType);

            var initialUp = eventType == WaterInteractionEventType.SurfaceExit;
            _water.CreateSurfaceImpactAt(
                _water.GetInteractionWorldPosition(eventPosition),
                strength,
                initialUp,
                _water.GetImpactRadius(velocity));
            _water.NotifyInteraction(eventData);
        }

        private void UpdateContactFoam(WaterSurfaceContactSample sample, Vector2 velocity)
        {
            if (_water == null || _water.SurfaceMode != WaterSurfaceMode.FlatStylized)
            {
                return;
            }

            var bounds = sample.AggregateBounds;
            var waterline = _water.WaterlineWorldY;
            var straddlesWaterline = bounds.min.y <= waterline && bounds.max.y >= waterline;
            if (!straddlesWaterline || bounds.size.y <= 0f)
            {
                _water.ReleaseSurfaceContactFoam(sample.BodyKey);
                return;
            }

            var submersion01 = Mathf.InverseLerp(bounds.min.y, bounds.max.y, waterline);
            _water.UpdateSurfaceContactFoam(
                sample.BodyKey,
                new Vector2(bounds.center.x, waterline),
                bounds.size.x,
                submersion01,
                1f);
        }

        private void UpdateWake(WaterSurfaceContactSample sample, SurfaceBodyState state)
        {
            if (_water == null || state.CurrentSurfaceSide != SurfaceSide.Straddling ||
                !state.HasQualifiedSurfaceContact)
            {
                _water?.ReleaseSurfaceWake(sample.BodyKey);
                return;
            }

            var bounds = sample.AggregateBounds;
            if (!IsFinite(bounds) || bounds.size.x < 0f)
            {
                _water.ReleaseSurfaceWake(sample.BodyKey);
                return;
            }

            _water.UpdateSurfaceWake(
                sample.BodyKey,
                new Vector2(bounds.center.x, _water.WaterlineWorldY),
                bounds.size.x,
                Time.fixedDeltaTime);
        }

        private void RemoveStatesForUntrackedBodies()
        {
            for (var i = _bodyStateCount - 1; i >= 0; i--)
            {
                var body = _bodyStates[i].Body;
                if (_tracker.ContainsBody(body))
                {
                    continue;
                }

                _water?.ReleaseSurfaceContactFoam(_bodyStates[i].BodyKey);
                _water?.ReleaseSurfaceWake(_bodyStates[i].BodyKey);
                RemoveBodyStateAt(i);
            }
        }

        private int FindBodyState(Rigidbody2D body)
        {
            for (var i = 0; i < _bodyStateCount; i++)
            {
                if (_bodyStates[i].Body == body)
                {
                    return i;
                }
            }

            return -1;
        }

        private void RemoveBodyStateAt(int index)
        {
            var lastIndex = _bodyStateCount - 1;
            if (index != lastIndex)
            {
                _bodyStates[index] = _bodyStates[lastIndex];
            }

            _bodyStates[lastIndex] = default;
            _bodyStateCount = lastIndex;
        }

        private void ClearBodyStates()
        {
            for (var i = 0; i < _bodyStates.Length; i++)
            {
                if (_bodyStates[i].Body != null)
                {
                    _water?.ReleaseSurfaceContactFoam(_bodyStates[i].BodyKey);
                    _water?.ReleaseSurfaceWake(_bodyStates[i].BodyKey);
                }

                _bodyStates[i] = default;
            }

            _bodyStateCount = 0;
        }

        private SurfaceSide ClassifySurfaceSide(Bounds bounds)
        {
            if (!IsFinite(bounds))
            {
                return SurfaceSide.Invalid;
            }

            var waterline = _water != null ? _water.WaterlineWorldY : 0f;
            if (bounds.min.y > waterline + _crossingEpsilon)
            {
                return SurfaceSide.Above;
            }

            if (bounds.max.y < waterline - _crossingEpsilon)
            {
                return SurfaceSide.Below;
            }

            return SurfaceSide.Straddling;
        }

        private static Vector2 GetFiniteVelocity(Rigidbody2D body)
        {
            if (body == null)
            {
                return Vector2.zero;
            }

            var velocity = body.linearVelocity;
            return IsFinite(velocity) ? velocity : Vector2.zero;
        }

        private static bool IsFinite(Bounds bounds)
        {
            return IsFinite(bounds.min) && IsFinite(bounds.max);
        }

        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
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
