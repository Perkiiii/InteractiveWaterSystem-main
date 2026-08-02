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
        private WaterReflectionRenderState _fallbackState = WaterReflectionRenderState.ForMode(
            WaterReflectionMode.Stylized,
            0.35f);

        public WaterReflectionRenderState LatestState =>
            _registration != null ? _registration.State : _fallbackState;

        public int StateVersion =>
            _registration != null ? _registration.StateVersion : 0;

        public void Configure(
            MeshRenderer topRenderer,
            Transform reflectionAnchor,
            Camera reflectionCameraSource,
            WaterReflectionMode reflectionMode,
            LayerMask reflectionCullingMask,
            LayerMask reflectionExclusionMask,
            float reflectionResolutionScale,
            int reflectionUpdateIntervalFrames,
            float reflectionStrength)
        {
            DisposeRegistration();
            _fallbackState = WaterReflectionRenderState.ForMode(reflectionMode, reflectionStrength);
            if (!Application.isPlaying ||
                reflectionMode != WaterReflectionMode.Planar ||
                topRenderer == null ||
                reflectionAnchor == null)
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
                reflectionStrength,
                reflectionExclusionMask);
        }

        public void Dispose()
        {
            DisposeRegistration();
            _fallbackState = WaterReflectionRenderState.Disabled;
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
