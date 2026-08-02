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
        public Color FrontSurfaceColor;
        public Color FrontDeepColor;
        public Color FoamColor;
        public float AmbientWaveAmplitude;
        public float AmbientWaveLength;
        public float AmbientWaveSpeed;
        public Vector2 AmbientWaveDirection;
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
            FrontSurfaceColor = new Color(0.08f, 0.38f, 0.52f, 0.84f),
            FrontDeepColor = new Color(0.025f, 0.10f, 0.18f, 0.94f),
            FoamColor = new Color(0.78f, 0.95f, 1f, 0.8f),
            AmbientWaveAmplitude = 0.06f,
            AmbientWaveLength = 3.5f,
            AmbientWaveSpeed = 0.8f,
            AmbientWaveDirection = new Vector2(1f, 0.15f),
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
            AmbientWaveAmplitude = Mathf.Max(0f, AmbientWaveAmplitude);
            AmbientWaveLength = Mathf.Max(0.01f, AmbientWaveLength);
            AmbientWaveSpeed = Mathf.Max(0f, AmbientWaveSpeed);
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

            block.SetColor(WaterShaderIds.BaseColor, TopColor);
            block.SetColor(WaterShaderIds.WaterColor, TopColor);
            block.SetColor(WaterShaderIds.FrontSurfaceColor, FrontSurfaceColor);
            block.SetColor(WaterShaderIds.SurfaceColor, FrontSurfaceColor);
            block.SetColor(WaterShaderIds.MainColor, FrontDeepColor);
            block.SetColor(WaterShaderIds.FrontDeepColor, FrontDeepColor);
            block.SetColor(WaterShaderIds.FoamColor, FoamColor);
            block.SetFloat(WaterShaderIds.WaveAmplitude, AmbientWaveAmplitude);
            block.SetFloat(WaterShaderIds.WaveLength, AmbientWaveLength);
            block.SetFloat(WaterShaderIds.WaveSpeed, AmbientWaveSpeed);
            block.SetVector(WaterShaderIds.WaveDirection, new Vector4(AmbientWaveDirection.x, AmbientWaveDirection.y, 0f, 0f));
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
                   FrontSurfaceColor == other.FrontSurfaceColor &&
                   FrontDeepColor == other.FrontDeepColor &&
                   FoamColor == other.FoamColor &&
                   Mathf.Approximately(AmbientWaveAmplitude, other.AmbientWaveAmplitude) &&
                   Mathf.Approximately(AmbientWaveLength, other.AmbientWaveLength) &&
                   Mathf.Approximately(AmbientWaveSpeed, other.AmbientWaveSpeed) &&
                   AmbientWaveDirection == other.AmbientWaveDirection &&
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
                hash = hash * 31 + FrontSurfaceColor.GetHashCode();
                hash = hash * 31 + FrontDeepColor.GetHashCode();
                hash = hash * 31 + FoamColor.GetHashCode();
                hash = hash * 31 + AmbientWaveAmplitude.GetHashCode();
                hash = hash * 31 + AmbientWaveLength.GetHashCode();
                hash = hash * 31 + AmbientWaveSpeed.GetHashCode();
                hash = hash * 31 + AmbientWaveDirection.GetHashCode();
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

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    [CreateAssetMenu(fileName = "WaterStyleProfile", menuName = "Water 2.5D/Style Profile")]
    public sealed class WaterStyleProfile : ScriptableObject
    {
        [Header("Surface Colors")]
        [SerializeField] private Color _topColor = new Color(0.20f, 0.48f, 0.60f, 0.92f);
        [SerializeField] private Color _frontSurfaceColor = new Color(0.08f, 0.38f, 0.52f, 0.84f);
        [SerializeField] private Color _frontDeepColor = new Color(0.025f, 0.10f, 0.18f, 0.94f);
        [SerializeField] private Color _foamColor = new Color(0.78f, 0.95f, 1f, 0.8f);

        [Header("Analytical Waves")]
        [Min(0f)] [SerializeField] private float _ambientWaveAmplitude = 0.06f;
        [Min(0.01f)] [SerializeField] private float _ambientWaveLength = 3.5f;
        [Min(0f)] [SerializeField] private float _ambientWaveSpeed = 0.8f;
        [SerializeField] private Vector2 _ambientWaveDirection = new Vector2(1f, 0.15f);

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
                FrontSurfaceColor = _frontSurfaceColor,
                FrontDeepColor = _frontDeepColor,
                FoamColor = _foamColor,
                AmbientWaveAmplitude = _ambientWaveAmplitude,
                AmbientWaveLength = _ambientWaveLength,
                AmbientWaveSpeed = _ambientWaveSpeed,
                AmbientWaveDirection = _ambientWaveDirection,
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
