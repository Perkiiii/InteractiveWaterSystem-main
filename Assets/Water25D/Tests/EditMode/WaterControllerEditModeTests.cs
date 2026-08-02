using UnityEditor;
using NUnit.Framework;
using UnityEngine;

namespace Water25D.Tests
{
    public sealed class WaterControllerEditModeTests
    {
        private const string TopMaterialPath = "Assets/Water25D/Materials/Water25D_Top.mat";
        private const string FrontMaterialPath = "Assets/Water25D/Materials/Water25D_Front.mat";
        private const string RippleMaterialPath = "Assets/Water25D/Materials/Water25D_RippleSimulation.mat";
        private const string StyleProfilePath = "Assets/Water25D/Profiles/Water25D_DefaultStyle.asset";
        private const string QualityProfilePath = "Assets/Water25D/Profiles/Water25D_MediumQuality.asset";
        private const string PersistencePrefabPath = "Assets/Water25D/Tests/EditMode/Water25D_PersistenceTest.prefab";

        [Test]
        public void ControllerRepairsNamedHierarchyWithoutRemovingUnrelatedChild()
        {
            var root = new GameObject("Water Test Root");
            try
            {
                var unrelated = new GameObject("Authored Child");
                unrelated.transform.SetParent(root.transform, false);
                var controller = root.AddComponent<Water25DController>();
                controller.RepairHierarchyAndRebuild();

                Assert.IsNotNull(root.transform.Find("TopSurface"));
                Assert.IsNotNull(root.transform.Find("FrontSurface"));
                Assert.IsNotNull(root.transform.Find("SurfaceCrossingTrigger"));
                Assert.IsNotNull(root.transform.Find("BuoyancyVolume"));
                Assert.IsNotNull(root.transform.Find("ReflectionAnchor"));
                Assert.IsNotNull(root.transform.Find("FXRoot"));
                Assert.IsNotNull(root.transform.Find("Authored Child"));
                Assert.IsNotNull(root.transform.Find("TopSurface").GetComponent<MeshFilter>().sharedMesh);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PackageDefaultsHavePersistentSupportedSurfaceShaders()
        {
            var top = AssetDatabase.LoadAssetAtPath<Material>(TopMaterialPath);
            var front = AssetDatabase.LoadAssetAtPath<Material>(FrontMaterialPath);
            var ripple = AssetDatabase.LoadAssetAtPath<Material>(RippleMaterialPath);
            var style = AssetDatabase.LoadAssetAtPath<WaterStyleProfile>(StyleProfilePath);
            var quality = AssetDatabase.LoadAssetAtPath<WaterQualityProfile>(QualityProfilePath);

            Assert.IsNotNull(top);
            Assert.IsNotNull(front);
            Assert.IsNotNull(ripple);
            Assert.IsNotNull(style);
            Assert.IsNotNull(quality);
            Assert.IsNotNull(top.shader);
            Assert.IsNotNull(front.shader);
            Assert.IsNotNull(ripple.shader);
            Assert.IsTrue(top.shader.isSupported);
            Assert.IsTrue(front.shader.isSupported);
            Assert.IsTrue(ripple.shader.isSupported);
            Assert.AreEqual("Water25D/Top Surface", top.shader.name);
            Assert.AreEqual("Water25D/Front Surface", front.shader.name);
            Assert.AreEqual("Water25D/Ripple Simulation", ripple.shader.name);
            Assert.IsTrue(top.HasProperty("_WaterFoamCount"));
            Assert.IsTrue(top.HasProperty("_WaterFoamSoftness"));
            Assert.IsTrue(top.HasProperty("_FoamReflectionOcclusion"));
            Assert.IsTrue(top.HasProperty("_WaterWakeCount"));
            Assert.IsTrue(front.HasProperty("_WaterFoamCount"));
            Assert.IsTrue(front.HasProperty("_WaterFoamSoftness"));
            Assert.IsTrue(front.HasProperty("_WaterWakeCount"));
        }

        [Test]
        public void PersistentMaterialsSurvivePrefabSaveReloadAndAuthoringLifecycle()
        {
            AssetDatabase.DeleteAsset(PersistencePrefabPath);
            var root = new GameObject("Water25D Persistence Test");
            GameObject reloaded = null;
            try
            {
                var controller = root.AddComponent<Water25DController>();
                AssignPackageDefaults(controller);
                controller.RepairHierarchyAndRebuild();

                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PersistencePrefabPath);
                Assert.IsNotNull(prefab);
                Object.DestroyImmediate(root);
                root = null;

                AssetDatabase.ImportAsset(PersistencePrefabPath);
                var persistedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PersistencePrefabPath);
                Assert.IsNotNull(persistedPrefab);
                reloaded = PrefabUtility.InstantiatePrefab(persistedPrefab) as GameObject;
                Assert.IsNotNull(reloaded);

                var reloadedController = reloaded.GetComponent<Water25DController>();
                Assert.IsNotNull(reloadedController);
                AssertPersistentSurface(reloadedController);

                reloadedController.enabled = false;
                reloadedController.enabled = true;
                AssertPersistentSurface(reloadedController);
            }
            finally
            {
                if (reloaded != null)
                {
                    Object.DestroyImmediate(reloaded);
                }

                if (root != null)
                {
                    Object.DestroyImmediate(root);
                }

                AssetDatabase.DeleteAsset(PersistencePrefabPath);
            }
        }

        private static void AssignPackageDefaults(Water25DController controller)
        {
            var serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("_topMaterialTemplate").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(TopMaterialPath);
            serializedObject.FindProperty("_frontMaterialTemplate").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(FrontMaterialPath);
            serializedObject.FindProperty("_rippleSimulationMaterialTemplate").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(RippleMaterialPath);
            serializedObject.FindProperty("_styleProfile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WaterStyleProfile>(StyleProfilePath);
            serializedObject.FindProperty("_qualityProfile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WaterQualityProfile>(QualityProfilePath);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssertPersistentSurface(Water25DController controller)
        {
            var topRenderer = controller.TopSurface.GetComponent<MeshRenderer>();
            var frontRenderer = controller.FrontSurface.GetComponent<MeshRenderer>();
            Assert.IsNotNull(topRenderer.sharedMaterial);
            Assert.IsNotNull(frontRenderer.sharedMaterial);
            Assert.IsNotNull(topRenderer.sharedMaterial.shader);
            Assert.IsNotNull(frontRenderer.sharedMaterial.shader);
            Assert.IsTrue(topRenderer.sharedMaterial.shader.isSupported);
            Assert.IsTrue(frontRenderer.sharedMaterial.shader.isSupported);
            Assert.AreEqual("Water25D/Top Surface", topRenderer.sharedMaterial.shader.name);
            Assert.AreEqual("Water25D/Front Surface", frontRenderer.sharedMaterial.shader.name);
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<Material>(TopMaterialPath), topRenderer.sharedMaterial);
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<Material>(FrontMaterialPath), frontRenderer.sharedMaterial);
            Assert.IsNotNull(controller.StyleProfile);
            Assert.IsNotNull(controller.QualityProfile);
            Assert.IsNotNull(controller.TopSurface.GetComponent<MeshFilter>().sharedMesh);
            Assert.IsNotNull(controller.FrontSurface.GetComponent<MeshFilter>().sharedMesh);
        }
    }
}
