using UnityEngine;

namespace Water25D
{
    /// <summary>
    /// Production-first ripple backend. It keeps a rectangular, no-mipmap, two-channel state texture
    /// and schedules bounded work so visual resolution does not determine gameplay physics.
    /// </summary>
    public sealed class CustomRenderTextureRippleSimulator : IWaterRippleSimulator
    {
        private static readonly int SpreadXId = Shader.PropertyToID("_SpreadX");
        private static readonly int SpreadZId = Shader.PropertyToID("_SpreadZ");
        private static readonly int DampingId = Shader.PropertyToID("_Damping");
        private static readonly int ImpactHeightId = Shader.PropertyToID("_ImpactHeight");
        private static readonly int ImpactCenterId = Shader.PropertyToID("_ImpactCenter");
        private static readonly int ImpactRadiusId = Shader.PropertyToID("_ImpactRadius");

        private readonly WaterRuntimeResources _resources;
        private readonly WaterQualitySettings _settings;
        private readonly Vector2 _waterSize;
        private readonly WaterRippleImpact[] _impactQueue;
        private readonly CustomRenderTextureUpdateZone[] _updateZones = new CustomRenderTextureUpdateZone[1];
        private readonly CustomRenderTexture _texture;
        private readonly Material _material;

        private int _queueHead;
        private int _queueCount;
        private int _droppedImpactCount;
        private float _timeAccumulator;
        private float _idleTime;
        private bool _disposed;

        /// <summary>
        /// Diagnostic counters used by benchmarks and EditMode scheduling tests. They count
        /// logical CRT updates, not GPU milliseconds.
        /// </summary>
        public int ImpactInjectionUpdateCount { get; private set; }
        public int FullSurfacePropagationUpdateCount { get; private set; }

        public bool IsAvailable => !_disposed && _texture != null && _material != null;
        public Texture HeightTexture => IsAvailable ? _texture : null;
        public int DroppedImpactCount => _droppedImpactCount;
        public bool IsSuspended => IsAvailable && _idleTime >= _settings.IdleTimeout && _queueCount == 0;

        public CustomRenderTextureRippleSimulator(
            WaterRuntimeResources resources,
            Vector2 waterSize,
            WaterQualitySettings settings,
            Material materialTemplate)
        {
            _resources = resources;
            _waterSize = new Vector2(Mathf.Max(0.01f, waterSize.x), Mathf.Max(0.01f, waterSize.y));
            _settings = settings;
            _settings.Sanitize();
            _impactQueue = new WaterRippleImpact[Mathf.Max(_settings.MaximumQueuedImpacts, _settings.MaximumImpactsPerStep)];

            var resolution = _settings.CalculateRippleResolution(_waterSize);
            CustomRenderTexture createdTexture;
            Material createdMaterial;
            _resources.TryCreateRippleResources(
                resolution.x,
                resolution.y,
                materialTemplate,
                Shader.Find("Water25D/Ripple Simulation"),
                out createdTexture,
                out createdMaterial);
            _texture = createdTexture;
            _material = createdMaterial;

            if (_texture != null)
            {
                _texture.name = "Water25D Ripple Simulation (Runtime)";
            }
        }

        public void EnqueueImpact(WaterRippleImpact impact)
        {
            if (!IsAvailable || _impactQueue.Length == 0)
            {
                return;
            }

            impact.CenterUV = new Vector2(Mathf.Clamp01(impact.CenterUV.x), Mathf.Clamp01(impact.CenterUV.y));
            impact.Strength = Mathf.Clamp(Mathf.Abs(impact.Strength), 0f, 1f);
            impact.Radius = Mathf.Max(0.005f, impact.Radius);
            if (impact.Strength <= 0f)
            {
                return;
            }

            if (_queueCount >= _impactQueue.Length)
            {
                _droppedImpactCount++;
                return;
            }

            var tail = (_queueHead + _queueCount) % _impactQueue.Length;
            _impactQueue[tail] = impact;
            _queueCount++;
            _idleTime = 0f;
        }

        public void Tick(float deltaTime, bool isVisible)
        {
            if (!IsAvailable)
            {
                return;
            }

            var safeDeltaTime = Mathf.Clamp(deltaTime, 0f, 0.25f);
            if (_queueCount > 0)
            {
                _idleTime = 0f;
            }
            else if (!isVisible)
            {
                _idleTime += safeDeltaTime;
                if (_idleTime >= _settings.IdleTimeout)
                {
                    _timeAccumulator = 0f;
                    return;
                }
            }
            else
            {
                _idleTime = 0f;
            }

            _timeAccumulator += safeDeltaTime;
            var step = 1f / Mathf.Max(1f, _settings.SimulationFrequency);
            var updates = 0;
            while (_timeAccumulator >= step && updates < _settings.MaximumCatchUpSubsteps)
            {
                SimulateStep(step);
                _timeAccumulator -= step;
                updates++;
            }

            // A stalled frame must not cause unbounded work on the next frame.
            if (updates >= _settings.MaximumCatchUpSubsteps && _timeAccumulator > step)
            {
                _timeAccumulator = step;
            }
        }

        public void ResetSimulation()
        {
            if (!IsAvailable)
            {
                return;
            }

            _texture.Initialize();
            _queueHead = 0;
            _queueCount = 0;
            _droppedImpactCount = 0;
            _timeAccumulator = 0f;
            _idleTime = 0f;
            ImpactInjectionUpdateCount = 0;
            FullSurfacePropagationUpdateCount = 0;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _resources.ReleaseRippleResources();
            _queueHead = 0;
            _queueCount = 0;
        }

        private void SimulateStep(float deltaTime)
        {
            var cellSizeX = _waterSize.x / Mathf.Max(1, _texture.width);
            var cellSizeZ = _waterSize.y / Mathf.Max(1, _texture.height);
            var propagationSubsteps = Mathf.Max(1, _settings.PropagationSubsteps);
            var substepDuration = deltaTime / propagationSubsteps;
            var normalizedSpeedX = _settings.WaveSpeed * substepDuration / Mathf.Max(0.0001f, cellSizeX);
            var normalizedSpeedZ = _settings.WaveSpeed * substepDuration / Mathf.Max(0.0001f, cellSizeZ);
            var rawSpreadX = normalizedSpeedX * normalizedSpeedX;
            var rawSpreadZ = normalizedSpeedZ * normalizedSpeedZ;
            var stabilityLimit = 0.45f;
            var spreadScale = rawSpreadX + rawSpreadZ > stabilityLimit
                ? stabilityLimit / Mathf.Max(0.0001f, rawSpreadX + rawSpreadZ)
                : 1f;
            var spreadX = rawSpreadX * spreadScale;
            var spreadZ = rawSpreadZ * spreadScale;
            var damping = Mathf.Exp(-_settings.DampingPerSecond * substepDuration);

            _material.SetFloat(SpreadXId, spreadX);
            _material.SetFloat(SpreadZId, spreadZ);
            _material.SetFloat(DampingId, damping);

            var impactsThisStep = Mathf.Min(_queueCount, _settings.MaximumImpactsPerStep);
            for (var i = 0; i < impactsThisStep; i++)
            {
                ApplyImpact(DequeueImpact());
            }

            // Impact zones are injected independently, so each impact only touches its
            // bounded area. The full-surface propagation is performed once after all
            // pending impacts have been injected.
            ApplyPropagation();
        }

        private void ApplyPropagation()
        {
            _updateZones[0] = new CustomRenderTextureUpdateZone
            {
                needSwap = true,
                passIndex = 0,
                rotation = 0f,
                updateZoneCenter = new Vector3(0.5f, 0.5f, 0f),
                updateZoneSize = new Vector3(1f, 1f, 0f)
            };
            _texture.ClearUpdateZones();
            _texture.SetUpdateZones(_updateZones);
            _texture.shaderPass = 0;
            _texture.Update(_settings.PropagationSubsteps);
            FullSurfacePropagationUpdateCount++;
        }

        private void ApplyImpact(WaterRippleImpact impact)
        {
            var radiusU = Mathf.Clamp(impact.Radius / _waterSize.x, 1f / _texture.width, 0.5f);
            var radiusV = Mathf.Clamp(impact.Radius / _waterSize.y, 1f / _texture.height, 0.5f);
            _material.SetFloat(ImpactHeightId, Mathf.Abs(impact.SignedStrength));
            _material.SetVector(ImpactCenterId, new Vector4(impact.CenterUV.x, impact.CenterUV.y, 0f, 0f));
            _material.SetVector(ImpactRadiusId, new Vector4(radiusU, radiusV, 0f, 0f));

            _updateZones[0] = new CustomRenderTextureUpdateZone
            {
                needSwap = true,
                passIndex = impact.InitialUp ? 1 : 2,
                rotation = 0f,
                updateZoneCenter = new Vector3(impact.CenterUV.x, impact.CenterUV.y, 0f),
                updateZoneSize = new Vector3(radiusU * 2f, radiusV * 2f, 0f)
            };
            _texture.ClearUpdateZones();
            _texture.SetUpdateZones(_updateZones);
            _texture.shaderPass = 0;
            _texture.Update(1);
            ImpactInjectionUpdateCount++;
        }

        private WaterRippleImpact DequeueImpact()
        {
            var impact = _impactQueue[_queueHead];
            _queueHead = (_queueHead + 1) % _impactQueue.Length;
            _queueCount--;
            return impact;
        }
    }
}
