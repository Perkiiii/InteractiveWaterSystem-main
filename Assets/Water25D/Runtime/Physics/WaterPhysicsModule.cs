using UnityEngine;

namespace Water25D
{
    /// <summary>
    /// Applies authored values to the independent crossing and buoyancy volumes.
    /// It never samples or owns the visual GPU height field.
    /// </summary>
    internal sealed class WaterPhysicsModule
    {
        public void Apply(
            Water25DController controller,
            WaterHierarchyModule hierarchy,
            Vector2 topSurfaceSize,
            float frontSurfaceDepth,
            float waterlineLocalY,
            float surfaceTriggerThickness,
            bool enableSurfaceInteraction,
            bool enableBuoyancy,
            LayerMask surfaceInteractionLayers,
            LayerMask surfaceTriggerInteractionLayers,
            LayerMask buoyancyLayers,
            bool includeTriggerCollidersInSurfaceInteraction,
            float buoyancyDensity,
            float buoyancyLinearDamping,
            float buoyancyAngularDamping,
            bool enableCustomDrag,
            float customLinearDrag,
            float customAngularDrag,
            int maximumTrackedBodies)
        {
            var width = Mathf.Max(0.01f, topSurfaceSize.x);
            var depth = Mathf.Max(0.01f, frontSurfaceDepth);
            var triggerThickness = Mathf.Max(0.01f, surfaceTriggerThickness);

            hierarchy.TopSurface.localPosition = new Vector3(0f, waterlineLocalY, 0f);
            hierarchy.FrontSurface.localPosition = new Vector3(0f, waterlineLocalY, 0f);
            hierarchy.SurfaceCrossingTrigger.localPosition = new Vector3(width * 0.5f, waterlineLocalY, 0f);
            hierarchy.BuoyancyVolume.localPosition = new Vector3(width * 0.5f, waterlineLocalY, 0f);
            hierarchy.ReflectionAnchor.localPosition = new Vector3(width * 0.5f, waterlineLocalY, 0f);
            hierarchy.FxRoot.localPosition = new Vector3(width * 0.5f, waterlineLocalY, 0f);

            hierarchy.SurfaceCollider.size = new Vector2(width, triggerThickness);
            hierarchy.SurfaceCollider.offset = Vector2.zero;
            hierarchy.SurfaceCollider.isTrigger = true;
            hierarchy.SurfaceCollider.enabled = enableSurfaceInteraction;
            hierarchy.SurfaceInteraction.Configure(
                controller,
                surfaceInteractionLayers,
                surfaceTriggerInteractionLayers,
                includeTriggerCollidersInSurfaceInteraction,
                maximumTrackedBodies,
                controller.SurfaceCrossingEpsilon);
            hierarchy.SurfaceInteraction.enabled = enableSurfaceInteraction;

            hierarchy.BuoyancyCollider.size = new Vector2(width, depth);
            hierarchy.BuoyancyCollider.offset = new Vector2(0f, -depth * 0.5f);
            hierarchy.BuoyancyCollider.isTrigger = true;
            hierarchy.BuoyancyCollider.enabled = enableBuoyancy;
            hierarchy.PhysicsVolume.Configure(
                controller,
                buoyancyLayers,
                enableCustomDrag,
                customLinearDrag,
                customAngularDrag,
                maximumTrackedBodies);
            hierarchy.PhysicsVolume.enabled = enableBuoyancy;

            if (enableBuoyancy && hierarchy.BuoyancyEffector == null)
            {
                hierarchy.BuoyancyEffector = hierarchy.BuoyancyVolume.gameObject.AddComponent<BuoyancyEffector2D>();
            }

            if (hierarchy.BuoyancyEffector != null)
            {
                hierarchy.BuoyancyEffector.enabled = enableBuoyancy;
                hierarchy.BuoyancyEffector.surfaceLevel = 0f;
                hierarchy.BuoyancyEffector.density = Mathf.Max(0f, buoyancyDensity);
                hierarchy.BuoyancyEffector.linearDamping = Mathf.Max(0f, buoyancyLinearDamping);
                hierarchy.BuoyancyEffector.angularDamping = Mathf.Max(0f, buoyancyAngularDamping);
                hierarchy.BuoyancyEffector.flowMagnitude = 0f;
                hierarchy.BuoyancyEffector.useColliderMask = true;
                hierarchy.BuoyancyEffector.colliderMask = buoyancyLayers.value;
            }

            hierarchy.BuoyancyCollider.usedByEffector = enableBuoyancy && hierarchy.BuoyancyEffector != null;
        }
    }
}
