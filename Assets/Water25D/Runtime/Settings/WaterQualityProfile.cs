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
        public int MaximumContactFoams;
        public int MaximumTrackedSurfaceBodies;
        public int MaximumWakeSegments;
        public int MaximumWakeEmissionsPerStep;
        public bool EnablePainterlyInteractionMasks;
        public bool EnablePainterlyAgeFrames;
        public bool EnableSecondaryAmbientDetail;
        public bool EnableStylizedHighlights;
        public bool EnableRefraction;
        public bool EnableCaustics;

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
            MaximumSurfaceRings = 8,
            MaximumContactFoams = 4,
            MaximumTrackedSurfaceBodies = 8,
            MaximumWakeSegments = 8,
            MaximumWakeEmissionsPerStep = 2,
            EnablePainterlyInteractionMasks = true,
            EnablePainterlyAgeFrames = true,
            EnableSecondaryAmbientDetail = true,
            EnableStylizedHighlights = true,
            EnableRefraction = false,
            EnableCaustics = false
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
            MaximumContactFoams = Mathf.Clamp(MaximumContactFoams, 1, 8);
            MaximumTrackedSurfaceBodies = Mathf.Clamp(MaximumTrackedSurfaceBodies, 1, 16);
            MaximumWakeSegments = Mathf.Clamp(MaximumWakeSegments, 1, 16);
            MaximumWakeEmissionsPerStep = Mathf.Clamp(MaximumWakeEmissionsPerStep, 1, 16);
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
                   MaximumSurfaceRings == other.MaximumSurfaceRings &&
                   MaximumContactFoams == other.MaximumContactFoams &&
                   MaximumTrackedSurfaceBodies == other.MaximumTrackedSurfaceBodies &&
                    MaximumWakeSegments == other.MaximumWakeSegments &&
                    MaximumWakeEmissionsPerStep == other.MaximumWakeEmissionsPerStep &&
                    EnablePainterlyInteractionMasks == other.EnablePainterlyInteractionMasks &&
                    EnablePainterlyAgeFrames == other.EnablePainterlyAgeFrames &&
                    EnableSecondaryAmbientDetail == other.EnableSecondaryAmbientDetail &&
                    EnableStylizedHighlights == other.EnableStylizedHighlights &&
                    EnableRefraction == other.EnableRefraction &&
                    EnableCaustics == other.EnableCaustics;
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
                hash = hash * 31 + MaximumSurfaceRings;
                hash = hash * 31 + MaximumContactFoams;
                hash = hash * 31 + MaximumTrackedSurfaceBodies;
                hash = hash * 31 + MaximumWakeSegments;
                hash = hash * 31 + MaximumWakeEmissionsPerStep;
                hash = hash * 31 + EnablePainterlyInteractionMasks.GetHashCode();
                hash = hash * 31 + EnablePainterlyAgeFrames.GetHashCode();
                hash = hash * 31 + EnableSecondaryAmbientDetail.GetHashCode();
                hash = hash * 31 + EnableStylizedHighlights.GetHashCode();
                hash = hash * 31 + EnableRefraction.GetHashCode();
                return hash * 31 + EnableCaustics.GetHashCode();
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

        [Header("Flat-Stylized Interaction")]
        [Range(1, 8)] [SerializeField] private int _maximumContactFoams = 4;
        [Range(1, 16)] [SerializeField] private int _maximumTrackedSurfaceBodies = 8;
        [Range(1, 16)] [SerializeField] private int _maximumWakeSegments = 8;
        [Range(1, 16)] [SerializeField] private int _maximumWakeEmissionsPerStep = 2;

        [Header("Painterly Interaction")]
        [SerializeField] private bool _enablePainterlyInteractionMasks = true;
        [SerializeField] private bool _enablePainterlyAgeFrames = true;

        [Header("Phase 3 Presentation")]
        [SerializeField] private bool _enableSecondaryAmbientDetail = true;
        [SerializeField] private bool _enableStylizedHighlights = true;
        [Tooltip("Requires the style profile's valid opaque-texture source flag.")]
        [SerializeField] private bool _enableRefraction;
        [Tooltip("Requires a caustic texture in the style profile.")]
        [SerializeField] private bool _enableCaustics;

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
                    : WaterQualitySettings.Default.MaximumSurfaceRings,
                MaximumContactFoams = _maximumContactFoams > 0
                    ? _maximumContactFoams
                    : WaterQualitySettings.Default.MaximumContactFoams,
                MaximumTrackedSurfaceBodies = _maximumTrackedSurfaceBodies > 0
                    ? _maximumTrackedSurfaceBodies
                    : WaterQualitySettings.Default.MaximumTrackedSurfaceBodies,
                MaximumWakeSegments = _maximumWakeSegments > 0
                    ? _maximumWakeSegments
                    : WaterQualitySettings.Default.MaximumWakeSegments,
                MaximumWakeEmissionsPerStep = _maximumWakeEmissionsPerStep > 0
                    ? _maximumWakeEmissionsPerStep
                    : WaterQualitySettings.Default.MaximumWakeEmissionsPerStep,
                EnablePainterlyInteractionMasks = _enablePainterlyInteractionMasks,
                EnablePainterlyAgeFrames = _enablePainterlyAgeFrames,
                EnableSecondaryAmbientDetail = _enableSecondaryAmbientDetail,
                EnableStylizedHighlights = _enableStylizedHighlights,
                EnableRefraction = _enableRefraction,
                EnableCaustics = _enableCaustics
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
            _maximumContactFoams = settings.MaximumContactFoams;
            _maximumTrackedSurfaceBodies = settings.MaximumTrackedSurfaceBodies;
            _maximumWakeSegments = settings.MaximumWakeSegments;
            _maximumWakeEmissionsPerStep = settings.MaximumWakeEmissionsPerStep;
            _enablePainterlyInteractionMasks = settings.EnablePainterlyInteractionMasks;
            _enablePainterlyAgeFrames = settings.EnablePainterlyAgeFrames;
            _enableSecondaryAmbientDetail = settings.EnableSecondaryAmbientDetail;
            _enableStylizedHighlights = settings.EnableStylizedHighlights;
            _enableRefraction = settings.EnableRefraction;
            _enableCaustics = settings.EnableCaustics;
        }
    }
}
