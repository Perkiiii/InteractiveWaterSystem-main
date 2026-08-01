using NUnit.Framework;
using UnityEngine;
using Water25D.FX;
using Water25D.Rendering;

namespace Water25D.Tests
{
    public sealed class WaterProductionTests
    {
        [Test]
        public void ReflectionGroupsQuantizeCoplanarPlanesAndSeparateDifferentHeights()
        {
            var first = new GameObject("Water25D Reflection Test A");
            var second = new GameObject("Water25D Reflection Test B");
            try
            {
                first.transform.position = new Vector3(0f, 2f, 0f);
                second.transform.position = new Vector3(0f, 2.003f, 0f);
                var firstKey = WaterReflectionGroupKey.Create(null, first.transform, ~0, WaterReflectionMode.Planar, 0.25f, 3);
                var secondKey = WaterReflectionGroupKey.Create(null, second.transform, ~0, WaterReflectionMode.Planar, 0.25f, 3);
                Assert.AreEqual(firstKey, secondKey);

                second.transform.position = new Vector3(0f, 2.02f, 0f);
                var separateKey = WaterReflectionGroupKey.Create(null, second.transform, ~0, WaterReflectionMode.Planar, 0.25f, 3);
                Assert.AreNotEqual(firstKey, separateKey);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void DisabledReflectionDoesNotCreateARegistration()
        {
            var root = new GameObject("Water25D Reflection Disabled Test");
            var renderer = root.AddComponent<MeshRenderer>();
            try
            {
                var registration = WaterReflectionManager.Register(
                    renderer,
                    root.transform,
                    null,
                    WaterReflectionMode.Disabled,
                    ~0,
                    0.25f,
                    3,
                    0.35f);
                Assert.IsNull(registration);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FXPoolRejectsExhaustionAndReturnsEntriesWithoutDestroyingDuringUse()
        {
            var root = new GameObject("Water25D FX Pool Test");
            WaterFXPool pool = null;
            try
            {
                pool = new WaterFXPool(root.transform, null, false, 2);
                Assert.AreEqual(2, pool.Capacity);
                Assert.IsTrue(pool.Spawn(Vector3.zero, Vector2.down, 1f));
                Assert.IsTrue(pool.Spawn(Vector3.right, Vector2.up, 0.5f));
                Assert.IsFalse(pool.Spawn(Vector3.left, Vector2.zero, 1f));
                Assert.AreEqual(2, pool.ActiveCount);

                pool.Tick(2f);
                Assert.AreEqual(0, pool.ActiveCount);
                Assert.IsTrue(pool.Spawn(Vector3.zero, Vector2.zero, 1f));
            }
            finally
            {
                pool?.Dispose();
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RippleBatchInjectsAllQueuedImpactsBeforeOneFullPropagationUpdate()
        {
            var resources = new WaterRuntimeResources();
            var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Water25D/Materials/Water25D_RippleSimulation.mat");
            var settings = WaterQualitySettings.Default;
            settings.SimulationFrequency = 4f;
            settings.MaximumImpactsPerStep = 8;
            settings.MaximumQueuedImpacts = 8;
            settings.Sanitize();
            CustomRenderTextureRippleSimulator simulator = null;
            try
            {
                simulator = new CustomRenderTextureRippleSimulator(resources, new Vector2(4f, 2f), settings, material);
                Assert.IsTrue(simulator.IsAvailable);
                for (var i = 0; i < 4; i++)
                {
                    simulator.EnqueueImpact(new WaterRippleImpact(new Vector2(0.2f + i * 0.15f, 0.5f), 0.5f, 0.2f, true));
                }

                simulator.Tick(0.25f, true);

                Assert.AreEqual(4, simulator.ImpactInjectionUpdateCount);
                Assert.AreEqual(1, simulator.FullSurfacePropagationUpdateCount);
            }
            finally
            {
                simulator?.Dispose();
                resources.Dispose();
            }
        }
    }
}
