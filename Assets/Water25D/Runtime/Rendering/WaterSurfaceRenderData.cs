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
        private readonly Vector4[] _ringsC;
        private readonly Vector4[] _foamsA;
        private readonly Vector4[] _foamsB;
        private readonly Vector4[] _foamsC;
        private readonly Vector4[] _wakesA;
        private readonly Vector4[] _wakesB;
        private readonly Vector4[] _wakesC;

        internal WaterSurfaceRenderData(
            Vector4[] ringsA,
            Vector4[] ringsB,
            Vector4[] ringsC,
            Vector4[] foamsA,
            Vector4[] foamsB,
            Vector4[] foamsC,
            Vector4[] wakesA,
            Vector4[] wakesB,
            Vector4[] wakesC)
        {
            _ringsA = ringsA;
            _ringsB = ringsB;
            _ringsC = ringsC;
            _foamsA = foamsA;
            _foamsB = foamsB;
            _foamsC = foamsC;
            _wakesA = wakesA;
            _wakesB = wakesB;
            _wakesC = wakesC;
        }

        public int ActiveRingCount { get; internal set; }
        public int ActiveContactFoamCount { get; internal set; }
        public int FadingContactFoamCount { get; internal set; }
        public int ActiveWakeCount { get; internal set; }

        public int ShaderArrayLength => _ringsA.Length;
        public int FoamShaderArrayLength => _foamsA.Length;
        public int WakeShaderArrayLength => _wakesA.Length;

        public Vector4 GetRingA(int index)
        {
            return _ringsA[index];
        }

        public Vector4 GetRingB(int index)
        {
            return _ringsB[index];
        }

        public Vector4 GetRingC(int index)
        {
            return _ringsC[index];
        }

        public Vector4 GetFoamA(int index)
        {
            return _foamsA[index];
        }

        public Vector4 GetFoamB(int index)
        {
            return _foamsB[index];
        }

        public Vector4 GetFoamC(int index)
        {
            return _foamsC[index];
        }

        public Vector4 GetWakeA(int index)
        {
            return _wakesA[index];
        }

        public Vector4 GetWakeB(int index)
        {
            return _wakesB[index];
        }

        public Vector4 GetWakeC(int index)
        {
            return _wakesC[index];
        }

        internal Vector4[] RingsA => _ringsA;
        internal Vector4[] RingsB => _ringsB;
        internal Vector4[] RingsC => _ringsC;
        internal Vector4[] FoamsA => _foamsA;
        internal Vector4[] FoamsB => _foamsB;
        internal Vector4[] FoamsC => _foamsC;
        internal Vector4[] WakesA => _wakesA;
        internal Vector4[] WakesB => _wakesB;
        internal Vector4[] WakesC => _wakesC;
    }
}
