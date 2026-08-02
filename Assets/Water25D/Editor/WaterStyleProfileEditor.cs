using UnityEditor;
using UnityEngine;

namespace Water25D.Editor
{
    [CustomEditor(typeof(WaterStyleProfile))]
    [CanEditMultipleObjects]
    public sealed class WaterStyleProfileEditor : UnityEditor.Editor
    {
        private const string SetupPath = "Assets/Water25D/Documentation/SETUP.md";

        public static bool DrawSurfaceFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water Style Surface Colors", delegate
            {
                Property(serializedObject, "_topColor", "Top Color", "Tint applied to the XZ top surface.");
                Property(serializedObject, "_frontSurfaceColor", "Front Surface Color", "Shallow color at the waterline of the XY front surface.");
                Property(serializedObject, "_frontDeepColor", "Front Deep Color", "Color used toward the bottom of the XY front surface.");
                Property(serializedObject, "_foamColor", "Foam Color", "Color used by the current surface-edge treatment and pooled water effects.");
            });
        }

        public static bool DrawAmbientFields(SerializedObject serializedObject)
        {
            var changed = DrawProperties(serializedObject, "Edit Water Style Ambient Waves", delegate
            {
                Property(serializedObject, "_ambientWaveAmplitude", "Amplitude", "Peak analytical wave displacement in local world units.");
                Property(serializedObject, "_ambientWaveLength", "Wavelength", "Distance between broad ambient wave peaks in local world units.");
                Property(serializedObject, "_ambientWaveSpeed", "Speed", "Travel speed of the analytical ambient wave in local world units per second.");
                Property(serializedObject, "_ambientWaveDirection", "Direction", "Normalized XZ direction used by the analytical wave phase.");
            });

            if (serializedObject != null && GUILayout.Button(new GUIContent("Normalize Direction", "Normalize the ambient wave direction without changing its heading."), Water25DInspectorStyles.SmallButton))
            {
                NormalizeDirection(serializedObject);
                changed = true;
            }

            return changed;
        }

        public static bool DrawRippleFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water Style Contact Ripples", delegate
            {
                Property(serializedObject, "_rippleAmplitude", "Ripple Amplitude", "Visual amplitude applied to the instance-owned contact ripple state.");
            });
        }

        public static bool DrawRingFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water Style Procedural Surface Rings", delegate
            {
                Property(serializedObject, "_ringLifetime", "Ring Lifetime", "Seconds before a procedural surface ring expires.");
                Property(serializedObject, "_ringExpansionMultiplier", "Expansion Multiplier", "Multiplies the impact radius to determine the ring's final radius.");
                Property(serializedObject, "_ringThickness", "Thickness", "Ring annulus thickness in local world units.");
                Property(serializedObject, "_ringSoftness", "Softness", "Soft edge width around the ring annulus in local world units.");
                Property(serializedObject, "_ringIntensity", "Intensity", "Highlight strength blended toward the profile foam colour.");
            });
        }

        public static bool DrawContactFoamFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water Style Contact Foam", delegate
            {
                Property(serializedObject, "_contactFoamWidthPadding", "Width Padding", "Additional local-X width around the aggregate logical-body contact.");
                Property(serializedObject, "_contactFoamHalfDepth", "Half Depth", "Local-Z half depth of the analytic contact ellipse.");
                Property(serializedObject, "_contactFoamSoftness", "Softness", "Normalized edge softness of the analytic contact ellipse.");
                Property(serializedObject, "_contactFoamIntensity", "Intensity", "Maximum contact-foam contribution before per-slot intensity and fade.");
                Property(serializedObject, "_contactFoamFadeDuration", "Fade Duration", "Seconds for a released contact-foam slot to fade to zero.");
                Property(serializedObject, "_foamReflectionOcclusion", "Reflection Occlusion", "How strongly contact foam suppresses reflection beneath its mask.");
            });
        }

        public static bool DrawMaterialFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water Style Material Templates", delegate
            {
                Property(serializedObject, "_topMaterialTemplate", "Optional Top Material", "Optional material template used when a controller has no explicit top material. It is never mutated at runtime.");
                Property(serializedObject, "_frontMaterialTemplate", "Optional Front Material", "Optional material template used when a controller has no explicit front material. It is never mutated at runtime.");
            });
        }

        public static void ResetAmbientWaveSettings(SerializedObject serializedObject)
        {
            var defaults = Water25DInspectorUtility.LoadPackageAsset<WaterStyleProfile>(Water25DInspectorUtility.StyleProfilePath);
            if (defaults == null || serializedObject == null || serializedObject.targetObject == defaults)
            {
                return;
            }

            var defaultObject = new SerializedObject(defaults);
            defaultObject.Update();
            serializedObject.Update();
            Undo.RecordObjects(serializedObject.targetObjects, "Reset Water25D Ambient Waves");
            CopyFloat(serializedObject, defaultObject, "_ambientWaveAmplitude");
            CopyFloat(serializedObject, defaultObject, "_ambientWaveLength");
            CopyFloat(serializedObject, defaultObject, "_ambientWaveSpeed");
            CopyVector2(serializedObject, defaultObject, "_ambientWaveDirection");
            ApplyProfileChanges(serializedObject, "Reset Water25D Ambient Waves");
        }

        public static void NormalizeDirection(SerializedObject serializedObject)
        {
            if (serializedObject == null)
            {
                return;
            }

            serializedObject.Update();
            var direction = serializedObject.FindProperty("_ambientWaveDirection");
            if (direction == null)
            {
                return;
            }

            Undo.RecordObjects(serializedObject.targetObjects, "Normalize Water25D Wave Direction");
            var value = direction.vector2Value;
            direction.vector2Value = value.sqrMagnitude > 0.0001f ? value.normalized : Vector2.right;
            ApplyProfileChanges(serializedObject, "Normalize Water25D Wave Direction");
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
            var profile = target as WaterStyleProfile;
            EditorGUILayout.BeginVertical(Water25DInspectorStyles.Header);
            EditorGUILayout.LabelField("WaterStyleProfile", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Shared Water25D appearance settings", Water25DInspectorStyles.Subtitle);
            EditorGUILayout.EndVertical();

            var users = Water25DInspectorUtility.CountProfileUsers(profile);
            EditorGUILayout.HelpBox(
                users > 0
                    ? "This profile is used by " + users + " loaded Water25D object(s). Changes affect every user."
                    : "This asset is not currently used by a loaded Water25D object.",
                users > 0 ? MessageType.Warning : MessageType.Info);

            if (GUILayout.Button(new GUIContent("Reset to Water25D Defaults", "Copy the package default style values into this asset.")))
            {
                var defaults = Water25DInspectorUtility.LoadPackageAsset<WaterStyleProfile>(Water25DInspectorUtility.StyleProfilePath);
                Water25DInspectorUtility.ResetAssetToDefault(profile, defaults, "Reset Water25D Style Profile");
                Water25DInspectorUtility.RefreshControllersForProfile(profile);
                serializedObject.Update();
            }

            if (GUILayout.Button(new GUIContent("Duplicate Profile", "Create a new asset without changing this shared profile.")))
            {
                Water25DInspectorUtility.DuplicateProfileAsset(profile, profile != null ? profile.name : "Water25D", "Style");
            }

            DrawStandaloneSection(serializedObject, "Surface Colors", true, DrawSurfaceFields);
            DrawStandaloneSection(serializedObject, "Ambient Waves", true, DrawAmbientFields);
            DrawStandaloneSection(serializedObject, "Contact Ripples", false, DrawRippleFields);
            DrawStandaloneSection(serializedObject, "Procedural Surface Rings", false, DrawRingFields);
            DrawStandaloneSection(serializedObject, "Contact Foam", false, DrawContactFoamFields);
            DrawStandaloneSection(serializedObject, "Material Templates", false, DrawMaterialFields);

            EditorGUILayout.BeginVertical(Water25DInspectorStyles.SectionCard);
            EditorGUILayout.LabelField("Usage / Shared Asset", Water25DInspectorStyles.Subsection);
            EditorGUILayout.HelpBox("A style profile is configuration, not per-water runtime state. Use Make Unique Copy in a Water25D controller before authoring a one-off look.", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Open Setup Documentation", "Open the package setup and profile workflow guide.")))
            {
                Water25DInspectorUtility.OpenDocumentation(SetupPath);
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawStandaloneSection(SerializedObject serializedObject, string label, bool defaultOpen, System.Func<SerializedObject, bool> draw)
        {
            var open = Water25DInspectorState.GetFoldout("StyleProfile." + label, defaultOpen);
            var next = EditorGUILayout.BeginFoldoutHeaderGroup(open, label);
            if (next != open)
            {
                Water25DInspectorState.SetFoldout("StyleProfile." + label, next);
            }

            if (next)
            {
                EditorGUILayout.BeginVertical(Water25DInspectorStyles.SectionCard);
                draw(serializedObject);
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

        private static void CopyFloat(SerializedObject destination, SerializedObject source, string propertyPath)
        {
            var destinationProperty = destination.FindProperty(propertyPath);
            var sourceProperty = source.FindProperty(propertyPath);
            if (destinationProperty != null && sourceProperty != null)
            {
                destinationProperty.floatValue = sourceProperty.floatValue;
            }
        }

        private static void CopyVector2(SerializedObject destination, SerializedObject source, string propertyPath)
        {
            var destinationProperty = destination.FindProperty(propertyPath);
            var sourceProperty = source.FindProperty(propertyPath);
            if (destinationProperty != null && sourceProperty != null)
            {
                destinationProperty.vector2Value = sourceProperty.vector2Value;
            }
        }
    }
}
