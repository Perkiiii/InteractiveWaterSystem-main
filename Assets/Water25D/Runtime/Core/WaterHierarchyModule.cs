using UnityEngine;
using UnityEngine.Rendering;
using Water25D.FX;

namespace Water25D
{
    /// <summary>
    /// Owns deterministic discovery and component wiring for one water hierarchy.
    /// It does not configure geometry, materials, physics values, or runtime resources.
    /// </summary>
    internal sealed class WaterHierarchyModule
    {
        private const string TopSurfaceName = "TopSurface";
        private const string FrontSurfaceName = "FrontSurface";
        private const string SurfaceCrossingTriggerName = "SurfaceCrossingTrigger";
        private const string BuoyancyVolumeName = "BuoyancyVolume";
        private const string ReflectionAnchorName = "ReflectionAnchor";
        private const string FxRootName = "FXRoot";

        public Transform TopSurface { get; private set; }
        public Transform FrontSurface { get; private set; }
        public Transform SurfaceCrossingTrigger { get; private set; }
        public Transform BuoyancyVolume { get; private set; }
        public Transform ReflectionAnchor { get; private set; }
        public Transform FxRoot { get; private set; }

        public MeshFilter TopMeshFilter { get; private set; }
        public MeshRenderer TopMeshRenderer { get; private set; }
        public SortingGroup TopSortingGroup { get; private set; }
        public MeshFilter FrontMeshFilter { get; private set; }
        public MeshRenderer FrontMeshRenderer { get; private set; }
        public SortingGroup FrontSortingGroup { get; private set; }
        public BoxCollider2D SurfaceCollider { get; private set; }
        public WaterSurfaceInteraction2D SurfaceInteraction { get; private set; }
        public BoxCollider2D BuoyancyCollider { get; private set; }
        public BuoyancyEffector2D BuoyancyEffector { get; internal set; }
        public WaterPhysicsVolume2D PhysicsVolume { get; private set; }
        public WaterFXController FxController { get; private set; }

        public void Initialise(
            Transform root,
            Transform serializedTopSurface,
            Transform serializedFrontSurface,
            Transform serializedSurfaceCrossingTrigger,
            Transform serializedBuoyancyVolume,
            Transform serializedReflectionAnchor,
            Transform serializedFxRoot,
            bool synchronizeGeneratedChildLayers)
        {
            TopSurface = EnsureChild(root, serializedTopSurface, TopSurfaceName);
            FrontSurface = EnsureChild(root, serializedFrontSurface, FrontSurfaceName);
            SurfaceCrossingTrigger = EnsureChild(root, serializedSurfaceCrossingTrigger, SurfaceCrossingTriggerName);
            BuoyancyVolume = EnsureChild(root, serializedBuoyancyVolume, BuoyancyVolumeName);
            ReflectionAnchor = EnsureChild(root, serializedReflectionAnchor, ReflectionAnchorName);
            FxRoot = EnsureChild(root, serializedFxRoot, FxRootName);

            TopMeshFilter = GetOrAddComponent<MeshFilter>(TopSurface.gameObject);
            TopMeshRenderer = GetOrAddComponent<MeshRenderer>(TopSurface.gameObject);
            TopSortingGroup = GetOrAddComponent<SortingGroup>(TopSurface.gameObject);
            FrontMeshFilter = GetOrAddComponent<MeshFilter>(FrontSurface.gameObject);
            FrontMeshRenderer = GetOrAddComponent<MeshRenderer>(FrontSurface.gameObject);
            FrontSortingGroup = GetOrAddComponent<SortingGroup>(FrontSurface.gameObject);

            SurfaceCollider = GetOrAddComponent<BoxCollider2D>(SurfaceCrossingTrigger.gameObject);
            SurfaceInteraction = GetOrAddComponent<WaterSurfaceInteraction2D>(SurfaceCrossingTrigger.gameObject);
            BuoyancyCollider = GetOrAddComponent<BoxCollider2D>(BuoyancyVolume.gameObject);
            PhysicsVolume = GetOrAddComponent<WaterPhysicsVolume2D>(BuoyancyVolume.gameObject);
            FxController = GetOrAddComponent<WaterFXController>(FxRoot.gameObject);
            BuoyancyVolume.gameObject.TryGetComponent(out BuoyancyEffector2D buoyancyEffector);
            BuoyancyEffector = buoyancyEffector;

            if (!synchronizeGeneratedChildLayers)
            {
                return;
            }

            var layer = root.gameObject.layer;
            TopSurface.gameObject.layer = layer;
            FrontSurface.gameObject.layer = layer;
            SurfaceCrossingTrigger.gameObject.layer = layer;
            BuoyancyVolume.gameObject.layer = layer;
            ReflectionAnchor.gameObject.layer = layer;
            FxRoot.gameObject.layer = layer;
        }

        private static Transform EnsureChild(Transform root, Transform serializedChild, string childName)
        {
            if (serializedChild != null && serializedChild.parent == root)
            {
                return serializedChild;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }

            var childObject = new GameObject(childName);
            childObject.transform.SetParent(root, false);
            return childObject.transform;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }
    }
}
