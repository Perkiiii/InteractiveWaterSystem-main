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

        public static bool DrawStylizedSurfaceFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Flat-Stylized Surface Colour", delegate
            {
                Property(serializedObject, "_shallowColor", "Shallow Colour", "Near-waterline colour for the top surface gradient.");
                Property(serializedObject, "_deepColor", "Deep Colour", "Far/deeper colour for the top surface gradient.");
                Property(serializedObject, "_topDepthPower", "Depth Gradient Power", "Shapes the normalized flat top-surface colour gradient without displacing vertices.");
                Property(serializedObject, "_topOpacity", "Top Opacity", "Final top-surface alpha before interaction foam is added.");
                Property(serializedObject, "_colorBandSteps", "Colour Band Steps", "Fixed quantization steps used for the restrained stylized colour banding.");
                Property(serializedObject, "_colorBandInfluence", "Colour Band Influence", "Blend from continuous colours to the fixed stylized bands.");
            });
        }

        public static bool DrawAmbientDetailFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Ambient Surface Detail", delegate
            {
                Property(serializedObject, "_surfaceNormalTexture", "Optional Normal Texture", "Optional package-owned normal input. Missing textures use deterministic procedural detail.");
                Property(serializedObject, "_surfaceDetailTexture", "Optional Detail Texture", "Optional secondary breakup input. Missing textures keep the procedural second layer.");
                Property(serializedObject, "_normalLayer1Scale", "Layer 1 Scale", "World-local XZ scale for the calm primary normal layer.");
                Property(serializedObject, "_normalLayer1Speed", "Layer 1 Speed", "Slow panning speed for the primary normal layer.");
                Property(serializedObject, "_normalLayer1Strength", "Layer 1 Strength", "Strength of the primary ambient normal layer.");
                Property(serializedObject, "_normalLayer2Scale", "Layer 2 Scale", "World-local XZ scale for the restrained secondary detail.");
                Property(serializedObject, "_normalLayer2Speed", "Layer 2 Speed", "Slow panning speed for the secondary detail.");
                Property(serializedObject, "_normalLayer2Strength", "Layer 2 Strength", "Strength of the secondary ambient detail layer.");
                Property(serializedObject, "_ambientNormalStrength", "Ambient Normal Strength", "Final blend from a calm flat normal to the layered detail normal.");
            });
        }

        public static bool DrawFresnelHighlightFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Fresnel and Stylized Highlights", delegate
            {
                Property(serializedObject, "_fresnelTint", "Fresnel Tint", "Tint blended toward grazing viewing angles.");
                Property(serializedObject, "_fresnelStrength", "Fresnel Strength", "Grazing-angle contribution used by both top and front surfaces.");
                Property(serializedObject, "_fresnelPower", "Fresnel Power", "Power shaping the restrained grazing-angle response.");
                Property(serializedObject, "_highlightColor", "Highlight Colour", "Colour of the optional broad stylized highlight.");
                Property(serializedObject, "_highlightStrength", "Highlight Strength", "Strength of the optional broad highlight.");
                Property(serializedObject, "_highlightThreshold", "Highlight Threshold", "Threshold for the stylized highlight response.");
                Property(serializedObject, "_highlightSoftness", "Highlight Softness", "Soft edge around the highlight threshold.");
                Property(serializedObject, "_highlightBreakup", "Highlight Breakup", "Deterministic low-frequency breakup applied to the highlight.");
                Property(serializedObject, "_highlightDirection", "Highlight Direction", "World-space light direction used by the stylized highlight.");
            });
        }

        public static bool DrawReflectionPresentationFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Reflection Presentation", delegate
            {
                Property(serializedObject, "_stylizedReflectionTint", "Stylized Reflection Tint", "Tint for the camera-free horizon-to-sky reflection fallback.");
                Property(serializedObject, "_stylizedReflectionHorizonColor", "Stylized Horizon Colour", "Horizon colour used by the camera-free stylized reflection.");
                Property(serializedObject, "_stylizedReflectionTopColor", "Stylized Top Colour", "Top colour used by the camera-free stylized reflection.");
                Property(serializedObject, "_stylizedReflectionStrength", "Stylized Reflection Strength", "Strength of the camera-free stylized reflection.");
                Property(serializedObject, "_planarReflectionTint", "Planar Reflection Tint", "Tint applied to the shared adaptive planar reflection texture.");
                Property(serializedObject, "_planarReflectionStrength", "Planar Reflection Strength", "Per-profile contribution of the shared planar reflection.");
                Property(serializedObject, "_ambientReflectionDistortion", "Ambient Distortion", "Small normal-driven offset for the reflection lookup.");
                Property(serializedObject, "_ringNormalStrength", "Ring Normal Strength", "Interaction-ring normal contribution used by reflection and highlights.");
                Property(serializedObject, "_ringReflectionDistortion", "Ring Reflection Distortion", "Reflection lookup distortion contributed by active rings.");
                Property(serializedObject, "_wakeNormalStrength", "Wake Normal Strength", "Interaction-wake normal contribution used by reflection and highlights.");
                Property(serializedObject, "_wakeReflectionDistortion", "Wake Reflection Distortion", "Reflection lookup distortion contributed by active wakes.");
            });
        }

        public static bool DrawBoundaryFoamFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Boundary Foam", delegate
            {
                Property(serializedObject, "_boundaryFoamWidth", "Boundary Width", "Normalized water-surface edge band used for deterministic boundary foam.");
                Property(serializedObject, "_boundaryFoamSoftness", "Boundary Softness", "Soft edge around the boundary foam band.");
                Property(serializedObject, "_boundaryFoamBreakup", "Boundary Breakup", "Low-frequency deterministic breakup of the boundary foam.");
                Property(serializedObject, "_boundaryFoamIntensity", "Boundary Intensity", "Maximum boundary foam contribution.");
            });
        }

        public static bool DrawRefractionFields(SerializedObject serializedObject)
        {
            var changed = DrawProperties(serializedObject, "Edit Water25D Optional Refraction", delegate
            {
                Property(serializedObject, "_refractionSourceAvailable", "Opaque Texture Source Available", "Set only when the URP camera provides a valid opaque texture. The feature remains disabled otherwise.");
                Property(serializedObject, "_refractionTint", "Refraction Tint", "Tint applied to the optional opaque-texture sample.");
                Property(serializedObject, "_refractionStrength", "Refraction Strength", "Small normal-driven screen-space offset. This is optional and source-gated.");
                Property(serializedObject, "_frontDistortionSourceAvailable", "Sorting Layer Texture Available", "Set only when the 2D Renderer provides a valid Camera Sorting Layer Texture.");
                Property(serializedObject, "_frontDistortionTint", "Front Distortion Tint", "Tint applied to the optional front-surface sorting-layer sample.");
                Property(serializedObject, "_frontDistortionStrength", "Front Distortion Strength", "Small normal-driven front-surface screen-space offset.");
            });
            if (serializedObject != null)
            {
                EditorGUILayout.HelpBox("Optional source inputs are opt-in and safely bypassed when unavailable. No global interaction render texture is created.", MessageType.Info);
            }
            return changed;
        }

        public static bool DrawCausticFields(SerializedObject serializedObject)
        {
            var changed = DrawProperties(serializedObject, "Edit Water25D Optional Caustics", delegate
            {
                Property(serializedObject, "_causticTexture", "Optional Caustic Texture", "Optional package-owned grayscale caustic input. A missing texture disables the feature safely.");
                Property(serializedObject, "_causticScale", "Scale", "World-local XZ scale for the optional caustic texture.");
                Property(serializedObject, "_causticSpeed", "Speed", "Slow panning speed for the optional caustic texture.");
                Property(serializedObject, "_causticTint", "Tint", "Tint applied to the optional caustic contribution.");
                Property(serializedObject, "_causticIntensity", "Intensity", "Strength of the optional caustic contribution.");
                Property(serializedObject, "_causticDepthFade", "Depth Fade", "Fade toward the deeper part of the front surface.");
            });
            if (serializedObject != null)
            {
                var texture = serializedObject.FindProperty("_causticTexture");
                if (texture != null && texture.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("No caustic texture is assigned; the caustic quality toggle will remain safely inactive.", MessageType.Info);
                }
            }
            return changed;
        }

        public static bool DrawFrontSurfaceFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Front Surface Presentation", delegate
            {
                Property(serializedObject, "_frontSurfaceColor", "Front Surface Colour", "Shallow colour at the top of the XY front surface.");
                Property(serializedObject, "_frontDeepColor", "Front Deep Colour", "Deep colour toward the bottom of the XY front surface.");
                Property(serializedObject, "_frontDepthPower", "Front Depth Power", "Shapes the front-surface depth gradient.");
                Property(serializedObject, "_frontOpacity", "Front Opacity", "Final front-surface alpha before interaction foam is added.");
                Property(serializedObject, "_waterlineBandWidth", "Waterline Band Width", "Width of the coherent waterline foam band shared with interaction presentation.");
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

        public static bool DrawWakeFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water Style Distance-Spaced Wakes", delegate
            {
                Property(serializedObject, "_wakeEmissionSpacing", "Emission Spacing", "World-space distance between accepted wake segment centres.");
                Property(serializedObject, "_wakeMinimumLateralSpeed", "Minimum Lateral Speed", "Minimum accepted surface speed in world units per second.");
                Property(serializedObject, "_wakeWidthMultiplier", "Width Multiplier", "Multiplier applied to the aggregate logical-body half width.");
                Property(serializedObject, "_wakeWidthPadding", "Width Padding", "Additional local half-width applied after the body-width multiplier.");
                Property(serializedObject, "_wakeMinimumHalfWidth", "Minimum Half Width", "Lower clamp for the analytical wake capsule half width.");
                Property(serializedObject, "_wakeMaximumHalfWidth", "Maximum Half Width", "Upper clamp for the analytical wake capsule half width.");
                Property(serializedObject, "_wakeLifetime", "Lifetime", "Seconds before a wake segment expires.");
                Property(serializedObject, "_wakeFadePower", "Fade Power", "Age fade exponent used by the top and front analytical wake shaders.");
                Property(serializedObject, "_wakeIntensity", "Intensity", "Maximum wake contribution before speed normalization.");
                Property(serializedObject, "_wakeDirectionReversalAngle", "Reversal Angle", "Direction change angle that resets the accumulator and prevents a bridge segment.");
            });
        }

        public static bool DrawPainterlyFields(SerializedObject serializedObject)
        {
            return DrawProperties(serializedObject, "Edit Water25D Painterly Interaction Masks", delegate
            {
                DrawPainterlyMask(serializedObject, "_ringMask", "Surface Rings", "Optional grayscale ring atlas. Missing or disabled artwork keeps the analytical ring.");
                DrawPainterlyMask(serializedObject, "_contactFoamMask", "Contact Foam", "Optional grayscale contact-foam atlas. Gameplay bounds and body ownership remain analytical.");
                DrawPainterlyMask(serializedObject, "_wakeMask", "Wake Segments", "Optional grayscale wake atlas. The analytical capsule keeps movement direction authoritative.");
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
            DrawStandaloneSection(serializedObject, "Flat-Stylized Surface Colour", true, DrawStylizedSurfaceFields);
            DrawStandaloneSection(serializedObject, "Ambient Waves", true, DrawAmbientFields);
            DrawStandaloneSection(serializedObject, "Ambient Surface Detail", false, DrawAmbientDetailFields);
            DrawStandaloneSection(serializedObject, "Fresnel and Highlights", false, DrawFresnelHighlightFields);
            DrawStandaloneSection(serializedObject, "Reflection Presentation", false, DrawReflectionPresentationFields);
            DrawStandaloneSection(serializedObject, "Boundary Foam", false, DrawBoundaryFoamFields);
            DrawStandaloneSection(serializedObject, "Optional Refraction", false, DrawRefractionFields);
            DrawStandaloneSection(serializedObject, "Optional Caustics", false, DrawCausticFields);
            DrawStandaloneSection(serializedObject, "Front Surface Presentation", false, DrawFrontSurfaceFields);
            DrawStandaloneSection(serializedObject, "Contact Ripples", false, DrawRippleFields);
            DrawStandaloneSection(serializedObject, "Procedural Surface Rings", false, DrawRingFields);
            DrawStandaloneSection(serializedObject, "Contact Foam", false, DrawContactFoamFields);
            DrawStandaloneSection(serializedObject, "Distance-Spaced Wakes", false, DrawWakeFields);
            DrawStandaloneSection(serializedObject, "Painterly Interaction Masks", false, DrawPainterlyFields);
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

        private static void DrawPainterlyMask(SerializedObject serializedObject, string propertyPath, string label, string guidance)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(guidance, MessageType.Info);
            Property(serializedObject, propertyPath + ".Atlas", "Atlas", "Fixed-grid grayscale mask atlas. Use clamp wrapping and no mipmaps to keep cells isolated.");
            Property(serializedObject, propertyPath + ".Grid", "Columns / Rows", "Fixed atlas grid. Cells are row-major with variants inside each age frame.");
            Property(serializedObject, propertyPath + ".VariantCount", "Variant Count", "Stable variant count. Values are clamped to the available grid cells.");
            Property(serializedObject, propertyPath + ".FrameCount", "Frame Count", "Optional age frames. One-frame atlases are valid and use the first frame.");
            Property(serializedObject, propertyPath + ".Influence", "Mask Influence", "Blends painterly grayscale into the analytical interaction; zero retains the analytical result.");
            Property(serializedObject, propertyPath + ".RotationVariation", "Rotation Variation", "Stable creation-time rotation range. Wake variation remains deliberately narrow.");
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
