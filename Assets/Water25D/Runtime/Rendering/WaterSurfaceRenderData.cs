using UnityEngine;

namespace Water25D.Rendering
{
    /// <summary>
    /// Prepared fixed-size surface presentation data. The backing arrays are owned by the
    /// presentation module and are exposed to the runtime rendering assembly only, while
    /// public access is limited to individual read-only values for diagnostics and tests.
    /// </summary>
    public sealed class WaterSurfaceRenderData
    {
        private readonly Vector4[] _ringsA;
        private readonly Vector4[] _ringsB;

        internal WaterSurfaceRenderData(Vector4[] ringsA, Vector4[] ringsB)
        {
            _ringsA = ringsA;
            _ringsB = ringsB;
        }

        public int ActiveRingCount { get; internal set; }

        public int ShaderArrayLength => _ringsA.Length;

        public Vector4 GetRingA(int index)
        {
            return _ringsA[index];
        }

        public Vector4 GetRingB(int index)
        {
            return _ringsB[index];
        }

        internal Vector4[] RingsA => _ringsA;
        internal Vector4[] RingsB => _ringsB;
    }
}
