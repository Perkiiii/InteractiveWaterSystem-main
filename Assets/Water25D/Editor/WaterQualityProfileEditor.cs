using UnityEditor;
using UnityEngine;

namespace Water25D.Editor
{
    [CustomEditor(typeof(WaterQualityProfile))]
    [CanEditMultipleObjects]
    public sealed class WaterQualityProfileEditor : UnityEditor.Editor
    {
        private const string SetupPath = "Assets/Water25D/Documentation/SETUP.md";

        public static bool DrawRippleResolutionFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Ripple Resolution", delegate
            {
                Property(serializedObject, "_rippleTexelsPerUnit", "Texels Per Unit", "World-space density used to calculate the rectangular ripple state texture.");
                Property(serializedObject, "_minimumRippleResolution", "Minimum Resolution", "Lower bound for the instance-owned ripple state dimensions.");
                Property(serializedObject, "_maximumRippleResolution", "Maximum Resolution", "Upper bound for the instance-owned ripple state dimensions. Higher limits increase memory and update work.");
            });
        }

        public static bool DrawSchedulingFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Simulation Scheduling", delegate
            {
                Property(serializedObject, "_simulationFrequency", "Simulation Frequency", "Target ripple simulation updates per second. This is scheduling, not a direct wave-speed control.");
                Property(serializedObject, "_propagationSubsteps", "Propagation Substeps", "Number of propagation passes per simulation step. Higher values increase estimated cell work.");
                Property(serializedObject, "_maximumCatchUpSubsteps", "Maximum Catch-up Steps", "Bounds work after a stalled frame so simulation catch-up cannot grow without limit.");
                Property(serializedObject, "_maximumImpactsPerStep", "Maximum Impacts Per Step", "Maximum queued impacts injected before one shared full-surface propagation update.");
                Property(serializedObject, "_maximumQueuedImpacts", "Maximum Queued Impacts", "Fixed impact queue capacity. Excess impacts are rejected and counted at runtime.");
            });
        }

        public static bool DrawWaveBehaviourFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Wave Behaviour", delegate
            {
                Property(serializedObject, "_dampingPerSecond", "Damping Per Second", "Exponential damping applied per second; this is independent of frame rate.");
                Property(serializedObject, "_waveSpeed", "Wave Speed", "World-space propagation speed used by the ripple solver.");
                Property(serializedObject, "_impactRadius", "Impact Radius", "World-space radius converted independently to ripple texture U and V extents.");
                Property(serializedObject, "_idleTimeout", "Idle Timeout", "Off-screen time without queued impacts before the ripple simulator suspends.");
                Property(serializedObject, "_ambientWaveBands", "Ambient Wave Bands", "Number of analytical ambient wave bands used by the current shader set.");
            });
        }

        public static bool DrawGeometryFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Geometry", delegate
            {
                Property(serializedObject, "_topVerticesPerUnit", "Top Vertices Per Unit", "World-space mesh density for the XZ top surface. Geometry is rebuilt when this changes.");
            });
        }

        public static bool DrawSurfaceRingFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Surface Ring Capacity", delegate
            {
                Property(serializedObject, "_maximumSurfaceRings", "Maximum Active Rings", "Fixed active-ring capacity for FlatStylized. Values are clamped to 1–16 and do not recreate CRT state.");
            });
        }

        public static bool DrawAmbientBandField(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Ambient Wave Bands", delegate
            {
                Property(serializedObject, "_ambientWaveBands", "Ambient Wave Bands", "Number of analytical ambient wave bands used by the current shader set.");
            });
        }

        public static void ApplyProfileChanges(SerializedObject serializedObject, string undoName)
        {
            if (serializedObject == null)
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();
            for (var i = 0; i < serializedObject.targetObjects.Length; i++)
            {
                var profile = serializedObject.targetObjects[i];
                EditorUtility.SetDirty(profile);
                Water25DInspectorUtility.RefreshControllersForProfile(profile);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Water25DInspectorStyles.Ensure();
            var profile = target as WaterQualityProfile;
            EditorGUILayout.BeginVertical(Water25DInspectorStyles.Header);
            EditorGUILayout.LabelField("WaterQualityProfile", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Shared Water25D simulation and geometry settings", Water25DInspectorStyles.Subtitle);
            EditorGUILayout.EndVertical();

            var users = Water25DInspectorUtility.CountProfileUsers(profile);
            EditorGUILayout.HelpBox(
                users > 0
                    ? "This profile is used by " + users + " loaded Water25D object(s). Changes affect every user."
                    : "This asset is not currently used by a loaded Water25D object.",
                users > 0 ? MessageType.Warning : MessageType.Info);

            if (GUILayout.Button(new GUIContent("Reset to Water25D Medium Defaults", "Copy the package medium quality values into this asset.")))
            {
                var defaults = Water25DInspectorUtility.LoadPackageAsset<WaterQualityProfile>(Water25DInspectorUtility.QualityProfilePath);
                Water25DInspectorUtility.ResetAssetToDefault(profile, defaults, "Reset Water25D Quality Profile");
                Water25DInspectorUtility.RefreshControllersForProfile(profile);
                serializedObject.Update();
            }

            if (GUILayout.Button(new GUIContent("Duplicate Profile", "Create a new quality asset without changing this shared profile.")))
            {
                Water25DInspectorUtility.DuplicateProfileAsset(profile, profile != null ? profile.name : "Water25D", "Quality");
            }

            DrawStandaloneSection(serializedObject, "Ripple Resolution", true, DrawRippleResolutionFields);
            DrawStandaloneSection(serializedObject, "Simulation Scheduling", true, DrawSchedulingFields);
            DrawStandaloneSection(serializedObject, "Wave Behaviour", false, DrawWaveBehaviourFields);
            DrawStandaloneSection(serializedObject, "Geometry", false, DrawGeometryFields);
            DrawStandaloneSection(serializedObject, "Surface Rings", false, DrawSurfaceRingFields);
            DrawValidation(serializedObject, profile);

            EditorGUILayout.BeginVertical(Water25DInspectorStyles.SectionCard);
            EditorGUILayout.LabelField("Usage / Shared Asset", Water25DInspectorStyles.Subsection);
            EditorGUILayout.HelpBox("Quality profiles control instance-owned resources. Use Make Unique Copy in a Water25D controller before creating a one-off performance budget.", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Open Setup Documentation", "Open the package setup and quality-profile workflow guide.")))
            {
                Water25DInspectorUtility.OpenDocumentation(SetupPath);
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawStandaloneSection(SerializedObject serializedObject, string label, bool defaultOpen, System.Func<SerializedObject, bool> draw)
        {
            var open = Water25DInspectorState.GetFoldout("QualityProfile." + label, defaultOpen);
            var next = EditorGUILayout.BeginFoldoutHeaderGroup(open, label);
            if (next != open)
            {
                Water25DInspectorState.SetFoldout("QualityProfile." + label, next);
            }

            if (next)
            {
                EditorGUILayout.BeginVertical(Water25DInspectorStyles.SectionCard);
                draw(serializedObject);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void DrawValidation(SerializedObject serializedObject, WaterQualityProfile profile)
        {
            var open = Water25DInspectorState.GetFoldout("QualityProfile.Validation", true);
            var next = EditorGUILayout.BeginFoldoutHeaderGroup(open, "Validation");
            if (next != open)
            {
                Water25DInspectorState.SetFoldout("QualityProfile.Validation", next);
            }

            if (next)
            {
                EditorGUILayout.BeginVertical(Water25DInspectorStyles.SectionCard);
                var minimum = serializedObject.FindProperty("_minimumRippleResolution");
                var maximum = serializedObject.FindProperty("_maximumRippleResolution");
                if (minimum != null && maximum != null && (minimum.vector2IntValue.x > maximum.vector2IntValue.x || minimum.vector2IntValue.y > maximum.vector2IntValue.y))
                {
                    EditorGUILayout.HelpBox("Minimum ripple resolution must not exceed maximum ripple resolution.", MessageType.Error);
                }

                var settings = profile != null ? profile.GetSettings() : WaterQualitySettings.Default;
                var exampleResolution = settings.CalculateRippleResolution(new Vector2(20f, 6.5f));
                EditorGUILayout.HelpBox(
                    "Example only: a 20 x 6.5 water surface calculates to " + exampleResolution.x + " x " + exampleResolution.y + " texels with this profile. This is an estimate, not a measured performance result.",
                    MessageType.Info);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static bool DrawProperties(SerializedObject serializedObject, string undoName, System.Action draw)
        {
            if (serializedObject == null)
            {
                return false;
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            draw();
            var changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                Undo.RecordObjects(serializedObject.targetObjects, undoName);
                ApplyProfileChanges(serializedObject, undoName);
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }

            return changed;
        }

        private static void Property(SerializedObject serializedObject, string propertyPath, string label, string tooltip)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip), true);
            }
        }
    }
}
