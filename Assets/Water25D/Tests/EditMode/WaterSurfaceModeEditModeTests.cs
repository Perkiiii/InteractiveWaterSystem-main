using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Water25D.Rendering;

namespace Water25D.Tests
{
    public sealed class WaterSurfaceModeEditModeTests : Water25DEditModeFixture
    {
        [Test]
        public void SurfaceModeNumericValuesAreSerializationContract()
        {
            Assert.AreEqual(0, (int)WaterSurfaceMode.SimulatedRipples);
            Assert.AreEqual(1, (int)WaterSurfaceMode.FlatStylized);
        }

        [Test]
        public void NewlyAddedControllerUsesFlatStylizedAfterReset()
        {
            var root = CreateGameObject("Water25D New Controller Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                Assert.AreEqual(WaterSurfaceMode.FlatStylized, controller.SurfaceMode);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ExistingSimulatedControllerIsNotAutomaticallyChanged()
        {
            var root = CreateGameObject("Water25D Existing Controller Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                var serializedObject = new SerializedObject(controller);
                serializedObject.FindProperty("_surfaceMode").enumValueIndex = (int)WaterSurfaceMode.SimulatedRipples;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                controller.RefreshAuthoringPreview();
                Assert.AreEqual(WaterSurfaceMode.SimulatedRipples, controller.SurfaceMode);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlatStylizedBuildsMinimalStaticTopAndFrontGeometry()
        {
            var root = CreateGameObject("Water25D Flat Geometry Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);

                var topMesh = controller.TopSurface.GetComponent<MeshFilter>().sharedMesh;
                var frontMesh = controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh;
                Assert.AreEqual(4, topMesh.vertexCount);
                Assert.AreEqual(6, topMesh.triangles.Length);
                Assert.AreEqual(4, frontMesh.vertexCount);
                Assert.AreEqual(6, frontMesh.triangles.Length);
                for (var i = 0; i < topMesh.vertexCount; i++)
                {
                    Assert.That(topMesh.vertices[i].y, Is.EqualTo(0f).Within(0.0001f));
                }

                Assert.AreEqual((float)WaterSurfaceMode.FlatStylized, GetSurfaceMode(controller.TopSurface.GetComponent<MeshRenderer>()));
                Assert.AreEqual((float)WaterSurfaceMode.FlatStylized, GetSurfaceMode(controller.FrontSurface.GetComponent<MeshRenderer>()));
                Assert.AreEqual(0f, GetFloat(controller.TopSurface.GetComponent<MeshRenderer>(), "_RippleEnabled"));
                Assert.AreEqual(0.35f, GetFloat(controller.TopSurface.GetComponent<MeshRenderer>(), "_ReflectionStrength"), 0.0001f);
                controller.SetWaterlineLocalY(1.25f);
                Assert.That(controller.FrontSurface.localPosition.y, Is.EqualTo(1.25f).Within(0.0001f));
                Assert.AreSame(frontMesh, controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh);
                Assert.That(frontMesh.vertices[0].y, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(frontMesh.vertices[1].y, Is.EqualTo(0f).Within(0.0001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlatVisualDepthResizeRebuildsOnlyTopGeometry()
        {
            var root = CreateGameObject("Water25D Flat Resize Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                var originalTop = controller.TopSurface.GetComponent<MeshFilter>().sharedMesh;
                var originalFront = controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh;
                controller.SetDimensions(
                    new Vector2(controller.TopSurfaceSize.x, controller.TopSurfaceSize.y + 2f),
                    controller.FrontSurfaceDepth);

                var resizedTop = controller.TopSurface.GetComponent<MeshFilter>().sharedMesh;
                var resizedFront = controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh;
                Assert.AreNotSame(originalTop, resizedTop);
                Assert.AreSame(originalFront, resizedFront);
                Assert.AreEqual(4, resizedTop.vertexCount);
                Assert.That(resizedTop.bounds.max.z, Is.EqualTo(controller.TopSurfaceSize.y).Within(0.0001f));
                Assert.AreEqual(4, resizedFront.vertexCount);

                controller.SetDimensions(
                    new Vector2(controller.TopSurfaceSize.x + 3f, controller.TopSurfaceSize.y),
                    controller.FrontSurfaceDepth);
                var widthTop = controller.TopSurface.GetComponent<MeshFilter>().sharedMesh;
                var widthFront = controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh;
                Assert.AreNotSame(resizedTop, widthTop);
                Assert.AreNotSame(resizedFront, widthFront);
                Assert.That(widthTop.bounds.max.x, Is.EqualTo(controller.TopSurfaceSize.x).Within(0.0001f));
                Assert.That(widthFront.bounds.max.x, Is.EqualTo(controller.TopSurfaceSize.x).Within(0.0001f));

                controller.SetDimensions(controller.TopSurfaceSize, controller.FrontSurfaceDepth + 2f);
                var physicalTop = controller.TopSurface.GetComponent<MeshFilter>().sharedMesh;
                var physicalFront = controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh;
                Assert.AreSame(widthTop, physicalTop);
                Assert.AreNotSame(widthFront, physicalFront);
                Assert.That(physicalFront.bounds.min.y, Is.EqualTo(-controller.FrontSurfaceDepth).Within(0.0001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SwitchingModesReplacesGeneratedMeshesWithoutDuplicatingHierarchy()
        {
            var root = CreateGameObject("Water25D Mode Switch Geometry Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                var flatTop = controller.TopSurface.GetComponent<MeshFilter>().sharedMesh;
                var flatFront = controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh;

                controller.SetSurfaceMode(WaterSurfaceMode.SimulatedRipples);
                var simulatedTop = controller.TopSurface.GetComponent<MeshFilter>().sharedMesh;
                var simulatedFront = controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh;
                var expectedCount = WaterMeshBuilder.CalculateTopVertexCount(
                    controller.TopSurfaceSize,
                    controller.QualityProfile != null
                        ? controller.QualityProfile.GetSettings().TopVerticesPerUnit
                        : WaterQualitySettings.Default.TopVerticesPerUnit);
                Assert.IsTrue(flatTop == null);
                Assert.IsTrue(flatFront == null);
                Assert.AreNotSame(flatTop, simulatedTop);
                Assert.AreNotSame(flatFront, simulatedFront);
                Assert.AreEqual(expectedCount.x * expectedCount.y, simulatedTop.vertexCount);
                Assert.AreEqual(expectedCount.x * 2, simulatedFront.vertexCount);
                Assert.AreEqual((float)WaterSurfaceMode.SimulatedRipples, GetSurfaceMode(controller.TopSurface.GetComponent<MeshRenderer>()));

                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                var restoredTop = controller.TopSurface.GetComponent<MeshFilter>().sharedMesh;
                var restoredFront = controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh;
                Assert.IsTrue(simulatedTop == null);
                Assert.IsTrue(simulatedFront == null);
                Assert.AreEqual(4, restoredTop.vertexCount);
                Assert.AreEqual(6, restoredTop.triangles.Length);
                Assert.AreEqual(4, restoredFront.vertexCount);
                Assert.AreEqual(6, restoredFront.triangles.Length);
                Assert.AreEqual((float)WaterSurfaceMode.FlatStylized, GetSurfaceMode(controller.FrontSurface.GetComponent<MeshRenderer>()));
                Assert.AreEqual(6, root.transform.childCount);
                Assert.IsNotNull(root.transform.Find("TopSurface"));
                Assert.IsNotNull(root.transform.Find("FrontSurface"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CreationMenuCreatesFlatStylizedController()
        {
            var previousSelection = Selection.activeObject;
            GameObject created = null;
            try
            {
                var menuType = typeof(global::Water25D.Editor.Water25DEditor).Assembly.GetType("Water25D.Editor.Water25DMenu");
                Assert.IsNotNull(menuType);
                var menuMethod = menuType.GetMethod("CreateWater", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.IsNotNull(menuMethod);
                var createMethod = menuType.GetMethod(
                    "CreateWaterInScene",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(Scene), typeof(bool) },
                    null);
                Assert.IsNotNull(createMethod);

                created = (GameObject)createMethod.Invoke(null, new object[] { TestScene, false });
                Track(created);
                Assert.IsNotNull(created);
                Assert.AreEqual("Water25D", created.name);
                Assert.AreEqual(WaterSurfaceMode.FlatStylized, created.GetComponent<Water25DController>().SurfaceMode);
            }
            finally
            {
                if (created != null)
                {
                    UnityEngine.Object.DestroyImmediate(created);
                }

                Selection.activeObject = previousSelection;
            }
        }

        [Test]
        public void FlatValidationSceneContainsSerializedBaselineAndFlatSetup()
        {
            var flatPath = FindScenePath("Water25D_VisualFlat");
            Assert.IsNotNull(flatPath);
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(flatPath));

            // Read the saved YAML here instead of opening the scene additively. The duplicated
            // sample intentionally contains a global 2D light, so additive loading would emit a
            // pre-existing Unity scene error unrelated to this package slice.
            var sceneText = System.IO.File.ReadAllText(flatPath);
            StringAssert.Contains("m_Name: Water25D_FlatTest", sceneText);
            StringAssert.Contains("_surfaceMode: 1", sceneText);
            StringAssert.Contains("m_Name: Water25D\n", sceneText);
            StringAssert.Contains("_surfaceMode: 0", sceneText);
            StringAssert.Contains("m_Name: Main Camera", sceneText);
            StringAssert.Contains("m_Name: TestPlayer", sceneText);

            var flatModeIndex = sceneText.IndexOf("_surfaceMode: 1", StringComparison.Ordinal);
            var flatControllerStart = sceneText.LastIndexOf("MonoBehaviour:", flatModeIndex, StringComparison.Ordinal);
            var flatControllerEnd = sceneText.IndexOf("--- !u!4", flatModeIndex, StringComparison.Ordinal);
            Assert.GreaterOrEqual(flatControllerStart, 0);
            Assert.Greater(flatControllerEnd, flatControllerStart);
            var flatControllerText = sceneText.Substring(flatControllerStart, flatControllerEnd - flatControllerStart);
            StringAssert.Contains("_styleProfile: {fileID: 11400000, guid: 35078a570c865084b9cf1edab572e7f1, type: 2}", flatControllerText);
            StringAssert.Contains("_qualityProfile: {fileID: 11400000, guid: 7ae429b46f8b03543a65ea6e619e76c7, type: 2}", flatControllerText);
            StringAssert.Contains("_topMaterialTemplate: {fileID: 2100000, guid: 7a535f86bf2ac5d4a8c6be20c105e27f, type: 2}", flatControllerText);
            StringAssert.Contains("_frontMaterialTemplate: {fileID: 2100000, guid: ac0653dce44e03d4d93782767757ff01, type: 2}", flatControllerText);
            StringAssert.Contains("_rippleSimulationMaterialTemplate: {fileID: 2100000, guid: 3a635641d8057e3428f97764524c4196, type: 2}", flatControllerText);
            StringAssert.DoesNotContain("_topSurface: {fileID: 0}", flatControllerText);
            StringAssert.DoesNotContain("_frontSurface: {fileID: 0}", flatControllerText);
            StringAssert.DoesNotContain("_surfaceCrossingTrigger: {fileID: 0}", flatControllerText);
            StringAssert.DoesNotContain("_buoyancyVolume: {fileID: 0}", flatControllerText);
            StringAssert.DoesNotContain("_reflectionAnchor: {fileID: 0}", flatControllerText);
            StringAssert.DoesNotContain("_fxRoot: {fileID: 0}", flatControllerText);
        }

        private static float GetSurfaceMode(MeshRenderer renderer)
        {
            return GetFloat(renderer, "_SurfaceMode");
        }

        private static float GetFloat(MeshRenderer renderer, string propertyName)
        {
            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            return propertyBlock.GetFloat(Shader.PropertyToID(propertyName));
        }

        private static string FindScenePath(string sceneName)
        {
            var guids = AssetDatabase.FindAssets(sceneName + " t:Scene");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (System.IO.Path.GetFileNameWithoutExtension(path).Equals(sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            return null;
        }
    }
}
