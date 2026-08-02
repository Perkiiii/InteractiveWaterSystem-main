using UnityEngine;

namespace Water25D.Rendering
{
    /// <summary>
    /// Owns fixed-capacity, per-water presentation state used by FlatStylized. Rings, contact
    /// foam and wake segments are uploaded through the existing top/front surface draws; this
    /// module creates no Unity render objects and never grows its storage after construction.
    /// </summary>
    public sealed class WaterSurfacePresentationModule
    {
        public const int ShaderMaximumRings = 16;
        public const int DefaultMaximumSurfaceRings = 8;
        public const int ShaderMaximumContactFoams = 8;
        public const int DefaultMaximumContactFoams = 4;
        public const int ShaderMaximumWakeSegments = 16;
        public const int DefaultMaximumWakeSegments = 8;
        public const int DefaultMaximumWakeEmissionsPerStep = 2;
        public const int CompileTimeMaximumWakeBodies = WaterLogicalBodyContactTracker.CompileTimeMaximumLogicalBodies;

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

        private struct ContactFoamSlot
        {
            public int BodyKey;
            public Vector2 CenterLocalXZ;
            public float HalfWidth;
            public float HalfDepth;
            public float Intensity;
            public float Submersion;
            public float FadeAmount;
            public float NoisePhase;
            public bool HasSlot;
            public bool Active;
            public bool Fading;
            public int CreationSequence;
        }

        private struct WakeSlot
        {
            public int BodyKey;
            public Vector2 StartLocalXZ;
            public Vector2 EndLocalXZ;
            public float HalfWidth;
            public float Age;
            public float Lifetime;
            public float Intensity;
            public float NoisePhase;
            public bool HasSlot;
            public int CreationSequence;
        }

        private struct WakeBodyState
        {
            public int BodyKey;
            public Vector2 LastAcceptedSurfacePosition;
            public Vector2 PreviousDirection;
            public float DistanceRemainder;
            public bool HasPreviousDirection;
            public bool HasState;
        }

        private readonly RingSlot[] _ringSlots = new RingSlot[ShaderMaximumRings];
        private readonly Vector4[] _ringsA = new Vector4[ShaderMaximumRings];
        private readonly Vector4[] _ringsB = new Vector4[ShaderMaximumRings];
        private readonly ContactFoamSlot[] _contactFoamSlots = new ContactFoamSlot[ShaderMaximumContactFoams];
        private readonly Vector4[] _foamsA = new Vector4[ShaderMaximumContactFoams];
        private readonly Vector4[] _foamsB = new Vector4[ShaderMaximumContactFoams];
        private readonly WakeSlot[] _wakeSlots = new WakeSlot[ShaderMaximumWakeSegments];
        private readonly WakeBodyState[] _wakeBodyStates = new WakeBodyState[CompileTimeMaximumWakeBodies];
        private readonly Vector4[] _wakesA = new Vector4[ShaderMaximumWakeSegments];
        private readonly Vector4[] _wakesB = new Vector4[ShaderMaximumWakeSegments];
        private readonly WaterSurfaceRenderData _renderData;

        private int _maximumSurfaceRings = DefaultMaximumSurfaceRings;
        private int _maximumContactFoams = DefaultMaximumContactFoams;
        private int _maximumWakeSegments = DefaultMaximumWakeSegments;
        private int _maximumWakeEmissionsPerStep = DefaultMaximumWakeEmissionsPerStep;
        private int _maximumWakeBodies = WaterQualitySettings.Default.MaximumTrackedSurfaceBodies;
        private int _activeRingCount;
        private int _replacedRingCount;
        private int _activeContactFoamCount;
        private int _fadingContactFoamCount;
        private int _droppedContactFoamCount;
        private int _activeWakeCount;
        private int _replacedWakeCount;
        private int _droppedWakeBodyCount;
        private int _wakeBodyStateCount;
        private int _nextCreationSequence;
        private int _nextWakeCreationSequence;
        private float _defaultImpactRadius;
        private float _ringLifetime;
        private float _ringExpansionMultiplier;
        private float _ringThickness;
        private float _ringSoftness;
        private float _ringIntensity;
        private float _contactFoamWidthPadding;
        private float _contactFoamHalfDepth;
        private float _contactFoamIntensity;
        private float _contactFoamFadeDuration;
        private float _wakeEmissionSpacing;
        private float _wakeMinimumLateralSpeed;
        private float _wakeWidthMultiplier;
        private float _wakeWidthPadding;
        private float _wakeMinimumHalfWidth;
        private float _wakeMaximumHalfWidth;
        private float _wakeLifetime;
        private float _wakeFadePower;
        private float _wakeIntensity;
        private float _wakeDirectionReversalCosine;

        public WaterSurfacePresentationModule(int maximumSurfaceRings = DefaultMaximumSurfaceRings)
        {
            _renderData = new WaterSurfaceRenderData(_ringsA, _ringsB, _foamsA, _foamsB, _wakesA, _wakesB);
            ApplyConfiguration(
                maximumSurfaceRings,
                DefaultMaximumContactFoams,
                WaterQualitySettings.Default.MaximumWakeSegments,
                WaterQualitySettings.Default.MaximumWakeEmissionsPerStep,
                WaterQualitySettings.Default.MaximumTrackedSurfaceBodies,
                WaterQualitySettings.Default.ImpactRadius,
                WaterStyleSettings.Default);
            Reset();
        }

        public int MaximumSurfaceRings => _maximumSurfaceRings;
        public int MaximumContactFoams => _maximumContactFoams;
        public int ActiveRingCount => _activeRingCount;
        public int ReplacedRingCount => _replacedRingCount;
        public int ActiveContactFoamCount => _activeContactFoamCount;
        public int FadingContactFoamCount => _fadingContactFoamCount;
        public int DroppedContactFoamCount => _droppedContactFoamCount;
        public int MaximumWakeSegments => _maximumWakeSegments;
        public int MaximumWakeEmissionsPerStep => _maximumWakeEmissionsPerStep;
        public int ActiveWakeSegmentCount => _activeWakeCount;
        public int ReplacedWakeCount => _replacedWakeCount;
        public int DroppedWakeBodyCount => _droppedWakeBodyCount;
        public WaterSurfaceRenderData RenderData => _renderData;

        /// <summary>
        /// Applies profile values without recreating fixed storage. Existing interactions
        /// retain their captured values; new slots use the current profile values.
        /// </summary>
        public void Configure(WaterQualitySettings qualitySettings, WaterStyleSettings styleSettings)
        {
            qualitySettings.Sanitize();
            styleSettings.Sanitize();
            ApplyConfiguration(
                qualitySettings.MaximumSurfaceRings,
                qualitySettings.MaximumContactFoams,
                qualitySettings.MaximumWakeSegments,
                qualitySettings.MaximumWakeEmissionsPerStep,
                qualitySettings.MaximumTrackedSurfaceBodies,
                qualitySettings.ImpactRadius,
                styleSettings);
        }

        /// <summary>
        /// Creates one logical ring in local XZ units. A non-positive radius uses the current
        /// quality-profile impact radius, matching the controller's mode-neutral API contract.
        /// </summary>
        public bool AddRing(Vector2 centerLocalXZ, float strength, float radius, bool initialUp = true)
        {
            if (!IsFinite(centerLocalXZ.x) || !IsFinite(centerLocalXZ.y) || !IsFinite(strength))
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
                slotIndex = FindOldestRingSlotIndex();
                _replacedRingCount++;
            }
            else
            {
                _activeRingCount++;
            }

            _ringSlots[slotIndex] = new RingSlot
            {
                CenterLocalXZ = centerLocalXZ,
                Age = 0f,
                Lifetime = _ringLifetime,
                StartRadius = safeRadius,
                EndRadius = safeRadius * _ringExpansionMultiplier,
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
        /// Updates or creates one body-keyed contact foam slot. This is intentionally internal:
        /// the controller is the package boundary that maps gameplay positions into local XZ.
        /// </summary>
        internal bool UpdateContactFoam(
            int bodyKey,
            Vector2 centerLocalXZ,
            float halfWidth,
            float submersion01,
            float intensity)
        {
            if (!IsFinite(centerLocalXZ.x) || !IsFinite(centerLocalXZ.y) ||
                !IsFinite(halfWidth) || !IsFinite(submersion01) || !IsFinite(intensity))
            {
                return false;
            }

            var slotIndex = FindContactFoamSlot(bodyKey);
            if (slotIndex < 0)
            {
                slotIndex = FindReclaimableContactFoamSlot();
                if (slotIndex < 0)
                {
                    _droppedContactFoamCount++;
                    return false;
                }

                var noisePhase = ComputeNoisePhase(bodyKey);
                _contactFoamSlots[slotIndex] = new ContactFoamSlot
                {
                    BodyKey = bodyKey,
                    CenterLocalXZ = centerLocalXZ,
                    HalfWidth = Mathf.Max(0.001f, halfWidth) + _contactFoamWidthPadding,
                    HalfDepth = _contactFoamHalfDepth,
                    Intensity = Mathf.Clamp01(Mathf.Abs(intensity)) * _contactFoamIntensity,
                    Submersion = Mathf.Clamp01(submersion01),
                    FadeAmount = 1f,
                    NoisePhase = noisePhase,
                    HasSlot = true,
                    Active = true,
                    Fading = false,
                    CreationSequence = ++_nextCreationSequence
                };
                RebuildRenderData();
                return true;
            }

            var slot = _contactFoamSlots[slotIndex];
            var changed = !Approximately(slot.CenterLocalXZ, centerLocalXZ) ||
                          !Mathf.Approximately(slot.HalfWidth, Mathf.Max(0.001f, halfWidth) + _contactFoamWidthPadding) ||
                          !Mathf.Approximately(slot.HalfDepth, _contactFoamHalfDepth) ||
                          !Mathf.Approximately(slot.Intensity, Mathf.Clamp01(Mathf.Abs(intensity)) * _contactFoamIntensity) ||
                          !Mathf.Approximately(slot.Submersion, Mathf.Clamp01(submersion01)) ||
                          !Mathf.Approximately(slot.FadeAmount, 1f) ||
                          !slot.Active || slot.Fading;

            slot.CenterLocalXZ = centerLocalXZ;
            slot.HalfWidth = Mathf.Max(0.001f, halfWidth) + _contactFoamWidthPadding;
            slot.HalfDepth = _contactFoamHalfDepth;
            slot.Intensity = Mathf.Clamp01(Mathf.Abs(intensity)) * _contactFoamIntensity;
            slot.Submersion = Mathf.Clamp01(submersion01);
            slot.FadeAmount = 1f;
            slot.Active = true;
            slot.Fading = false;
            _contactFoamSlots[slotIndex] = slot;
            if (changed)
            {
                RebuildRenderData();
            }

            return changed;
        }

        internal bool ReleaseContactFoam(int bodyKey)
        {
            var slotIndex = FindContactFoamSlot(bodyKey);
            if (slotIndex < 0)
            {
                return false;
            }

            var slot = _contactFoamSlots[slotIndex];
            if (!slot.Active && slot.Fading)
            {
                return false;
            }

            slot.Active = false;
            slot.Fading = true;
            _contactFoamSlots[slotIndex] = slot;
            RebuildRenderData();
            return true;
        }

        /// <summary>
        /// Accepts one qualified logical-body surface sample. The sample position is the
        /// controller-mapped local XZ contact centre. Distance, rather than update count or
        /// elapsed time, determines emission positions. The fractional remainder survives a
        /// per-step cap; whole skipped spacing intervals are deliberately discarded so a long
        /// stalled step cannot cause an unbounded burst on the next step.
        /// </summary>
        internal bool UpdateWake(
            int bodyKey,
            Vector2 centerLocalXZ,
            float aggregateHalfWidth,
            float elapsedTime)
        {
            if (!IsFinite(centerLocalXZ.x) || !IsFinite(centerLocalXZ.y) ||
                !IsFinite(aggregateHalfWidth) || !IsFinite(elapsedTime) || elapsedTime <= 0f)
            {
                ReleaseWakeBody(bodyKey);
                return false;
            }

            var stateIndex = FindWakeBodyState(bodyKey);
            if (stateIndex < 0)
            {
                stateIndex = FindFreeWakeBodyState();
                if (stateIndex < 0)
                {
                    _droppedWakeBodyCount++;
                    return false;
                }

                _wakeBodyStates[stateIndex] = new WakeBodyState
                {
                    BodyKey = bodyKey,
                    LastAcceptedSurfacePosition = centerLocalXZ,
                    PreviousDirection = Vector2.zero,
                    DistanceRemainder = 0f,
                    HasPreviousDirection = false,
                    HasState = true
                };
                _wakeBodyStateCount++;
                return false;
            }

            var state = _wakeBodyStates[stateIndex];
            var previousPosition = state.LastAcceptedSurfacePosition;
            var delta = centerLocalXZ - previousPosition;
            state.LastAcceptedSurfacePosition = centerLocalXZ;
            if (!IsFinite(delta.x) || !IsFinite(delta.y))
            {
                state.DistanceRemainder = 0f;
                state.HasPreviousDirection = false;
                _wakeBodyStates[stateIndex] = state;
                return false;
            }

            var distance = delta.magnitude;
            if (!IsFinite(distance) || distance <= 0.00001f)
            {
                _wakeBodyStates[stateIndex] = state;
                return false;
            }

            var speed = distance / elapsedTime;
            if (!IsFinite(speed) || speed < _wakeMinimumLateralSpeed)
            {
                _wakeBodyStates[stateIndex] = state;
                return false;
            }

            var direction = delta / distance;
            if (state.HasPreviousDirection &&
                Vector2.Dot(state.PreviousDirection, direction) <= _wakeDirectionReversalCosine)
            {
                // A reversal starts a new trail. Resetting both the phase and anchor prevents
                // an interpolated segment from bridging the old and new directions.
                state.DistanceRemainder = 0f;
                state.PreviousDirection = direction;
                state.HasPreviousDirection = true;
                _wakeBodyStates[stateIndex] = state;
                return false;
            }

            state.PreviousDirection = direction;
            state.HasPreviousDirection = true;
            var totalDistance = state.DistanceRemainder + distance;
            var spacing = _wakeEmissionSpacing;
            var potentialEmissionCount = totalDistance >= spacing
                ? Mathf.FloorToInt(Mathf.Min(totalDistance / spacing, 4096f))
                : 0;
            var emissionCount = Mathf.Min(potentialEmissionCount, _maximumWakeEmissionsPerStep);
            var firstEmissionDistance = spacing - state.DistanceRemainder;
            if (firstEmissionDistance <= 0.00001f || firstEmissionDistance > spacing)
            {
                firstEmissionDistance = spacing;
            }

            var safeHalfWidth = SanitizeWakeHalfWidth(aggregateHalfWidth);
            var normalizedSpeed = Mathf.Clamp01(speed / Mathf.Max(0.01f, _wakeMinimumLateralSpeed * 2f));
            var intensity = Mathf.Clamp01(normalizedSpeed * _wakeIntensity);
            for (var emissionIndex = 0; emissionIndex < emissionCount; emissionIndex++)
            {
                var distanceAlongPath = firstEmissionDistance + emissionIndex * spacing;
                var emissionPosition = previousPosition + direction * distanceAlongPath;
                AddWakeSegment(
                    bodyKey,
                    emissionPosition - direction * (spacing * 0.5f),
                    emissionPosition + direction * (spacing * 0.5f),
                    safeHalfWidth,
                    intensity);
            }

            state.DistanceRemainder = Mathf.Repeat(totalDistance, spacing);
            _wakeBodyStates[stateIndex] = state;
            if (emissionCount <= 0)
            {
                return false;
            }

            RebuildRenderData();
            return true;
        }

        internal bool ReleaseWakeBody(int bodyKey)
        {
            var stateIndex = FindWakeBodyState(bodyKey);
            if (stateIndex < 0)
            {
                return false;
            }

            RemoveWakeBodyStateAt(stateIndex);
            return true;
        }

        internal bool TryGetWakeDistanceRemainder(int bodyKey, out float remainder)
        {
            var stateIndex = FindWakeBodyState(bodyKey);
            if (stateIndex < 0)
            {
                remainder = 0f;
                return false;
            }

            remainder = _wakeBodyStates[stateIndex].DistanceRemainder;
            return true;
        }

        internal int GetWakeBodyKeyAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _maximumWakeSegments || !_wakeSlots[slotIndex].HasSlot)
            {
                return 0;
            }

            return _wakeSlots[slotIndex].BodyKey;
        }

        /// <summary>
        /// Advances active ring, wake and fading foam ages. The return value indicates that
        /// upload data changed. Fading foam slots remain keyed until their fade reaches zero.
        /// </summary>
        public bool Tick(float elapsedTime)
        {
            if (!IsFinite(elapsedTime) || elapsedTime <= 0f)
            {
                return false;
            }

            var safeElapsedTime = Mathf.Min(elapsedTime, 60f);
            var changed = false;
            for (var i = _activeRingCount - 1; i >= 0; i--)
            {
                var slot = _ringSlots[i];
                slot.Age += safeElapsedTime;
                _ringSlots[i] = slot;
                if (slot.Age >= slot.Lifetime)
                {
                    RemoveRingSlotAt(i);
                }

                changed = true;
            }

            for (var i = 0; i < _maximumWakeSegments; i++)
            {
                var wake = _wakeSlots[i];
                if (!wake.HasSlot)
                {
                    continue;
                }

                wake.Age += safeElapsedTime;
                if (wake.Age >= wake.Lifetime)
                {
                    _wakeSlots[i] = default;
                    _activeWakeCount--;
                }
                else
                {
                    _wakeSlots[i] = wake;
                }

                changed = true;
            }

            for (var i = 0; i < _maximumContactFoams; i++)
            {
                var foam = _contactFoamSlots[i];
                if (!foam.HasSlot || !foam.Fading)
                {
                    continue;
                }

                foam.FadeAmount = Mathf.Max(0f, foam.FadeAmount - safeElapsedTime / _contactFoamFadeDuration);
                if (foam.FadeAmount <= 0f)
                {
                    _contactFoamSlots[i] = default;
                }
                else
                {
                    _contactFoamSlots[i] = foam;
                }

                changed = true;
            }

            if (changed)
            {
                RebuildRenderData();
            }

            return changed;
        }

        /// <summary>
        /// Clears all transient ring, foam and wake data. No event or fade is emitted by a reset.
        /// </summary>
        public void Reset()
        {
            _activeRingCount = 0;
            _replacedRingCount = 0;
            _activeContactFoamCount = 0;
            _fadingContactFoamCount = 0;
            _droppedContactFoamCount = 0;
            _activeWakeCount = 0;
            _replacedWakeCount = 0;
            _droppedWakeBodyCount = 0;
            _wakeBodyStateCount = 0;
            _nextCreationSequence = 0;
            _nextWakeCreationSequence = 0;
            for (var i = 0; i < _ringSlots.Length; i++)
            {
                _ringSlots[i] = default;
            }

            for (var i = 0; i < _contactFoamSlots.Length; i++)
            {
                _contactFoamSlots[i] = default;
            }

            for (var i = 0; i < _wakeSlots.Length; i++)
            {
                _wakeSlots[i] = default;
            }

            for (var i = 0; i < _wakeBodyStates.Length; i++)
            {
                _wakeBodyStates[i] = default;
            }

            RebuildRenderData();
        }

        private void ApplyConfiguration(
            int maximumSurfaceRings,
            int maximumContactFoams,
            int maximumWakeSegments,
            int maximumWakeEmissionsPerStep,
            int maximumWakeBodies,
            float defaultImpactRadius,
            WaterStyleSettings styleSettings)
        {
            var previousRingCapacity = _maximumSurfaceRings;
            var previousFoamCapacity = _maximumContactFoams;
            var previousWakeCapacity = _maximumWakeSegments;
            var previousWakeBodyCapacity = _maximumWakeBodies;
            _maximumSurfaceRings = Mathf.Clamp(maximumSurfaceRings, 1, ShaderMaximumRings);
            _maximumContactFoams = Mathf.Clamp(maximumContactFoams, 1, ShaderMaximumContactFoams);
            _maximumWakeSegments = Mathf.Clamp(maximumWakeSegments, 1, ShaderMaximumWakeSegments);
            _maximumWakeEmissionsPerStep = Mathf.Clamp(maximumWakeEmissionsPerStep, 1, ShaderMaximumWakeSegments);
            _maximumWakeBodies = Mathf.Clamp(maximumWakeBodies, 1, CompileTimeMaximumWakeBodies);
            _defaultImpactRadius = SanitizeRadius(defaultImpactRadius, WaterQualitySettings.Default.ImpactRadius);
            _ringLifetime = SanitizePositive(styleSettings.RingLifetime, WaterStyleSettings.Default.RingLifetime, 0.01f, 60f);
            _ringExpansionMultiplier = SanitizePositive(styleSettings.RingExpansionMultiplier, WaterStyleSettings.Default.RingExpansionMultiplier, 1f, 100f);
            _ringThickness = SanitizePositive(styleSettings.RingThickness, WaterStyleSettings.Default.RingThickness, 0.001f, 10f);
            _ringSoftness = SanitizeNonNegative(styleSettings.RingSoftness, WaterStyleSettings.Default.RingSoftness, 0f, 10f);
            _ringIntensity = SanitizeIntensity(styleSettings.RingIntensity, WaterStyleSettings.Default.RingIntensity);
            _contactFoamWidthPadding = SanitizeNonNegative(styleSettings.ContactFoamWidthPadding, WaterStyleSettings.Default.ContactFoamWidthPadding, 0f, 2f);
            _contactFoamHalfDepth = SanitizePositive(styleSettings.ContactFoamHalfDepth, WaterStyleSettings.Default.ContactFoamHalfDepth, 0.01f, 2f);
            _contactFoamIntensity = SanitizeIntensity(styleSettings.ContactFoamIntensity, WaterStyleSettings.Default.ContactFoamIntensity);
            _contactFoamFadeDuration = SanitizePositive(styleSettings.ContactFoamFadeDuration, WaterStyleSettings.Default.ContactFoamFadeDuration, 0.01f, 5f);
            _wakeEmissionSpacing = SanitizePositive(styleSettings.WakeEmissionSpacing, WaterStyleSettings.Default.WakeEmissionSpacing, 0.01f, 10f);
            _wakeMinimumLateralSpeed = SanitizeNonNegative(styleSettings.WakeMinimumLateralSpeed, WaterStyleSettings.Default.WakeMinimumLateralSpeed, 0f, 100f);
            _wakeWidthMultiplier = SanitizeNonNegative(styleSettings.WakeWidthMultiplier, WaterStyleSettings.Default.WakeWidthMultiplier, 0f, 4f);
            _wakeWidthPadding = SanitizeNonNegative(styleSettings.WakeWidthPadding, WaterStyleSettings.Default.WakeWidthPadding, 0f, 2f);
            _wakeMinimumHalfWidth = SanitizePositive(styleSettings.WakeMinimumHalfWidth, WaterStyleSettings.Default.WakeMinimumHalfWidth, 0.001f, 2f);
            _wakeMaximumHalfWidth = SanitizePositive(styleSettings.WakeMaximumHalfWidth, WaterStyleSettings.Default.WakeMaximumHalfWidth, _wakeMinimumHalfWidth, 4f);
            _wakeLifetime = SanitizePositive(styleSettings.WakeLifetime, WaterStyleSettings.Default.WakeLifetime, 0.01f, 60f);
            _wakeFadePower = SanitizePositive(styleSettings.WakeFadePower, WaterStyleSettings.Default.WakeFadePower, 0.1f, 4f);
            _wakeIntensity = SanitizeIntensity(styleSettings.WakeIntensity, WaterStyleSettings.Default.WakeIntensity);
            var reversalAngle = IsFinite(styleSettings.WakeDirectionReversalAngle)
                ? Mathf.Clamp(styleSettings.WakeDirectionReversalAngle, 90f, 179f)
                : WaterStyleSettings.Default.WakeDirectionReversalAngle;
            _wakeDirectionReversalCosine = Mathf.Cos(reversalAngle * Mathf.Deg2Rad);

            if (_activeRingCount > _maximumSurfaceRings)
            {
                while (_activeRingCount > _maximumSurfaceRings)
                {
                    RemoveRingSlotAt(FindOldestRingSlotIndex());
                }

                RebuildRenderData();
            }

            if (previousFoamCapacity != _maximumContactFoams)
            {
                ClearContactFoamSlots();
            }
            else if (previousRingCapacity != _maximumSurfaceRings && _activeRingCount > 0)
            {
                RebuildRenderData();
            }

            if (previousWakeCapacity != _maximumWakeSegments || previousWakeBodyCapacity != _maximumWakeBodies)
            {
                ClearWakeSlotsAndBodies();
            }
        }

        private void AddWakeSegment(
            int bodyKey,
            Vector2 startLocalXZ,
            Vector2 endLocalXZ,
            float halfWidth,
            float intensity)
        {
            var slotIndex = FindWakeSlotForEmission();
            if (slotIndex < 0)
            {
                return;
            }

            var replacingLiveSlot = _wakeSlots[slotIndex].HasSlot;
            _wakeSlots[slotIndex] = new WakeSlot
            {
                BodyKey = bodyKey,
                StartLocalXZ = startLocalXZ,
                EndLocalXZ = endLocalXZ,
                HalfWidth = halfWidth,
                Age = 0f,
                Lifetime = _wakeLifetime,
                Intensity = Mathf.Clamp01(intensity),
                NoisePhase = ComputeWakePhase(bodyKey, _nextWakeCreationSequence + 1),
                HasSlot = true,
                CreationSequence = ++_nextWakeCreationSequence
            };

            if (replacingLiveSlot)
            {
                _replacedWakeCount++;
            }
            else
            {
                _activeWakeCount++;
            }
        }

        private int FindWakeSlotForEmission()
        {
            for (var i = 0; i < _maximumWakeSegments; i++)
            {
                var slot = _wakeSlots[i];
                if (!slot.HasSlot)
                {
                    return i;
                }

                if (slot.Age < slot.Lifetime)
                {
                    continue;
                }

                _wakeSlots[i] = default;
                _activeWakeCount--;
                return i;
            }

            var fadingIndex = -1;
            var fadingIntensity = float.MaxValue;
            var fadingSequence = int.MaxValue;
            for (var i = 0; i < _maximumWakeSegments; i++)
            {
                var slot = _wakeSlots[i];
                var age01 = slot.Age / Mathf.Max(0.01f, slot.Lifetime);
                if (age01 < 0.5f)
                {
                    continue;
                }

                if (slot.Intensity < fadingIntensity - 0.00001f ||
                    (Mathf.Approximately(slot.Intensity, fadingIntensity) && slot.CreationSequence < fadingSequence))
                {
                    fadingIndex = i;
                    fadingIntensity = slot.Intensity;
                    fadingSequence = slot.CreationSequence;
                }
            }

            if (fadingIndex >= 0)
            {
                return fadingIndex;
            }

            var oldestIndex = 0;
            var oldestSequence = _wakeSlots[0].CreationSequence;
            for (var i = 1; i < _maximumWakeSegments; i++)
            {
                if (_wakeSlots[i].CreationSequence < oldestSequence)
                {
                    oldestIndex = i;
                    oldestSequence = _wakeSlots[i].CreationSequence;
                }
            }

            return oldestIndex;
        }

        private int FindWakeBodyState(int bodyKey)
        {
            for (var i = 0; i < _maximumWakeBodies; i++)
            {
                if (_wakeBodyStates[i].HasState && _wakeBodyStates[i].BodyKey == bodyKey)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindFreeWakeBodyState()
        {
            for (var i = 0; i < _maximumWakeBodies; i++)
            {
                if (!_wakeBodyStates[i].HasState)
                {
                    return i;
                }
            }

            return -1;
        }

        private void RemoveWakeBodyStateAt(int index)
        {
            if (index < 0 || index >= _maximumWakeBodies || !_wakeBodyStates[index].HasState)
            {
                return;
            }

            _wakeBodyStates[index] = default;
            _wakeBodyStateCount = Mathf.Max(0, _wakeBodyStateCount - 1);
        }

        private void ClearWakeSlotsAndBodies()
        {
            for (var i = 0; i < _wakeSlots.Length; i++)
            {
                _wakeSlots[i] = default;
            }

            for (var i = 0; i < _wakeBodyStates.Length; i++)
            {
                _wakeBodyStates[i] = default;
            }

            _activeWakeCount = 0;
            _wakeBodyStateCount = 0;
            _nextWakeCreationSequence = 0;
            RebuildRenderData();
        }

        private float SanitizeWakeHalfWidth(float aggregateHalfWidth)
        {
            var safeHalfWidth = Mathf.Abs(aggregateHalfWidth) * _wakeWidthMultiplier + _wakeWidthPadding;
            if (!IsFinite(safeHalfWidth))
            {
                safeHalfWidth = _wakeMinimumHalfWidth;
            }

            return Mathf.Clamp(safeHalfWidth, _wakeMinimumHalfWidth, _wakeMaximumHalfWidth);
        }

        private static float ComputeWakePhase(int bodyKey, int creationSequence)
        {
            var hash = unchecked((uint)bodyKey);
            hash ^= unchecked((uint)creationSequence) * 0x9e3779b9u;
            hash ^= hash >> 16;
            hash *= 0x7feb352du;
            hash ^= hash >> 15;
            hash *= 0x846ca68bu;
            hash ^= hash >> 16;
            return (hash & 0x00ffffffu) / 16777215f;
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

                RemoveRingSlotAt(i);
                removed = true;
            }

            if (removed)
            {
                RebuildRenderData();
            }
        }

        private int FindOldestRingSlotIndex()
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

        private void RemoveRingSlotAt(int index)
        {
            var lastIndex = _activeRingCount - 1;
            if (index != lastIndex)
            {
                _ringSlots[index] = _ringSlots[lastIndex];
            }

            _ringSlots[lastIndex] = default;
            _activeRingCount = lastIndex;
        }

        private int FindContactFoamSlot(int bodyKey)
        {
            for (var i = 0; i < _maximumContactFoams; i++)
            {
                if (_contactFoamSlots[i].HasSlot && _contactFoamSlots[i].BodyKey == bodyKey)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindReclaimableContactFoamSlot()
        {
            var oldestFadingIndex = -1;
            var oldestFadingSequence = int.MaxValue;
            for (var i = 0; i < _maximumContactFoams; i++)
            {
                var slot = _contactFoamSlots[i];
                if (!slot.HasSlot)
                {
                    return i;
                }

                if (!slot.Active && !slot.Fading || slot.Fading && slot.FadeAmount <= 0f)
                {
                    return i;
                }

                if (slot.Fading && slot.CreationSequence < oldestFadingSequence)
                {
                    oldestFadingIndex = i;
                    oldestFadingSequence = slot.CreationSequence;
                }
            }

            return oldestFadingIndex;
        }

        private void ClearContactFoamSlots()
        {
            for (var i = 0; i < _contactFoamSlots.Length; i++)
            {
                _contactFoamSlots[i] = default;
            }

            _activeContactFoamCount = 0;
            _fadingContactFoamCount = 0;
            RebuildRenderData();
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

            for (var i = 0; i < ShaderMaximumContactFoams; i++)
            {
                _foamsA[i] = Vector4.zero;
                _foamsB[i] = Vector4.zero;
            }

            for (var i = 0; i < ShaderMaximumWakeSegments; i++)
            {
                _wakesA[i] = Vector4.zero;
                _wakesB[i] = Vector4.zero;
            }

            _activeContactFoamCount = 0;
            _fadingContactFoamCount = 0;
            var renderFoamCount = 0;
            for (var i = 0; i < _maximumContactFoams; i++)
            {
                var slot = _contactFoamSlots[i];
                if (!slot.HasSlot)
                {
                    continue;
                }

                if (slot.Active)
                {
                    _activeContactFoamCount++;
                }
                else if (slot.Fading)
                {
                    _fadingContactFoamCount++;
                }

                _foamsA[renderFoamCount] = new Vector4(
                    slot.CenterLocalXZ.x,
                    slot.CenterLocalXZ.y,
                    slot.HalfWidth,
                    slot.Intensity);
                _foamsB[renderFoamCount] = new Vector4(
                    slot.HalfDepth,
                    slot.FadeAmount,
                    slot.Submersion,
                    slot.NoisePhase);
                renderFoamCount++;
            }

            var renderWakeCount = 0;
            for (var i = 0; i < _maximumWakeSegments; i++)
            {
                var slot = _wakeSlots[i];
                if (!slot.HasSlot || slot.Age >= slot.Lifetime)
                {
                    continue;
                }

                _wakesA[renderWakeCount] = new Vector4(
                    slot.StartLocalXZ.x,
                    slot.StartLocalXZ.y,
                    slot.EndLocalXZ.x,
                    slot.EndLocalXZ.y);
                _wakesB[renderWakeCount] = new Vector4(
                    slot.HalfWidth,
                    Mathf.Clamp01(slot.Age / Mathf.Max(0.01f, slot.Lifetime)),
                    slot.Intensity,
                    slot.NoisePhase);
                renderWakeCount++;
            }

            _renderData.ActiveRingCount = _activeRingCount;
            _renderData.ActiveContactFoamCount = renderFoamCount;
            _renderData.FadingContactFoamCount = _fadingContactFoamCount;
            _renderData.ActiveWakeCount = renderWakeCount;
        }

        private static float ComputeNoisePhase(int bodyKey)
        {
            var hash = unchecked((uint)bodyKey);
            hash ^= hash >> 16;
            hash *= 0x7feb352du;
            hash ^= hash >> 15;
            hash *= 0x846ca68bu;
            hash ^= hash >> 16;
            return (hash & 0x00ffffffu) / 16777215f;
        }

        private static bool Approximately(Vector2 first, Vector2 second)
        {
            return Mathf.Approximately(first.x, second.x) && Mathf.Approximately(first.y, second.y);
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
