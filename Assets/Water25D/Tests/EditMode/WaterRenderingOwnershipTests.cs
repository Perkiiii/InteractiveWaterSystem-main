using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Water25D.Rendering;

namespace Water25D.Tests
{
    public sealed class WaterRenderingOwnershipTests
    {
        [Test]
        public void FinalPropertyBlockWritesHaveOnePackageOwner()
        {
            var packageRoot = Path.Combine(Application.dataPath, "Water25D");
            var sourceFiles = new List<string>();
            sourceFiles.AddRange(Directory.GetFiles(Path.Combine(packageRoot, "Runtime"), "*.cs", SearchOption.AllDirectories));
            sourceFiles.AddRange(Directory.GetFiles(Path.Combine(packageRoot, "Editor"), "*.cs", SearchOption.AllDirectories));
            var writeFiles = new List<string>();
            var readFiles = new List<string>();

            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var source = File.ReadAllText(sourceFiles[i]);
                if (source.IndexOf("SetPropertyBlock", StringComparison.Ordinal) >= 0)
                {
                    writeFiles.Add(sourceFiles[i]);
                }

                if (source.IndexOf("GetPropertyBlock", StringComparison.Ordinal) >= 0)
                {
                    readFiles.Add(sourceFiles[i]);
                }
            }

            Assert.That(writeFiles.Count, Is.EqualTo(1), string.Join("\n", writeFiles));
            Assert.That(Path.GetFileName(writeFiles[0]), Is.EqualTo("WaterRenderingModule.cs"));
            Assert.That(readFiles, Is.Empty, string.Join("\n", readFiles));
        }

        [Test]
        public void ReflectionStatePreservesModeAndPublishedOutput()
        {
            var disabled = WaterReflectionRenderState.ForMode(WaterReflectionMode.Disabled, 0.8f);
            var stylized = WaterReflectionRenderState.ForMode(WaterReflectionMode.Stylized, 0.8f);
            var planar = WaterReflectionRenderState.ForMode(WaterReflectionMode.Planar, 0.8f);
            Assert.IsFalse(disabled.Enabled);
            Assert.IsFalse(disabled.StylizedFallback);
            Assert.IsFalse(stylized.Enabled);
            Assert.IsTrue(stylized.StylizedFallback);
            Assert.IsFalse(planar.Enabled);
            Assert.IsFalse(planar.StylizedFallback);

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                var matrix = Matrix4x4.TRS(new Vector3(1f, 2f, 3f), Quaternion.Euler(10f, 20f, 30f), Vector3.one);
                var published = new WaterReflectionRenderState(texture, matrix, true, false, 0.65f, 17);
                Assert.IsTrue(published.Enabled);
                Assert.IsFalse(published.StylizedFallback);
                Assert.AreSame(texture, published.Texture);
                Assert.That(published.ViewProjection, Is.EqualTo(matrix));
                Assert.AreEqual(0.65f, published.Strength, 0.0001f);
                Assert.AreEqual(17, published.RenderFrame);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void ReflectionRegistrationPublishesSnapshotWithoutRendererMutation()
        {
            var root = new GameObject("Water Reflection Registration Test");
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            WaterReflectionManager.ReflectionRegistration registration = null;
            try
            {
                var renderer = root.AddComponent<MeshRenderer>();
                registration = WaterReflectionManager.Register(
                    renderer,
                    root.transform,
                    null,
                    WaterReflectionMode.Planar,
                    ~0,
                    0.25f,
                    3,
                    0.7f);
                Assert.IsNotNull(registration);
                Assert.IsFalse(registration.State.Enabled);

                var matrix = Matrix4x4.Translate(new Vector3(4f, 5f, 6f));
                registration.Publish(texture, matrix, true, false, 23);
                Assert.IsTrue(registration.State.Enabled);
                Assert.AreSame(texture, registration.State.Texture);
                Assert.That(registration.State.ViewProjection, Is.EqualTo(matrix));
                Assert.AreEqual(0.7f, registration.State.Strength, 0.0001f);
                Assert.AreEqual(23, registration.State.RenderFrame);
                Assert.AreEqual(1, registration.StateVersion);

                registration.Publish(null, Matrix4x4.identity, false, true, 24);
                Assert.IsFalse(registration.State.Enabled);
                Assert.IsTrue(registration.State.StylizedFallback);
                Assert.AreEqual(2, registration.StateVersion);
            }
            finally
            {
                if (registration != null)
                {
                    registration.Dispose();
                }

                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CompleteWriteKeepsPresentationAndReflectionTogether()
        {
            var root = new GameObject("Water Complete Rendering State Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                var presentation = GetPrivateField<WaterSurfacePresentationModule>(controller, "_surfacePresentation");
                var rendering = GetPrivateField<WaterRenderingModule>(controller, "_rendering");
                presentation.Configure(WaterQualitySettings.Default, WaterStyleSettings.Default);
                Assert.IsTrue(presentation.AddRing(new Vector2(2f, 1f), 0.8f, 0.25f));
                Assert.IsTrue(presentation.UpdateContactFoam(42, new Vector2(2f, 1f), 0.5f, 0.6f, 0.8f));
                presentation.UpdateWake(42, new Vector2(0f, 0f), 0.5f, 0.1f);
                Assert.IsTrue(presentation.UpdateWake(42, new Vector2(1f, 0f), 0.5f, 0.1f));

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    var matrix = Matrix4x4.TRS(new Vector3(2f, 3f, 4f), Quaternion.identity, Vector3.one);
                    rendering.ApplyReflectionState(new WaterReflectionRenderState(texture, matrix, true, false, 0.9f, 12));
                    var topRenderer = controller.TopSurface.GetComponent<MeshRenderer>();
                    var frontRenderer = controller.FrontSurface.GetComponent<MeshRenderer>();
                    AssertCompleteBlock(topRenderer, matrix);
                    AssertCompleteBlock(frontRenderer, matrix);

                    var topBlock = GetPrivateField<MaterialPropertyBlock>(rendering, "_topPropertyBlock");
                    rendering.ApplySurfacePresentation(
                        topRenderer,
                        frontRenderer,
                        presentation.RenderData,
                        true);
                    Assert.AreSame(topBlock, GetPrivateField<MaterialPropertyBlock>(rendering, "_topPropertyBlock"));
                    AssertCompleteBlock(topRenderer, matrix);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AssertCompleteBlock(MeshRenderer renderer, Matrix4x4 reflectionMatrix)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            Assert.AreEqual(1f, block.GetFloat(Shader.PropertyToID("_SurfaceMode")), 0.0001f);
            Assert.AreEqual(1f, block.GetFloat(Shader.PropertyToID("_ReflectionEnabled")), 0.0001f);
            Assert.AreEqual(0.9f, block.GetFloat(Shader.PropertyToID("_ReflectionStrength")), 0.0001f);
            Assert.That(block.GetMatrix(Shader.PropertyToID("_ReflectionViewProjection")), Is.EqualTo(reflectionMatrix));
            Assert.AreEqual(1f, block.GetFloat(Shader.PropertyToID("_WaterRingCount")), 0.0001f);
            Assert.AreEqual(1f, block.GetFloat(Shader.PropertyToID("_WaterFoamCount")), 0.0001f);
            Assert.AreEqual(1f, block.GetFloat(Shader.PropertyToID("_WaterWakeCount")), 0.0001f);
            Assert.That(block.GetVectorArray(Shader.PropertyToID("_WaterRingsC"))[0].x, Is.GreaterThanOrEqualTo(0f));
            Assert.AreEqual(1f, block.GetVectorArray(Shader.PropertyToID("_WaterFoamsC"))[0].w, 0.0001f);
            Assert.That(block.GetVectorArray(Shader.PropertyToID("_WaterWakesC"))[0].y, Is.InRange(-0.18f, 0.18f));
        }

        private static T GetPrivateField<T>(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, fieldName);
            return (T)field.GetValue(instance);
        }
    }
}
