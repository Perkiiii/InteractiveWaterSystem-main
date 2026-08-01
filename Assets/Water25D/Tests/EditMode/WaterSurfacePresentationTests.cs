using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Water25D.Rendering;

namespace Water25D.Tests
{
    public sealed class WaterSurfacePresentationTests
    {
        [Test]
        public void DefaultCapacityIsEightAndClampsToShaderMaximum()
        {
            var defaultModule = new WaterSurfacePresentationModule();
            var lowModule = new WaterSurfacePresentationModule(-4);
            var highModule = new WaterSurfacePresentationModule(32);

            Assert.AreEqual(8, defaultModule.MaximumSurfaceRings);
            Assert.AreEqual(1, lowModule.MaximumSurfaceRings);
            Assert.AreEqual(16, highModule.MaximumSurfaceRings);
            Assert.AreEqual(16, defaultModule.RenderData.ShaderArrayLength);
        }

        [Test]
        public void AddingRingStoresLocalXZDataAndActivatesOneSlot()
        {
            var module = new WaterSurfacePresentationModule();

            Assert.IsTrue(module.AddRing(new Vector2(3.5f, 1.25f), 0.5f, 0.25f, false));
            Assert.AreEqual(1, module.ActiveRingCount);
            Assert.AreEqual(3.5f, module.RenderData.GetRingA(0).x, 0.0001f);
            Assert.AreEqual(1.25f, module.RenderData.GetRingA(0).y, 0.0001f);
            Assert.AreEqual(0f, module.RenderData.GetRingA(0).z, 0.0001f);
            Assert.AreEqual(0.375f, module.RenderData.GetRingA(0).w, 0.0001f);
            Assert.AreEqual(0.25f, module.RenderData.GetRingB(0).x, 0.0001f);
            Assert.AreEqual(1.5f, module.RenderData.GetRingB(0).y, 0.0001f);
        }

        [Test]
        public void RingAgeAdvancesAndExpiresAtLifetime()
        {
            var module = new WaterSurfacePresentationModule();
            Assert.IsTrue(module.AddRing(Vector2.one, 1f, 0.2f));

            Assert.IsTrue(module.Tick(0.25f));
            Assert.AreEqual(0.2f, module.RenderData.GetRingA(0).z, 0.0001f);
            Assert.AreEqual(1, module.ActiveRingCount);

            Assert.IsTrue(module.Tick(1f));
            Assert.AreEqual(0, module.ActiveRingCount);
            Assert.AreEqual(0, module.RenderData.ActiveRingCount);
        }

        [Test]
        public void FullCapacityReplacesOldestDeterministically()
        {
            var module = new WaterSurfacePresentationModule(2);
            Assert.IsTrue(module.AddRing(new Vector2(1f, 1f), 1f, 0.1f));
            Assert.IsTrue(module.AddRing(new Vector2(2f, 2f), 1f, 0.1f));
            Assert.IsTrue(module.AddRing(new Vector2(3f, 3f), 1f, 0.1f));

            Assert.AreEqual(2, module.ActiveRingCount);
            Assert.AreEqual(1, module.ReplacedRingCount);
            Assert.AreEqual(3f, module.RenderData.GetRingA(0).x, 0.0001f);
            Assert.AreEqual(2f, module.RenderData.GetRingA(1).x, 0.0001f);
        }

        [Test]
        public void ExpiredSlotsAreReclaimedBeforeReplacement()
        {
            var module = new WaterSurfacePresentationModule(2);
            Assert.IsTrue(module.AddRing(Vector2.zero, 1f, 0.1f));
            Assert.IsTrue(module.Tick(1.25f));
            Assert.AreEqual(0, module.ActiveRingCount);
            Assert.IsTrue(module.AddRing(Vector2.one, 1f, 0.1f));
            Assert.AreEqual(0, module.ReplacedRingCount);
        }

        [Test]
        public void RepeatedOperationsReusePreparedRenderDataAndClearState()
        {
            var module = new WaterSurfacePresentationModule();
            var renderData = module.RenderData;
            for (var i = 0; i < 64; i++)
            {
                module.AddRing(new Vector2(i % 8, i / 8), 0.5f, 0.1f);
                module.Tick(0.01f);
            }

            Assert.AreSame(renderData, module.RenderData);
            Assert.AreEqual(16, module.RenderData.ShaderArrayLength);
            module.Reset();
            Assert.AreEqual(0, module.ActiveRingCount);
            Assert.AreEqual(0, module.ReplacedRingCount);
            Assert.AreEqual(0f, module.RenderData.GetRingA(0).x, 0.0001f);
            Assert.AreEqual(0f, module.RenderData.GetRingB(0).x, 0.0001f);
        }

        [Test]
        public void InvalidImpactValuesAreRejectedOrSafelySanitized()
        {
            var module = new WaterSurfacePresentationModule();
            Assert.IsFalse(module.AddRing(Vector2.zero, float.NaN, 0.2f));
            Assert.IsFalse(module.AddRing(new Vector2(float.PositiveInfinity, 0f), 1f, 0.2f));
            Assert.IsTrue(module.AddRing(Vector2.zero, -0.5f, -1f));
            Assert.AreEqual(WaterQualitySettings.Default.ImpactRadius, module.RenderData.GetRingB(0).x, 0.0001f);
        }

        [Test]
        public void RingSettingsHaveUsableDefaultsAndSanitize()
        {
            var style = WaterStyleSettings.Default;
            Assert.Greater(style.RingLifetime, 0f);
            Assert.Greater(style.RingExpansionMultiplier, 0f);
            Assert.Greater(style.RingThickness, 0f);
            Assert.Greater(style.RingSoftness, 0f);
            Assert.Greater(style.RingIntensity, 0f);

            style.RingLifetime = -1f;
            style.RingExpansionMultiplier = float.NaN;
            style.RingThickness = -1f;
            style.RingSoftness = -1f;
            style.RingIntensity = 2f;
            style.Sanitize();
            Assert.Greater(style.RingLifetime, 0f);
            Assert.Greater(style.RingExpansionMultiplier, 0f);
            Assert.Greater(style.RingThickness, 0f);
            Assert.GreaterOrEqual(style.RingSoftness, 0f);
            Assert.LessOrEqual(style.RingIntensity, 1f);
        }

        [Test]
        public void QualityRingCapacityIsEightAndNotSimulationRelevant()
        {
            var settings = WaterQualitySettings.Default;
            Assert.AreEqual(8, settings.MaximumSurfaceRings);

            var changed = settings;
            changed.MaximumSurfaceRings = 16;
            Assert.IsTrue(settings.SimulationEquals(changed));
            Assert.IsFalse(settings.Equals(changed));

            changed.MaximumSurfaceRings = 0;
            changed.Sanitize();
            Assert.That(changed.MaximumSurfaceRings, Is.InRange(1, 16));
        }

        [Test]
        public void ExistingProfileAssetsResolveSafeRingDefaultsWithoutAssetMigration()
        {
            var styleProfile = AssetDatabase.LoadAssetAtPath<WaterStyleProfile>("Assets/Water25D/Profiles/Water25D_DefaultStyle.asset");
            var qualityProfile = AssetDatabase.LoadAssetAtPath<WaterQualityProfile>("Assets/Water25D/Profiles/Water25D_MediumQuality.asset");
            Assert.IsNotNull(styleProfile);
            Assert.IsNotNull(qualityProfile);

            var style = styleProfile.GetSettings();
            var quality = qualityProfile.GetSettings();
            var defaultStyle = WaterStyleSettings.Default;
            Assert.AreEqual(defaultStyle.RingLifetime, style.RingLifetime, 0.0001f);
            Assert.AreEqual(defaultStyle.RingExpansionMultiplier, style.RingExpansionMultiplier, 0.0001f);
            Assert.AreEqual(defaultStyle.RingThickness, style.RingThickness, 0.0001f);
            Assert.AreEqual(defaultStyle.RingSoftness, style.RingSoftness, 0.0001f);
            Assert.AreEqual(defaultStyle.RingIntensity, style.RingIntensity, 0.0001f);
            Assert.AreEqual(8, quality.MaximumSurfaceRings);
        }

        [Test]
        public void ControllerSurfaceMappingReturnsLocalXZWorldUnits()
        {
            var root = new GameObject("Water25D Surface Mapping Test");
            try
            {
                root.transform.position = new Vector3(10f, 2f, -4f);
                root.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
                var controller = root.AddComponent<Water25DController>();
                controller.SetDimensions(new Vector2(20f, 6.5f), controller.FrontSurfaceDepth);

                var worldPosition = root.transform.TransformPoint(new Vector3(4f, controller.WaterlineLocalY, 2f));
                Assert.IsTrue(controller.TryGetSurfaceLocalXZ(worldPosition, out var localXZ));
                Assert.AreEqual(4f, localXZ.x, 0.0001f);
                Assert.AreEqual(2f, localXZ.y, 0.0001f);
                Assert.IsFalse(controller.TryGetSurfaceLocalXZ(root.transform.TransformPoint(new Vector3(20.1f, 0f, 2f)), out _));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
