using NUnit.Framework;
using UnityEngine;
using Water25D.Rendering;

namespace Water25D.Tests
{
    public sealed class WaterPhase3PresentationTests
    {
        [Test]
        public void DefaultStylizedPresentationIsCalmAndSourceGated()
        {
            var settings = WaterStyleSettings.Default;
            settings.Sanitize();

            Assert.That(settings.ShallowColor, Is.EqualTo(settings.TopColor));
            Assert.That(settings.DeepColor.a, Is.GreaterThan(0f));
            Assert.That(settings.AmbientNormalStrength, Is.LessThanOrEqualTo(0.2f));
            Assert.That(settings.FresnelPower, Is.GreaterThan(0f));
            Assert.That(settings.RefractionSourceAvailable, Is.False);
            Assert.That(settings.FrontDistortionSourceAvailable, Is.False);
            Assert.That(settings.CausticTexture, Is.Null);
        }

        [Test]
        public void PresentationQualityFlagsRemainSeparateFromSimulationSettings()
        {
            var first = WaterQualitySettings.Default;
            var second = first;
            second.EnableRefraction = !second.EnableRefraction;
            second.EnableCaustics = !second.EnableCaustics;

            Assert.That(first.SimulationEquals(second), Is.True);
            Assert.That(first.Equals(second), Is.False);
            Assert.That(first.EnableSecondaryAmbientDetail, Is.True);
            Assert.That(first.EnableStylizedHighlights, Is.True);
        }

        [Test]
        public void FlatSurfaceShadersExposePhase3PresentationContract()
        {
            var topMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Water25D/Materials/Water25D_Top.mat");
            var frontMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Water25D/Materials/Water25D_Front.mat");

            Assert.IsNotNull(topMaterial);
            Assert.IsNotNull(frontMaterial);
            Assert.IsTrue(topMaterial.HasProperty("_ShallowColor"));
            Assert.IsTrue(topMaterial.HasProperty("_DeepColor"));
            Assert.IsTrue(topMaterial.HasProperty("_SurfaceNormalTexture"));
            Assert.IsTrue(topMaterial.HasProperty("_FresnelStrength"));
            Assert.IsTrue(topMaterial.HasProperty("_ReflectionTexture"));
            Assert.IsTrue(frontMaterial.HasProperty("_FrontDepthPower"));
            Assert.IsTrue(frontMaterial.HasProperty("_BoundaryFoamIntensity"));
            Assert.IsTrue(frontMaterial.HasProperty("_CausticTexture"));
        }

        [Test]
        public void ReflectionGroupsSeparateExplicitExclusionMasks()
        {
            var first = new GameObject("Water25D Phase3 Reflection Key A");
            var second = new GameObject("Water25D Phase3 Reflection Key B");
            try
            {
                var firstKey = WaterReflectionGroupKey.Create(
                    null,
                    first.transform,
                    ~0,
                    WaterReflectionMode.Planar,
                    0.25f,
                    3,
                    (LayerMask)(1 << 8));
                var secondKey = WaterReflectionGroupKey.Create(
                    null,
                    second.transform,
                    ~0,
                    WaterReflectionMode.Planar,
                    0.25f,
                    3,
                    (LayerMask)(1 << 9));

                Assert.That(firstKey.Equals(secondKey), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }
    }
}
