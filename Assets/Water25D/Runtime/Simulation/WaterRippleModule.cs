using UnityEngine;

namespace Water25D
{
    /// <summary>
    /// Owns one water body's ripple simulator instance and its rebuild contract.
    /// Gameplay only sees queued impacts; it never reads GPU state for physics.
    /// </summary>
    internal sealed class WaterRippleModule : System.IDisposable
    {
        private IWaterRippleSimulator _simulator;
        private WaterQualitySettings _appliedQualitySettings;
        private Vector2 _appliedWaterSize;
        private Material _appliedMaterialTemplate;
        private bool _hasLoggedMissingShader;

        public bool IsAvailable => _simulator != null && _simulator.IsAvailable;
        public bool IsSuspended => _simulator != null && _simulator.IsSuspended;
        public Texture HeightTexture => _simulator != null ? _simulator.HeightTexture : null;
        public int DroppedImpactCount => _simulator != null ? _simulator.DroppedImpactCount : 0;

        public void Ensure(
            WaterRuntimeResources resources,
            Vector2 waterSize,
            WaterQualitySettings qualitySettings,
            Material materialTemplate,
            Object context)
        {
            var needsNewSimulator = _simulator == null ||
                                    !_simulator.IsAvailable ||
                                    _appliedWaterSize != waterSize ||
                                    !_appliedQualitySettings.SimulationEquals(qualitySettings) ||
                                    _appliedMaterialTemplate != materialTemplate;
            if (!needsNewSimulator)
            {
                return;
            }

            DisposeSimulator();
            var fallbackShader = Shader.Find("Water25D/Ripple Simulation");
            if (materialTemplate == null && fallbackShader == null)
            {
                if (!_hasLoggedMissingShader)
                {
                    Debug.LogWarning("Water25D ripple simulation could not find its package shader. Assign a ripple material template or reimport the package shader.", context);
                    _hasLoggedMissingShader = true;
                }

                return;
            }

            _simulator = new CustomRenderTextureRippleSimulator(resources, waterSize, qualitySettings, materialTemplate);
            _appliedMaterialTemplate = materialTemplate;
            _appliedWaterSize = waterSize;
            _appliedQualitySettings = qualitySettings;
        }

        public void EnqueueImpact(WaterRippleImpact impact)
        {
            _simulator?.EnqueueImpact(impact);
        }

        public void Tick(float deltaTime, bool isVisible)
        {
            _simulator?.Tick(deltaTime, isVisible);
        }

        public void Dispose()
        {
            DisposeSimulator();
        }

        private void DisposeSimulator()
        {
            if (_simulator == null)
            {
                return;
            }

            _simulator.Dispose();
            _simulator = null;
        }
    }
}
