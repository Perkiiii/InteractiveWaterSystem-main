using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Water25D.Rendering;

namespace Water25D.Tests
{
    public sealed class WaterControllerPlayModeTests
    {
        [UnityTest]
        public IEnumerator ControllerCreatesRuntimeStateAndAcceptsImpact()
        {
            var root = new GameObject("Water PlayMode Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.SimulatedRipples);
                yield return null;

                Assert.IsNotNull(controller.TopSurface);
                Assert.IsNotNull(controller.FrontSurface);
                Assert.IsNotNull(controller.TopSurface.GetComponent<MeshFilter>().sharedMesh);
                Assert.IsNotNull(controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh);
                Assert.IsNotNull(controller.TopSurface.GetComponent<MeshRenderer>().sharedMaterial);
                Assert.IsNotNull(controller.FrontSurface.GetComponent<MeshRenderer>().sharedMaterial);
                Assert.AreEqual("Water25D/Top Surface", controller.TopSurface.GetComponent<MeshRenderer>().sharedMaterial.shader.name);
                Assert.AreEqual("Water25D/Front Surface", controller.FrontSurface.GetComponent<MeshRenderer>().sharedMaterial.shader.name);
                var simulatedCount = WaterMeshBuilder.CalculateTopVertexCount(controller.TopSurfaceSize, WaterQualitySettings.Default.TopVerticesPerUnit);
                Assert.AreEqual(simulatedCount.x * simulatedCount.y, controller.TopSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                Assert.AreEqual(simulatedCount.x * 2, controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                Assert.AreEqual(0f, GetSurfaceMode(controller.TopSurface.GetComponent<MeshRenderer>()));
                Assert.AreEqual(0f, GetSurfaceMode(controller.FrontSurface.GetComponent<MeshRenderer>()));
                Assert.IsNotNull(controller.RippleTexture);
                var initialTexture = (CustomRenderTexture)controller.RippleTexture;
                Assert.AreEqual(320, initialTexture.width);
                Assert.AreEqual(104, initialTexture.height);

                var ripplePosition = controller.GetInteractionWorldPosition(new Vector2(10f, 0f));
                Assert.IsTrue(controller.CreateContactRippleAt(ripplePosition, 0.5f, true));
                yield return null;
                Assert.AreEqual(0, controller.DroppedRippleImpactCount);

                controller.SetDimensions(new Vector2(10f, 6.5f), controller.FrontSurfaceDepth);
                yield return null;
                var resizedTexture = (CustomRenderTexture)controller.RippleTexture;
                Assert.AreEqual(160, resizedTexture.width);
                Assert.AreEqual(104, resizedTexture.height);
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        [UnityTest]
        public IEnumerator FlatStylizedDoesNotCreateRippleResourcesAndCanSwitchModes()
        {
            var root = new GameObject("Water Flat PlayMode Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                yield return null;

                Assert.AreEqual(WaterSurfaceMode.FlatStylized, controller.SurfaceMode);
                Assert.IsNull(controller.RippleTexture);
                Assert.IsFalse(controller.RippleSimulationAvailable);
                Assert.AreEqual(4, controller.TopSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                Assert.AreEqual(6, controller.TopSurface.GetComponent<MeshFilter>().sharedMesh.triangles.Length);
                Assert.AreEqual(4, controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                Assert.AreEqual(6, controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh.triangles.Length);
                Assert.AreEqual(1f, GetSurfaceMode(controller.TopSurface.GetComponent<MeshRenderer>()));
                Assert.AreEqual(1f, GetSurfaceMode(controller.FrontSurface.GetComponent<MeshRenderer>()));

                var interactionPosition = controller.GetInteractionWorldPosition(new Vector2(10f, 0f));
                Assert.IsFalse(controller.CreateContactRippleAt(interactionPosition, 0.5f, true, 0.22f));
                controller.SetDimensions(new Vector2(10f, 6.5f), controller.FrontSurfaceDepth);
                yield return null;
                Assert.IsNull(controller.RippleTexture);
                Assert.AreEqual(4, controller.TopSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                Assert.AreEqual(4, controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);

                controller.SetSurfaceMode(WaterSurfaceMode.SimulatedRipples);
                yield return null;
                Assert.IsNotNull(controller.RippleTexture);
                var simulatedCount = WaterMeshBuilder.CalculateTopVertexCount(new Vector2(10f, 6.5f), WaterQualitySettings.Default.TopVerticesPerUnit);
                Assert.AreEqual(simulatedCount.x * simulatedCount.y, controller.TopSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                Assert.AreEqual(simulatedCount.x * 2, controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                Assert.AreEqual(0f, GetSurfaceMode(controller.TopSurface.GetComponent<MeshRenderer>()));
                Assert.AreEqual(0f, GetSurfaceMode(controller.FrontSurface.GetComponent<MeshRenderer>()));
                Assert.IsTrue(controller.CreateContactRippleAt(interactionPosition, 0.5f, true, 0.22f));

                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                Assert.AreEqual(WaterSurfaceMode.FlatStylized, controller.SurfaceMode);
                Assert.IsNull(controller.RippleTexture);
                yield return null;
                Assert.IsNull(controller.RippleTexture);
                Assert.AreEqual(4, controller.TopSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                Assert.AreEqual(4, controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                Assert.AreEqual(1f, GetSurfaceMode(controller.TopSurface.GetComponent<MeshRenderer>()));
                Assert.AreEqual(1f, GetSurfaceMode(controller.FrontSurface.GetComponent<MeshRenderer>()));
                Assert.AreEqual(6, root.transform.childCount);
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        private static float GetSurfaceMode(MeshRenderer renderer)
        {
            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            return propertyBlock.GetFloat(Shader.PropertyToID("_SurfaceMode"));
        }
    }
}
