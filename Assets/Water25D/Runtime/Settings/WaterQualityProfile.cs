using System;
using UnityEngine;

namespace Water25D
{
    [Serializable]
    public struct WaterQualitySettings : IEquatable<WaterQualitySettings>
    {
        public float RippleTexelsPerUnit;
        public Vector2Int MinimumRippleResolution;
        public Vector2Int MaximumRippleResolution;
        public float SimulationFrequency;
        public int PropagationSubsteps;
        public int MaximumCatchUpSubsteps;
        public int MaximumImpactsPerStep;
        public int MaximumQueuedImpacts;
        public float DampingPerSecond;
        public float WaveSpeed;
        public float ImpactRadius;
        public float IdleTimeout;
        public float TopVerticesPerUnit;
        public int AmbientWaveBands;
        public int MaximumSurfaceRings;

        public static WaterQualitySettings Default => new WaterQualitySettings
        {
            RippleTexelsPerUnit = 16f,
            MinimumRippleResolution = new Vector2Int(64, 32),
            MaximumRippleResolution = new Vector2Int(512, 192),
            SimulationFrequency = 30f,
            PropagationSubsteps = 2,
            MaximumCatchUpSubsteps = 2,
            MaximumImpactsPerStep = 32,
            MaximumQueuedImpacts = 128,
            DampingPerSecond = 0.65f,
            WaveSpeed = 5f,
            ImpactRadius = 0.22f,
            IdleTimeout = 2f,
            TopVerticesPerUnit = 8f,
            AmbientWaveBands = 3,
            MaximumSurfaceRings = 8
        };

        public void Sanitize()
        {
            RippleTexelsPerUnit = Mathf.Clamp(RippleTexelsPerUnit, 1f, 128f);
            MinimumRippleResolution.x = Mathf.Clamp(MinimumRippleResolution.x, 2, 4096);
            MinimumRippleResolution.y = Mathf.Clamp(MinimumRippleResolution.y, 2, 4096);
            MaximumRippleResolution.x = Mathf.Clamp(MaximumRippleResolution.x, MinimumRippleResolution.x, 4096);
            MaximumRippleResolution.y = Mathf.Clamp(MaximumRippleResolution.y, MinimumRippleResolution.y, 4096);
            SimulationFrequency = Mathf.Clamp(SimulationFrequency, 1f, 120f);
            PropagationSubsteps = Mathf.Clamp(PropagationSubsteps, 1, 8);
            MaximumCatchUpSubsteps = Mathf.Clamp(MaximumCatchUpSubsteps, 1, 8);
            MaximumImpactsPerStep = Mathf.Clamp(MaximumImpactsPerStep, 1, 128);
            MaximumQueuedImpacts = Mathf.Clamp(MaximumQueuedImpacts, MaximumImpactsPerStep, 1024);
            DampingPerSecond = Mathf.Clamp(DampingPerSecond, 0f, 20f);
            WaveSpeed = Mathf.Clamp(WaveSpeed, 0.01f, 50f);
            ImpactRadius = Mathf.Clamp(ImpactRadius, 0.01f, 10f);
            IdleTimeout = Mathf.Max(0f, IdleTimeout);
            TopVerticesPerUnit = Mathf.Clamp(TopVerticesPerUnit, 0.5f, 64f);
            AmbientWaveBands = Mathf.Clamp(AmbientWaveBands, 1, 4);
            MaximumSurfaceRings = Mathf.Clamp(MaximumSurfaceRings, 1, 16);
        }

        public Vector2Int CalculateRippleResolution(Vector2 topSurfaceSize)
        {
            var settings = this;
            settings.Sanitize();
            var width = Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(0.01f, topSurfaceSize.x) * settings.RippleTexelsPerUnit), settings.MinimumRippleResolution.x, settings.MaximumRippleResolution.x);
            var height = Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(0.01f, topSurfaceSize.y) * settings.RippleTexelsPerUnit), settings.MinimumRippleResolution.y, settings.MaximumRippleResolution.y);
            return new Vector2Int(width, height);
        }

        public bool SimulationEquals(WaterQualitySettings other)
        {
            return Mathf.Approximately(RippleTexelsPerUnit, other.RippleTexelsPerUnit) &&
                   MinimumRippleResolution == other.MinimumRippleResolution &&
                   MaximumRippleResolution == other.MaximumRippleResolution &&
                   Mathf.Approximately(SimulationFrequency, other.SimulationFrequency) &&
                   PropagationSubsteps == other.PropagationSubsteps &&
                   MaximumCatchUpSubsteps == other.MaximumCatchUpSubsteps &&
                   MaximumImpactsPerStep == other.MaximumImpactsPerStep &&
                   MaximumQueuedImpacts == other.MaximumQueuedImpacts &&
                   Mathf.Approximately(DampingPerSecond, other.DampingPerSecond) &&
                   Mathf.Approximately(WaveSpeed, other.WaveSpeed) &&
                   Mathf.Approximately(ImpactRadius, other.ImpactRadius) &&
                   Mathf.Approximately(IdleTimeout, other.IdleTimeout) &&
                   AmbientWaveBands == other.AmbientWaveBands;
        }

        public bool Equals(WaterQualitySettings other)
        {
            return SimulationEquals(other) &&
                   Mathf.Approximately(TopVerticesPerUnit, other.TopVerticesPerUnit) &&
                   MaximumSurfaceRings == other.MaximumSurfaceRings;
        }

        public override bool Equals(object obj)
        {
            return obj is WaterQualitySettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = RippleTexelsPerUnit.GetHashCode();
                hash = hash * 31 + MinimumRippleResolution.GetHashCode();
                hash = hash * 31 + MaximumRippleResolution.GetHashCode();
                hash = hash * 31 + SimulationFrequency.GetHashCode();
                hash = hash * 31 + PropagationSubsteps;
                hash = hash * 31 + MaximumCatchUpSubsteps;
                hash = hash * 31 + MaximumImpactsPerStep;
                hash = hash * 31 + MaximumQueuedImpacts;
                hash = hash * 31 + DampingPerSecond.GetHashCode();
                hash = hash * 31 + WaveSpeed.GetHashCode();
                hash = hash * 31 + ImpactRadius.GetHashCode();
                hash = hash * 31 + IdleTimeout.GetHashCode();
                hash = hash * 31 + TopVerticesPerUnit.GetHashCode();
                hash = hash * 31 + AmbientWaveBands;
                return hash * 31 + MaximumSurfaceRings;
            }
        }
    }

    [CreateAssetMenu(fileName = "WaterQualityProfile", menuName = "Water 2.5D/Quality Profile")]
    public sealed class WaterQualityProfile : ScriptableObject
    {
        [Header("Ripple State")]
        [Min(1f)] [SerializeField] private float _rippleTexelsPerUnit = 16f;
        [SerializeField] private Vector2Int _minimumRippleResolution = new Vector2Int(64, 32);
        [SerializeField] private Vector2Int _maximumRippleResolution = new Vector2Int(512, 192);

        [Header("Simulation Scheduling")]
        [Min(1f)] [SerializeField] private float _simulationFrequency = 30f;
        [Range(1, 8)] [SerializeField] private int _propagationSubsteps = 2;
        [Range(1, 8)] [SerializeField] private int _maximumCatchUpSubsteps = 2;
        [Range(1, 128)] [SerializeField] private int _maximumImpactsPerStep = 32;
        [Range(1, 1024)] [SerializeField] private int _maximumQueuedImpacts = 128;

        [Header("Wave Behaviour")]
        [Min(0f)] [SerializeField] private float _dampingPerSecond = 0.65f;
        [Min(0.01f)] [SerializeField] private float _waveSpeed = 5f;
        [Min(0.01f)] [SerializeField] private float _impactRadius = 0.22f;
        [Min(0f)] [SerializeField] private float _idleTimeout = 2f;
        [Range(1, 4)] [SerializeField] private int _ambientWaveBands = 3;

        [Header("Procedural Surface Rings")]
        [Range(1, 16)] [SerializeField] private int _maximumSurfaceRings = 8;

        [Header("Geometry")]
        [Min(0.5f)] [SerializeField] private float _topVerticesPerUnit = 8f;

        public WaterQualitySettings GetSettings()
        {
            var settings = new WaterQualitySettings
            {
                RippleTexelsPerUnit = _rippleTexelsPerUnit,
                MinimumRippleResolution = _minimumRippleResolution,
                MaximumRippleResolution = _maximumRippleResolution,
                SimulationFrequency = _simulationFrequency,
                PropagationSubsteps = _propagationSubsteps,
                MaximumCatchUpSubsteps = _maximumCatchUpSubsteps,
                MaximumImpactsPerStep = _maximumImpactsPerStep,
                MaximumQueuedImpacts = _maximumQueuedImpacts,
                DampingPerSecond = _dampingPerSecond,
                WaveSpeed = _waveSpeed,
                ImpactRadius = _impactRadius,
                IdleTimeout = _idleTimeout,
                TopVerticesPerUnit = _topVerticesPerUnit,
                AmbientWaveBands = _ambientWaveBands,
                MaximumSurfaceRings = _maximumSurfaceRings > 0
                    ? _maximumSurfaceRings
                    : WaterQualitySettings.Default.MaximumSurfaceRings
            };
            settings.Sanitize();
            return settings;
        }

        private void OnValidate()
        {
            var settings = GetSettings();
            _rippleTexelsPerUnit = settings.RippleTexelsPerUnit;
            _minimumRippleResolution = settings.MinimumRippleResolution;
            _maximumRippleResolution = settings.MaximumRippleResolution;
            _simulationFrequency = settings.SimulationFrequency;
            _propagationSubsteps = settings.PropagationSubsteps;
            _maximumCatchUpSubsteps = settings.MaximumCatchUpSubsteps;
            _maximumImpactsPerStep = settings.MaximumImpactsPerStep;
            _maximumQueuedImpacts = settings.MaximumQueuedImpacts;
            _dampingPerSecond = settings.DampingPerSecond;
            _waveSpeed = settings.WaveSpeed;
            _impactRadius = settings.ImpactRadius;
            _idleTimeout = settings.IdleTimeout;
            _topVerticesPerUnit = settings.TopVerticesPerUnit;
            _ambientWaveBands = settings.AmbientWaveBands;
            _maximumSurfaceRings = settings.MaximumSurfaceRings;
        }
    }
}
