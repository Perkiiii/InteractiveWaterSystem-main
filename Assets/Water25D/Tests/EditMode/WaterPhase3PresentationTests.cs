using System.IO;
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
        public void FlatStylizedProfileUsesPackageOwnedAmeyeFork()
        {
            var profile = UnityEditor.AssetDatabase.LoadAssetAtPath<WaterStyleProfile>(
                "Assets/Water25D/Profiles/Water25D_FlatStylizedStyle.asset");

            Assert.IsNotNull(profile);

            var settings = profile.GetSettings();
            Assert.IsNotNull(profile.TopMaterialTemplate);
            Assert.IsNotNull(profile.FrontMaterialTemplate);
            Assert.AreEqual("Water25D/Stylized Ameye Top Surface", profile.TopMaterialTemplate.shader.name);
            Assert.AreEqual("Water25D/Stylized Ameye Front Surface", profile.FrontMaterialTemplate.shader.name);
            Assert.IsNotNull(settings.SurfaceNormalTexture);
            Assert.IsNotNull(settings.SurfaceDetailTexture);
            Assert.That(
                UnityEditor.AssetDatabase.GetAssetPath(settings.SurfaceNormalTexture),
                Does.StartWith("Assets/Water25D/Textures/Stylized/"));
            Assert.That(
                UnityEditor.AssetDatabase.GetAssetPath(settings.SurfaceDetailTexture),
                Does.StartWith("Assets/Water25D/Textures/Stylized/"));
        }

        [Test]
        public void AmeyeForkHasNoGerstnerOrReferenceOnlyGraphDependency()
        {
            const string graphPath =
                "Assets/Water25D/Shaders/Stylized/Water25D_AmeyeStylizedWater.shadergraph";
            Assert.IsTrue(File.Exists(graphPath));

            var graphText = File.ReadAllText(graphPath);
            Assert.That(graphText, Does.Not.Contain("GerstnerWaves"));
            Assert.That(graphText, Does.Not.Contain("2c87"));
            Assert.That(graphText, Does.Not.Contain("ReferenceOnly"));
            Assert.That(graphText, Does.Not.Contain("_GlobalEffectRT"));
            Assert.That(graphText, Does.Not.Contain("_OrthographicCamSize"));
        }

        [Test]
        public void AmeyeForkShadersResolveWithWater25DRuntimeContract()
        {
            var topMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Water25D/Materials/Stylized/Water25D_AmeyeTop.mat");
            var frontMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Water25D/Materials/Stylized/Water25D_AmeyeFront.mat");

            Assert.IsNotNull(topMaterial);
            Assert.IsNotNull(frontMaterial);
            Assert.IsTrue(topMaterial.shader.isSupported);
            Assert.IsTrue(frontMaterial.shader.isSupported);
            Assert.IsTrue(topMaterial.HasProperty("_ReflectionTexture"));
            Assert.IsTrue(topMaterial.HasProperty("_ReflectionEnabled"));

            foreach (var property in new[]
                     {
                         "_WaterRingCount",
                         "_WaterFoamCount",
                         "_WaterWakeCount",
                         "_AmeyeIntersectionFoamTexture",
                         "_AmeyeSurfaceFoamTexture"
                     })
            {
                Assert.IsTrue(topMaterial.HasProperty(property), property);
                Assert.IsTrue(frontMaterial.HasProperty(property), property);
            }
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
