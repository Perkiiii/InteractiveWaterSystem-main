using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Water25D.Editor;
using Water25D.Rendering;

namespace Water25D.Tests
{
    public sealed class Water25DEditorTests
    {
        private const string TopMaterialPath = "Assets/Water25D/Materials/Water25D_Top.mat";
        private const string FrontMaterialPath = "Assets/Water25D/Materials/Water25D_Front.mat";
        private const string RippleMaterialPath = "Assets/Water25D/Materials/Water25D_RippleSimulation.mat";
        private const string StyleProfilePath = "Assets/Water25D/Profiles/Water25D_DefaultStyle.asset";
        private const string QualityProfilePath = "Assets/Water25D/Profiles/Water25D_MediumQuality.asset";

        [Test]
        public void ValidationReportsMissingTopMaterial()
        {
            var root = CreateController();
            try
            {
                SetReference(root, "_topMaterialTemplate", null);
                SetReference(root, "_styleProfile", null);
                root.TopSurface.GetComponent<MeshRenderer>().sharedMaterial = null;
                Assert.IsTrue(HasResult(root, "Top material missing"));
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void ValidationReportsMissingFrontMaterial()
        {
            var root = CreateController();
            try
            {
                SetReference(root, "_frontMaterialTemplate", null);
                SetReference(root, "_styleProfile", null);
                root.FrontSurface.GetComponent<MeshRenderer>().sharedMaterial = null;
                Assert.IsTrue(HasResult(root, "Front material missing"));
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void ValidationReportsMissingStyleProfile()
        {
            var root = CreateController();
            try
            {
                SetReference(root, "_styleProfile", null);
                Assert.IsTrue(HasResult(root, "Style profile missing"));
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void ValidationReportsMissingQualityProfile()
        {
            var root = CreateController();
            try
            {
                SetReference(root, "_qualityProfile", null);
                Assert.IsTrue(HasResult(root, "Quality profile missing"));
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void ValidationReportsUnexpectedShader()
        {
            var root = CreateController();
            var material = new Material(Shader.Find("Sprites/Default"));
            try
            {
                SetReference(root, "_topMaterialTemplate", material);
                Assert.IsTrue(HasResult(root, "Top shader differs from the package shader"));
            }
            finally
            {
                Object.DestroyImmediate(material);
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void PackageDefaultsHaveNoValidationErrors()
        {
            var root = CreateController();
            try
            {
                var results = Water25DValidation.Validate(root);
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.AreNotEqual(Water25DValidationSeverity.Error, results[i].Severity, results[i].Title + ": " + results[i].Message);
                }
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void SafeDefaultsDoNotReplaceValidUserAssets()
        {
            var root = CreateController();
            var customTop = new Material(Shader.Find("Water25D/Top Surface"));
            try
            {
                SetReference(root, "_topMaterialTemplate", customTop);
                Water25DInspectorUtility.FixSafeDefaults(root);
                var serializedObject = new SerializedObject(root);
                Assert.AreSame(customTop, serializedObject.FindProperty("_topMaterialTemplate").objectReferenceValue);
                Assert.IsNotNull(serializedObject.FindProperty("_frontMaterialTemplate").objectReferenceValue);
                Assert.IsNotNull(serializedObject.FindProperty("_styleProfile").objectReferenceValue);
                Assert.IsNotNull(serializedObject.FindProperty("_qualityProfile").objectReferenceValue);
            }
            finally
            {
                Object.DestroyImmediate(customTop);
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void ProfileDuplicationCreatesDistinctAssetAndDoesNotModifyDefault()
        {
            var defaultProfile = AssetDatabase.LoadAssetAtPath<WaterStyleProfile>(StyleProfilePath);
            Assert.IsNotNull(defaultProfile);
            var duplicate = Water25DInspectorUtility.DuplicateProfileAsset(defaultProfile, "Water25D Editor Test", "StyleTest");
            try
            {
                Assert.IsNotNull(duplicate);
                Assert.AreNotSame(defaultProfile, duplicate);
                Assert.AreNotEqual(AssetDatabase.GetAssetPath(defaultProfile), AssetDatabase.GetAssetPath(duplicate));
                Assert.AreEqual(defaultProfile.GetSettings(), duplicate.GetSettings());
            }
            finally
            {
                if (duplicate != null)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(duplicate));
                }
            }
        }

        [Test]
        public void CalculatedMetricsMatchQualityProfileAndGeometryBuilder()
        {
            var root = CreateController();
            try
            {
                var metrics = Water25DInspectorUtility.CalculateMetrics(root);
                var settings = root.QualityProfile.GetSettings();
                Assert.AreEqual(settings.CalculateRippleResolution(root.TopSurfaceSize), metrics.RippleResolution);
                Assert.AreEqual(WaterMeshBuilder.CalculateTopVertexCount(root.TopSurfaceSize, settings.TopVerticesPerUnit), metrics.TopVertexCount);
                Assert.Greater(metrics.RippleStateBytes, 0);
                Assert.Greater(metrics.PropagatedCellsPerSecond, 0d);
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void ReflectionValidationRequiresCameraOnlyInPlanarMode()
        {
            var root = CreateController();
            try
            {
                SetEnum(root, "_reflectionMode", WaterReflectionMode.Stylized);
                Assert.IsFalse(HasResult(root, "Planar reflection camera missing"));
                SetEnum(root, "_reflectionMode", WaterReflectionMode.Planar);
                Assert.IsTrue(HasResult(root, "Planar reflection camera missing"));
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void SurfaceTriggerValidationChecksTriggerState()
        {
            var root = CreateController();
            try
            {
                root.SurfaceCrossingTrigger.GetComponent<BoxCollider2D>().isTrigger = false;
                Assert.IsTrue(HasResult(root, "Surface collider is not a trigger"));
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void BuoyancyValidationChecksEffectorRelationship()
        {
            var root = CreateController();
            try
            {
                root.BuoyancyVolume.GetComponent<BoxCollider2D>().usedByEffector = false;
                Assert.IsTrue(HasResult(root, "Buoyancy collider is not linked"));
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void PreviewRefreshKeepsPersistentMaterials()
        {
            var root = CreateController();
            try
            {
                var top = AssetDatabase.LoadAssetAtPath<Material>(TopMaterialPath);
                var front = AssetDatabase.LoadAssetAtPath<Material>(FrontMaterialPath);
                root.RefreshAuthoringPreview();
                Assert.AreSame(top, root.TopSurface.GetComponent<MeshRenderer>().sharedMaterial);
                Assert.AreSame(front, root.FrontSurface.GetComponent<MeshRenderer>().sharedMaterial);
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void QualityProfileMetricsUsePositiveFiniteMemoryEstimate()
        {
            var root = CreateController();
            try
            {
                var metrics = Water25DInspectorUtility.CalculateMetrics(root);
                Assert.Greater(metrics.RippleStateBytes, 0);
                Assert.IsFalse(double.IsNaN(metrics.PropagatedCellsPerSecond));
                Assert.IsFalse(double.IsInfinity(metrics.PropagatedCellsPerSecond));
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }

        private static Water25DController CreateController()
        {
            var root = new GameObject("Water25D Editor Test");
            var controller = root.AddComponent<Water25DController>();
            AssignPackageDefaults(controller);
            controller.RepairHierarchyAndRebuild();
            return controller;
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

        private static void SetReference(Water25DController controller, string propertyPath, Object value)
        {
            var serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty(propertyPath).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(Water25DController controller, string propertyPath, WaterReflectionMode value)
        {
            var serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty(propertyPath).enumValueIndex = (int)value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool HasResult(Water25DController controller, string title)
        {
            var results = Water25DValidation.Validate(controller);
            for (var i = 0; i < results.Count; i++)
            {
                if (results[i].Title == title)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
