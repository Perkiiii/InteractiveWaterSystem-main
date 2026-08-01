using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using Water25D.FX;

namespace Water25D.Editor
{
    public enum Water25DMaterialStatus
    {
        Missing,
        Valid,
        Unsupported,
        Unexpected
    }

    public struct Water25DPerformanceMetrics
    {
        public Vector2Int TopVertexCount;
        public int TopTriangleCount;
        public int FrontVertexCount;
        public int FrontTriangleCount;
        public Vector2Int RippleResolution;
        public long RippleStateBytes;
        public bool UsesRgHalf;
        public double PropagatedCellsPerSecond;
        public int MaximumImpactsPerStep;
        public int MaximumQueuedImpacts;
        public float SimulationFrequency;
        public int PropagationSubsteps;
        public float TopVerticesPerUnit;
    }

    public static class Water25DInspectorUtility
    {
        internal const string ProfilesFolder = "Assets/Water25D/Profiles";
        internal const string StyleProfilePath = ProfilesFolder + "/Water25D_DefaultStyle.asset";
        internal const string QualityProfilePath = ProfilesFolder + "/Water25D_MediumQuality.asset";
        internal const string TopMaterialPath = "Assets/Water25D/Materials/Water25D_Top.mat";
        internal const string FrontMaterialPath = "Assets/Water25D/Materials/Water25D_Front.mat";
        internal const string RippleMaterialPath = "Assets/Water25D/Materials/Water25D_RippleSimulation.mat";
        internal const string TopShaderName = "Water25D/Top Surface";
        internal const string FrontShaderName = "Water25D/Front Surface";
        internal const string RippleShaderName = "Water25D/Ripple Simulation";

        public static T LoadPackageAsset<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static bool IsPackageAsset(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return false;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            return path.StartsWith("Assets/Water25D/", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPackageDefaultStyle(WaterStyleProfile profile)
        {
            return profile != null && AssetDatabase.GetAssetPath(profile) == StyleProfilePath;
        }

        public static bool IsPackageDefaultQuality(WaterQualityProfile profile)
        {
            return profile != null && AssetDatabase.GetAssetPath(profile) == QualityProfilePath;
        }

        public static Water25DMaterialStatus GetMaterialStatus(Material material, string expectedShader)
        {
            if (material == null || material.shader == null)
            {
                return Water25DMaterialStatus.Missing;
            }

            if (!material.shader.isSupported)
            {
                return Water25DMaterialStatus.Unsupported;
            }

            return material.shader.name == expectedShader
                ? Water25DMaterialStatus.Valid
                : Water25DMaterialStatus.Unexpected;
        }

        public static string GetMaterialStatusText(Water25DMaterialStatus status)
        {
            switch (status)
            {
                case Water25DMaterialStatus.Valid:
                    return "Valid";
                case Water25DMaterialStatus.Unsupported:
                    return "Unsupported";
                case Water25DMaterialStatus.Unexpected:
                    return "Unexpected shader";
                default:
                    return "Missing";
            }
        }

        public static Color GetMaterialStatusColor(Water25DMaterialStatus status)
        {
            switch (status)
            {
                case Water25DMaterialStatus.Valid:
                    return Water25DInspectorStyles.ValidColor;
                case Water25DMaterialStatus.Unsupported:
                case Water25DMaterialStatus.Unexpected:
                    return Water25DInspectorStyles.WarningColor;
                default:
                    return Water25DInspectorStyles.ErrorColor;
            }
        }

        public static bool AssignObjectReference(
            SerializedObject serializedObject,
            string propertyPath,
            UnityEngine.Object value,
            string undoName,
            bool markSceneDirty = true)
        {
            if (serializedObject == null)
            {
                return false;
            }

            var property = serializedObject.FindProperty(propertyPath);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            Undo.RecordObjects(serializedObject.targetObjects, undoName);
            property.objectReferenceValue = value;
            var changed = serializedObject.ApplyModifiedProperties();
            if (!changed)
            {
                return false;
            }

            for (var i = 0; i < serializedObject.targetObjects.Length; i++)
            {
                var targetObject = serializedObject.targetObjects[i];
                EditorUtility.SetDirty(targetObject);
                if (targetObject is Water25DController controller)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
                    if (markSceneDirty && controller.gameObject.scene.IsValid() && controller.gameObject.scene.isLoaded)
                    {
                        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
                    }
                }
            }

            return true;
        }

        public static void MarkControllerPropertiesChanged(SerializedObject serializedObject)
        {
            if (serializedObject == null)
            {
                return;
            }

            for (var i = 0; i < serializedObject.targetObjects.Length; i++)
            {
                if (!(serializedObject.targetObjects[i] is Water25DController controller))
                {
                    continue;
                }

                EditorUtility.SetDirty(controller);
                PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
                if (controller.gameObject.scene.IsValid() && controller.gameObject.scene.isLoaded)
                {
                    EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
                }
            }
        }

        public static T CreateProfileAsset<T>(T packageDefault, string objectName, string suffix) where T : ScriptableObject
        {
            var safeName = MakeSafeFileName(string.IsNullOrEmpty(objectName) ? "Water25D" : objectName);
            var path = AssetDatabase.GenerateUniqueAssetPath(ProfilesFolder + "/" + safeName + "_" + suffix + ".asset");
            var profile = ScriptableObject.CreateInstance<T>();
            if (packageDefault != null)
            {
                EditorUtility.CopySerialized(packageDefault, profile);
            }

            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            EditorGUIUtility.PingObject(profile);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static T DuplicateProfileAsset<T>(T source, string objectName, string suffix) where T : ScriptableObject
        {
            if (source == null)
            {
                return null;
            }

            var sourcePath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(sourcePath))
            {
                return CreateProfileAsset(source, objectName, suffix);
            }

            var safeName = MakeSafeFileName(string.IsNullOrEmpty(objectName) ? "Water25D" : objectName);
            var path = AssetDatabase.GenerateUniqueAssetPath(ProfilesFolder + "/" + safeName + "_" + suffix + ".asset");
            if (!AssetDatabase.CopyAsset(sourcePath, path))
            {
                return null;
            }

            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
            var duplicate = AssetDatabase.LoadAssetAtPath<T>(path);
            EditorGUIUtility.PingObject(duplicate);
            return duplicate;
        }

        public static WaterFXDefinition CreateFxDefinitionAsset(string objectName, string suffix)
        {
            var safeName = MakeSafeFileName(string.IsNullOrEmpty(objectName) ? "Water25D" : objectName);
            var path = AssetDatabase.GenerateUniqueAssetPath(ProfilesFolder + "/" + safeName + "_" + suffix + ".asset");
            var definition = ScriptableObject.CreateInstance<WaterFXDefinition>();
            AssetDatabase.CreateAsset(definition, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            EditorGUIUtility.PingObject(definition);
            return AssetDatabase.LoadAssetAtPath<WaterFXDefinition>(path);
        }

        public static void ResetAssetToDefault(ScriptableObject asset, ScriptableObject packageDefault, string undoName)
        {
            if (asset == null || packageDefault == null)
            {
                return;
            }

            Undo.RecordObject(asset, undoName);
            EditorUtility.CopySerialized(packageDefault, asset);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        public static void RefreshControllersForProfile(UnityEngine.Object profile)
        {
            if (profile == null)
            {
                return;
            }

            var controllers = Resources.FindObjectsOfTypeAll<Water25DController>();
            for (var i = 0; i < controllers.Length; i++)
            {
                var controller = controllers[i];
                if (controller == null || EditorUtility.IsPersistent(controller) || !controller.gameObject.scene.IsValid() || !controller.gameObject.scene.isLoaded)
                {
                    continue;
                }

                if (controller.StyleProfile == profile || controller.QualityProfile == profile)
                {
                    controller.RefreshAuthoringPreview();
                }
            }

            SceneView.RepaintAll();
            InternalEditorUtility.RepaintAllViews();
        }

        public static void RefreshAllControllers()
        {
            var controllers = Resources.FindObjectsOfTypeAll<Water25DController>();
            for (var i = 0; i < controllers.Length; i++)
            {
                var controller = controllers[i];
                if (controller == null || EditorUtility.IsPersistent(controller) || !controller.gameObject.scene.IsValid() || !controller.gameObject.scene.isLoaded)
                {
                    continue;
                }

                controller.RefreshAuthoringPreview();
            }

            SceneView.RepaintAll();
            InternalEditorUtility.RepaintAllViews();
        }

        public static void RepairHierarchyWithUndo(Water25DController controller)
        {
            if (controller == null)
            {
                return;
            }

            var existingChildren = new HashSet<int>();
            for (var i = 0; i < controller.transform.childCount; i++)
            {
                existingChildren.Add(controller.transform.GetChild(i).GetEntityId().GetHashCode());
            }

            Undo.RegisterFullObjectHierarchyUndo(controller.gameObject, "Repair Water25D Hierarchy");
            controller.RepairHierarchyAndRebuild();
            for (var i = 0; i < controller.transform.childCount; i++)
            {
                var child = controller.transform.GetChild(i);
                if (!existingChildren.Contains(child.GetEntityId().GetHashCode()))
                {
                    Undo.RegisterCreatedObjectUndo(child.gameObject, "Create Water25D Generated Child");
                }
            }

            EditorUtility.SetDirty(controller);
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            if (controller.gameObject.scene.IsValid() && controller.gameObject.scene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            }

            SceneView.RepaintAll();
        }

        public static void FixSafeDefaults(Water25DController controller)
        {
            if (controller == null)
            {
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(controller.gameObject, "Fix Water25D Safe Defaults");
            Water25DEditorDefaults.AssignDefaults(controller);
            RepairHierarchyWithUndo(controller);
        }

        public static void RebuildGeometry(Water25DController controller)
        {
            if (controller == null)
            {
                return;
            }

            controller.RebuildGeometryPreview();
            SceneView.RepaintAll();
        }

        public static void MarkControllerAuthoringChange(Water25DController controller)
        {
            if (controller == null)
            {
                return;
            }

            EditorUtility.SetDirty(controller);
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            if (controller.gameObject.scene.IsValid() && controller.gameObject.scene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            }
        }

        public static void SelectObject(UnityEngine.Object targetObject)
        {
            if (targetObject == null)
            {
                return;
            }

            Selection.activeObject = targetObject;
            EditorGUIUtility.PingObject(targetObject);
        }

        public static void OpenDocumentation(string path)
        {
            var document = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (document != null)
            {
                AssetDatabase.OpenAsset(document);
            }
            else
            {
                EditorUtility.DisplayDialog("Water25D Documentation", "The package setup document could not be found at:\n" + path, "OK");
            }
        }

        public static Water25DPerformanceMetrics CalculateMetrics(Water25DController controller)
        {
            var settings = controller != null && controller.QualityProfile != null
                ? controller.QualityProfile.GetSettings()
                : WaterQualitySettings.Default;
            var size = controller != null ? controller.TopSurfaceSize : new Vector2(0.01f, 0.01f);
            var topVertexCount = WaterMeshBuilder.CalculateTopVertexCount(size, settings.TopVerticesPerUnit);
            var rippleResolution = settings.CalculateRippleResolution(size);
            var usesRgHalf = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf);
            var bytesPerTexel = usesRgHalf ? 4L : 8L;
            var stateBytes = (long)rippleResolution.x * rippleResolution.y * bytesPerTexel;
            var propagatedCells = (double)rippleResolution.x * rippleResolution.y * settings.SimulationFrequency * settings.PropagationSubsteps;
            return new Water25DPerformanceMetrics
            {
                TopVertexCount = topVertexCount,
                TopTriangleCount = Mathf.Max(0, (topVertexCount.x - 1) * (topVertexCount.y - 1) * 2),
                FrontVertexCount = topVertexCount.x * 2,
                FrontTriangleCount = Mathf.Max(0, (topVertexCount.x - 1) * 2),
                RippleResolution = rippleResolution,
                RippleStateBytes = stateBytes,
                UsesRgHalf = usesRgHalf,
                PropagatedCellsPerSecond = propagatedCells,
                MaximumImpactsPerStep = settings.MaximumImpactsPerStep,
                MaximumQueuedImpacts = settings.MaximumQueuedImpacts,
                SimulationFrequency = settings.SimulationFrequency,
                PropagationSubsteps = settings.PropagationSubsteps,
                TopVerticesPerUnit = settings.TopVerticesPerUnit
            };
        }

        public static bool TryGetTextureDimensions(Texture texture, out Vector2Int dimensions)
        {
            if (texture == null)
            {
                dimensions = default;
                return false;
            }

            dimensions = new Vector2Int(texture.width, texture.height);
            return dimensions.x > 0 && dimensions.y > 0;
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024L)
            {
                return (bytes / (1024f * 1024f)).ToString("0.00") + " MiB";
            }

            return (bytes / 1024f).ToString("0.0") + " KiB";
        }

        public static int CountProfileUsers(UnityEngine.Object profile)
        {
            if (profile == null)
            {
                return 0;
            }

            var count = 0;
            var controllers = Resources.FindObjectsOfTypeAll<Water25DController>();
            for (var i = 0; i < controllers.Length; i++)
            {
                var controller = controllers[i];
                if (controller == null || EditorUtility.IsPersistent(controller) || !controller.gameObject.scene.IsValid() || !controller.gameObject.scene.isLoaded)
                {
                    continue;
                }

                if (controller.StyleProfile == profile || controller.QualityProfile == profile)
                {
                    count++;
                }
            }

            return count;
        }

        private static string MakeSafeFileName(string value)
        {
            var characters = value.ToCharArray();
            for (var i = 0; i < characters.Length; i++)
            {
                if (!char.IsLetterOrDigit(characters[i]) && characters[i] != '_' && characters[i] != '-')
                {
                    characters[i] = '_';
                }
            }

            return new string(characters);
        }
    }
}
