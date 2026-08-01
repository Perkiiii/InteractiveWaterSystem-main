using UnityEditor;
using UnityEngine;

namespace Water25D.Editor
{
    [CustomEditor(typeof(Water25DController))]
    public sealed class Water25DEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            Water25DEditorDefaults.AssignDefaults((Water25DController)target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSection("Dimensions", "_topSurfaceSize", "_frontSurfaceDepth", "_waterlineLocalY", "_interactionDepth01", "_surfaceTriggerThickness");
            DrawSection("Profiles", "_styleProfile", "_qualityProfile");
            DrawSection("Material Templates", "_topMaterialTemplate", "_frontMaterialTemplate", "_rippleSimulationMaterialTemplate");
            DrawSection("Sorting", "_topSortingLayerName", "_topSortingOrder", "_frontSortingLayerName", "_frontSortingOrder");
            DrawSection("Reflection", "_reflectionMode", "_reflectionCameraSource", "_reflectionCullingMask", "_reflectionResolutionScale", "_reflectionUpdateIntervalFrames", "_reflectionStrength");
            DrawSection(
                "Physics and Interaction",
                "_enableSurfaceInteraction",
                "_enableBuoyancy",
                "_surfaceInteractionLayers",
                "_surfaceTriggerInteractionLayers",
                "_buoyancyLayers",
                "_includeTriggerCollidersInSurfaceInteraction",
                "_buoyancyDensity",
                "_buoyancyLinearDamping",
                "_buoyancyAngularDamping",
                "_enableCustomDrag",
                "_customLinearDrag",
                "_customAngularDrag");
            DrawSection(
                "Ripple Simulation",
                "_enableRippleSimulation",
                "_impactSpeedForFullStrength",
                "_minimumImpactStrength",
                "_impactStrengthMultiplier");
            DrawSection("Authoring", "_synchronizeGeneratedChildLayers");
            DrawSection("Events", "_onSurfaceEnter", "_onSurfaceExit", "_onSubmerged", "_onResurfaced");

            serializedObject.ApplyModifiedProperties();

            DrawAuthoringStatus((Water25DController)target);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Generated children are found by their names and serialized references. Preview meshes are transient and are rebuilt after reload; package surface materials and profiles are persistent assets.",
                MessageType.Info);
            if (GUILayout.Button("Assign Package Defaults"))
            {
                var controller = (Water25DController)target;
                Undo.RecordObject(controller, "Assign Water 2.5D Package Defaults");
                Water25DEditorDefaults.AssignDefaults(controller);
                serializedObject.Update();
                EditorUtility.SetDirty(controller);
            }
            if (GUILayout.Button("Repair Hierarchy and Rebuild"))
            {
                var controller = (Water25DController)target;
                Undo.RecordObject(controller, "Repair 2.5D Water Hierarchy");
                controller.RepairHierarchyAndRebuild();
                EditorUtility.SetDirty(controller);
            }
        }

        private static void DrawAuthoringStatus(Water25DController controller)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Authoring Status", EditorStyles.boldLabel);

            var qualitySettings = controller.QualityProfile != null
                ? controller.QualityProfile.GetSettings()
                : WaterQualitySettings.Default;
            var rippleResolution = qualitySettings.CalculateRippleResolution(controller.TopSurfaceSize);
            EditorGUILayout.LabelField("Ripple State", rippleResolution.x + " x " + rippleResolution.y + " texels");
            EditorGUILayout.LabelField("Generated Meshes", "Transient editor/runtime resources");

            if (controller.StyleProfile == null || controller.QualityProfile == null)
            {
                EditorGUILayout.HelpBox("A package style or quality profile is not assigned. Use Assign Package Defaults or assign authored profiles.", MessageType.Warning);
            }

            DrawMaterialStatus(controller.TopSurface, "Top Surface", "Water25D/Top Surface");
            DrawMaterialStatus(controller.FrontSurface, "Front Surface", "Water25D/Front Surface");
            EditorGUILayout.EndVertical();
        }

        private static void DrawMaterialStatus(Transform surface, string label, string expectedShader)
        {
            var renderer = surface != null ? surface.GetComponent<MeshRenderer>() : null;
            var material = renderer != null ? renderer.sharedMaterial : null;
            if (material == null)
            {
                EditorGUILayout.HelpBox(label + " has no persistent material assigned.", MessageType.Error);
                return;
            }

            if (material.shader == null || !material.shader.isSupported || material.shader.name != expectedShader)
            {
                EditorGUILayout.HelpBox(label + " material shader is missing, unsupported, or unexpected.", MessageType.Error);
            }
        }

        private void DrawSection(string label, params string[] propertyNames)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            for (var i = 0; i < propertyNames.Length; i++)
            {
                var serializedProperty = serializedObject.FindProperty(propertyNames[i]);
                if (serializedProperty != null)
                {
                    var displayName = ObjectNames.NicifyVariableName(propertyNames[i].TrimStart('_'));
                    EditorGUILayout.PropertyField(serializedProperty, new GUIContent(displayName), true);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void OnSceneGUI()
        {
            var controller = (Water25DController)target;
            if (controller == null || !controller.isActiveAndEnabled)
            {
                return;
            }

            var size = controller.TopSurfaceSize;
            var waterline = controller.WaterlineLocalY;
            var frontDepth = controller.FrontSurfaceDepth;
            var transform = controller.transform;

            using (new Handles.DrawingScope(transform.localToWorldMatrix))
            {
                Handles.color = new Color(0.1f, 0.75f, 1f, 0.8f);
                Handles.DrawWireCube(new Vector3(size.x * 0.5f, waterline, size.y * 0.5f), new Vector3(size.x, 0.02f, size.y));
                Handles.color = new Color(0.1f, 0.35f, 0.8f, 0.6f);
                Handles.DrawWireCube(new Vector3(size.x * 0.5f, waterline - frontDepth * 0.5f, 0f), new Vector3(size.x, frontDepth, 0.02f));

                var widthHandle = Handles.Slider(
                    new Vector3(size.x, waterline, 0f),
                    Vector3.right,
                    HandleUtility.GetHandleSize(transform.TransformPoint(new Vector3(size.x, waterline, 0f))) * 0.1f,
                    Handles.CubeHandleCap,
                    0f);
                if (!Mathf.Approximately(widthHandle.x, size.x))
                {
                    Undo.RecordObject(controller, "Resize 2.5D Water Width");
                    controller.SetDimensions(new Vector2(Mathf.Max(0.01f, widthHandle.x), size.y), frontDepth);
                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }

                var topDepthHandle = Handles.Slider(
                    new Vector3(0f, waterline, size.y),
                    Vector3.forward,
                    HandleUtility.GetHandleSize(transform.TransformPoint(new Vector3(0f, waterline, size.y))) * 0.1f,
                    Handles.CubeHandleCap,
                    0f);
                if (!Mathf.Approximately(topDepthHandle.z, size.y))
                {
                    Undo.RecordObject(controller, "Resize 2.5D Water Visual Depth");
                    controller.SetDimensions(new Vector2(size.x, Mathf.Max(0.01f, topDepthHandle.z)), frontDepth);
                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }

                var frontDepthHandle = Handles.Slider(
                    new Vector3(size.x * 0.5f, waterline - frontDepth, 0f),
                    Vector3.down,
                    HandleUtility.GetHandleSize(transform.TransformPoint(new Vector3(size.x * 0.5f, waterline - frontDepth, 0f))) * 0.1f,
                    Handles.CubeHandleCap,
                    0f);
                var newFrontDepth = waterline - frontDepthHandle.y;
                if (!Mathf.Approximately(newFrontDepth, frontDepth))
                {
                    Undo.RecordObject(controller, "Resize 2.5D Water Physical Depth");
                    controller.SetDimensions(size, Mathf.Max(0.01f, newFrontDepth));
                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }
            }
        }
    }
}
