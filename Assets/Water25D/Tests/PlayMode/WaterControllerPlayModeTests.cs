using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Water25D.Rendering;

namespace Water25D.Tests
{
    public sealed class WaterControllerPlayModeTests
    {
        [UnityTest]
        public IEnumerator SimulatedSurfaceImpactResolvesOmittedAndInvalidRadiusOnce()
        {
            var root = new GameObject("Water Radius Regression Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.SimulatedRipples);
                yield return null;

                var position = controller.GetInteractionWorldPosition(new Vector2(10f, 0f));
                Assert.IsTrue(controller.CreateSurfaceImpactAt(position, 0.5f));
                Assert.IsTrue(controller.CreateSurfaceImpactAt(position, 0.5f, true, 0.37f));
                Assert.IsTrue(controller.CreateSurfaceImpactAt(position, 0.5f, true, float.NaN));
                Assert.IsTrue(controller.CreateSurfaceImpactAt(position, 0.5f, true, float.PositiveInfinity));

                var rippleModule = typeof(Water25DController).GetField("_ripple", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);
                var simulator = rippleModule.GetType().GetField("_simulator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(rippleModule);
                var impactQueue = (WaterRippleImpact[])simulator.GetType().GetField("_impactQueue", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(simulator);
                var queueCount = (int)simulator.GetType().GetField("_queueCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(simulator);

                Assert.AreEqual(4, queueCount);
                Assert.AreEqual(WaterQualitySettings.Default.ImpactRadius, impactQueue[0].Radius, 0.0001f);
                Assert.AreEqual(0.37f, impactQueue[1].Radius, 0.0001f);
                Assert.AreEqual(WaterQualitySettings.Default.ImpactRadius, impactQueue[2].Radius, 0.0001f);
                Assert.AreEqual(WaterQualitySettings.Default.ImpactRadius, impactQueue[3].Radius, 0.0001f);
                Assert.Greater(impactQueue[0].Radius, 0.005f);
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        [UnityTest]
        public IEnumerator QualifiedCrossingsAreLogicalBodyKeyedAndUseExactWaterlinePosition()
        {
            var root = new GameObject("Water Qualified Crossing Test");
            var bodyObject = new GameObject("Water Multi Collider Crossing Body");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                yield return null;

                var body = bodyObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                var first = bodyObject.AddComponent<BoxCollider2D>();
                var second = bodyObject.AddComponent<BoxCollider2D>();
                second.offset = new Vector2(1f, 0f);
                body.position = new Vector2(100f, 1f);

                var surface = controller.SurfaceCrossingTrigger.GetComponent<WaterSurfaceInteraction2D>();
                var entered = 0;
                var exited = 0;
                var lastEvent = default(WaterInteractionEvent);
                controller.SurfaceEntered += eventData =>
                {
                    entered++;
                    lastEvent = eventData;
                };
                controller.SurfaceExited += eventData =>
                {
                    exited++;
                    lastEvent = eventData;
                };

                body.position = new Vector2(10f, 1f);
                InvokeTrigger(surface, "OnTriggerEnter2D", first);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(0, entered);

                InvokeTrigger(surface, "OnTriggerEnter2D", second);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(0, entered);

                body.linearVelocity = Vector2.down;
                body.position = new Vector2(10f, 0f);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(1, entered);
                Assert.AreEqual(1, controller.ActiveSurfaceRingCount);
                Assert.AreEqual(controller.WaterlineWorldY, lastEvent.Position.y, 0.0001f);
                Assert.AreEqual(10.5f, lastEvent.Position.x, 0.0001f);
                Assert.AreEqual(1, controller.ActiveContactFoamCount);

                body.linearVelocity = Vector2.zero;
                InvokeFixedUpdate(surface);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(1, entered);
                Assert.AreEqual(1, controller.ActiveContactFoamCount);

                body.linearVelocity = Vector2.down;
                body.position = new Vector2(10f, -1f);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(1, entered);

                body.linearVelocity = Vector2.up;
                body.position = new Vector2(10f, 0f);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(1, exited);
                Assert.AreEqual(2, controller.ActiveSurfaceRingCount);
                Assert.AreEqual(controller.WaterlineWorldY, lastEvent.Position.y, 0.0001f);
            }
            finally
            {
                Object.Destroy(root);
                Object.Destroy(bodyObject);
            }
        }

        [UnityTest]
        public IEnumerator SideEntryAndInitialRestingContactCreateFoamWithoutSyntheticCrossings()
        {
            var root = new GameObject("Water Side Entry Test");
            var bodyObject = new GameObject("Water Side Entry Body");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                yield return null;

                var body = bodyObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                var collider = bodyObject.AddComponent<BoxCollider2D>();
                var surface = controller.SurfaceCrossingTrigger.GetComponent<WaterSurfaceInteraction2D>();
                var entered = 0;
                var exited = 0;
                controller.SurfaceEntered += _ => entered++;
                controller.SurfaceExited += _ => exited++;

                body.position = new Vector2(10f, -1f);
                body.linearVelocity = Vector2.down;
                InvokeTrigger(surface, "OnTriggerEnter2D", collider);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(0, entered);
                Assert.AreEqual(0, exited);

                body.position = new Vector2(10f, 0f);
                body.linearVelocity = Vector2.zero;
                InvokeFixedUpdate(surface);
                InvokeFixedUpdate(surface);
                InvokeFixedUpdate(surface);

                Assert.AreEqual(0, entered);
                Assert.AreEqual(0, exited);
                Assert.AreEqual(0, controller.ActiveSurfaceRingCount);
                Assert.AreEqual(1, controller.ActiveContactFoamCount);

                body.position = new Vector2(11f, 0f);
                body.linearVelocity = Vector2.right;
                InvokeFixedUpdate(surface);
                Assert.AreEqual(0, entered);
                Assert.AreEqual(0, exited);

                body.position = new Vector2(10f, -2f);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(0, entered);
                Assert.GreaterOrEqual(controller.FadingContactFoamCount, 1);
            }
            finally
            {
                Object.Destroy(root);
                Object.Destroy(bodyObject);
            }
        }

        [UnityTest]
        public IEnumerator SimulatedQualifiedCrossingQueuesOneCrtImpactWithoutRingsOrFoam()
        {
            var root = new GameObject("Water Simulated Qualified Crossing Test");
            var bodyObject = new GameObject("Water Simulated Crossing Body");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.SimulatedRipples);
                yield return null;

                var body = bodyObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                var collider = bodyObject.AddComponent<BoxCollider2D>();
                var surface = controller.SurfaceCrossingTrigger.GetComponent<WaterSurfaceInteraction2D>();
                var entered = 0;
                controller.SurfaceEntered += _ => entered++;

                body.position = new Vector2(10f, 1f);
                InvokeTrigger(surface, "OnTriggerEnter2D", collider);
                InvokeFixedUpdate(surface);
                body.linearVelocity = Vector2.down;
                body.position = new Vector2(10f, 0f);
                InvokeFixedUpdate(surface);

                Assert.AreEqual(1, entered);
                Assert.AreEqual(0, controller.ActiveSurfaceRingCount);
                Assert.AreEqual(0, controller.ActiveContactFoamCount);
                Assert.IsNotNull(controller.RippleTexture);
                Assert.AreEqual(1, GetQueuedRippleCount(controller));
            }
            finally
            {
                Object.Destroy(root);
                Object.Destroy(bodyObject);
            }
        }

        [UnityTest]
        public IEnumerator ContactFoamFadesWhenAContactLeavesFullyAboveTheWaterline()
        {
            var root = new GameObject("Water Above Foam Fade Test");
            var bodyObject = new GameObject("Water Above Foam Fade Body");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                yield return null;

                var body = bodyObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                var collider = bodyObject.AddComponent<BoxCollider2D>();
                var surface = controller.SurfaceCrossingTrigger.GetComponent<WaterSurfaceInteraction2D>();
                body.position = new Vector2(10f, 0f);
                InvokeTrigger(surface, "OnTriggerEnter2D", collider);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(1, controller.ActiveContactFoamCount);

                body.linearVelocity = Vector2.up;
                body.position = new Vector2(10f, 2f);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(0, controller.ActiveContactFoamCount);
                Assert.GreaterOrEqual(controller.FadingContactFoamCount, 1);
            }
            finally
            {
                Object.Destroy(root);
                Object.Destroy(bodyObject);
            }
        }

        [UnityTest]
        public IEnumerator CollidersLeavingOnDifferentStepsDoNotEmitSurfaceExit()
        {
            var root = new GameObject("Water Staggered Exit Test");
            var bodyObject = new GameObject("Water Staggered Exit Body");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                yield return null;

                var body = bodyObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                var first = bodyObject.AddComponent<BoxCollider2D>();
                var second = bodyObject.AddComponent<BoxCollider2D>();
                second.offset = Vector2.right;
                var surface = controller.SurfaceCrossingTrigger.GetComponent<WaterSurfaceInteraction2D>();
                var exited = 0;
                controller.SurfaceExited += _ => exited++;

                body.position = new Vector2(10f, 0f);
                InvokeTrigger(surface, "OnTriggerEnter2D", first);
                InvokeTrigger(surface, "OnTriggerEnter2D", second);
                InvokeFixedUpdate(surface);
                InvokeTrigger(surface, "OnTriggerExit2D", first);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(1, controller.TrackedSurfaceBodyCount);
                Assert.AreEqual(0, exited);

                InvokeTrigger(surface, "OnTriggerExit2D", second);
                InvokeFixedUpdate(surface);
                Assert.AreEqual(0, controller.TrackedSurfaceBodyCount);
                Assert.AreEqual(0, exited);
                Assert.GreaterOrEqual(controller.FadingContactFoamCount, 1);
            }
            finally
            {
                Object.Destroy(root);
                Object.Destroy(bodyObject);
            }
        }

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
                Assert.IsTrue(controller.CreateContactRippleAt(interactionPosition, 0.5f, true, 0.22f));
                Assert.AreEqual(1, controller.ActiveSurfaceRingCount);
                Assert.AreEqual(0, controller.ReplacedSurfaceRingCount);
                Assert.IsNull(controller.RippleTexture);
                controller.SetDimensions(new Vector2(10f, 6.5f), controller.FrontSurfaceDepth);
                Assert.AreEqual(0, controller.ActiveSurfaceRingCount);
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

        [UnityTest]
        public IEnumerator SurfaceImpactRoutingAndPresentationLifecycleRemainModeSpecific()
        {
            var root = new GameObject("Water Surface Presentation PlayMode Test");
            try
            {
                var controller = root.AddComponent<Water25DController>();
                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                yield return null;

                var topRenderer = controller.TopSurface.GetComponent<MeshRenderer>();
                var frontRenderer = controller.FrontSurface.GetComponent<MeshRenderer>();
                var topBlock = new MaterialPropertyBlock();
                topRenderer.GetPropertyBlock(topBlock);
                var reflectionMatrix = Matrix4x4.TRS(new Vector3(2f, 3f, 4f), Quaternion.Euler(5f, 10f, 15f), Vector3.one);
                topBlock.SetFloat(Shader.PropertyToID("_ReflectionEnabled"), 1f);
                topBlock.SetFloat(Shader.PropertyToID("_ReflectionStrength"), 0.9f);
                topBlock.SetMatrix(Shader.PropertyToID("_ReflectionViewProjection"), reflectionMatrix);
                topRenderer.SetPropertyBlock(topBlock);

                var center = controller.transform.TransformPoint(new Vector3(10f, controller.WaterlineLocalY, 3.25f));
                Assert.IsTrue(controller.CreateSurfaceImpactAt(center, 0.75f, true));
                Assert.AreEqual(1, controller.ActiveSurfaceRingCount);
                Assert.IsNull(controller.RippleTexture);
                Assert.AreEqual(1f, GetFloat(topRenderer, "_WaterRingCount"));
                Assert.AreEqual(1f, GetFloat(frontRenderer, "_WaterRingCount"));
                Assert.AreEqual(1f, GetFloat(topRenderer, "_SurfaceMode"));
                Assert.AreEqual(0.9f, GetFloat(topRenderer, "_ReflectionStrength"), 0.0001f);
                Assert.That(GetMatrix(topRenderer, "_ReflectionViewProjection"), Is.EqualTo(reflectionMatrix));

                controller.UpdateSurfaceContactFoam(42, new Vector2(10f, controller.WaterlineWorldY), 1f, 0.5f, 1f);
                Assert.AreEqual(1f, GetFloat(topRenderer, "_WaterFoamCount"), 0.0001f);
                Assert.AreEqual(1f, GetFloat(frontRenderer, "_WaterFoamCount"), 0.0001f);
                Assert.AreEqual(10f, GetVectorArray(topRenderer, "_WaterFoamsA")[0].x, 0.0001f);
                Assert.AreEqual(GetVectorArray(topRenderer, "_WaterFoamsA")[0], GetVectorArray(frontRenderer, "_WaterFoamsA")[0]);
                Assert.AreEqual(0.9f, GetFloat(topRenderer, "_ReflectionStrength"), 0.0001f);
                Assert.That(GetMatrix(topRenderer, "_ReflectionViewProjection"), Is.EqualTo(reflectionMatrix));

                var topRingData = GetVectorArray(topRenderer, "_WaterRingsA");
                var frontRingData = GetVectorArray(frontRenderer, "_WaterRingsA");
                Assert.LessOrEqual(topRingData.Length, 16);
                Assert.LessOrEqual(frontRingData.Length, 16);
                Assert.AreEqual(topRingData[0], frontRingData[0]);

                Assert.IsFalse(controller.CreateSurfaceImpactAt(root.transform.TransformPoint(new Vector3(-0.1f, 0f, 2f)), 0.5f));
                Assert.AreEqual(1, controller.ActiveSurfaceRingCount);
                Assert.AreEqual(6, root.transform.childCount);

                controller.SetSurfaceMode(WaterSurfaceMode.SimulatedRipples);
                yield return null;
                Assert.AreEqual(0, controller.ActiveSurfaceRingCount);
                Assert.AreEqual(0f, GetFloat(topRenderer, "_WaterFoamCount"), 0.0001f);
                Assert.AreEqual(0f, GetFloat(frontRenderer, "_WaterFoamCount"), 0.0001f);
                Assert.IsNotNull(controller.RippleTexture);
                Assert.IsTrue(controller.CreateSurfaceImpactAt(center, 0.5f, true));
                Assert.AreEqual(0, controller.ActiveSurfaceRingCount);

                controller.SetSurfaceMode(WaterSurfaceMode.FlatStylized);
                yield return null;
                Assert.AreEqual(0, controller.ActiveSurfaceRingCount);
                Assert.IsNull(controller.RippleTexture);
                Assert.IsTrue(controller.CreateSurfaceImpactAt(center, 0.5f, true));
                controller.enabled = false;
                Assert.AreEqual(0, controller.ActiveSurfaceRingCount);
                controller.enabled = true;
                yield return null;
                Assert.AreEqual(0, controller.ActiveSurfaceRingCount);
                Assert.IsNull(controller.RippleTexture);
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

        private static float GetFloat(MeshRenderer renderer, string propertyName)
        {
            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            return propertyBlock.GetFloat(Shader.PropertyToID(propertyName));
        }

        private static Matrix4x4 GetMatrix(MeshRenderer renderer, string propertyName)
        {
            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            return propertyBlock.GetMatrix(Shader.PropertyToID(propertyName));
        }

        private static Vector4[] GetVectorArray(MeshRenderer renderer, string propertyName)
        {
            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            return propertyBlock.GetVectorArray(Shader.PropertyToID(propertyName));
        }

        private static int GetQueuedRippleCount(Water25DController controller)
        {
            var rippleModule = typeof(Water25DController).GetField("_ripple", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);
            var simulator = rippleModule.GetType().GetField("_simulator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(rippleModule);
            return (int)simulator.GetType().GetField("_queueCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(simulator);
        }

        private static void InvokeTrigger(WaterSurfaceInteraction2D surface, string methodName, Collider2D collider)
        {
            typeof(WaterSurfaceInteraction2D)
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(surface, new object[] { collider });
        }

        private static void InvokeFixedUpdate(WaterSurfaceInteraction2D surface)
        {
            typeof(WaterSurfaceInteraction2D)
                .GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(surface, null);
        }
    }
}
