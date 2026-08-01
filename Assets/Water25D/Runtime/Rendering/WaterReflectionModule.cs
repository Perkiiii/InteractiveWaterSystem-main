using UnityEngine;

namespace Water25D.Rendering
{
    /// <summary>
    /// Owns a controller's registration with the shared reflection manager.
    /// The manager remains responsible for compatible-plane grouping and camera resources.
    /// </summary>
    internal sealed class WaterReflectionModule : System.IDisposable
    {
        private WaterReflectionManager.ReflectionRegistration _registration;

        public void Configure(
            MeshRenderer topRenderer,
            Transform reflectionAnchor,
            Camera reflectionCameraSource,
            WaterReflectionMode reflectionMode,
            LayerMask reflectionCullingMask,
            float reflectionResolutionScale,
            int reflectionUpdateIntervalFrames,
            float reflectionStrength)
        {
            DisposeRegistration();
            if (!Application.isPlaying || reflectionMode == WaterReflectionMode.Disabled || topRenderer == null || reflectionAnchor == null)
            {
                return;
            }

            _registration = WaterReflectionManager.Register(
                topRenderer,
                reflectionAnchor,
                reflectionCameraSource,
                reflectionMode,
                reflectionCullingMask,
                reflectionResolutionScale,
                reflectionUpdateIntervalFrames,
                reflectionStrength);
        }

        public void Dispose()
        {
            DisposeRegistration();
        }

        private void DisposeRegistration()
        {
            if (_registration == null)
            {
                return;
            }

            _registration.Dispose();
            _registration = null;
        }
    }
}
