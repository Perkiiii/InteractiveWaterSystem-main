using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
                yield return null;

                Assert.IsNotNull(controller.TopSurface);
                Assert.IsNotNull(controller.FrontSurface);
                Assert.IsNotNull(controller.TopSurface.GetComponent<MeshFilter>().sharedMesh);
                Assert.IsNotNull(controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh);
                Assert.IsNotNull(controller.TopSurface.GetComponent<MeshRenderer>().sharedMaterial);
                Assert.IsNotNull(controller.FrontSurface.GetComponent<MeshRenderer>().sharedMaterial);
                Assert.AreEqual("Water25D/Top Surface", controller.TopSurface.GetComponent<MeshRenderer>().sharedMaterial.shader.name);
                Assert.AreEqual("Water25D/Front Surface", controller.FrontSurface.GetComponent<MeshRenderer>().sharedMaterial.shader.name);
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
    }
}
