using NUnit.Framework;
using UnityEngine;
using Water25D.Rendering;

namespace Water25D.Tests
{
    public sealed class WaterPainterlyInteractionTests
    {
        [Test]
        public void PainterlyMaskSettingsClampToFixedGridCapacity()
        {
            var settings = new WaterPainterlyMaskSettings
            {
                Grid = new Vector2Int(0, 99),
                VariantCount = 999,
                FrameCount = 999,
                Influence = float.NaN,
                RotationVariation = float.PositiveInfinity
            };
            settings.Sanitize();

            Assert.AreEqual(new Vector2Int(1, 16), settings.Grid);
            Assert.AreEqual(16, settings.VariantCount);
            Assert.AreEqual(1, settings.FrameCount);
            Assert.AreEqual(1f, settings.Influence, 0.0001f);
            Assert.AreEqual(1f, settings.RotationVariation, 0.0001f);

            settings = new WaterPainterlyMaskSettings
            {
                Grid = new Vector2Int(4, 2),
                VariantCount = 3,
                FrameCount = 99,
                Influence = 0.5f,
                RotationVariation = 0.25f
            };
            settings.Sanitize();
            Assert.AreEqual(3, settings.VariantCount);
            Assert.AreEqual(2, settings.FrameCount);
        }

        [Test]
        public void PainterlyQualityFlagsDoNotChangeSimulationIdentity()
        {
            var enabled = WaterQualitySettings.Default;
            var disabled = enabled;
            disabled.EnablePainterlyInteractionMasks = false;
            disabled.EnablePainterlyAgeFrames = false;

            Assert.IsTrue(enabled.SimulationEquals(disabled));
            Assert.IsFalse(enabled.Equals(disabled));
            Assert.AreNotEqual(enabled.GetHashCode(), disabled.GetHashCode());
        }

        [Test]
        public void AnalyticalFallbackIsUploadedWhenMasksAreMissingOrDisabled()
        {
            var style = WaterStyleSettings.Default;
            var quality = WaterQualitySettings.Default;
            var block = new MaterialPropertyBlock();
            style.ApplyPainterlyMaskSettings(block, quality);
            Assert.AreEqual(1f, block.GetFloat(Shader.PropertyToID("_PainterlyMasksEnabled")), 0.0001f);
            Assert.AreEqual(0f, block.GetFloat(Shader.PropertyToID("_RingMaskAtlasValid")), 0.0001f);
            Assert.AreEqual(0f, block.GetFloat(Shader.PropertyToID("_FoamMaskAtlasValid")), 0.0001f);
            Assert.AreEqual(0f, block.GetFloat(Shader.PropertyToID("_WakeMaskAtlasValid")), 0.0001f);

            quality.EnablePainterlyInteractionMasks = false;
            style.ApplyPainterlyMaskSettings(block, quality);
            Assert.AreEqual(0f, block.GetFloat(Shader.PropertyToID("_PainterlyMasksEnabled")), 0.0001f);
            Assert.AreEqual(0f, block.GetFloat(Shader.PropertyToID("_RingMaskAtlasValid")), 0.0001f);
        }

        [Test]
        public void PainterlyMetadataIsDeterministicAndWakeMetadataStaysAligned()
        {
            var quality = WaterQualitySettings.Default;
            quality.MaximumSurfaceRings = 2;
            quality.MaximumContactFoams = 2;
            quality.MaximumWakeSegments = 4;
            var style = WaterStyleSettings.Default;
            var ringMask = style.RingMask;
            ringMask.RotationVariation = 0.5f;
            style.RingMask = ringMask;

            var first = new WaterSurfacePresentationModule(2);
            var second = new WaterSurfacePresentationModule(2);
            try
            {
                first.Configure(quality, style);
                second.Configure(quality, style);
                Assert.IsTrue(first.AddRing(new Vector2(1f, 2f), 0.8f, 0.25f));
                Assert.IsTrue(second.AddRing(new Vector2(1f, 2f), 0.8f, 0.25f));
                Assert.AreEqual(1, first.ActiveRingCount);
                Assert.AreEqual(1, second.ActiveRingCount);
                Assert.AreNotEqual(Vector4.zero, first.RenderData.GetRingC(0));
                Assert.AreNotEqual(Vector4.zero, second.RenderData.GetRingC(0));
                var firstRingMetadata = first.RenderData.GetRingC(0);
                var secondRingMetadata = second.RenderData.GetRingC(0);
                Assert.That(firstRingMetadata, Is.EqualTo(secondRingMetadata), $"first={firstRingMetadata} second={secondRingMetadata}");

                Assert.IsTrue(first.UpdateContactFoam(7, new Vector2(1f, 2f), 0.4f, 0.5f, 0.7f));
                Assert.IsTrue(second.UpdateContactFoam(7, new Vector2(1f, 2f), 0.4f, 0.5f, 0.7f));
                Assert.AreEqual(1, first.ActiveContactFoamCount);
                Assert.AreEqual(1, second.ActiveContactFoamCount);
                Assert.AreNotEqual(Vector4.zero, first.RenderData.GetFoamC(0));
                Assert.AreNotEqual(Vector4.zero, second.RenderData.GetFoamC(0));
                Assert.That(first.RenderData.GetFoamC(0), Is.EqualTo(second.RenderData.GetFoamC(0)));

                first.UpdateWake(9, Vector2.zero, 0.4f, 0.1f);
                Assert.IsTrue(first.UpdateWake(9, Vector2.right, 0.4f, 0.1f));
                var wakeMetadata = first.RenderData.GetWakeC(0);
                Assert.AreEqual(1, first.ActiveWakeSegmentCount);
                Assert.That(wakeMetadata.y, Is.InRange(-0.18f, 0.18f));
                Assert.That(Mathf.Abs(wakeMetadata.w), Is.EqualTo(1f).Within(0.0001f));

                Assert.IsTrue(first.AddRing(new Vector2(2f, 2f), 0.8f, 0.25f));
                var retainedRingMetadata = first.RenderData.GetRingC(1);
                Assert.IsTrue(first.AddRing(new Vector2(3f, 2f), 0.8f, 0.25f));
                Assert.That(first.RenderData.GetRingC(1), Is.EqualTo(retainedRingMetadata));
            }
            finally
            {
                first = null;
                second = null;
            }
        }
    }
}
