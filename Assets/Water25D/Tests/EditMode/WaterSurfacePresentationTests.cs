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
            Assert.AreEqual(8, defaultModule.MaximumWakeSegments);
            Assert.AreEqual(2, defaultModule.MaximumWakeEmissionsPerStep);
            Assert.AreEqual(16, defaultModule.RenderData.WakeShaderArrayLength);
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
            changed.MaximumWakeSegments = 16;
            changed.MaximumWakeEmissionsPerStep = 4;
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
        public void WakeDefaultsAreCalmAndSanitizeDeterministically()
        {
            var style = WaterStyleSettings.Default;
            Assert.AreEqual(0.75f, style.WakeEmissionSpacing, 0.0001f);
            Assert.Greater(style.WakeMinimumLateralSpeed, 0f);
            Assert.Greater(style.WakeLifetime, 0f);
            Assert.Greater(style.WakeIntensity, 0f);

            style.WakeEmissionSpacing = -1f;
            style.WakeMinimumLateralSpeed = float.NaN;
            style.WakeWidthMultiplier = -1f;
            style.WakeMinimumHalfWidth = 2f;
            style.WakeMaximumHalfWidth = 0.1f;
            style.WakeLifetime = float.PositiveInfinity;
            style.WakeFadePower = -1f;
            style.WakeDirectionReversalAngle = float.NaN;
            style.Sanitize();

            Assert.Greater(style.WakeEmissionSpacing, 0f);
            Assert.GreaterOrEqual(style.WakeMinimumLateralSpeed, 0f);
            Assert.GreaterOrEqual(style.WakeWidthMultiplier, 0f);
            Assert.GreaterOrEqual(style.WakeMaximumHalfWidth, style.WakeMinimumHalfWidth);
            Assert.Greater(style.WakeLifetime, 0f);
            Assert.Greater(style.WakeFadePower, 0f);
            Assert.That(style.WakeDirectionReversalAngle, Is.InRange(90f, 179f));

            var quality = WaterQualitySettings.Default;
            quality.MaximumWakeSegments = 0;
            quality.MaximumWakeEmissionsPerStep = 0;
            quality.Sanitize();
            Assert.That(quality.MaximumWakeSegments, Is.InRange(1, 16));
            Assert.That(quality.MaximumWakeEmissionsPerStep, Is.InRange(1, 16));
        }

        [Test]
        public void WakeCapacityUsesPreallocatedArraysAndDeterministicReplacement()
        {
            var module = CreateWakeModule(2, 16);
            var renderData = module.RenderData;
            var wakesA = renderData.WakesA;
            var wakesB = renderData.WakesB;

            Assert.IsFalse(module.UpdateWake(7, Vector2.zero, 0.5f, 1f));
            Assert.IsTrue(module.UpdateWake(7, Vector2.right, 0.5f, 1f));
            Assert.IsTrue(module.UpdateWake(7, Vector2.right * 2f, 0.5f, 1f));
            Assert.IsTrue(module.UpdateWake(7, Vector2.right * 3f, 0.5f, 1f));

            Assert.AreEqual(2, module.ActiveWakeSegmentCount);
            Assert.AreEqual(1, module.ReplacedWakeCount);
            Assert.AreSame(wakesA, module.RenderData.WakesA);
            Assert.AreSame(wakesB, module.RenderData.WakesB);
            Assert.AreEqual(16, module.RenderData.WakeShaderArrayLength);
            Assert.AreEqual(2.5f, module.RenderData.GetWakeA(0).x, 0.0001f);
            // The newest segment remains in the second slot while the oldest slot is reclaimed.
            Assert.AreEqual(1.5f, module.RenderData.GetWakeA(1).x, 0.0001f);
        }

        [Test]
        public void WakeBodyStateCapacityDropsWithoutGrowingStorage()
        {
            var module = new WaterSurfacePresentationModule();
            var quality = WakeQuality(16, 16);
            quality.MaximumTrackedSurfaceBodies = 1;
            module.Configure(quality, WakeStyle());

            Assert.IsFalse(module.UpdateWake(1, Vector2.zero, 0.5f, 1f));
            Assert.IsFalse(module.UpdateWake(2, Vector2.zero, 0.5f, 1f));
            Assert.AreEqual(1, module.DroppedWakeBodyCount);
            Assert.IsFalse(module.UpdateWake(2, Vector2.right, 0.5f, 1f));
            Assert.AreEqual(2, module.DroppedWakeBodyCount);
            Assert.AreEqual(16, module.RenderData.WakeShaderArrayLength);
        }

        [Test]
        public void WakeSpacingUsesSurfaceDistanceAndInterpolatesLargeSteps()
        {
            var module = CreateWakeModule(16, 16);
            Assert.IsFalse(module.UpdateWake(1, Vector2.zero, 0.5f, 1f));
            Assert.IsTrue(module.UpdateWake(1, new Vector2(2.4f, 0f), 0.5f, 2.4f));

            Assert.AreEqual(2, module.ActiveWakeSegmentCount);
            Assert.AreEqual(0.5f, module.RenderData.GetWakeA(0).x, 0.0001f);
            Assert.AreEqual(1.5f, module.RenderData.GetWakeA(1).x, 0.0001f);
            Assert.AreEqual(1.5f, module.RenderData.GetWakeA(0).z, 0.0001f);
            Assert.AreEqual(2.5f, module.RenderData.GetWakeA(1).z, 0.0001f);
        }

        [Test]
        public void WakeRemainderIsRetainedAndEquivalentPathsMatch()
        {
            var first = CreateWakeModule(16, 16);
            Assert.IsFalse(first.UpdateWake(1, Vector2.zero, 0.5f, 1f));
            Assert.IsFalse(first.UpdateWake(1, new Vector2(0.4f, 0f), 0.5f, 1f));
            Assert.IsTrue(first.TryGetWakeDistanceRemainder(1, out var remainder));
            Assert.AreEqual(0.4f, remainder, 0.0001f);
            Assert.IsTrue(first.UpdateWake(1, new Vector2(1.1f, 0f), 0.5f, 1f));
            Assert.AreEqual(1, first.ActiveWakeSegmentCount);
            Assert.AreEqual(0.5f, first.RenderData.GetWakeA(0).x, 0.0001f);

            var largeStep = CreateWakeModule(16, 16);
            var smallSteps = CreateWakeModule(16, 16);
            largeStep.UpdateWake(1, Vector2.zero, 0.5f, 1f);
            smallSteps.UpdateWake(1, Vector2.zero, 0.5f, 1f);
            Assert.IsTrue(largeStep.UpdateWake(1, new Vector2(2f, 0f), 0.5f, 1f));
            Assert.IsFalse(smallSteps.UpdateWake(1, new Vector2(0.8f, 0f), 0.5f, 0.8f));
            Assert.IsTrue(smallSteps.UpdateWake(1, new Vector2(2f, 0f), 0.5f, 1.2f));
            Assert.AreEqual(largeStep.ActiveWakeSegmentCount, smallSteps.ActiveWakeSegmentCount);
            for (var i = 0; i < largeStep.ActiveWakeSegmentCount; i++)
            {
                Assert.AreEqual(largeStep.RenderData.GetWakeA(i), smallSteps.RenderData.GetWakeA(i));
                Assert.AreEqual(largeStep.RenderData.GetWakeB(i), smallSteps.RenderData.GetWakeB(i));
            }
        }

        [Test]
        public void WakeEmissionCapRetainsOnlyBoundedFractionalRemainder()
        {
            var module = CreateWakeModule(16, 2);
            module.UpdateWake(1, Vector2.zero, 0.5f, 1f);
            Assert.IsTrue(module.UpdateWake(1, new Vector2(5.25f, 0f), 0.5f, 1f));
            Assert.AreEqual(2, module.ActiveWakeSegmentCount);
            Assert.IsTrue(module.TryGetWakeDistanceRemainder(1, out var remainder));
            Assert.AreEqual(0.25f, remainder, 0.0001f);

            Assert.IsFalse(module.UpdateWake(1, new Vector2(5.85f, 0f), 0.6f, 0.6f));
            Assert.IsTrue(module.UpdateWake(1, new Vector2(6.45f, 0f), 0.6f, 0.6f));
            Assert.AreEqual(3, module.ActiveWakeSegmentCount);
        }

        [Test]
        public void WakeRejectsSlowZeroAndNonFiniteMovement()
        {
            var module = CreateWakeModule(16, 16);
            var style = WakeStyle();
            style.WakeMinimumLateralSpeed = 1f;
            module.Configure(WakeQuality(16, 16), style);
            module.UpdateWake(1, Vector2.zero, 0.5f, 1f);
            Assert.IsFalse(module.UpdateWake(1, new Vector2(0.5f, 0f), 0.5f, 1f));
            Assert.IsFalse(module.UpdateWake(1, new Vector2(0.5f, 0f), 0.5f, 1f));
            Assert.IsFalse(module.UpdateWake(1, new Vector2(float.NaN, 0f), 0.5f, 1f));
            Assert.AreEqual(0, module.ActiveWakeSegmentCount);
        }

        [Test]
        public void WakeWidthIsDerivedAndClampedFromAggregateContactWidth()
        {
            var style = WakeStyle();
            style.WakeWidthMultiplier = 2f;
            style.WakeWidthPadding = 0.1f;
            style.WakeMinimumHalfWidth = 0.2f;
            style.WakeMaximumHalfWidth = 0.4f;
            var module = new WaterSurfacePresentationModule();
            module.Configure(WakeQuality(16, 16), style);
            module.UpdateWake(1, Vector2.zero, 0.1f, 1f);
            module.UpdateWake(1, Vector2.right, 0.1f, 1f);
            Assert.AreEqual(0.3f, module.RenderData.GetWakeB(0).x, 0.0001f);

            var capped = new WaterSurfacePresentationModule();
            capped.Configure(WakeQuality(16, 16), style);
            capped.UpdateWake(1, Vector2.zero, 1f, 1f);
            capped.UpdateWake(1, Vector2.right, 1f, 1f);
            Assert.AreEqual(0.4f, capped.RenderData.GetWakeB(0).x, 0.0001f);
        }

        [Test]
        public void WakeAgesFadesAndReusesExpiredSlots()
        {
            var style = WakeStyle();
            style.WakeLifetime = 1f;
            var module = new WaterSurfacePresentationModule(2);
            module.Configure(WakeQuality(2, 16), style);
            module.UpdateWake(1, Vector2.zero, 0.5f, 1f);
            module.UpdateWake(1, Vector2.right, 0.5f, 1f);
            Assert.IsTrue(module.Tick(0.25f));
            Assert.AreEqual(0.25f, module.RenderData.GetWakeB(0).y, 0.0001f);
            Assert.IsTrue(module.Tick(0.75f));
            Assert.AreEqual(0, module.ActiveWakeSegmentCount);
            Assert.AreEqual(0, module.ReplacedWakeCount);

            module.UpdateWake(1, Vector2.right * 2f, 0.5f, 1f);
            Assert.AreEqual(1, module.ActiveWakeSegmentCount);
            Assert.AreEqual(0, module.ReplacedWakeCount);
        }

        [Test]
        public void WakeDirectionReversalResetsAccumulatorWithoutBridgeSegment()
        {
            var module = CreateWakeModule(16, 16);
            module.UpdateWake(1, Vector2.zero, 0.5f, 1f);
            module.UpdateWake(1, new Vector2(1.2f, 0f), 0.5f, 1f);
            Assert.AreEqual(1, module.ActiveWakeSegmentCount);

            Assert.IsFalse(module.UpdateWake(1, new Vector2(-0.1f, 0f), 0.5f, 1f));
            Assert.IsTrue(module.TryGetWakeDistanceRemainder(1, out var remainder));
            Assert.AreEqual(0f, remainder, 0.0001f);
            Assert.IsTrue(module.UpdateWake(1, new Vector2(-1.1f, 0f), 0.5f, 1f));
            Assert.AreEqual(2, module.ActiveWakeSegmentCount);
            Assert.Less(module.RenderData.GetWakeA(1).z, 0f);
            Assert.Less(module.RenderData.GetWakeA(1).x, module.RenderData.GetWakeA(0).x);
        }

        [Test]
        public void WakeVariationIsStableAndRingFoamDataCoexists()
        {
            var first = CreateWakeModule(16, 16);
            var second = CreateWakeModule(16, 16);
            Assert.IsTrue(first.AddRing(Vector2.one, 1f, 0.2f));
            Assert.IsTrue(first.UpdateContactFoam(42, Vector2.one, 0.5f, 0.5f, 1f));
            first.UpdateWake(7, Vector2.zero, 0.5f, 1f);
            second.UpdateWake(7, Vector2.zero, 0.5f, 1f);
            first.UpdateWake(7, Vector2.right, 0.5f, 1f);
            second.UpdateWake(7, Vector2.right, 0.5f, 1f);

            Assert.AreEqual(1, first.RenderData.ActiveRingCount);
            Assert.AreEqual(1, first.RenderData.ActiveContactFoamCount);
            Assert.AreEqual(1, first.RenderData.ActiveWakeCount);
            Assert.AreEqual(first.RenderData.GetWakeB(0).w, second.RenderData.GetWakeB(0).w, 0.0001f);
            Assert.AreEqual(Vector2.one.x, first.RenderData.GetRingA(0).x, 0.0001f);
            Assert.AreEqual(Vector2.one.y, first.RenderData.GetRingA(0).y, 0.0001f);
            Assert.AreEqual(Vector2.one.x, first.RenderData.GetFoamA(0).x, 0.0001f);
        }

        [Test]
        public void MultipleLogicalBodiesKeepIndependentWakeAccumulators()
        {
            var module = CreateWakeModule(16, 16);
            module.UpdateWake(1, Vector2.zero, 0.5f, 1f);
            module.UpdateWake(2, Vector2.zero, 0.5f, 1f);
            module.UpdateWake(1, Vector2.right, 0.5f, 1f);
            Assert.AreEqual(1, module.ActiveWakeSegmentCount);
            module.UpdateWake(2, Vector2.right * 0.5f, 0.5f, 1f);
            Assert.AreEqual(1, module.ActiveWakeSegmentCount);
            module.UpdateWake(2, Vector2.right, 0.5f, 1f);
            Assert.AreEqual(2, module.ActiveWakeSegmentCount);
        }

        [Test]
        public void ReleasedWakeBodyDropsAccumulatorWhileExistingSegmentsFade()
        {
            var style = WakeStyle();
            style.WakeLifetime = 1f;
            var module = new WaterSurfacePresentationModule();
            module.Configure(WakeQuality(16, 16), style);
            module.UpdateWake(9, Vector2.zero, 0.5f, 1f);
            module.UpdateWake(9, Vector2.right, 0.5f, 1f);
            Assert.AreEqual(1, module.ActiveWakeSegmentCount);

            Assert.IsTrue(module.ReleaseWakeBody(9));
            Assert.IsFalse(module.TryGetWakeDistanceRemainder(9, out _));
            Assert.AreEqual(1, module.ActiveWakeSegmentCount);
            Assert.IsTrue(module.Tick(1f));
            Assert.AreEqual(0, module.ActiveWakeSegmentCount);
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
            Assert.AreEqual(WaterQualitySettings.Default.MaximumWakeSegments, quality.MaximumWakeSegments);
            Assert.AreEqual(WaterQualitySettings.Default.MaximumWakeEmissionsPerStep, quality.MaximumWakeEmissionsPerStep);
            Assert.AreEqual(defaultStyle.WakeEmissionSpacing, style.WakeEmissionSpacing, 0.0001f);
            Assert.AreEqual(defaultStyle.WakeLifetime, style.WakeLifetime, 0.0001f);
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

        private static WaterSurfacePresentationModule CreateWakeModule(int maximumWakeSegments, int maximumWakeEmissionsPerStep)
        {
            var module = new WaterSurfacePresentationModule();
            module.Configure(WakeQuality(maximumWakeSegments, maximumWakeEmissionsPerStep), WakeStyle());
            return module;
        }

        private static WaterQualitySettings WakeQuality(int maximumWakeSegments, int maximumWakeEmissionsPerStep)
        {
            var quality = WaterQualitySettings.Default;
            quality.MaximumWakeSegments = maximumWakeSegments;
            quality.MaximumWakeEmissionsPerStep = maximumWakeEmissionsPerStep;
            quality.MaximumContactFoams = 8;
            quality.MaximumSurfaceRings = 16;
            quality.Sanitize();
            return quality;
        }

        private static WaterStyleSettings WakeStyle()
        {
            var style = WaterStyleSettings.Default;
            style.WakeEmissionSpacing = 1f;
            style.WakeMinimumLateralSpeed = 0.1f;
            style.WakeWidthMultiplier = 1f;
            style.WakeWidthPadding = 0f;
            style.WakeMinimumHalfWidth = 0.05f;
            style.WakeMaximumHalfWidth = 1f;
            style.WakeLifetime = 2f;
            style.WakeFadePower = 1f;
            style.WakeIntensity = 1f;
            style.WakeDirectionReversalAngle = 120f;
            style.Sanitize();
            return style;
        }
    }
}
