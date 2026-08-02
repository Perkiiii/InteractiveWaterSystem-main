using UnityEngine;

namespace Water25D
{
    internal static class WaterShaderIds
    {
        public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public static readonly int WaterColor = Shader.PropertyToID("_WaterColor");
        public static readonly int FrontSurfaceColor = Shader.PropertyToID("_FrontSurfaceColor");
        public static readonly int FrontDeepColor = Shader.PropertyToID("_FrontDeepColor");
        public static readonly int SurfaceColor = Shader.PropertyToID("_SurfaceColor");
        public static readonly int MainColor = Shader.PropertyToID("_MainColor");
        public static readonly int FoamColor = Shader.PropertyToID("_FoamColor");
        public static readonly int WaterSize = Shader.PropertyToID("_WaterSize");
        public static readonly int WaterMeshDepth = Shader.PropertyToID("_WaterMeshDepth");
        public static readonly int FrontDepth = Shader.PropertyToID("_FrontDepth");
        public static readonly int Waterline = Shader.PropertyToID("_Waterline");
        public static readonly int RippleTexture = Shader.PropertyToID("_RippleTexture");
        public static readonly int RippleSimulationTexture = Shader.PropertyToID("_RippleSimulationTexture");
        public static readonly int RippleEnabled = Shader.PropertyToID("_RippleEnabled");
        public static readonly int RippleAmplitude = Shader.PropertyToID("_RippleAmplitude");
        public static readonly int SurfaceMode = Shader.PropertyToID("_SurfaceMode");
        public static readonly int RippleScale = Shader.PropertyToID("_RippleScale");
        public static readonly int RippleHeightOffset = Shader.PropertyToID("_RippleHeightOffset");
        public static readonly int SurfaceRingCount = Shader.PropertyToID("_WaterRingCount");
        public static readonly int SurfaceRingsA = Shader.PropertyToID("_WaterRingsA");
        public static readonly int SurfaceRingsB = Shader.PropertyToID("_WaterRingsB");
        public static readonly int SurfaceRingsC = Shader.PropertyToID("_WaterRingsC");
        public static readonly int SurfaceFoamCount = Shader.PropertyToID("_WaterFoamCount");
        public static readonly int SurfaceFoamsA = Shader.PropertyToID("_WaterFoamsA");
        public static readonly int SurfaceFoamsB = Shader.PropertyToID("_WaterFoamsB");
        public static readonly int SurfaceFoamsC = Shader.PropertyToID("_WaterFoamsC");
        public static readonly int SurfaceFoamSoftness = Shader.PropertyToID("_WaterFoamSoftness");
        public static readonly int FoamReflectionOcclusion = Shader.PropertyToID("_FoamReflectionOcclusion");
        public static readonly int SurfaceWakeCount = Shader.PropertyToID("_WaterWakeCount");
        public static readonly int SurfaceWakesA = Shader.PropertyToID("_WaterWakesA");
        public static readonly int SurfaceWakesB = Shader.PropertyToID("_WaterWakesB");
        public static readonly int SurfaceWakesC = Shader.PropertyToID("_WaterWakesC");
        public static readonly int WakeFadePower = Shader.PropertyToID("_WakeFadePower");
        public static readonly int WaveAmplitude = Shader.PropertyToID("_WaveAmplitude");
        public static readonly int WaveLength = Shader.PropertyToID("_WaveLength");
        public static readonly int WaveSpeed = Shader.PropertyToID("_WaveSpeed");
        public static readonly int WaveDirection = Shader.PropertyToID("_WaveDirection");
        public static readonly int WaveBands = Shader.PropertyToID("_WaveBands");
        public static readonly int WaveScale = Shader.PropertyToID("_WaveScale");
        public static readonly int ReflectionTexture = Shader.PropertyToID("_ReflectionTexture");
        public static readonly int ReflectionViewProjection = Shader.PropertyToID("_ReflectionViewProjection");
        public static readonly int ReflectionEnabled = Shader.PropertyToID("_ReflectionEnabled");
        public static readonly int ReflectionFallback = Shader.PropertyToID("_ReflectionFallback");
        public static readonly int ReflectionStrength = Shader.PropertyToID("_ReflectionStrength");
        public static readonly int PainterlyMasksEnabled = Shader.PropertyToID("_PainterlyMasksEnabled");
        public static readonly int PainterlyAgeFrames = Shader.PropertyToID("_PainterlyAgeFrames");
        public static readonly int RingMaskAtlas = Shader.PropertyToID("_RingMaskAtlas");
        public static readonly int RingMaskAtlasValid = Shader.PropertyToID("_RingMaskAtlasValid");
        public static readonly int RingMaskAtlasGrid = Shader.PropertyToID("_RingMaskAtlasGrid");
        public static readonly int RingMaskVariantCount = Shader.PropertyToID("_RingMaskVariantCount");
        public static readonly int RingMaskFrameCount = Shader.PropertyToID("_RingMaskFrameCount");
        public static readonly int RingMaskInfluence = Shader.PropertyToID("_RingMaskInfluence");
        public static readonly int FoamMaskAtlas = Shader.PropertyToID("_FoamMaskAtlas");
        public static readonly int FoamMaskAtlasValid = Shader.PropertyToID("_FoamMaskAtlasValid");
        public static readonly int FoamMaskAtlasGrid = Shader.PropertyToID("_FoamMaskAtlasGrid");
        public static readonly int FoamMaskVariantCount = Shader.PropertyToID("_FoamMaskVariantCount");
        public static readonly int FoamMaskFrameCount = Shader.PropertyToID("_FoamMaskFrameCount");
        public static readonly int FoamMaskInfluence = Shader.PropertyToID("_FoamMaskInfluence");
        public static readonly int WakeMaskAtlas = Shader.PropertyToID("_WakeMaskAtlas");
        public static readonly int WakeMaskAtlasValid = Shader.PropertyToID("_WakeMaskAtlasValid");
        public static readonly int WakeMaskAtlasGrid = Shader.PropertyToID("_WakeMaskAtlasGrid");
        public static readonly int WakeMaskVariantCount = Shader.PropertyToID("_WakeMaskVariantCount");
        public static readonly int WakeMaskFrameCount = Shader.PropertyToID("_WakeMaskFrameCount");
        public static readonly int WakeMaskInfluence = Shader.PropertyToID("_WakeMaskInfluence");
    }
}
