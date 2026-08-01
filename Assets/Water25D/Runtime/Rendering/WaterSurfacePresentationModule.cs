using UnityEngine;

namespace Water25D.Rendering
{
    /// <summary>
    /// Owns the transient, per-water presentation state used by FlatStylized. Rings are
    /// uploaded through the existing top/front surface draws; this module creates no Unity
    /// render objects and never grows its fixed storage after construction.
    /// </summary>
    public sealed class WaterSurfacePresentationModule
    {
        public const int ShaderMaximumRings = 16;
        public const int DefaultMaximumSurfaceRings = 8;

        private struct RingSlot
        {
            public Vector2 CenterLocalXZ;
            public float Age;
            public float Lifetime;
            public float StartRadius;
            public float EndRadius;
            public float Thickness;
            public float Softness;
            public float Intensity;
            public float DirectionSign;
            public int CreationSequence;
        }

        private readonly RingSlot[] _ringSlots = new RingSlot[ShaderMaximumRings];
        private readonly Vector4[] _ringsA = new Vector4[ShaderMaximumRings];
        private readonly Vector4[] _ringsB = new Vector4[ShaderMaximumRings];
        private readonly WaterSurfaceRenderData _renderData;

        private int _maximumSurfaceRings = DefaultMaximumSurfaceRings;
        private int _activeRingCount;
        private int _replacedRingCount;
        private int _nextCreationSequence;
        private float _defaultImpactRadius;
        private float _ringLifetime;
        private float _ringExpansionMultiplier;
        private float _ringThickness;
        private float _ringSoftness;
        private float _ringIntensity;

        public WaterSurfacePresentationModule(int maximumSurfaceRings = DefaultMaximumSurfaceRings)
        {
            _renderData = new WaterSurfaceRenderData(_ringsA, _ringsB);
            ApplyConfiguration(maximumSurfaceRings, WaterQualitySettings.Default.ImpactRadius, WaterStyleSettings.Default);
            Reset();
        }

        public int MaximumSurfaceRings => _maximumSurfaceRings;
        public int ActiveRingCount => _activeRingCount;
        public int ReplacedRingCount => _replacedRingCount;
        public WaterSurfaceRenderData RenderData => _renderData;

        /// <summary>
        /// Applies profile values without recreating fixed storage. Existing rings retain the
        /// values captured when they were created; new rings use the current profile values.
        /// </summary>
        public void Configure(WaterQualitySettings qualitySettings, WaterStyleSettings styleSettings)
        {
            qualitySettings.Sanitize();
            styleSettings.Sanitize();
            ApplyConfiguration(qualitySettings.MaximumSurfaceRings, qualitySettings.ImpactRadius, styleSettings);
        }

        /// <summary>
        /// Creates one logical ring in local XZ units. A non-positive radius uses the current
        /// quality-profile impact radius, matching the controller's mode-neutral API contract.
        /// </summary>
        public bool AddRing(Vector2 centerLocalXZ, float strength, float radius, bool initialUp = true)
        {
            if (!IsFinite(centerLocalXZ.x) || !IsFinite(centerLocalXZ.y) ||
                !IsFinite(strength))
            {
                return false;
            }

            ReclaimExpiredSlots();

            var safeStrength = Mathf.Clamp(Mathf.Abs(strength), 0f, 1f);
            if (safeStrength <= 0f)
            {
                return false;
            }

            var safeRadius = radius;
            if (!IsFinite(safeRadius) || safeRadius <= 0f)
            {
                safeRadius = _defaultImpactRadius;
            }

            safeRadius = Mathf.Clamp(safeRadius, 0.005f, 10f);
            var slotIndex = _activeRingCount;
            if (_activeRingCount >= _maximumSurfaceRings)
            {
                slotIndex = FindOldestSlotIndex();
                _replacedRingCount++;
            }
            else
            {
                _activeRingCount++;
            }

            var expansion = Mathf.Max(0.01f, _ringExpansionMultiplier);
            _ringSlots[slotIndex] = new RingSlot
            {
                CenterLocalXZ = centerLocalXZ,
                Age = 0f,
                Lifetime = _ringLifetime,
                StartRadius = safeRadius,
                EndRadius = safeRadius * expansion,
                Thickness = _ringThickness,
                Softness = _ringSoftness,
                Intensity = Mathf.Clamp01(safeStrength * _ringIntensity),
                DirectionSign = initialUp ? 1f : -1f,
                CreationSequence = ++_nextCreationSequence
            };

            RebuildRenderData();
            return true;
        }

        /// <summary>
        /// Advances active ring ages. The return value indicates that the upload data changed.
        /// </summary>
        public bool Tick(float elapsedTime)
        {
            if (_activeRingCount == 0 || !IsFinite(elapsedTime) || elapsedTime <= 0f)
            {
                return false;
            }

            var safeElapsedTime = Mathf.Min(elapsedTime, 60f);
            for (var i = _activeRingCount - 1; i >= 0; i--)
            {
                var slot = _ringSlots[i];
                slot.Age += safeElapsedTime;
                _ringSlots[i] = slot;
                if (slot.Age >= slot.Lifetime)
                {
                    RemoveSlotAt(i);
                }
            }

            RebuildRenderData();
            return true;
        }

        /// <summary>
        /// Clears all transient ring data and uploads zero active entries through RenderData.
        /// </summary>
        public void Reset()
        {
            _activeRingCount = 0;
            _replacedRingCount = 0;
            _nextCreationSequence = 0;
            RebuildRenderData();
        }

        private void ApplyConfiguration(int maximumSurfaceRings, float defaultImpactRadius, WaterStyleSettings styleSettings)
        {
            var previousCapacity = _maximumSurfaceRings;
            _maximumSurfaceRings = Mathf.Clamp(maximumSurfaceRings, 1, ShaderMaximumRings);
            _defaultImpactRadius = SanitizeRadius(defaultImpactRadius, WaterQualitySettings.Default.ImpactRadius);
            _ringLifetime = SanitizePositive(styleSettings.RingLifetime, WaterStyleSettings.Default.RingLifetime, 0.01f, 60f);
            _ringExpansionMultiplier = SanitizePositive(styleSettings.RingExpansionMultiplier, WaterStyleSettings.Default.RingExpansionMultiplier, 0.01f, 100f);
            _ringThickness = SanitizePositive(styleSettings.RingThickness, WaterStyleSettings.Default.RingThickness, 0.001f, 10f);
            _ringSoftness = SanitizeNonNegative(styleSettings.RingSoftness, WaterStyleSettings.Default.RingSoftness, 0f, 10f);
            _ringIntensity = SanitizeIntensity(styleSettings.RingIntensity, WaterStyleSettings.Default.RingIntensity);

            if (_activeRingCount > _maximumSurfaceRings)
            {
                while (_activeRingCount > _maximumSurfaceRings)
                {
                    RemoveSlotAt(FindOldestSlotIndex());
                }

                RebuildRenderData();
            }
            else if (previousCapacity != _maximumSurfaceRings && _activeRingCount > 0)
            {
                RebuildRenderData();
            }
        }

        private void ReclaimExpiredSlots()
        {
            var removed = false;
            for (var i = _activeRingCount - 1; i >= 0; i--)
            {
                if (_ringSlots[i].Age < _ringSlots[i].Lifetime)
                {
                    continue;
                }

                RemoveSlotAt(i);
                removed = true;
            }

            if (removed)
            {
                RebuildRenderData();
            }
        }

        private int FindOldestSlotIndex()
        {
            var oldestIndex = 0;
            var oldestSequence = _ringSlots[0].CreationSequence;
            for (var i = 1; i < _activeRingCount; i++)
            {
                if (_ringSlots[i].CreationSequence < oldestSequence)
                {
                    oldestIndex = i;
                    oldestSequence = _ringSlots[i].CreationSequence;
                }
            }

            return oldestIndex;
        }

        private void RemoveSlotAt(int index)
        {
            var lastIndex = _activeRingCount - 1;
            if (index != lastIndex)
            {
                _ringSlots[index] = _ringSlots[lastIndex];
            }

            _ringSlots[lastIndex] = default;
            _activeRingCount = lastIndex;
        }

        private void RebuildRenderData()
        {
            for (var i = 0; i < ShaderMaximumRings; i++)
            {
                _ringsA[i] = Vector4.zero;
                _ringsB[i] = Vector4.zero;
            }

            for (var i = 0; i < _activeRingCount; i++)
            {
                var slot = _ringSlots[i];
                var age01 = Mathf.Clamp01(slot.Age / Mathf.Max(0.01f, slot.Lifetime));
                _ringsA[i] = new Vector4(slot.CenterLocalXZ.x, slot.CenterLocalXZ.y, age01, slot.Intensity);
                _ringsB[i] = new Vector4(slot.StartRadius, slot.EndRadius, slot.Thickness, slot.Softness);
            }

            _renderData.ActiveRingCount = _activeRingCount;
        }

        private static float SanitizeRadius(float value, float fallback)
        {
            if (!IsFinite(value) || value <= 0f)
            {
                value = fallback;
            }

            return Mathf.Clamp(value, 0.005f, 10f);
        }

        private static float SanitizePositive(float value, float fallback, float minimum, float maximum)
        {
            if (!IsFinite(value) || value <= 0f)
            {
                value = fallback;
            }

            return Mathf.Clamp(value, minimum, maximum);
        }

        private static float SanitizeNonNegative(float value, float fallback, float minimum, float maximum)
        {
            if (!IsFinite(value) || value < 0f)
            {
                value = fallback;
            }

            return Mathf.Clamp(value, minimum, maximum);
        }

        private static float SanitizeIntensity(float value, float fallback)
        {
            return IsFinite(value) ? Mathf.Clamp01(value) : Mathf.Clamp01(fallback);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
