using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Water25D.Rendering;

namespace Water25D.Editor
{
    /// <summary>
    /// Bridges package-owned default assets into serialized authoring objects. Runtime code
    /// deliberately does not depend on AssetDatabase, so this editor-only repair keeps scene
    /// creation and scene reopening deterministic without putting assets in Resources.
    /// </summary>
    [InitializeOnLoad]
    internal static class Water25DEditorDefaults
    {
        private const string TopMaterialPath = "Assets/Water25D/Materials/Water25D_Top.mat";
        private const string FrontMaterialPath = "Assets/Water25D/Materials/Water25D_Front.mat";
        private const string RippleMaterialPath = "Assets/Water25D/Materials/Water25D_RippleSimulation.mat";
        private const string StyleProfilePath = "Assets/Water25D/Profiles/Water25D_DefaultStyle.asset";
        private const string QualityProfilePath = "Assets/Water25D/Profiles/Water25D_MediumQuality.asset";
        private const string FlatStylizedStyleProfilePath = "Assets/Water25D/Profiles/Water25D_FlatStylizedStyle.asset";
        private const string FlatStylizedQualityProfilePath = "Assets/Water25D/Profiles/Water25D_FlatMediumQuality.asset";

        private static bool _isRepairingLoadedControllers;

        static Water25DEditorDefaults()
        {
            EditorApplication.delayCall += RepairLoadedControllers;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        internal static bool AssignDefaults(Water25DController controller)
        {
            return AssignDefaults(controller, false);
        }

        internal static bool AssignDefaults(Water25DController controller, bool isNewObject)
        {
            if (controller == null || Application.isPlaying)
            {
                return false;
            }

            var topMaterial = AssetDatabase.LoadAssetAtPath<Material>(TopMaterialPath);
            var frontMaterial = AssetDatabase.LoadAssetAtPath<Material>(FrontMaterialPath);
            var rippleMaterial = AssetDatabase.LoadAssetAtPath<Material>(RippleMaterialPath);
            var styleProfilePath = isNewObject ? FlatStylizedStyleProfilePath : StyleProfilePath;
            var qualityProfilePath = isNewObject ? FlatStylizedQualityProfilePath : QualityProfilePath;
            var styleProfile = AssetDatabase.LoadAssetAtPath<WaterStyleProfile>(styleProfilePath);
            var qualityProfile = AssetDatabase.LoadAssetAtPath<WaterQualityProfile>(qualityProfilePath);
            styleProfile = styleProfile != null
                ? styleProfile
                : AssetDatabase.LoadAssetAtPath<WaterStyleProfile>(StyleProfilePath);
            qualityProfile = qualityProfile != null
                ? qualityProfile
                : AssetDatabase.LoadAssetAtPath<WaterQualityProfile>(QualityProfilePath);
            if (topMaterial == null || frontMaterial == null || rippleMaterial == null || styleProfile == null || qualityProfile == null)
            {
                return false;
            }

            var serializedObject = new SerializedObject(controller);
            serializedObject.Update();
            var changed = false;
            changed |= AssignIfMissing(serializedObject, "_topMaterialTemplate", topMaterial);
            changed |= AssignIfMissing(serializedObject, "_frontMaterialTemplate", frontMaterial);
            changed |= AssignIfMissing(serializedObject, "_rippleSimulationMaterialTemplate", rippleMaterial);
            changed |= AssignIfMissing(serializedObject, "_styleProfile", styleProfile);
            changed |= AssignIfMissing(serializedObject, "_qualityProfile", qualityProfile);
            if (isNewObject)
            {
                changed |= AssignSurfaceMode(serializedObject, WaterSurfaceMode.FlatStylized);
            }
            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(controller);
            }

            controller.RepairHierarchyAndRebuild();
            if (changed && controller.gameObject.scene.IsValid() && controller.gameObject.scene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            }

            return changed;
        }

        private static bool AssignIfMissing(SerializedObject serializedObject, string propertyPath, Object value)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null || property.objectReferenceValue != null)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool AssignSurfaceMode(SerializedObject serializedObject, WaterSurfaceMode surfaceMode)
        {
            var property = serializedObject.FindProperty("_surfaceMode");
            if (property == null || property.enumValueIndex == (int)surfaceMode)
            {
                return false;
            }

            property.enumValueIndex = (int)surfaceMode;
            return true;
        }

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            EditorApplication.delayCall += RepairLoadedControllers;
        }

        private static void RepairLoadedControllers()
        {
            if (_isRepairingLoadedControllers)
            {
                return;
            }

            _isRepairingLoadedControllers = true;
            try
            {
                var controllers = Resources.FindObjectsOfTypeAll<Water25DController>();
                for (var i = 0; i < controllers.Length; i++)
                {
                    var controller = controllers[i];
                    if (controller == null || EditorUtility.IsPersistent(controller) || !controller.gameObject.scene.IsValid() || !controller.gameObject.scene.isLoaded)
                    {
                        continue;
                    }

                    AssignDefaults(controller);
                }
            }
            finally
            {
                _isRepairingLoadedControllers = false;
            }
        }
    }
}
