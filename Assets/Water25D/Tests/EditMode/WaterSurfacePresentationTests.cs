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
            Assert.AreEqual(4, WaterQualitySettings.Default.MaximumContactFoams);
            Assert.AreEqual(8, WaterQualitySettings.Default.MaximumTrackedSurfaceBodies);
        }

        [Test]
        public void ContactFoamDefaultCapacityIsFourAndClampsToShaderMaximum()
        {
            var module = new WaterSurfacePresentationModule();
            var quality = WaterQualitySettings.Default;
            quality.MaximumContactFoams = 1;
            module.Configure(quality, WaterStyleSettings.Default);
            Assert.AreEqual(1, module.MaximumContactFoams);

            quality.MaximumContactFoams = 32;
            module.Configure(quality, WaterStyleSettings.Default);
            Assert.AreEqual(8, module.MaximumContactFoams);
            Assert.AreEqual(8, module.RenderData.FoamShaderArrayLength);
        }

        [Test]
        public void ContactFoamUpdatesOneKeyedSlotAndPreservesRingData()
        {
            var module = new WaterSurfacePresentationModule();
            Assert.IsTrue(module.AddRing(new Vector2(2f, 3f), 1f, 0.2f));
            var ring = module.RenderData.GetRingA(0);

            Assert.IsTrue(module.UpdateContactFoam(42, new Vector2(4f, 1f), 0.5f, 0.25f, 1f));
            Assert.AreEqual(1, module.ActiveContactFoamCount);
            Assert.AreEqual(1, module.RenderData.ActiveContactFoamCount);
            Assert.AreEqual(4f, module.RenderData.GetFoamA(0).x, 0.0001f);
            Assert.AreEqual(0.5f + WaterStyleSettings.Default.ContactFoamWidthPadding, module.RenderData.GetFoamA(0).z, 0.0001f);

            Assert.IsTrue(module.UpdateContactFoam(42, new Vector2(6f, 2f), 0.75f, 0.5f, 1f));
            Assert.AreEqual(1, module.ActiveContactFoamCount);
            Assert.AreEqual(6f, module.RenderData.GetFoamA(0).x, 0.0001f);
            Assert.AreEqual(ring, module.RenderData.GetRingA(0));
        }

        [Test]
        public void ReleasedContactFoamFadesAndFullyFadedSlotsAreReclaimed()
        {
            var module = new WaterSurfacePresentationModule(2);
            Assert.IsTrue(module.UpdateContactFoam(1, Vector2.zero, 0.5f, 0.5f, 1f));
            Assert.IsTrue(module.ReleaseContactFoam(1));
            Assert.AreEqual(0, module.ActiveContactFoamCount);
            Assert.AreEqual(1, module.FadingContactFoamCount);
            Assert.IsTrue(module.Tick(WaterStyleSettings.Default.ContactFoamFadeDuration));
            Assert.AreEqual(0, module.FadingContactFoamCount);

            Assert.IsTrue(module.UpdateContactFoam(2, Vector2.one, 0.5f, 0.5f, 1f));
            Assert.AreEqual(1, module.ActiveContactFoamCount);
        }

        [Test]
        public void ActiveContactFoamsAreNotEvictedAndOldestFadingSlotIsReclaimed()
        {
            var module = new WaterSurfacePresentationModule(2);
            var quality = WaterQualitySettings.Default;
            quality.MaximumContactFoams = 2;
            module.Configure(quality, WaterStyleSettings.Default);
            Assert.IsTrue(module.UpdateContactFoam(1, Vector2.zero, 0.5f, 0.5f, 1f));
            Assert.IsTrue(module.UpdateContactFoam(2, Vector2.one, 0.5f, 0.5f, 1f));
            Assert.IsFalse(module.UpdateContactFoam(3, Vector2.one * 2f, 0.5f, 0.5f, 1f));
            Assert.AreEqual(1, module.DroppedContactFoamCount);

            Assert.IsTrue(module.ReleaseContactFoam(1));
            Assert.IsTrue(module.UpdateContactFoam(3, Vector2.one * 2f, 0.5f, 0.5f, 1f));
            Assert.AreEqual(2, module.ActiveContactFoamCount);
            Assert.AreEqual(2, module.RenderData.ActiveContactFoamCount);
            Assert.IsTrue(module.UpdateContactFoam(2, Vector2.one * 3f, 0.5f, 0.5f, 1f));
            Assert.AreEqual(2, module.ActiveContactFoamCount);
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
            var ringsA = renderData.RingsA;
            var foamsA = renderData.FoamsA;
            for (var i = 0; i < 64; i++)
            {
                module.AddRing(new Vector2(i % 8, i / 8), 0.5f, 0.1f);
                module.UpdateContactFoam(i, new Vector2(i % 8, i / 8), 0.25f, 0.5f, 1f);
                module.Tick(0.01f);
            }

            Assert.AreSame(renderData, module.RenderData);
            Assert.AreSame(ringsA, renderData.RingsA);
            Assert.AreSame(foamsA, renderData.FoamsA);
            Assert.AreEqual(16, module.RenderData.ShaderArrayLength);
            Assert.AreEqual(8, module.RenderData.FoamShaderArrayLength);
            module.Reset();
            Assert.AreEqual(0, module.ActiveRingCount);
            Assert.AreEqual(0, module.ActiveContactFoamCount);
            Assert.AreEqual(0, module.ReplacedRingCount);
            Assert.AreEqual(0f, module.RenderData.GetRingA(0).x, 0.0001f);
            Assert.AreEqual(0f, module.RenderData.GetRingB(0).x, 0.0001f);
            Assert.AreEqual(0f, module.RenderData.GetFoamA(0).x, 0.0001f);
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
            style.RingExpansionMultiplier = 0.25f;
            style.ContactFoamHalfDepth = -1f;
            style.ContactFoamFadeDuration = float.NaN;
            style.FoamReflectionOcclusion = 2f;
            style.Sanitize();
            Assert.Greater(style.RingLifetime, 0f);
            Assert.Greater(style.RingExpansionMultiplier, 0f);
            Assert.GreaterOrEqual(style.RingExpansionMultiplier, 1f);
            Assert.Greater(style.RingThickness, 0f);
            Assert.GreaterOrEqual(style.RingSoftness, 0f);
            Assert.LessOrEqual(style.RingIntensity, 1f);
            Assert.Greater(style.ContactFoamHalfDepth, 0f);
            Assert.Greater(style.ContactFoamFadeDuration, 0f);
            Assert.LessOrEqual(style.FoamReflectionOcclusion, 1f);

            var changed = WaterStyleSettings.Default;
            changed.ContactFoamIntensity = 0.25f;
            Assert.IsFalse(WaterStyleSettings.Default.Equals(changed));
            Assert.AreNotEqual(WaterStyleSettings.Default.GetHashCode(), changed.GetHashCode());
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

            changed = settings;
            changed.MaximumContactFoams = 8;
            changed.MaximumTrackedSurfaceBodies = 16;
            Assert.IsTrue(settings.SimulationEquals(changed));
            Assert.IsFalse(settings.Equals(changed));
            Assert.AreNotEqual(settings.GetHashCode(), changed.GetHashCode());

            changed.MaximumSurfaceRings = 0;
            changed.Sanitize();
            Assert.That(changed.MaximumSurfaceRings, Is.InRange(1, 16));
            changed.MaximumContactFoams = 0;
            changed.MaximumTrackedSurfaceBodies = 0;
            changed.Sanitize();
            Assert.That(changed.MaximumContactFoams, Is.InRange(1, 8));
            Assert.That(changed.MaximumTrackedSurfaceBodies, Is.InRange(1, 16));
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
            Assert.AreEqual(defaultStyle.ContactFoamWidthPadding, style.ContactFoamWidthPadding, 0.0001f);
            Assert.AreEqual(defaultStyle.ContactFoamHalfDepth, style.ContactFoamHalfDepth, 0.0001f);
            Assert.AreEqual(defaultStyle.ContactFoamSoftness, style.ContactFoamSoftness, 0.0001f);
            Assert.AreEqual(defaultStyle.ContactFoamIntensity, style.ContactFoamIntensity, 0.0001f);
            Assert.AreEqual(defaultStyle.ContactFoamFadeDuration, style.ContactFoamFadeDuration, 0.0001f);
            Assert.AreEqual(defaultStyle.FoamReflectionOcclusion, style.FoamReflectionOcclusion, 0.0001f);
            Assert.AreEqual(8, quality.MaximumSurfaceRings);
            Assert.AreEqual(4, quality.MaximumContactFoams);
            Assert.AreEqual(8, quality.MaximumTrackedSurfaceBodies);
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
