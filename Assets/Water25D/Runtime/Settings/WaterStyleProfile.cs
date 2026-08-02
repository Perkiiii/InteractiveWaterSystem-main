using System;
using UnityEngine;

namespace Water25D
{
    /// <summary>
    /// Fixed-grid, package-owned configuration for one optional interaction-mask atlas.
    /// Gameplay remains authoritative for the interaction bounds; this data only controls
    /// painterly breakup and deterministic presentation variation.
    /// </summary>
    [Serializable]
    public struct WaterPainterlyMaskSettings : IEquatable<WaterPainterlyMaskSettings>
    {
        public Texture2D Atlas;
        public Vector2Int Grid;
        public int VariantCount;
        public int FrameCount;
        public float Influence;
        public float RotationVariation;

        public static WaterPainterlyMaskSettings Default => new WaterPainterlyMaskSettings
        {
            Atlas = null,
            Grid = Vector2Int.one,
            VariantCount = 1,
            FrameCount = 1,
            Influence = 1f,
            RotationVariation = 1f
        };

        public void Sanitize()
        {
            var defaults = Default;
            Grid.x = Mathf.Clamp(Grid.x, 1, 16);
            Grid.y = Mathf.Clamp(Grid.y, 1, 16);
            var cellCount = Mathf.Max(1, Grid.x * Grid.y);
            VariantCount = Mathf.Clamp(VariantCount > 0 ? VariantCount : defaults.VariantCount, 1, cellCount);
            var maximumFrames = Mathf.Max(1, cellCount / VariantCount);
            FrameCount = Mathf.Clamp(FrameCount > 0 ? FrameCount : defaults.FrameCount, 1, maximumFrames);
            Influence = IsFinite(Influence) ? Mathf.Clamp01(Influence) : defaults.Influence;
            RotationVariation = IsFinite(RotationVariation) ? Mathf.Clamp01(RotationVariation) : defaults.RotationVariation;
        }

        public bool Equals(WaterPainterlyMaskSettings other)
        {
            return Atlas == other.Atlas &&
                   Grid == other.Grid &&
                   VariantCount == other.VariantCount &&
                   FrameCount == other.FrameCount &&
                   Mathf.Approximately(Influence, other.Influence) &&
                   Mathf.Approximately(RotationVariation, other.RotationVariation);
        }

        public override bool Equals(object obj)
        {
            return obj is WaterPainterlyMaskSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Atlas != null ? Atlas.GetHashCode() : 0;
                hash = hash * 31 + Grid.GetHashCode();
                hash = hash * 31 + VariantCount;
                hash = hash * 31 + FrameCount;
                hash = hash * 31 + Influence.GetHashCode();
                return hash * 31 + RotationVariation.GetHashCode();
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    [Serializable]
    public struct WaterStyleSettings : IEquatable<WaterStyleSettings>
    {
        public Color TopColor;
        public Color ShallowColor;
        public Color DeepColor;
        public Color FrontSurfaceColor;
        public Color FrontDeepColor;
        public Color FoamColor;
        public float TopDepthPower;
        public float TopOpacity;
        public float ColorBandSteps;
        public float ColorBandInfluence;
        public float AmbientWaveAmplitude;
        public float AmbientWaveLength;
        public float AmbientWaveSpeed;
        public Vector2 AmbientWaveDirection;
        public Texture2D SurfaceNormalTexture;
        public Texture2D SurfaceDetailTexture;
        public Vector2 NormalLayer1Scale;
        public Vector2 NormalLayer1Speed;
        public float NormalLayer1Strength;
        public Vector2 NormalLayer2Scale;
        public Vector2 NormalLayer2Speed;
        public float NormalLayer2Strength;
        public float AmbientNormalStrength;
        public Color FresnelTint;
        public float FresnelStrength;
        public float FresnelPower;
        public Color HighlightColor;
        public float HighlightStrength;
        public float HighlightThreshold;
        public float HighlightSoftness;
        public float HighlightBreakup;
        public Vector3 HighlightDirection;
        public Color StylizedReflectionTint;
        public Color StylizedReflectionHorizonColor;
        public Color StylizedReflectionTopColor;
        public float StylizedReflectionStrength;
        public Color PlanarReflectionTint;
        public float PlanarReflectionStrength;
        public float AmbientReflectionDistortion;
        public float RingNormalStrength;
        public float RingReflectionDistortion;
        public float WakeNormalStrength;
        public float WakeReflectionDistortion;
        public float BoundaryFoamWidth;
        public float BoundaryFoamSoftness;
        public float BoundaryFoamBreakup;
        public float BoundaryFoamIntensity;
        public bool RefractionSourceAvailable;
        public Color RefractionTint;
        public float RefractionStrength;
        public bool FrontDistortionSourceAvailable;
        public Color FrontDistortionTint;
        public float FrontDistortionStrength;
        public float FrontDepthPower;
        public float FrontOpacity;
        public float WaterlineBandWidth;
        public Texture2D CausticTexture;
        public Vector2 CausticScale;
        public Vector2 CausticSpeed;
        public Color CausticTint;
        public float CausticIntensity;
        public float CausticDepthFade;
        public float RippleAmplitude;
        public float RingLifetime;
        public float RingExpansionMultiplier;
        public float RingThickness;
        public float RingSoftness;
        public float RingIntensity;
        public float ContactFoamWidthPadding;
        public float ContactFoamHalfDepth;
        public float ContactFoamSoftness;
        public float ContactFoamIntensity;
        public float ContactFoamFadeDuration;
        public float FoamReflectionOcclusion;
        public float WakeEmissionSpacing;
        public float WakeMinimumLateralSpeed;
        public float WakeWidthMultiplier;
        public float WakeWidthPadding;
        public float WakeMinimumHalfWidth;
        public float WakeMaximumHalfWidth;
        public float WakeLifetime;
        public float WakeFadePower;
        public float WakeIntensity;
        public float WakeDirectionReversalAngle;
        public WaterPainterlyMaskSettings RingMask;
        public WaterPainterlyMaskSettings ContactFoamMask;
        public WaterPainterlyMaskSettings WakeMask;

        public static WaterStyleSettings Default => new WaterStyleSettings
        {
            TopColor = new Color(0.20f, 0.48f, 0.60f, 0.92f),
            ShallowColor = new Color(0.20f, 0.48f, 0.60f, 0.92f),
            DeepColor = new Color(0.04f, 0.22f, 0.36f, 0.94f),
            FrontSurfaceColor = new Color(0.08f, 0.38f, 0.52f, 0.84f),
            FrontDeepColor = new Color(0.025f, 0.10f, 0.18f, 0.94f),
            FoamColor = new Color(0.78f, 0.95f, 1f, 0.8f),
            TopDepthPower = 0.9f,
            TopOpacity = 0.92f,
            ColorBandSteps = 4f,
            ColorBandInfluence = 0.15f,
            AmbientWaveAmplitude = 0.06f,
            AmbientWaveLength = 3.5f,
            AmbientWaveSpeed = 0.8f,
            AmbientWaveDirection = new Vector2(1f, 0.15f),
            NormalLayer1Scale = new Vector2(0.55f, 0.45f),
            NormalLayer1Speed = new Vector2(0.035f, -0.021f),
            NormalLayer1Strength = 0.65f,
            NormalLayer2Scale = new Vector2(1.15f, 0.9f),
            NormalLayer2Speed = new Vector2(-0.017f, 0.029f),
            NormalLayer2Strength = 0.25f,
            AmbientNormalStrength = 0.10f,
            FresnelTint = new Color(0.75f, 0.95f, 1f, 1f),
            FresnelStrength = 0.30f,
            FresnelPower = 4f,
            HighlightColor = new Color(0.88f, 0.98f, 1f, 1f),
            HighlightStrength = 0.18f,
            HighlightThreshold = 0.65f,
            HighlightSoftness = 0.20f,
            HighlightBreakup = 0.25f,
            HighlightDirection = new Vector3(-0.3f, 0.85f, -0.25f),
            StylizedReflectionTint = Color.white,
            StylizedReflectionHorizonColor = new Color(0.22f, 0.52f, 0.68f, 1f),
            StylizedReflectionTopColor = new Color(0.48f, 0.78f, 0.88f, 1f),
            StylizedReflectionStrength = 0.30f,
            PlanarReflectionTint = Color.white,
            PlanarReflectionStrength = 0.35f,
            AmbientReflectionDistortion = 0.0025f,
            RingNormalStrength = 0.18f,
            RingReflectionDistortion = 0.008f,
            WakeNormalStrength = 0.12f,
            WakeReflectionDistortion = 0.006f,
            BoundaryFoamWidth = 0.025f,
            BoundaryFoamSoftness = 0.04f,
            BoundaryFoamBreakup = 0.25f,
            BoundaryFoamIntensity = 0.45f,
            RefractionSourceAvailable = false,
            RefractionTint = Color.white,
            RefractionStrength = 0.003f,
            FrontDistortionSourceAvailable = false,
            FrontDistortionTint = new Color(0.80f, 0.95f, 1f, 1f),
            FrontDistortionStrength = 0.003f,
            FrontDepthPower = 1.15f,
            FrontOpacity = 0.90f,
            WaterlineBandWidth = 0.07f,
            CausticScale = new Vector2(0.16f, 0.16f),
            CausticSpeed = new Vector2(0.018f, -0.012f),
            CausticTint = new Color(0.78f, 1f, 0.82f, 1f),
            CausticIntensity = 0.20f,
            CausticDepthFade = 0.70f,
            RippleAmplitude = 0.18f,
            RingLifetime = 1.25f,
            RingExpansionMultiplier = 6f,
            RingThickness = 0.05f,
            RingSoftness = 0.04f,
            RingIntensity = 0.75f,
            ContactFoamWidthPadding = 0.08f,
            ContactFoamHalfDepth = 0.16f,
            ContactFoamSoftness = 0.06f,
            ContactFoamIntensity = 0.85f,
            ContactFoamFadeDuration = 0.35f,
            FoamReflectionOcclusion = 0.85f,
            WakeEmissionSpacing = 0.75f,
            WakeMinimumLateralSpeed = 0.5f,
            WakeWidthMultiplier = 0.9f,
            WakeWidthPadding = 0.02f,
            WakeMinimumHalfWidth = 0.05f,
            WakeMaximumHalfWidth = 0.35f,
            WakeLifetime = 1.35f,
            WakeFadePower = 1.25f,
            WakeIntensity = 0.45f,
            WakeDirectionReversalAngle = 120f,
            RingMask = WaterPainterlyMaskSettings.Default,
            ContactFoamMask = WaterPainterlyMaskSettings.Default,
            WakeMask = WaterPainterlyMaskSettings.Default
        };

        public void Sanitize()
        {
            var defaults = Default;
            TopColor = SanitizeColor(TopColor, defaults.TopColor);
            ShallowColor = IsClear(ShallowColor) ? TopColor : SanitizeColor(ShallowColor, TopColor);
            DeepColor = IsClear(DeepColor) ? defaults.DeepColor : SanitizeColor(DeepColor, defaults.DeepColor);
            FrontSurfaceColor = SanitizeColor(FrontSurfaceColor, defaults.FrontSurfaceColor);
            FrontDeepColor = SanitizeColor(FrontDeepColor, defaults.FrontDeepColor);
            FoamColor = SanitizeColor(FoamColor, defaults.FoamColor);
            TopDepthPower = SanitizePositive(TopDepthPower, defaults.TopDepthPower, 0.05f, 8f);
            TopOpacity = SanitizePositive(TopOpacity, defaults.TopOpacity, 0.01f, 1f);
            ColorBandSteps = SanitizePositive(ColorBandSteps, defaults.ColorBandSteps, 1f, 12f);
            ColorBandInfluence = IsFinite(ColorBandInfluence) ? Mathf.Clamp01(ColorBandInfluence) : defaults.ColorBandInfluence;
            AmbientWaveAmplitude = Mathf.Max(0f, AmbientWaveAmplitude);
            AmbientWaveLength = Mathf.Max(0.01f, AmbientWaveLength);
            AmbientWaveSpeed = Mathf.Max(0f, AmbientWaveSpeed);
            NormalLayer1Scale = SanitizeVector2(NormalLayer1Scale, defaults.NormalLayer1Scale, 0.01f, 20f);
            NormalLayer1Speed = SanitizeVector2(NormalLayer1Speed, defaults.NormalLayer1Speed, -10f, 10f);
            NormalLayer1Strength = SanitizeNonNegative(NormalLayer1Strength, defaults.NormalLayer1Strength, 0f, 2f);
            NormalLayer2Scale = SanitizeVector2(NormalLayer2Scale, defaults.NormalLayer2Scale, 0.01f, 20f);
            NormalLayer2Speed = SanitizeVector2(NormalLayer2Speed, defaults.NormalLayer2Speed, -10f, 10f);
            NormalLayer2Strength = SanitizeNonNegative(NormalLayer2Strength, defaults.NormalLayer2Strength, 0f, 2f);
            AmbientNormalStrength = SanitizeNonNegative(AmbientNormalStrength, defaults.AmbientNormalStrength, 0f, 1f);
            FresnelTint = SanitizeColor(FresnelTint, defaults.FresnelTint);
            FresnelStrength = SanitizeNonNegative(FresnelStrength, defaults.FresnelStrength, 0f, 1f);
            FresnelPower = SanitizePositive(FresnelPower, defaults.FresnelPower, 0.1f, 16f);
            HighlightColor = SanitizeColor(HighlightColor, defaults.HighlightColor);
            HighlightStrength = SanitizeNonNegative(HighlightStrength, defaults.HighlightStrength, 0f, 1f);
            HighlightThreshold = IsFinite(HighlightThreshold) ? Mathf.Clamp01(HighlightThreshold) : defaults.HighlightThreshold;
            HighlightSoftness = SanitizePositive(HighlightSoftness, defaults.HighlightSoftness, 0.01f, 1f);
            HighlightBreakup = IsFinite(HighlightBreakup) ? Mathf.Clamp01(HighlightBreakup) : defaults.HighlightBreakup;
            HighlightDirection = SanitizeVector3(HighlightDirection, defaults.HighlightDirection, -10f, 10f);
            if (HighlightDirection.sqrMagnitude < 0.0001f)
            {
                HighlightDirection = defaults.HighlightDirection;
            }
            else
            {
                HighlightDirection.Normalize();
            }
            StylizedReflectionTint = SanitizeColor(StylizedReflectionTint, defaults.StylizedReflectionTint);
            StylizedReflectionHorizonColor = SanitizeColor(StylizedReflectionHorizonColor, defaults.StylizedReflectionHorizonColor);
            StylizedReflectionTopColor = SanitizeColor(StylizedReflectionTopColor, defaults.StylizedReflectionTopColor);
            StylizedReflectionStrength = SanitizeNonNegative(StylizedReflectionStrength, defaults.StylizedReflectionStrength, 0f, 1f);
            PlanarReflectionTint = SanitizeColor(PlanarReflectionTint, defaults.PlanarReflectionTint);
            PlanarReflectionStrength = SanitizeNonNegative(PlanarReflectionStrength, defaults.PlanarReflectionStrength, 0f, 1f);
            AmbientReflectionDistortion = SanitizeNonNegative(AmbientReflectionDistortion, defaults.AmbientReflectionDistortion, 0f, 0.05f);
            RingNormalStrength = SanitizeNonNegative(RingNormalStrength, defaults.RingNormalStrength, 0f, 1f);
            RingReflectionDistortion = SanitizeNonNegative(RingReflectionDistortion, defaults.RingReflectionDistortion, 0f, 0.05f);
            WakeNormalStrength = SanitizeNonNegative(WakeNormalStrength, defaults.WakeNormalStrength, 0f, 1f);
            WakeReflectionDistortion = SanitizeNonNegative(WakeReflectionDistortion, defaults.WakeReflectionDistortion, 0f, 0.05f);
            BoundaryFoamWidth = SanitizePositive(BoundaryFoamWidth, defaults.BoundaryFoamWidth, 0.0001f, 0.5f);
            BoundaryFoamSoftness = SanitizeNonNegative(BoundaryFoamSoftness, defaults.BoundaryFoamSoftness, 0f, 0.5f);
            BoundaryFoamBreakup = IsFinite(BoundaryFoamBreakup) ? Mathf.Clamp01(BoundaryFoamBreakup) : defaults.BoundaryFoamBreakup;
            BoundaryFoamIntensity = IsFinite(BoundaryFoamIntensity) ? Mathf.Clamp01(BoundaryFoamIntensity) : defaults.BoundaryFoamIntensity;
            RefractionTint = SanitizeColor(RefractionTint, defaults.RefractionTint);
            RefractionStrength = SanitizeNonNegative(RefractionStrength, defaults.RefractionStrength, 0f, 0.02f);
            FrontDistortionTint = SanitizeColor(FrontDistortionTint, defaults.FrontDistortionTint);
            FrontDistortionStrength = SanitizeNonNegative(FrontDistortionStrength, defaults.FrontDistortionStrength, 0f, 0.01f);
            FrontDepthPower = SanitizePositive(FrontDepthPower, defaults.FrontDepthPower, 0.05f, 8f);
            FrontOpacity = SanitizePositive(FrontOpacity, defaults.FrontOpacity, 0.01f, 1f);
            WaterlineBandWidth = SanitizePositive(WaterlineBandWidth, defaults.WaterlineBandWidth, 0.001f, 0.5f);
            CausticScale = SanitizeVector2(CausticScale, defaults.CausticScale, 0.001f, 20f);
            CausticSpeed = SanitizeVector2(CausticSpeed, defaults.CausticSpeed, -10f, 10f);
            CausticTint = SanitizeColor(CausticTint, defaults.CausticTint);
            CausticIntensity = IsFinite(CausticIntensity) ? Mathf.Clamp01(CausticIntensity) : defaults.CausticIntensity;
            CausticDepthFade = IsFinite(CausticDepthFade) ? Mathf.Clamp01(CausticDepthFade) : defaults.CausticDepthFade;
            RippleAmplitude = Mathf.Max(0f, RippleAmplitude);
            RingLifetime = SanitizePositive(RingLifetime, Default.RingLifetime, 0.01f, 60f);
            RingExpansionMultiplier = SanitizePositive(RingExpansionMultiplier, Default.RingExpansionMultiplier, 1f, 100f);
            RingThickness = SanitizePositive(RingThickness, Default.RingThickness, 0.001f, 10f);
            RingSoftness = SanitizeNonNegative(RingSoftness, Default.RingSoftness, 0f, 10f);
            RingIntensity = IsFinite(RingIntensity) ? Mathf.Clamp01(RingIntensity) : Default.RingIntensity;
            ContactFoamWidthPadding = SanitizeNonNegative(ContactFoamWidthPadding, Default.ContactFoamWidthPadding, 0f, 2f);
            ContactFoamHalfDepth = SanitizePositive(ContactFoamHalfDepth, Default.ContactFoamHalfDepth, 0.01f, 2f);
            ContactFoamSoftness = SanitizeNonNegative(ContactFoamSoftness, Default.ContactFoamSoftness, 0f, 1f);
            ContactFoamIntensity = IsFinite(ContactFoamIntensity) ? Mathf.Clamp01(ContactFoamIntensity) : Default.ContactFoamIntensity;
            ContactFoamFadeDuration = SanitizePositive(ContactFoamFadeDuration, Default.ContactFoamFadeDuration, 0.01f, 5f);
            FoamReflectionOcclusion = IsFinite(FoamReflectionOcclusion) ? Mathf.Clamp01(FoamReflectionOcclusion) : Default.FoamReflectionOcclusion;
            WakeEmissionSpacing = SanitizePositive(WakeEmissionSpacing, Default.WakeEmissionSpacing, 0.01f, 10f);
            WakeMinimumLateralSpeed = SanitizeNonNegative(WakeMinimumLateralSpeed, Default.WakeMinimumLateralSpeed, 0f, 100f);
            WakeWidthMultiplier = SanitizeNonNegative(WakeWidthMultiplier, Default.WakeWidthMultiplier, 0f, 4f);
            WakeWidthPadding = SanitizeNonNegative(WakeWidthPadding, Default.WakeWidthPadding, 0f, 2f);
            WakeMinimumHalfWidth = SanitizePositive(WakeMinimumHalfWidth, Default.WakeMinimumHalfWidth, 0.001f, 2f);
            WakeMaximumHalfWidth = SanitizePositive(WakeMaximumHalfWidth, Default.WakeMaximumHalfWidth, WakeMinimumHalfWidth, 4f);
            WakeLifetime = SanitizePositive(WakeLifetime, Default.WakeLifetime, 0.01f, 60f);
            WakeFadePower = SanitizePositive(WakeFadePower, Default.WakeFadePower, 0.1f, 4f);
            WakeIntensity = IsFinite(WakeIntensity) ? Mathf.Clamp01(WakeIntensity) : Default.WakeIntensity;
            WakeDirectionReversalAngle = IsFinite(WakeDirectionReversalAngle)
                ? Mathf.Clamp(WakeDirectionReversalAngle, 90f, 179f)
                : Default.WakeDirectionReversalAngle;
            RingMask.Sanitize();
            ContactFoamMask.Sanitize();
            WakeMask.Sanitize();
            if (AmbientWaveDirection.sqrMagnitude < 0.0001f)
            {
                AmbientWaveDirection = Vector2.right;
            }
            else
            {
                AmbientWaveDirection.Normalize();
            }
        }

        public void Apply(MaterialPropertyBlock block)
        {
            if (block == null)
            {
                return;
            }

            block.SetColor(WaterShaderIds.ShallowColor, ShallowColor);
            block.SetColor(WaterShaderIds.DeepColor, DeepColor);
            block.SetColor(WaterShaderIds.BaseColor, TopColor);
            block.SetColor(WaterShaderIds.WaterColor, TopColor);
            block.SetColor(WaterShaderIds.FrontSurfaceColor, FrontSurfaceColor);
            block.SetColor(WaterShaderIds.SurfaceColor, FrontSurfaceColor);
            block.SetColor(WaterShaderIds.MainColor, FrontDeepColor);
            block.SetColor(WaterShaderIds.FrontDeepColor, FrontDeepColor);
            block.SetColor(WaterShaderIds.FoamColor, FoamColor);
            block.SetFloat(WaterShaderIds.TopDepthPower, TopDepthPower);
            block.SetFloat(WaterShaderIds.TopOpacity, TopOpacity);
            block.SetFloat(WaterShaderIds.ColorBandSteps, ColorBandSteps);
            block.SetFloat(WaterShaderIds.ColorBandInfluence, ColorBandInfluence);
            block.SetFloat(WaterShaderIds.WaveAmplitude, AmbientWaveAmplitude);
            block.SetFloat(WaterShaderIds.WaveLength, AmbientWaveLength);
            block.SetFloat(WaterShaderIds.WaveSpeed, AmbientWaveSpeed);
            block.SetVector(WaterShaderIds.WaveDirection, new Vector4(AmbientWaveDirection.x, AmbientWaveDirection.y, 0f, 0f));
            if (SurfaceNormalTexture != null)
            {
                block.SetTexture(WaterShaderIds.SurfaceNormalTexture, SurfaceNormalTexture);
            }
            block.SetFloat(WaterShaderIds.SurfaceNormalTextureValid, SurfaceNormalTexture != null ? 1f : 0f);
            if (SurfaceDetailTexture != null)
            {
                block.SetTexture(WaterShaderIds.SurfaceDetailTexture, SurfaceDetailTexture);
            }
            block.SetFloat(WaterShaderIds.SurfaceDetailTextureValid, SurfaceDetailTexture != null ? 1f : 0f);
            block.SetVector(WaterShaderIds.NormalLayer1Scale, new Vector4(NormalLayer1Scale.x, NormalLayer1Scale.y, 0f, 0f));
            block.SetVector(WaterShaderIds.NormalLayer1Speed, new Vector4(NormalLayer1Speed.x, NormalLayer1Speed.y, 0f, 0f));
            block.SetFloat(WaterShaderIds.NormalLayer1Strength, NormalLayer1Strength);
            block.SetVector(WaterShaderIds.NormalLayer2Scale, new Vector4(NormalLayer2Scale.x, NormalLayer2Scale.y, 0f, 0f));
            block.SetVector(WaterShaderIds.NormalLayer2Speed, new Vector4(NormalLayer2Speed.x, NormalLayer2Speed.y, 0f, 0f));
            block.SetFloat(WaterShaderIds.NormalLayer2Strength, NormalLayer2Strength);
            block.SetFloat(WaterShaderIds.AmbientNormalStrength, AmbientNormalStrength);
            block.SetColor(WaterShaderIds.FresnelTint, FresnelTint);
            block.SetFloat(WaterShaderIds.FresnelStrength, FresnelStrength);
            block.SetFloat(WaterShaderIds.FresnelPower, FresnelPower);
            block.SetColor(WaterShaderIds.HighlightColor, HighlightColor);
            block.SetFloat(WaterShaderIds.HighlightStrength, HighlightStrength);
            block.SetFloat(WaterShaderIds.HighlightThreshold, HighlightThreshold);
            block.SetFloat(WaterShaderIds.HighlightSoftness, HighlightSoftness);
            block.SetFloat(WaterShaderIds.HighlightBreakup, HighlightBreakup);
            block.SetVector(WaterShaderIds.HighlightDirection, new Vector4(HighlightDirection.x, HighlightDirection.y, HighlightDirection.z, 0f));
            block.SetColor(WaterShaderIds.StylizedReflectionTint, StylizedReflectionTint);
            block.SetColor(WaterShaderIds.StylizedReflectionHorizonColor, StylizedReflectionHorizonColor);
            block.SetColor(WaterShaderIds.StylizedReflectionTopColor, StylizedReflectionTopColor);
            block.SetFloat(WaterShaderIds.StylizedReflectionStrength, StylizedReflectionStrength);
            block.SetColor(WaterShaderIds.PlanarReflectionTint, PlanarReflectionTint);
            block.SetFloat(WaterShaderIds.PlanarReflectionStrength, PlanarReflectionStrength);
            block.SetFloat(WaterShaderIds.AmbientReflectionDistortion, AmbientReflectionDistortion);
            block.SetFloat(WaterShaderIds.RingNormalStrength, RingNormalStrength);
            block.SetFloat(WaterShaderIds.RingReflectionDistortion, RingReflectionDistortion);
            block.SetFloat(WaterShaderIds.WakeNormalStrength, WakeNormalStrength);
            block.SetFloat(WaterShaderIds.WakeReflectionDistortion, WakeReflectionDistortion);
            block.SetFloat(WaterShaderIds.BoundaryFoamWidth, BoundaryFoamWidth);
            block.SetFloat(WaterShaderIds.BoundaryFoamSoftness, BoundaryFoamSoftness);
            block.SetFloat(WaterShaderIds.BoundaryFoamBreakup, BoundaryFoamBreakup);
            block.SetFloat(WaterShaderIds.BoundaryFoamIntensity, BoundaryFoamIntensity);
            block.SetFloat(WaterShaderIds.RefractionSourceAvailable, RefractionSourceAvailable ? 1f : 0f);
            block.SetColor(WaterShaderIds.RefractionTint, RefractionTint);
            block.SetFloat(WaterShaderIds.RefractionStrength, RefractionStrength);
            block.SetFloat(WaterShaderIds.FrontDistortionSourceAvailable, FrontDistortionSourceAvailable ? 1f : 0f);
            block.SetColor(WaterShaderIds.FrontDistortionTint, FrontDistortionTint);
            block.SetFloat(WaterShaderIds.FrontDistortionStrength, FrontDistortionStrength);
            block.SetFloat(WaterShaderIds.FrontDepthPower, FrontDepthPower);
            block.SetFloat(WaterShaderIds.FrontOpacity, FrontOpacity);
            block.SetFloat(WaterShaderIds.WaterlineBandWidth, WaterlineBandWidth);
            if (CausticTexture != null)
            {
                block.SetTexture(WaterShaderIds.CausticTexture, CausticTexture);
            }
            block.SetFloat(WaterShaderIds.CausticTextureValid, CausticTexture != null ? 1f : 0f);
            block.SetVector(WaterShaderIds.CausticScale, new Vector4(CausticScale.x, CausticScale.y, 0f, 0f));
            block.SetVector(WaterShaderIds.CausticSpeed, new Vector4(CausticSpeed.x, CausticSpeed.y, 0f, 0f));
            block.SetColor(WaterShaderIds.CausticTint, CausticTint);
            block.SetFloat(WaterShaderIds.CausticIntensity, CausticIntensity);
            block.SetFloat(WaterShaderIds.CausticDepthFade, CausticDepthFade);
            block.SetFloat(WaterShaderIds.WaveScale, 1f);
            block.SetFloat(WaterShaderIds.RippleAmplitude, RippleAmplitude);
            block.SetFloat(WaterShaderIds.RippleScale, 1f);
            block.SetFloat(WaterShaderIds.RippleHeightOffset, 0f);
            block.SetFloat(WaterShaderIds.SurfaceFoamSoftness, ContactFoamSoftness);
            block.SetFloat(WaterShaderIds.FoamReflectionOcclusion, FoamReflectionOcclusion);
            block.SetFloat(WaterShaderIds.WakeFadePower, WakeFadePower);
        }

        public void ApplyPainterlyMaskSettings(MaterialPropertyBlock block, WaterQualitySettings qualitySettings)
        {
            if (block == null)
            {
                return;
            }

            qualitySettings.Sanitize();
            var enabled = qualitySettings.EnablePainterlyInteractionMasks;
            block.SetFloat(WaterShaderIds.PainterlyMasksEnabled, enabled ? 1f : 0f);
            block.SetFloat(WaterShaderIds.PainterlyAgeFrames, qualitySettings.EnablePainterlyAgeFrames ? 1f : 0f);
            ApplyPainterlyMask(block, RingMask, WaterShaderIds.RingMaskAtlas, WaterShaderIds.RingMaskAtlasValid, WaterShaderIds.RingMaskAtlasGrid, WaterShaderIds.RingMaskVariantCount, WaterShaderIds.RingMaskFrameCount, WaterShaderIds.RingMaskInfluence, enabled);
            ApplyPainterlyMask(block, ContactFoamMask, WaterShaderIds.FoamMaskAtlas, WaterShaderIds.FoamMaskAtlasValid, WaterShaderIds.FoamMaskAtlasGrid, WaterShaderIds.FoamMaskVariantCount, WaterShaderIds.FoamMaskFrameCount, WaterShaderIds.FoamMaskInfluence, enabled);
            ApplyPainterlyMask(block, WakeMask, WaterShaderIds.WakeMaskAtlas, WaterShaderIds.WakeMaskAtlasValid, WaterShaderIds.WakeMaskAtlasGrid, WaterShaderIds.WakeMaskVariantCount, WaterShaderIds.WakeMaskFrameCount, WaterShaderIds.WakeMaskInfluence, enabled);
        }

        public bool Equals(WaterStyleSettings other)
        {
            return TopColor == other.TopColor &&
                   ShallowColor == other.ShallowColor &&
                   DeepColor == other.DeepColor &&
                   FrontSurfaceColor == other.FrontSurfaceColor &&
                   FrontDeepColor == other.FrontDeepColor &&
                   FoamColor == other.FoamColor &&
                   Mathf.Approximately(TopDepthPower, other.TopDepthPower) &&
                   Mathf.Approximately(TopOpacity, other.TopOpacity) &&
                   Mathf.Approximately(ColorBandSteps, other.ColorBandSteps) &&
                   Mathf.Approximately(ColorBandInfluence, other.ColorBandInfluence) &&
                   Mathf.Approximately(AmbientWaveAmplitude, other.AmbientWaveAmplitude) &&
                   Mathf.Approximately(AmbientWaveLength, other.AmbientWaveLength) &&
                   Mathf.Approximately(AmbientWaveSpeed, other.AmbientWaveSpeed) &&
                   AmbientWaveDirection == other.AmbientWaveDirection &&
                   SurfaceNormalTexture == other.SurfaceNormalTexture &&
                   SurfaceDetailTexture == other.SurfaceDetailTexture &&
                   NormalLayer1Scale == other.NormalLayer1Scale &&
                   NormalLayer1Speed == other.NormalLayer1Speed &&
                   Mathf.Approximately(NormalLayer1Strength, other.NormalLayer1Strength) &&
                   NormalLayer2Scale == other.NormalLayer2Scale &&
                   NormalLayer2Speed == other.NormalLayer2Speed &&
                   Mathf.Approximately(NormalLayer2Strength, other.NormalLayer2Strength) &&
                   Mathf.Approximately(AmbientNormalStrength, other.AmbientNormalStrength) &&
                   FresnelTint == other.FresnelTint &&
                   Mathf.Approximately(FresnelStrength, other.FresnelStrength) &&
                   Mathf.Approximately(FresnelPower, other.FresnelPower) &&
                   HighlightColor == other.HighlightColor &&
                   Mathf.Approximately(HighlightStrength, other.HighlightStrength) &&
                   Mathf.Approximately(HighlightThreshold, other.HighlightThreshold) &&
                   Mathf.Approximately(HighlightSoftness, other.HighlightSoftness) &&
                   Mathf.Approximately(HighlightBreakup, other.HighlightBreakup) &&
                   HighlightDirection == other.HighlightDirection &&
                   StylizedReflectionTint == other.StylizedReflectionTint &&
                   StylizedReflectionHorizonColor == other.StylizedReflectionHorizonColor &&
                   StylizedReflectionTopColor == other.StylizedReflectionTopColor &&
                   Mathf.Approximately(StylizedReflectionStrength, other.StylizedReflectionStrength) &&
                   PlanarReflectionTint == other.PlanarReflectionTint &&
                   Mathf.Approximately(PlanarReflectionStrength, other.PlanarReflectionStrength) &&
                   Mathf.Approximately(AmbientReflectionDistortion, other.AmbientReflectionDistortion) &&
                   Mathf.Approximately(RingNormalStrength, other.RingNormalStrength) &&
                   Mathf.Approximately(RingReflectionDistortion, other.RingReflectionDistortion) &&
                   Mathf.Approximately(WakeNormalStrength, other.WakeNormalStrength) &&
                   Mathf.Approximately(WakeReflectionDistortion, other.WakeReflectionDistortion) &&
                   Mathf.Approximately(BoundaryFoamWidth, other.BoundaryFoamWidth) &&
                   Mathf.Approximately(BoundaryFoamSoftness, other.BoundaryFoamSoftness) &&
                   Mathf.Approximately(BoundaryFoamBreakup, other.BoundaryFoamBreakup) &&
                   Mathf.Approximately(BoundaryFoamIntensity, other.BoundaryFoamIntensity) &&
                   RefractionSourceAvailable == other.RefractionSourceAvailable &&
                   RefractionTint == other.RefractionTint &&
                   Mathf.Approximately(RefractionStrength, other.RefractionStrength) &&
                   FrontDistortionSourceAvailable == other.FrontDistortionSourceAvailable &&
                   FrontDistortionTint == other.FrontDistortionTint &&
                   Mathf.Approximately(FrontDistortionStrength, other.FrontDistortionStrength) &&
                   Mathf.Approximately(FrontDepthPower, other.FrontDepthPower) &&
                   Mathf.Approximately(FrontOpacity, other.FrontOpacity) &&
                   Mathf.Approximately(WaterlineBandWidth, other.WaterlineBandWidth) &&
                   CausticTexture == other.CausticTexture &&
                   CausticScale == other.CausticScale &&
                   CausticSpeed == other.CausticSpeed &&
                   CausticTint == other.CausticTint &&
                   Mathf.Approximately(CausticIntensity, other.CausticIntensity) &&
                   Mathf.Approximately(CausticDepthFade, other.CausticDepthFade) &&
                   Mathf.Approximately(RippleAmplitude, other.RippleAmplitude) &&
                   Mathf.Approximately(RingLifetime, other.RingLifetime) &&
                   Mathf.Approximately(RingExpansionMultiplier, other.RingExpansionMultiplier) &&
                   Mathf.Approximately(RingThickness, other.RingThickness) &&
                   Mathf.Approximately(RingSoftness, other.RingSoftness) &&
                   Mathf.Approximately(RingIntensity, other.RingIntensity) &&
                   Mathf.Approximately(ContactFoamWidthPadding, other.ContactFoamWidthPadding) &&
                   Mathf.Approximately(ContactFoamHalfDepth, other.ContactFoamHalfDepth) &&
                   Mathf.Approximately(ContactFoamSoftness, other.ContactFoamSoftness) &&
                   Mathf.Approximately(ContactFoamIntensity, other.ContactFoamIntensity) &&
                   Mathf.Approximately(ContactFoamFadeDuration, other.ContactFoamFadeDuration) &&
                   Mathf.Approximately(FoamReflectionOcclusion, other.FoamReflectionOcclusion) &&
                   Mathf.Approximately(WakeEmissionSpacing, other.WakeEmissionSpacing) &&
                   Mathf.Approximately(WakeMinimumLateralSpeed, other.WakeMinimumLateralSpeed) &&
                   Mathf.Approximately(WakeWidthMultiplier, other.WakeWidthMultiplier) &&
                   Mathf.Approximately(WakeWidthPadding, other.WakeWidthPadding) &&
                   Mathf.Approximately(WakeMinimumHalfWidth, other.WakeMinimumHalfWidth) &&
                   Mathf.Approximately(WakeMaximumHalfWidth, other.WakeMaximumHalfWidth) &&
                   Mathf.Approximately(WakeLifetime, other.WakeLifetime) &&
                   Mathf.Approximately(WakeFadePower, other.WakeFadePower) &&
                   Mathf.Approximately(WakeIntensity, other.WakeIntensity) &&
                   Mathf.Approximately(WakeDirectionReversalAngle, other.WakeDirectionReversalAngle) &&
                   RingMask.Equals(other.RingMask) &&
                   ContactFoamMask.Equals(other.ContactFoamMask) &&
                   WakeMask.Equals(other.WakeMask);
        }

        public override bool Equals(object obj)
        {
            return obj is WaterStyleSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = TopColor.GetHashCode();
                hash = hash * 31 + ShallowColor.GetHashCode();
                hash = hash * 31 + DeepColor.GetHashCode();
                hash = hash * 31 + FrontSurfaceColor.GetHashCode();
                hash = hash * 31 + FrontDeepColor.GetHashCode();
                hash = hash * 31 + FoamColor.GetHashCode();
                hash = hash * 31 + TopDepthPower.GetHashCode();
                hash = hash * 31 + TopOpacity.GetHashCode();
                hash = hash * 31 + ColorBandSteps.GetHashCode();
                hash = hash * 31 + ColorBandInfluence.GetHashCode();
                hash = hash * 31 + AmbientWaveAmplitude.GetHashCode();
                hash = hash * 31 + AmbientWaveLength.GetHashCode();
                hash = hash * 31 + AmbientWaveSpeed.GetHashCode();
                hash = hash * 31 + AmbientWaveDirection.GetHashCode();
                hash = hash * 31 + (SurfaceNormalTexture != null ? SurfaceNormalTexture.GetHashCode() : 0);
                hash = hash * 31 + (SurfaceDetailTexture != null ? SurfaceDetailTexture.GetHashCode() : 0);
                hash = hash * 31 + NormalLayer1Scale.GetHashCode();
                hash = hash * 31 + NormalLayer1Speed.GetHashCode();
                hash = hash * 31 + NormalLayer1Strength.GetHashCode();
                hash = hash * 31 + NormalLayer2Scale.GetHashCode();
                hash = hash * 31 + NormalLayer2Speed.GetHashCode();
                hash = hash * 31 + NormalLayer2Strength.GetHashCode();
                hash = hash * 31 + AmbientNormalStrength.GetHashCode();
                hash = hash * 31 + FresnelTint.GetHashCode();
                hash = hash * 31 + FresnelStrength.GetHashCode();
                hash = hash * 31 + FresnelPower.GetHashCode();
                hash = hash * 31 + HighlightColor.GetHashCode();
                hash = hash * 31 + HighlightStrength.GetHashCode();
                hash = hash * 31 + HighlightThreshold.GetHashCode();
                hash = hash * 31 + HighlightSoftness.GetHashCode();
                hash = hash * 31 + HighlightBreakup.GetHashCode();
                hash = hash * 31 + HighlightDirection.GetHashCode();
                hash = hash * 31 + StylizedReflectionTint.GetHashCode();
                hash = hash * 31 + StylizedReflectionHorizonColor.GetHashCode();
                hash = hash * 31 + StylizedReflectionTopColor.GetHashCode();
                hash = hash * 31 + StylizedReflectionStrength.GetHashCode();
                hash = hash * 31 + PlanarReflectionTint.GetHashCode();
                hash = hash * 31 + PlanarReflectionStrength.GetHashCode();
                hash = hash * 31 + AmbientReflectionDistortion.GetHashCode();
                hash = hash * 31 + RingNormalStrength.GetHashCode();
                hash = hash * 31 + RingReflectionDistortion.GetHashCode();
                hash = hash * 31 + WakeNormalStrength.GetHashCode();
                hash = hash * 31 + WakeReflectionDistortion.GetHashCode();
                hash = hash * 31 + BoundaryFoamWidth.GetHashCode();
                hash = hash * 31 + BoundaryFoamSoftness.GetHashCode();
                hash = hash * 31 + BoundaryFoamBreakup.GetHashCode();
                hash = hash * 31 + BoundaryFoamIntensity.GetHashCode();
                hash = hash * 31 + RefractionSourceAvailable.GetHashCode();
                hash = hash * 31 + RefractionTint.GetHashCode();
                hash = hash * 31 + RefractionStrength.GetHashCode();
                hash = hash * 31 + FrontDistortionSourceAvailable.GetHashCode();
                hash = hash * 31 + FrontDistortionTint.GetHashCode();
                hash = hash * 31 + FrontDistortionStrength.GetHashCode();
                hash = hash * 31 + FrontDepthPower.GetHashCode();
                hash = hash * 31 + FrontOpacity.GetHashCode();
                hash = hash * 31 + WaterlineBandWidth.GetHashCode();
                hash = hash * 31 + (CausticTexture != null ? CausticTexture.GetHashCode() : 0);
                hash = hash * 31 + CausticScale.GetHashCode();
                hash = hash * 31 + CausticSpeed.GetHashCode();
                hash = hash * 31 + CausticTint.GetHashCode();
                hash = hash * 31 + CausticIntensity.GetHashCode();
                hash = hash * 31 + CausticDepthFade.GetHashCode();
                hash = hash * 31 + RippleAmplitude.GetHashCode();
                hash = hash * 31 + RingLifetime.GetHashCode();
                hash = hash * 31 + RingExpansionMultiplier.GetHashCode();
                hash = hash * 31 + RingThickness.GetHashCode();
                hash = hash * 31 + RingSoftness.GetHashCode();
                hash = hash * 31 + RingIntensity.GetHashCode();
                hash = hash * 31 + ContactFoamWidthPadding.GetHashCode();
                hash = hash * 31 + ContactFoamHalfDepth.GetHashCode();
                hash = hash * 31 + ContactFoamSoftness.GetHashCode();
                hash = hash * 31 + ContactFoamIntensity.GetHashCode();
                hash = hash * 31 + ContactFoamFadeDuration.GetHashCode();
                hash = hash * 31 + FoamReflectionOcclusion.GetHashCode();
                hash = hash * 31 + WakeEmissionSpacing.GetHashCode();
                hash = hash * 31 + WakeMinimumLateralSpeed.GetHashCode();
                hash = hash * 31 + WakeWidthMultiplier.GetHashCode();
                hash = hash * 31 + WakeWidthPadding.GetHashCode();
                hash = hash * 31 + WakeMinimumHalfWidth.GetHashCode();
                hash = hash * 31 + WakeMaximumHalfWidth.GetHashCode();
                hash = hash * 31 + WakeLifetime.GetHashCode();
                hash = hash * 31 + WakeFadePower.GetHashCode();
                hash = hash * 31 + WakeIntensity.GetHashCode();
                hash = hash * 31 + WakeDirectionReversalAngle.GetHashCode();
                hash = hash * 31 + RingMask.GetHashCode();
                hash = hash * 31 + ContactFoamMask.GetHashCode();
                return hash * 31 + WakeMask.GetHashCode();
            }
        }

        private static void ApplyPainterlyMask(
            MaterialPropertyBlock block,
            WaterPainterlyMaskSettings settings,
            int atlasId,
            int validId,
            int gridId,
            int variantId,
            int frameId,
            int influenceId,
            bool enabled)
        {
            settings.Sanitize();
            block.SetFloat(validId, enabled && settings.Atlas != null && settings.Influence > 0f ? 1f : 0f);
            if (settings.Atlas != null)
            {
                block.SetTexture(atlasId, settings.Atlas);
            }

            block.SetVector(gridId, new Vector4(settings.Grid.x, settings.Grid.y, 0f, 0f));
            block.SetFloat(variantId, settings.VariantCount);
            block.SetFloat(frameId, settings.FrameCount);
            block.SetFloat(influenceId, settings.Influence);
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

        private static Color SanitizeColor(Color value, Color fallback)
        {
            if (!IsFinite(value.r) || !IsFinite(value.g) || !IsFinite(value.b) || !IsFinite(value.a))
            {
                return fallback;
            }

            return new Color(
                Mathf.Clamp01(value.r),
                Mathf.Clamp01(value.g),
                Mathf.Clamp01(value.b),
                Mathf.Clamp01(value.a));
        }

        private static bool IsClear(Color value)
        {
            return value.r == 0f && value.g == 0f && value.b == 0f && value.a == 0f;
        }

        private static Vector2 SanitizeVector2(Vector2 value, Vector2 fallback, float minimum, float maximum)
        {
            if (!IsFinite(value.x) || !IsFinite(value.y) || value.sqrMagnitude < 0.000001f)
            {
                value = fallback;
            }

            return new Vector2(
                Mathf.Clamp(value.x, -maximum, maximum),
                Mathf.Clamp(value.y, -maximum, maximum));
        }

        private static Vector3 SanitizeVector3(Vector3 value, Vector3 fallback, float minimum, float maximum)
        {
            if (!IsFinite(value.x) || !IsFinite(value.y) || !IsFinite(value.z) || value.sqrMagnitude < 0.000001f)
            {
                value = fallback;
            }

            return new Vector3(
                Mathf.Clamp(value.x, -maximum, maximum),
                Mathf.Clamp(value.y, -maximum, maximum),
                Mathf.Clamp(value.z, -maximum, maximum));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    [CreateAssetMenu(fileName = "WaterStyleProfile", menuName = "Water 2.5D/Style Profile")]
    public sealed class WaterStyleProfile : ScriptableObject
    {
        [SerializeField, HideInInspector] private int _serializedDataVersion;

        [Header("Surface Colors")]
        [SerializeField] private Color _topColor = new Color(0.20f, 0.48f, 0.60f, 0.92f);
        [SerializeField] private Color _shallowColor = new Color(0.20f, 0.48f, 0.60f, 0.92f);
        [SerializeField] private Color _deepColor = new Color(0.04f, 0.22f, 0.36f, 0.94f);
        [SerializeField] private Color _frontSurfaceColor = new Color(0.08f, 0.38f, 0.52f, 0.84f);
        [SerializeField] private Color _frontDeepColor = new Color(0.025f, 0.10f, 0.18f, 0.94f);
        [SerializeField] private Color _foamColor = new Color(0.78f, 0.95f, 1f, 0.8f);

        [Header("Stylized Surface Colour")]
        [Min(0.05f)] [SerializeField] private float _topDepthPower = 0.9f;
        [Range(0.01f, 1f)] [SerializeField] private float _topOpacity = 0.92f;
        [Min(1f)] [SerializeField] private float _colorBandSteps = 4f;
        [Range(0f, 1f)] [SerializeField] private float _colorBandInfluence = 0.15f;

        [Header("Analytical Waves")]
        [Min(0f)] [SerializeField] private float _ambientWaveAmplitude = 0.06f;
        [Min(0.01f)] [SerializeField] private float _ambientWaveLength = 3.5f;
        [Min(0f)] [SerializeField] private float _ambientWaveSpeed = 0.8f;
        [SerializeField] private Vector2 _ambientWaveDirection = new Vector2(1f, 0.15f);

        [Header("Ambient Surface Detail")]
        [Tooltip("Optional normal texture. When absent, Water25D uses a deterministic procedural detail fallback.")]
        [SerializeField] private Texture2D _surfaceNormalTexture;
        [Tooltip("Optional secondary detail texture. When absent, the second procedural layer remains available.")]
        [SerializeField] private Texture2D _surfaceDetailTexture;
        [SerializeField] private Vector2 _normalLayer1Scale = new Vector2(0.55f, 0.45f);
        [SerializeField] private Vector2 _normalLayer1Speed = new Vector2(0.035f, -0.021f);
        [Range(0f, 2f)] [SerializeField] private float _normalLayer1Strength = 0.65f;
        [SerializeField] private Vector2 _normalLayer2Scale = new Vector2(1.15f, 0.9f);
        [SerializeField] private Vector2 _normalLayer2Speed = new Vector2(-0.017f, 0.029f);
        [Range(0f, 2f)] [SerializeField] private float _normalLayer2Strength = 0.25f;
        [Range(0f, 1f)] [SerializeField] private float _ambientNormalStrength = 0.10f;

        [Header("Fresnel and Highlights")]
        [SerializeField] private Color _fresnelTint = new Color(0.75f, 0.95f, 1f, 1f);
        [Range(0f, 1f)] [SerializeField] private float _fresnelStrength = 0.30f;
        [Min(0.1f)] [SerializeField] private float _fresnelPower = 4f;
        [SerializeField] private Color _highlightColor = new Color(0.88f, 0.98f, 1f, 1f);
        [Range(0f, 1f)] [SerializeField] private float _highlightStrength = 0.18f;
        [Range(0f, 1f)] [SerializeField] private float _highlightThreshold = 0.65f;
        [Range(0.01f, 1f)] [SerializeField] private float _highlightSoftness = 0.20f;
        [Range(0f, 1f)] [SerializeField] private float _highlightBreakup = 0.25f;
        [SerializeField] private Vector3 _highlightDirection = new Vector3(-0.3f, 0.85f, -0.25f);

        [Header("Reflection Presentation")]
        [SerializeField] private Color _stylizedReflectionTint = Color.white;
        [SerializeField] private Color _stylizedReflectionHorizonColor = new Color(0.22f, 0.52f, 0.68f, 1f);
        [SerializeField] private Color _stylizedReflectionTopColor = new Color(0.48f, 0.78f, 0.88f, 1f);
        [Range(0f, 1f)] [SerializeField] private float _stylizedReflectionStrength = 0.30f;
        [SerializeField] private Color _planarReflectionTint = Color.white;
        [Range(0f, 1f)] [SerializeField] private float _planarReflectionStrength = 0.35f;
        [Range(0f, 0.05f)] [SerializeField] private float _ambientReflectionDistortion = 0.0025f;
        [Range(0f, 1f)] [SerializeField] private float _ringNormalStrength = 0.18f;
        [Range(0f, 0.05f)] [SerializeField] private float _ringReflectionDistortion = 0.008f;
        [Range(0f, 1f)] [SerializeField] private float _wakeNormalStrength = 0.12f;
        [Range(0f, 0.05f)] [SerializeField] private float _wakeReflectionDistortion = 0.006f;

        [Header("Boundary Foam")]
        [Range(0.0001f, 0.5f)] [SerializeField] private float _boundaryFoamWidth = 0.025f;
        [Range(0f, 0.5f)] [SerializeField] private float _boundaryFoamSoftness = 0.04f;
        [Range(0f, 1f)] [SerializeField] private float _boundaryFoamBreakup = 0.25f;
        [Range(0f, 1f)] [SerializeField] private float _boundaryFoamIntensity = 0.45f;

        [Header("Optional Refraction")]
        [Tooltip("Enable only when the project provides a valid URP Camera Opaque Texture.")]
        [SerializeField] private bool _refractionSourceAvailable;
        [SerializeField] private Color _refractionTint = Color.white;
        [Range(0f, 0.02f)] [SerializeField] private float _refractionStrength = 0.003f;
        [Tooltip("Enable only when the 2D Renderer provides a valid Camera Sorting Layer Texture.")]
        [SerializeField] private bool _frontDistortionSourceAvailable;
        [SerializeField] private Color _frontDistortionTint = new Color(0.80f, 0.95f, 1f, 1f);
        [Range(0f, 0.01f)] [SerializeField] private float _frontDistortionStrength = 0.003f;

        [Header("Front Surface")]
        [Min(0.05f)] [SerializeField] private float _frontDepthPower = 1.15f;
        [Range(0.01f, 1f)] [SerializeField] private float _frontOpacity = 0.90f;
        [Range(0.001f, 0.5f)] [SerializeField] private float _waterlineBandWidth = 0.07f;

        [Header("Optional Caustics")]
        [Tooltip("Optional package-owned caustic texture. Missing texture safely disables caustics.")]
        [SerializeField] private Texture2D _causticTexture;
        [SerializeField] private Vector2 _causticScale = new Vector2(0.16f, 0.16f);
        [SerializeField] private Vector2 _causticSpeed = new Vector2(0.018f, -0.012f);
        [SerializeField] private Color _causticTint = new Color(0.78f, 1f, 0.82f, 1f);
        [Range(0f, 1f)] [SerializeField] private float _causticIntensity = 0.20f;
        [Range(0f, 1f)] [SerializeField] private float _causticDepthFade = 0.70f;

        [Header("Contact Ripples")]
        [Min(0f)] [SerializeField] private float _rippleAmplitude = 0.18f;

        [Header("Procedural Surface Rings")]
        [Min(0.01f)] [SerializeField] private float _ringLifetime = 1.25f;
        [Min(1f)] [SerializeField] private float _ringExpansionMultiplier = 6f;
        [Min(0.001f)] [SerializeField] private float _ringThickness = 0.05f;
        [Min(0f)] [SerializeField] private float _ringSoftness = 0.04f;
        [Range(0f, 1f)] [SerializeField] private float _ringIntensity = 0.75f;

        [Header("Contact Foam")]
        [Min(0f)] [SerializeField] private float _contactFoamWidthPadding = 0.08f;
        [Min(0.01f)] [SerializeField] private float _contactFoamHalfDepth = 0.16f;
        [Range(0f, 1f)] [SerializeField] private float _contactFoamSoftness = 0.06f;
        [Range(0f, 1f)] [SerializeField] private float _contactFoamIntensity = 0.85f;
        [Min(0.01f)] [SerializeField] private float _contactFoamFadeDuration = 0.35f;
        [Range(0f, 1f)] [SerializeField] private float _foamReflectionOcclusion = 0.85f;

        [Header("Distance-Spaced Wakes")]
        [Min(0.01f)] [SerializeField] private float _wakeEmissionSpacing = 0.75f;
        [Min(0f)] [SerializeField] private float _wakeMinimumLateralSpeed = 0.5f;
        [Min(0f)] [SerializeField] private float _wakeWidthMultiplier = 0.9f;
        [Min(0f)] [SerializeField] private float _wakeWidthPadding = 0.02f;
        [Min(0.001f)] [SerializeField] private float _wakeMinimumHalfWidth = 0.05f;
        [Min(0.001f)] [SerializeField] private float _wakeMaximumHalfWidth = 0.35f;
        [Min(0.01f)] [SerializeField] private float _wakeLifetime = 1.35f;
        [Min(0.1f)] [SerializeField] private float _wakeFadePower = 1.25f;
        [Range(0f, 1f)] [SerializeField] private float _wakeIntensity = 0.45f;
        [Range(90f, 179f)] [SerializeField] private float _wakeDirectionReversalAngle = 120f;

        [Header("Painterly Interaction Masks")]
        [SerializeField] private WaterPainterlyMaskSettings _ringMask = new WaterPainterlyMaskSettings
        {
            Grid = new Vector2Int(1, 1),
            VariantCount = 1,
            FrameCount = 1,
            Influence = 1f,
            RotationVariation = 1f
        };
        [SerializeField] private WaterPainterlyMaskSettings _contactFoamMask = new WaterPainterlyMaskSettings
        {
            Grid = new Vector2Int(1, 1),
            VariantCount = 1,
            FrameCount = 1,
            Influence = 1f,
            RotationVariation = 1f
        };
        [SerializeField] private WaterPainterlyMaskSettings _wakeMask = new WaterPainterlyMaskSettings
        {
            Grid = new Vector2Int(1, 1),
            VariantCount = 1,
            FrameCount = 1,
            Influence = 1f,
            RotationVariation = 1f
        };

        [Header("Optional Material Templates")]
        [Tooltip("Optional project or package-owned template. It is never mutated at runtime.")]
        [SerializeField] private Material _topMaterialTemplate;
        [Tooltip("Optional project or package-owned template. It is never mutated at runtime.")]
        [SerializeField] private Material _frontMaterialTemplate;

        public Material TopMaterialTemplate => _topMaterialTemplate;
        public Material FrontMaterialTemplate => _frontMaterialTemplate;

        public WaterStyleSettings GetSettings()
        {
            var settings = new WaterStyleSettings
            {
                TopColor = _topColor,
                ShallowColor = _shallowColor,
                DeepColor = _deepColor,
                FrontSurfaceColor = _frontSurfaceColor,
                FrontDeepColor = _frontDeepColor,
                FoamColor = _foamColor,
                TopDepthPower = _topDepthPower,
                TopOpacity = _topOpacity,
                ColorBandSteps = _colorBandSteps,
                ColorBandInfluence = _colorBandInfluence,
                AmbientWaveAmplitude = _ambientWaveAmplitude,
                AmbientWaveLength = _ambientWaveLength,
                AmbientWaveSpeed = _ambientWaveSpeed,
                AmbientWaveDirection = _ambientWaveDirection,
                SurfaceNormalTexture = _surfaceNormalTexture,
                SurfaceDetailTexture = _surfaceDetailTexture,
                NormalLayer1Scale = _normalLayer1Scale,
                NormalLayer1Speed = _normalLayer1Speed,
                NormalLayer1Strength = _normalLayer1Strength,
                NormalLayer2Scale = _normalLayer2Scale,
                NormalLayer2Speed = _normalLayer2Speed,
                NormalLayer2Strength = _normalLayer2Strength,
                AmbientNormalStrength = _ambientNormalStrength,
                FresnelTint = _fresnelTint,
                FresnelStrength = _fresnelStrength,
                FresnelPower = _fresnelPower,
                HighlightColor = _highlightColor,
                HighlightStrength = _highlightStrength,
                HighlightThreshold = _highlightThreshold,
                HighlightSoftness = _highlightSoftness,
                HighlightBreakup = _highlightBreakup,
                HighlightDirection = _highlightDirection,
                StylizedReflectionTint = _stylizedReflectionTint,
                StylizedReflectionHorizonColor = _stylizedReflectionHorizonColor,
                StylizedReflectionTopColor = _stylizedReflectionTopColor,
                StylizedReflectionStrength = _stylizedReflectionStrength,
                PlanarReflectionTint = _planarReflectionTint,
                PlanarReflectionStrength = _planarReflectionStrength,
                AmbientReflectionDistortion = _ambientReflectionDistortion,
                RingNormalStrength = _ringNormalStrength,
                RingReflectionDistortion = _ringReflectionDistortion,
                WakeNormalStrength = _wakeNormalStrength,
                WakeReflectionDistortion = _wakeReflectionDistortion,
                BoundaryFoamWidth = _boundaryFoamWidth,
                BoundaryFoamSoftness = _boundaryFoamSoftness,
                BoundaryFoamBreakup = _boundaryFoamBreakup,
                BoundaryFoamIntensity = _boundaryFoamIntensity,
                RefractionSourceAvailable = _refractionSourceAvailable,
                RefractionTint = _refractionTint,
                RefractionStrength = _refractionStrength,
                FrontDistortionSourceAvailable = _frontDistortionSourceAvailable,
                FrontDistortionTint = _frontDistortionTint,
                FrontDistortionStrength = _frontDistortionStrength,
                FrontDepthPower = _frontDepthPower,
                FrontOpacity = _frontOpacity,
                WaterlineBandWidth = _waterlineBandWidth,
                CausticTexture = _causticTexture,
                CausticScale = _causticScale,
                CausticSpeed = _causticSpeed,
                CausticTint = _causticTint,
                CausticIntensity = _causticIntensity,
                CausticDepthFade = _causticDepthFade,
                RippleAmplitude = _rippleAmplitude,
                RingLifetime = _ringLifetime,
                RingExpansionMultiplier = _ringExpansionMultiplier,
                RingThickness = _ringThickness,
                RingSoftness = _ringSoftness,
                RingIntensity = _ringIntensity,
                ContactFoamWidthPadding = _contactFoamWidthPadding,
                ContactFoamHalfDepth = _contactFoamHalfDepth,
                ContactFoamSoftness = _contactFoamSoftness,
                ContactFoamIntensity = _contactFoamIntensity,
                ContactFoamFadeDuration = _contactFoamFadeDuration,
                FoamReflectionOcclusion = _foamReflectionOcclusion,
                WakeEmissionSpacing = _wakeEmissionSpacing,
                WakeMinimumLateralSpeed = _wakeMinimumLateralSpeed,
                WakeWidthMultiplier = _wakeWidthMultiplier,
                WakeWidthPadding = _wakeWidthPadding,
                WakeMinimumHalfWidth = _wakeMinimumHalfWidth,
                WakeMaximumHalfWidth = _wakeMaximumHalfWidth,
                WakeLifetime = _wakeLifetime,
                WakeFadePower = _wakeFadePower,
                WakeIntensity = _wakeIntensity,
                WakeDirectionReversalAngle = _wakeDirectionReversalAngle,
                RingMask = _ringMask,
                ContactFoamMask = _contactFoamMask,
                WakeMask = _wakeMask
            };
            settings.Sanitize();
            return settings;
        }

        private void OnValidate()
        {
            _ambientWaveAmplitude = Mathf.Max(0f, _ambientWaveAmplitude);
            _ambientWaveLength = Mathf.Max(0.01f, _ambientWaveLength);
            _ambientWaveSpeed = Mathf.Max(0f, _ambientWaveSpeed);
            _rippleAmplitude = Mathf.Max(0f, _rippleAmplitude);
            var settings = GetSettings();
            if (_serializedDataVersion > 0)
            {
                _topColor = settings.TopColor;
                _shallowColor = settings.ShallowColor;
                _deepColor = settings.DeepColor;
                _frontSurfaceColor = settings.FrontSurfaceColor;
                _frontDeepColor = settings.FrontDeepColor;
                _foamColor = settings.FoamColor;
                _topDepthPower = settings.TopDepthPower;
                _topOpacity = settings.TopOpacity;
                _colorBandSteps = settings.ColorBandSteps;
                _colorBandInfluence = settings.ColorBandInfluence;
                _normalLayer1Scale = settings.NormalLayer1Scale;
                _normalLayer1Speed = settings.NormalLayer1Speed;
                _normalLayer1Strength = settings.NormalLayer1Strength;
                _normalLayer2Scale = settings.NormalLayer2Scale;
                _normalLayer2Speed = settings.NormalLayer2Speed;
                _normalLayer2Strength = settings.NormalLayer2Strength;
                _ambientNormalStrength = settings.AmbientNormalStrength;
                _fresnelTint = settings.FresnelTint;
                _fresnelStrength = settings.FresnelStrength;
                _fresnelPower = settings.FresnelPower;
                _highlightColor = settings.HighlightColor;
                _highlightStrength = settings.HighlightStrength;
                _highlightThreshold = settings.HighlightThreshold;
                _highlightSoftness = settings.HighlightSoftness;
                _highlightBreakup = settings.HighlightBreakup;
                _highlightDirection = settings.HighlightDirection;
                _stylizedReflectionTint = settings.StylizedReflectionTint;
                _stylizedReflectionHorizonColor = settings.StylizedReflectionHorizonColor;
                _stylizedReflectionTopColor = settings.StylizedReflectionTopColor;
                _stylizedReflectionStrength = settings.StylizedReflectionStrength;
                _planarReflectionTint = settings.PlanarReflectionTint;
                _planarReflectionStrength = settings.PlanarReflectionStrength;
                _ambientReflectionDistortion = settings.AmbientReflectionDistortion;
                _ringNormalStrength = settings.RingNormalStrength;
                _ringReflectionDistortion = settings.RingReflectionDistortion;
                _wakeNormalStrength = settings.WakeNormalStrength;
                _wakeReflectionDistortion = settings.WakeReflectionDistortion;
                _boundaryFoamWidth = settings.BoundaryFoamWidth;
                _boundaryFoamSoftness = settings.BoundaryFoamSoftness;
                _boundaryFoamBreakup = settings.BoundaryFoamBreakup;
                _boundaryFoamIntensity = settings.BoundaryFoamIntensity;
                _refractionStrength = settings.RefractionStrength;
                _refractionTint = settings.RefractionTint;
                _frontDistortionStrength = settings.FrontDistortionStrength;
                _frontDistortionTint = settings.FrontDistortionTint;
                _frontDepthPower = settings.FrontDepthPower;
                _frontOpacity = settings.FrontOpacity;
                _waterlineBandWidth = settings.WaterlineBandWidth;
                _causticScale = settings.CausticScale;
                _causticSpeed = settings.CausticSpeed;
                _causticTint = settings.CausticTint;
                _causticIntensity = settings.CausticIntensity;
                _causticDepthFade = settings.CausticDepthFade;
            }
            _ringLifetime = settings.RingLifetime;
            _ringExpansionMultiplier = settings.RingExpansionMultiplier;
            _ringThickness = settings.RingThickness;
            _ringSoftness = settings.RingSoftness;
            _ringIntensity = settings.RingIntensity;
            _contactFoamWidthPadding = settings.ContactFoamWidthPadding;
            _contactFoamHalfDepth = settings.ContactFoamHalfDepth;
            _contactFoamSoftness = settings.ContactFoamSoftness;
            _contactFoamIntensity = settings.ContactFoamIntensity;
            _contactFoamFadeDuration = settings.ContactFoamFadeDuration;
            _foamReflectionOcclusion = settings.FoamReflectionOcclusion;
            _wakeEmissionSpacing = settings.WakeEmissionSpacing;
            _wakeMinimumLateralSpeed = settings.WakeMinimumLateralSpeed;
            _wakeWidthMultiplier = settings.WakeWidthMultiplier;
            _wakeWidthPadding = settings.WakeWidthPadding;
            _wakeMinimumHalfWidth = settings.WakeMinimumHalfWidth;
            _wakeMaximumHalfWidth = settings.WakeMaximumHalfWidth;
            _wakeLifetime = settings.WakeLifetime;
            _wakeFadePower = settings.WakeFadePower;
            _wakeIntensity = settings.WakeIntensity;
            _wakeDirectionReversalAngle = settings.WakeDirectionReversalAngle;
            _ringMask = settings.RingMask;
            _contactFoamMask = settings.ContactFoamMask;
            _wakeMask = settings.WakeMask;
            if (_ambientWaveDirection.sqrMagnitude < 0.0001f)
            {
                _ambientWaveDirection = Vector2.right;
            }
        }
    }
}
