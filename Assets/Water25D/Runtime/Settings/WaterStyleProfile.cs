using System;
using UnityEngine;

namespace Water25D
{
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
            FoamReflectionOcclusion = 0.85f
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
                   Mathf.Approximately(FoamReflectionOcclusion, other.FoamReflectionOcclusion);
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
                return hash * 31 + FoamReflectionOcclusion.GetHashCode();
            }
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
                FoamReflectionOcclusion = _foamReflectionOcclusion
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
            if (_ambientWaveDirection.sqrMagnitude < 0.0001f)
            {
                _ambientWaveDirection = Vector2.right;
            }
        }
    }
}
