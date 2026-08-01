using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Water25D.Rendering;

namespace Water25D.Editor
{
    [CustomEditor(typeof(Water25DController))]
    [CanEditMultipleObjects]
    public sealed class Water25DEditor : UnityEditor.Editor
    {
        private const string SetupDocumentationPath = "Assets/Water25D/Documentation/SETUP.md";
        private const string TopLevelFoldoutPrefix = "Controller.PixelWaterStyle.TopLevel.";

        private SerializedObject _styleProfileSerializedObject;
        private SerializedObject _qualityProfileSerializedObject;
        private UnityEngine.Object _cachedStyleProfile;
        private UnityEngine.Object _cachedQualityProfile;
        private List<Water25DValidationResult> _validationResults = new List<Water25DValidationResult>();

        private void OnEnable()
        {
            for (var i = 0; i < targets.Length; i++)
            {
                Water25DEditorDefaults.AssignDefaults(targets[i] as Water25DController);
            }

            Undo.undoRedoPerformed += OnUndoRedo;
            RefreshProfileSerializedObjects();
            RefreshValidation();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            _styleProfileSerializedObject = null;
            _qualityProfileSerializedObject = null;
            _cachedStyleProfile = null;
            _cachedQualityProfile = null;
        }

        public override void OnInspectorGUI()
        {
            Water25DInspectorStyles.Ensure();
            serializedObject.Update();
            RefreshProfileSerializedObjects();

            DrawScriptProperty();
            DrawBasicSection();
            DrawRenderingSection();
            DrawFxSection();
            DrawPhysicsSection();
            DrawEventSection();
            DrawActionSection();

            var changed = serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                Water25DInspectorUtility.MarkControllerPropertiesChanged(serializedObject);
                for (var i = 0; i < targets.Length; i++)
                {
                    (targets[i] as Water25DController)?.RefreshAuthoringPreview();
                }
                RefreshProfileSerializedObjects();
                RefreshValidation();
                SceneView.RepaintAll();
            }
        }

        private void DrawScriptProperty()
        {
            var scriptProperty = serializedObject.FindProperty("m_Script");
            if (scriptProperty == null)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProperty);
            }
        }

        private void DrawBasicSection()
        {
            var open = BeginTopLevelSection("Basic", "Basic");
            if (open)
            {
                Property("_surfaceMode", "Surface Mode", "Existing serialized controllers remain SimulatedRipples. New controllers use FlatStylized; surface impacts use CRT or procedural rings according to this mode.");
                var size = serializedObject.FindProperty("_topSurfaceSize");
                if (size != null)
                {
                    EditorGUILayout.PropertyField(size.FindPropertyRelative("x"), new GUIContent("Width", "Width of the XZ top surface in local world units. Geometry is rebuilt when it changes."));
                    EditorGUILayout.PropertyField(size.FindPropertyRelative("y"), new GUIContent("Visual Depth", "Depth of the XZ top surface along local Z. This is separate from the physical front depth."));
                }

                Property("_frontSurfaceDepth", "Physical Depth", "Depth of the XY front surface and full buoyancy volume below the waterline.");
                Property("_waterlineLocalY", "Waterline", "Local Y coordinate of the waterline. The root transform is not recentered when this changes.");
                Property("_interactionDepth01", "Interaction Lane", "Normalized 0–1 depth lane used to map flat 2D gameplay positions into the visual XZ surface.");
                Property("_surfaceTriggerThickness", "Crossing Band Thickness", "Thickness of the separate surface-crossing trigger around the waterline.");
                Property("_synchronizeGeneratedChildLayers", "Synchronize Generated Child Layers", "Keep generated children on the controller's layer. Disable only when a deliberate layer layout is authored manually.");

                var showHandles = Water25DInspectorState.ShowSceneHandles;
                var nextShowHandles = EditorGUILayout.ToggleLeft(new GUIContent("Show Scene Handles", "Show optional width, visual-depth, physical-depth and waterline handles in the Scene View."), showHandles);
                if (nextShowHandles != showHandles)
                {
                    Water25DInspectorState.ShowSceneHandles = nextShowHandles;
                    SceneView.RepaintAll();
                }
            }

            EndTopLevelSection();
        }

        private void DrawRenderingSection()
        {
            var open = BeginTopLevelSection("Rendering", "Rendering");
            if (open)
            {
                var styleProperty = serializedObject.FindProperty("_styleProfile");
                if (styleProperty != null)
                {
                    EditorGUILayout.PropertyField(styleProperty, new GUIContent("Style Profile", "Shared colors and analytical wave settings. Changes affect every Water25D object using this asset."));
                    DrawProfileActions(styleProperty, styleProperty.objectReferenceValue as WaterStyleProfile, true);
                }

                var qualityProperty = serializedObject.FindProperty("_qualityProfile");
                if (qualityProperty != null)
                {
                    EditorGUILayout.PropertyField(qualityProperty, new GUIContent("Quality Profile", "Shared ripple scheduling, resolution, wave behaviour and geometry settings."));
                    DrawProfileActions(qualityProperty, qualityProperty.objectReferenceValue as WaterQualityProfile, false);
                }

                var styleProfile = GetStyleProfile();
                var appearanceOpen = BeginNestedSection("Rendering.Appearance", "Top / Front Appearance");
                if (appearanceOpen && styleProfile != null && _styleProfileSerializedObject != null)
                {
                    DrawSharedProfileNotice(styleProfile, true);
                    WaterStyleProfileEditor.DrawSurfaceFields(_styleProfileSerializedObject);
                }

                var materialOpen = BeginNestedSection("Rendering.Materials", "Material Templates and Sorting");
                if (materialOpen)
                {
                    DrawMaterialRow("Top Material Template", "_topMaterialTemplate", Water25DInspectorUtility.TopMaterialPath, styleProfile != null ? styleProfile.TopMaterialTemplate : null);
                    DrawMaterialRow("Front Material Template", "_frontMaterialTemplate", Water25DInspectorUtility.FrontMaterialPath, styleProfile != null ? styleProfile.FrontMaterialTemplate : null);
                    DrawMaterialRow("Ripple Simulation Material", "_rippleSimulationMaterialTemplate", Water25DInspectorUtility.RippleMaterialPath, null);
                    DrawSortingLayerProperty("Top Sorting Layer", "_topSortingLayerName", "Sorting layer for the XZ top renderer.");
                    Property("_topSortingOrder", "Top Sorting Order", "Ordering within the selected top sorting layer.");
                    DrawSortingLayerProperty("Front Sorting Layer", "_frontSortingLayerName", "Sorting layer for the XY front renderer.");
                    Property("_frontSortingOrder", "Front Sorting Order", "Ordering within the selected front sorting layer.");

                    if (styleProfile != null && _styleProfileSerializedObject != null)
                    {
                        WaterStyleProfileEditor.DrawMaterialFields(_styleProfileSerializedObject);
                    }
                }

                DrawAmbientWavesSection();
                DrawContactRipplesSection();
                DrawReflectionSection();
            }

            EndTopLevelSection();
        }

        private void DrawAmbientWavesSection()
        {
            var open = BeginNestedSection("Rendering.AmbientWaves", "Ambient Waves");
            if (!open)
            {
                return;
            }

            var styleProfile = GetStyleProfile();
            if (styleProfile != null && _styleProfileSerializedObject != null)
            {
                DrawSharedProfileNotice(styleProfile, true);
                WaterStyleProfileEditor.DrawAmbientFields(_styleProfileSerializedObject);
                if (GUILayout.Button(new GUIContent("Reset Wave Settings", "Restore amplitude, wavelength, speed and direction from the Water25D default style asset."), Water25DInspectorStyles.SmallButton))
                {
                    WaterStyleProfileEditor.ResetAmbientWaveSettings(_styleProfileSerializedObject);
                }
            }

            var qualityProfile = GetQualityProfile();
            if (qualityProfile != null && _qualityProfileSerializedObject != null)
            {
                WaterQualityProfileEditor.DrawAmbientBandField(_qualityProfileSerializedObject);
            }
        }

        private void DrawContactRipplesSection()
        {
            var open = BeginNestedSection("Rendering.ContactRipples", "Contact Ripples");
            if (!open)
            {
                return;
            }

            var flatMode = GetSurfaceMode() == WaterSurfaceMode.FlatStylized;
            EditorGUILayout.HelpBox(
                flatMode
                    ? "Procedural surface rings active. FlatStylized does not allocate or update the CRT; contact foam, wakes and splash redesign remain separate slices."
                    : "SimulatedRipples uses the instance-owned CRT for surface impacts. Procedural surface rings are inactive in this mode.",
                MessageType.Info);

            Property("_impactSpeedForFullStrength", "Full-strength Impact Speed", "Velocity magnitude in world units per second that reaches full impact strength.");
            Property("_minimumImpactStrength", "Minimum Impact Strength", "Floor applied to non-zero impact strength before the multiplier.");
            Property("_impactStrengthMultiplier", "Impact Strength Multiplier", "Scales calculated impact strength before it is clamped to 0–1.");

            var qualityProfile = GetQualityProfile();
            var styleProfile = GetStyleProfile();
            if (flatMode)
            {
                if (qualityProfile != null && _qualityProfileSerializedObject != null)
                {
                    DrawSharedProfileNotice(qualityProfile, false);
                    DrawNestedQualitySection(_qualityProfileSerializedObject, "Surface Rings", "Rendering.ContactRipples.SurfaceRings", WaterQualityProfileEditor.DrawSurfaceRingFields);
                }

                if (styleProfile != null && _styleProfileSerializedObject != null)
                {
                    DrawSharedProfileNotice(styleProfile, true);
                    WaterStyleProfileEditor.DrawRingFields(_styleProfileSerializedObject);
                }

                var flatController = target as Water25DController;
                if (flatController != null)
                {
                    Metric("Maximum Active Rings", qualityProfile != null
                        ? qualityProfile.GetSettings().MaximumSurfaceRings
                        : WaterQualitySettings.Default.MaximumSurfaceRings);
                    Metric("Runtime Active Rings", flatController.ActiveSurfaceRingCount);
                    Metric("Runtime Replacements", flatController.ReplacedSurfaceRingCount);
                }
            }
            else
            {
                Property("_enableRippleSimulation", "Ripples Enabled", "Allocate and update the instance-owned CRT ripple state in Play Mode.");
                if (qualityProfile != null && _qualityProfileSerializedObject != null)
                {
                    DrawSharedProfileNotice(qualityProfile, false);
                    DrawNestedQualitySection(_qualityProfileSerializedObject, "Resolution", "Rendering.ContactRipples.Resolution", WaterQualityProfileEditor.DrawRippleResolutionFields);
                    DrawNestedQualitySection(_qualityProfileSerializedObject, "Scheduling", "Rendering.ContactRipples.Scheduling", WaterQualityProfileEditor.DrawSchedulingFields);
                    DrawNestedQualitySection(_qualityProfileSerializedObject, "Wave Behaviour", "Rendering.ContactRipples.WaveBehaviour", WaterQualityProfileEditor.DrawWaveBehaviourFields);
                }

                if (styleProfile != null && _styleProfileSerializedObject != null)
                {
                    WaterStyleProfileEditor.DrawRippleFields(_styleProfileSerializedObject);
                }
            }
        }

        private void DrawReflectionSection()
        {
            var open = BeginNestedSection("Rendering.Reflection", "Reflection");
            if (!open)
            {
                return;
            }

            var modeProperty = serializedObject.FindProperty("_reflectionMode");
            if (modeProperty != null)
            {
                EditorGUILayout.PropertyField(modeProperty, new GUIContent("Reflection Mode", "Disabled, camera-free Stylized fallback, or shared adaptive Planar reflection."));
            }

            var mode = modeProperty != null ? (WaterReflectionMode)modeProperty.enumValueIndex : WaterReflectionMode.Stylized;
            if (mode == WaterReflectionMode.Stylized)
            {
                Property("_reflectionStrength", "Reflection Strength", "Strength of the current camera-free stylized reflection fallback.");
                return;
            }

            if (mode == WaterReflectionMode.Planar)
            {
                Property("_reflectionCameraSource", "Camera Source", "Camera used to define the shared planar reflection group. Assign explicitly for deterministic grouping.");
                Property("_reflectionCullingMask", "Planar Culling Mask", "Layers rendered by the shared reflection camera. Exclude water and reflection-helper layers when recursion is possible.");
                Property("_reflectionResolutionScale", "Resolution Scale", "Fraction of the source camera resolution used by the shared reflection texture.");
                Property("_reflectionUpdateIntervalFrames", "Update Interval", "Minimum frame interval between adaptive reflection renders unless the camera moves.");
                Property("_reflectionStrength", "Reflection Strength", "Blend strength applied to the planar reflection.");
                return;
            }

            Property("_reflectionStrength", "Reflection Strength", "Blend strength applied to the reflection when enabled.");
        }

        private void DrawFxSection()
        {
            var open = BeginTopLevelSection("FX", "FX");
            if (open)
            {
                Property("_enableEffects", "Effects Enabled", "Enable pooled splash and bubble presentation effects for interaction events.");
                DrawFxDefinitionRow("Splash Definition", "_splashDefinition", "Splash", "Definition used for surface enter and exit effects.");
                DrawFxDefinitionRow("Bubble Definition", "_bubbleDefinition", "Bubble", "Definition used for submerged effects.");
                Property("_maximumFxPoolSize", "Pool Size", "Upper bound for each fixed-capacity effect pool. Exhaustion rejects requests instead of instantiating during gameplay.");
            }

            EndTopLevelSection();
        }

        private void DrawPhysicsSection()
        {
            var open = BeginTopLevelSection("Physics", "Physics");
            if (open)
            {
                EditorGUILayout.LabelField("Buoyancy", EditorStyles.boldLabel);
                Property("_enableBuoyancy", "Enable Buoyancy", "Enable the full underwater trigger and its BuoyancyEffector2D.");
                Property("_buoyancyDensity", "Density", "Density used by the generated BuoyancyEffector2D.");
                Property("_buoyancyLayers", "Buoyancy Layers", "Rigidbody2D layers eligible for the full underwater volume.");
                Property("_buoyancyLinearDamping", "Effector Linear Damping", "Linear damping applied by BuoyancyEffector2D when custom drag is disabled or kept modest.");
                Property("_buoyancyAngularDamping", "Effector Angular Damping", "Angular damping applied by BuoyancyEffector2D when custom drag is disabled or kept modest.");

                EditorGUILayout.LabelField("Drag", EditorStyles.boldLabel);
                Property("_enableCustomDrag", "Enable Custom Drag", "Apply optional per-Rigidbody2D drag from WaterPhysicsVolume2D in addition to effector damping.");
                Property("_customLinearDrag", "Custom Linear Drag", "Additional linear drag applied once per logical Rigidbody2D contact.");
                Property("_customAngularDrag", "Custom Angular Drag", "Additional angular drag applied once per logical Rigidbody2D contact.");
                if (GetFloat("_customLinearDrag", 0f) > 1f && GetFloat("_buoyancyLinearDamping", 0f) > 1f ||
                    GetFloat("_customAngularDrag", 0f) > 1f && GetFloat("_buoyancyAngularDamping", 0f) > 1f)
                {
                    EditorGUILayout.HelpBox("Strong custom drag and strong effector damping may over-damp motion. Tune one source down if needed.", MessageType.Warning);
                }

                EditorGUILayout.LabelField("Surface Interaction", EditorStyles.boldLabel);
                Property("_enableSurfaceInteraction", "Interaction Enabled", "Enable the thin surface-crossing trigger and logical enter/exit tracking.");
                Property("_surfaceInteractionLayers", "Surface Interaction Layers", "Non-trigger collider layers eligible for surface crossings and contact ripples.");
                Property("_surfaceTriggerInteractionLayers", "Trigger Interaction Layers", "Trigger collider layers eligible for surface crossings when trigger colliders are included.");
                Property("_includeTriggerCollidersInSurfaceInteraction", "Include Trigger Colliders", "Allow eligible trigger colliders to produce one logical surface contact per Rigidbody2D.");
            }

            EndTopLevelSection();
        }

        private void DrawEventSection()
        {
            var open = BeginTopLevelSection("Event", "Event");
            if (open)
            {
                DrawEventProperty("_onSurfaceEnter", "Surface Enter", "Fires once when a logical Rigidbody2D first crosses the thin surface trigger.");
                DrawEventProperty("_onSurfaceExit", "Surface Exit", "Fires once when the last collider of a logical Rigidbody2D leaves the thin surface trigger.");
                DrawEventProperty("_onSubmerged", "Submerged", "Fires once when a logical Rigidbody2D first enters the full buoyancy volume.");
                DrawEventProperty("_onResurfaced", "Resurfaced", "Fires once when the last collider of a logical Rigidbody2D leaves the full buoyancy volume.");
            }

            EndTopLevelSection();
        }

        private void DrawActionSection()
        {
            var open = BeginTopLevelSection("Action", "Action");
            if (open)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Assign Defaults", "Assign only missing package-owned defaults.")))
                {
                    AssignPackageDefaults();
                }

                if (GUILayout.Button(new GUIContent("Repair Hierarchy", "Repair generated children and rebuild the transient preview.")))
                {
                    RepairHierarchy();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Refresh Preview", "Reapply property blocks and refresh the edit-mode preview.")))
                {
                    RefreshPreview();
                }

                if (GUILayout.Button(new GUIContent("Rebuild Geometry", "Rebuild transient top/front preview meshes using current dimensions and quality density.")))
                {
                    RebuildGeometry();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button(new GUIContent("Reset Ripple Simulation", "Clear the instance-owned runtime ripple state. Available in Play Mode.")))
                    {
                        ResetRippleSimulation();
                    }
                }

                using (new EditorGUI.DisabledScope(targets.Length != 1))
                {
                    if (GUILayout.Button(new GUIContent("Make Profiles Unique", "Duplicate assigned style and quality profiles and assign the copies to this controller.")))
                    {
                        MakeProfilesUnique();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(targets.Length != 1))
                {
                    var controller = target as Water25DController;
                    if (GUILayout.Button(new GUIContent("Select Top Surface", "Select the generated TopSurface child.")))
                    {
                        Water25DInspectorUtility.SelectObject(controller != null ? controller.TopSurface : null);
                    }

                    if (GUILayout.Button(new GUIContent("Select Front Surface", "Select the generated FrontSurface child.")))
                    {
                        Water25DInspectorUtility.SelectObject(controller != null ? controller.FrontSurface : null);
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button(new GUIContent("Open Setup Documentation", "Open the Water25D setup and authoring documentation.")))
                {
                    Water25DInspectorUtility.OpenDocumentation(SetupDocumentationPath);
                }

                var diagnosticsOpen = BeginNestedSection("Action.Diagnostics", "Diagnostics");
                if (diagnosticsOpen)
                {
                    DrawDiagnostics(target as Water25DController);
                }

                var advancedOpen = BeginNestedSection("Action.Advanced", "Advanced");
                if (advancedOpen)
                {
                    DrawAdvancedDetails(target as Water25DController);
                }
            }

            EndTopLevelSection();
        }

        private void DrawDiagnostics(Water25DController controller)
        {
            DrawValidationResults();

            EditorGUILayout.LabelField("Performance Estimates", EditorStyles.boldLabel);
            var metrics = Water25DInspectorUtility.CalculateMetrics(controller);
            var flatMode = GetSurfaceMode() == WaterSurfaceMode.FlatStylized;
            Metric("Surface Mode", GetSurfaceMode().ToString());
            Metric("Ripple Simulation", flatMode ? "Inactive (FlatStylized)" : "Active in Play Mode");
            Metric("Procedural Rings", flatMode ? "Active in FlatStylized" : "Inactive (SimulatedRipples)");
            Metric("Top Mesh Vertices", metrics.TopVertexCount.x * metrics.TopVertexCount.y);
            Metric("Top Mesh Triangles", metrics.TopTriangleCount);
            Metric("Front Mesh Vertices", metrics.FrontVertexCount);
            Metric("Front Mesh Triangles", metrics.FrontTriangleCount);
            Metric("Top Vertices Per Unit", metrics.TopVerticesPerUnit.ToString("0.##"));
            Metric("Ripple Resolution", metrics.RippleResolution.x + " x " + metrics.RippleResolution.y);
            Metric("Ripple State", Water25DInspectorUtility.FormatBytes(metrics.RippleStateBytes));
            Metric("Ripple Format", metrics.UsesRgHalf ? "RGHalf" : "RGFloat fallback");
            Metric("Mipmaps", "Disabled");
            Metric("Simulation Updates / Second", metrics.SimulationFrequency.ToString("0.##"));
            Metric("Propagation Substeps", metrics.PropagationSubsteps);
            Metric("Propagated Cells / Second", metrics.PropagatedCellsPerSecond.ToString("N0"));
            Metric("Maximum Impacts / Update", metrics.MaximumImpactsPerStep);
            Metric("Queue Capacity", metrics.MaximumQueuedImpacts);
            var qualityProfile = GetQualityProfile();
            Metric("Maximum Active Rings", qualityProfile != null
                ? qualityProfile.GetSettings().MaximumSurfaceRings
                : WaterQualitySettings.Default.MaximumSurfaceRings);
            Metric("Runtime Active Rings", controller != null ? controller.ActiveSurfaceRingCount : 0);
            Metric("Runtime Ring Replacements", controller != null ? controller.ReplacedSurfaceRingCount : 0);

            if (GetReflectionMode() == WaterReflectionMode.Planar)
            {
                EditorGUILayout.LabelField("Reflection Estimate", EditorStyles.boldLabel);
                var scale = GetFloat("_reflectionResolutionScale", 0.25f);
                var camera = GetReflectionCamera();
                if (camera != null && camera.pixelWidth > 0 && camera.pixelHeight > 0)
                {
                    Metric("Resolution Scale", scale.ToString("0.##"));
                    Metric("Estimated Texture", Mathf.Max(1, Mathf.RoundToInt(camera.pixelWidth * scale)) + " x " + Mathf.Max(1, Mathf.RoundToInt(camera.pixelHeight * scale)));
                }
                else
                {
                    Metric("Estimated Texture", "Camera dimensions unavailable");
                }

                var interval = GetInt("_reflectionUpdateIntervalFrames", 3);
                Metric("Update Interval", interval + " frame(s)");
                Metric("Estimated Maximum Frequency", (60f / Mathf.Max(1, interval)).ToString("0.##") + " Hz at 60 FPS");
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Validate", "Refresh validation findings without changing authored data."), Water25DInspectorStyles.SmallButton))
            {
                RefreshValidation();
            }

            using (new EditorGUI.DisabledScope(!HasSafeFixes()))
            {
                if (GUILayout.Button(new GUIContent("Fix All Safe Issues", "Apply package-owned, Undo-backed repairs without changing unrelated authored children."), Water25DInspectorStyles.SmallButton))
                {
                    FixAllSafeIssues();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (Application.isPlaying && controller != null)
            {
                var canTestSurfaceImpact = GetSurfaceMode() == WaterSurfaceMode.FlatStylized || GetBool("_enableRippleSimulation", true);
                using (new EditorGUI.DisabledScope(!canTestSurfaceImpact))
                {
                    if (GUILayout.Button(new GUIContent("Test Surface Impact at Center", "Create a mode-appropriate test impact at the centre of the water in Play Mode."), Water25DInspectorStyles.SmallButton))
                    {
                        CreateTestRipple(controller);
                    }
                }
            }
        }

        private void DrawValidationResults()
        {
            if (_validationResults == null)
            {
                RefreshValidation();
            }

            for (var i = 0; i < _validationResults.Count; i++)
            {
                var result = _validationResults[i];
                if (result.Severity == Water25DValidationSeverity.Info)
                {
                    EditorGUILayout.LabelField(result.Title, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(result.Message, EditorStyles.wordWrappedMiniLabel);
                }
                else
                {
                    var messageType = result.Severity == Water25DValidationSeverity.Error ? MessageType.Error : MessageType.Warning;
                    EditorGUILayout.HelpBox(result.Title + "\n" + result.Message, messageType);
                }

                if (result.HasFix || result.TargetObject != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (result.HasFix)
                    {
                        if (GUILayout.Button(new GUIContent(GetFixLabel(result.FixAction), "Apply this safe, non-destructive repair."), Water25DInspectorStyles.SmallButton))
                        {
                            ApplyFix(result.FixAction, result.TargetObject);
                        }
                    }

                    if (result.TargetObject != null && result.FixAction != Water25DFixAction.SelectObject)
                    {
                        if (GUILayout.Button(new GUIContent("Select", "Select the object associated with this diagnostic."), Water25DInspectorStyles.SmallButton))
                        {
                            Water25DInspectorUtility.SelectObject(result.TargetObject);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawAdvancedDetails(Water25DController controller)
        {
            EditorGUILayout.LabelField("Generated Hierarchy", EditorStyles.boldLabel);
            DrawGeneratedReference("Top Surface", controller != null ? controller.TopSurface : null);
            DrawGeneratedReference("Front Surface", controller != null ? controller.FrontSurface : null);
            DrawGeneratedReference("Surface Crossing Trigger", controller != null ? controller.SurfaceCrossingTrigger : null);
            DrawGeneratedReference("Buoyancy Volume", controller != null ? controller.BuoyancyVolume : null);
            DrawGeneratedReference("Reflection Anchor", controller != null ? controller.ReflectionAnchor : null);
            DrawGeneratedReference("FX Root", controller != null ? controller.FxRoot : null);
        }

        private bool BeginTopLevelSection(string key, string label)
        {
            var stateKey = TopLevelFoldoutPrefix + key;
            var open = Water25DInspectorState.GetFoldout(stateKey, false);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Keep the section as a help-box container with a compact button-like header
            // inset into it, using only standard Unity editor APIs.
            var row = EditorGUILayout.GetControlRect(GUILayout.MinWidth(0f));
            var header = row;
            header.yMin -= 3f;
            header.yMax += 3f;
            header.xMin -= 4f;
            header.xMax += 4f;
            EditorGUI.LabelField(header, GUIContent.none, GUI.skin.button);

            var arrow = row;
            arrow.x += 1.5f;
            arrow.y -= 1f;
            arrow.y += row.height * 0.23f;
            arrow.width = 13f;
            arrow.height = 13f;
            var next = GUI.Toggle(arrow, open, GUIContent.none, EditorStyles.foldout);

            var labelRect = row;
            labelRect.x += 17f;
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRect.Contains(Event.current.mousePosition))
            {
                next = !next;
                GUI.changed = true;
                Event.current.Use();
            }

            if (next != open)
            {
                Water25DInspectorState.SetFoldout(stateKey, next);
            }

            if (next)
            {
                EditorGUILayout.Space(2f);
            }

            return next;
        }

        private void EndTopLevelSection()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2f);
        }

        private bool BeginNestedSection(string key, string label)
        {
            var stateKey = "Controller.Nested." + key;
            var open = Water25DInspectorState.GetFoldout(stateKey, false);
            var next = EditorGUILayout.Foldout(open, label, true);
            if (next != open)
            {
                Water25DInspectorState.SetFoldout(stateKey, next);
            }

            return next;
        }

        private void DrawNestedQualitySection(SerializedObject profile, string label, string stateKey, Func<SerializedObject, bool> draw)
        {
            if (BeginNestedSection(stateKey, label) && profile != null)
            {
                draw(profile);
            }
        }

        private void DrawSharedProfileNotice(UnityEngine.Object profile, bool style)
        {
            var users = Water25DInspectorUtility.CountProfileUsers(profile);
            var isDefault = style
                ? Water25DInspectorUtility.IsPackageDefaultStyle(profile as WaterStyleProfile)
                : Water25DInspectorUtility.IsPackageDefaultQuality(profile as WaterQualityProfile);
            if (!isDefault && users <= 1)
            {
                return;
            }

            var message = isDefault
                ? "This is a package default asset. Changes affect every Water25D object using it; use Make Unique Copy before creating a one-off customization."
                : "Changes to this profile affect every Water25D object using it (" + users + " loaded user(s)).";
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }

        private void DrawProfileActions(SerializedProperty profileProperty, UnityEngine.Object profile, bool style)
        {
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(profile == null || targets.Length != 1))
            {
                if (GUILayout.Button(new GUIContent("Select", "Select the assigned profile asset."), Water25DInspectorStyles.SmallButton))
                {
                    Water25DInspectorUtility.SelectObject(profile);
                }

                if (GUILayout.Button(new GUIContent("Ping", "Ping the assigned profile asset in the Project window."), Water25DInspectorStyles.SmallButton))
                {
                    EditorGUIUtility.PingObject(profile);
                }
            }

            if (GUILayout.Button(new GUIContent("Create New", "Create a profile initialized from the package default and assign it."), Water25DInspectorStyles.SmallButton))
            {
                if (style)
                {
                    var created = Water25DInspectorUtility.CreateProfileAsset(
                        Water25DInspectorUtility.LoadPackageAsset<WaterStyleProfile>(Water25DInspectorUtility.StyleProfilePath),
                        target.name,
                        "Style");
                    Water25DInspectorUtility.AssignObjectReference(serializedObject, profileProperty.propertyPath, created, "Assign New Water Style Profile");
                }
                else
                {
                    var created = Water25DInspectorUtility.CreateProfileAsset(
                        Water25DInspectorUtility.LoadPackageAsset<WaterQualityProfile>(Water25DInspectorUtility.QualityProfilePath),
                        target.name,
                        "Quality");
                    Water25DInspectorUtility.AssignObjectReference(serializedObject, profileProperty.propertyPath, created, "Assign New Water Quality Profile");
                }
                RefreshProfileSerializedObjects();
            }

            using (new EditorGUI.DisabledScope(profile == null || targets.Length != 1))
            {
                if (GUILayout.Button(new GUIContent("Duplicate", "Duplicate the assigned profile asset and assign the copy."), Water25DInspectorStyles.SmallButton))
                {
                    DuplicateAndAssignProfile(profile, profileProperty, style);
                }

                if (GUILayout.Button(new GUIContent("Make Unique Copy", "Create and assign a private copy before editing this shared asset."), Water25DInspectorStyles.SmallButton))
                {
                    DuplicateAndAssignProfile(profile, profileProperty, style);
                }
            }

            if (GUILayout.Button(new GUIContent("Package Default", "Assign the package default without modifying that asset."), Water25DInspectorStyles.SmallButton))
            {
                UnityEngine.Object defaultProfile;
                if (style)
                {
                    defaultProfile = Water25DInspectorUtility.LoadPackageAsset<WaterStyleProfile>(Water25DInspectorUtility.StyleProfilePath);
                }
                else
                {
                    defaultProfile = Water25DInspectorUtility.LoadPackageAsset<WaterQualityProfile>(Water25DInspectorUtility.QualityProfilePath);
                }
                Water25DInspectorUtility.AssignObjectReference(serializedObject, profileProperty.propertyPath, defaultProfile, "Assign Water25D Package Default");
                RefreshProfileSerializedObjects();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DuplicateAndAssignProfile(UnityEngine.Object profile, SerializedProperty profileProperty, bool style)
        {
            if (profile == null || targets.Length != 1)
            {
                return;
            }

            UnityEngine.Object duplicate;
            if (style)
            {
                duplicate = Water25DInspectorUtility.DuplicateProfileAsset(profile as WaterStyleProfile, target.name, "Style");
            }
            else
            {
                duplicate = Water25DInspectorUtility.DuplicateProfileAsset(profile as WaterQualityProfile, target.name, "Quality");
            }

            if (duplicate != null)
            {
                Undo.RegisterCreatedObjectUndo(duplicate, "Create Water25D Profile Copy");
                Water25DInspectorUtility.AssignObjectReference(serializedObject, profileProperty.propertyPath, duplicate, "Assign Water25D Profile Copy");
                RefreshProfileSerializedObjects();
            }
        }

        private void DrawMaterialRow(string label, string propertyPath, string packagePath, Material profileFallback)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                return;
            }

            var assigned = property.objectReferenceValue as Material;
            var resolved = assigned != null ? assigned : profileFallback;
            if (resolved == null)
            {
                var controller = target as Water25DController;
                var surface = propertyPath == "_topMaterialTemplate"
                    ? controller != null ? controller.TopSurface : null
                    : controller != null ? controller.FrontSurface : null;
                var renderer = surface != null ? surface.GetComponent<MeshRenderer>() : null;
                resolved = renderer != null ? renderer.sharedMaterial : null;
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label, "Persistent material template. Runtime property blocks carry per-water values without mutating this asset."));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.labelWidth);
            using (new EditorGUI.DisabledScope(resolved == null))
            {
                if (GUILayout.Button(new GUIContent("Select", "Select the resolved material asset."), Water25DInspectorStyles.SmallButton))
                {
                    Water25DInspectorUtility.SelectObject(resolved);
                }
            }

            if (GUILayout.Button(new GUIContent("Assign Package", "Assign the persistent package material template."), Water25DInspectorStyles.SmallButton))
            {
                var packageMaterial = Water25DInspectorUtility.LoadPackageAsset<Material>(packagePath);
                Water25DInspectorUtility.AssignObjectReference(serializedObject, propertyPath, packageMaterial, "Assign Water25D Package Material");
            }

            using (new EditorGUI.DisabledScope(assigned != null))
            {
                if (GUILayout.Button(new GUIContent("Repair Missing", "Assign the package material only when this controller field is empty."), Water25DInspectorStyles.SmallButton))
                {
                    var packageMaterial = Water25DInspectorUtility.LoadPackageAsset<Material>(packagePath);
                    Water25DInspectorUtility.AssignObjectReference(serializedObject, propertyPath, packageMaterial, "Repair Water25D Material");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSortingLayerProperty(string label, string propertyPath, string tooltip)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                return;
            }

            var layers = SortingLayer.layers;
            if (layers == null || layers.Length == 0)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
                return;
            }

            var names = new string[layers.Length];
            var currentIndex = 0;
            for (var i = 0; i < layers.Length; i++)
            {
                names[i] = layers[i].name;
                if (names[i] == property.stringValue)
                {
                    currentIndex = i;
                }
            }

            var previousMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            var nextIndex = EditorGUILayout.Popup(new GUIContent(label, tooltip), currentIndex, names);
            if (EditorGUI.EndChangeCheck() && nextIndex >= 0 && nextIndex < names.Length)
            {
                Undo.RecordObjects(serializedObject.targetObjects, "Change Water25D Sorting Layer");
                property.stringValue = names[nextIndex];
                serializedObject.ApplyModifiedProperties();
                Water25DInspectorUtility.MarkControllerPropertiesChanged(serializedObject);
            }
            EditorGUI.showMixedValue = previousMixed;
        }

        private void DrawFxDefinitionRow(string label, string propertyPath, string suffix, string tooltip)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
            if (GUILayout.Button(new GUIContent("Create", "Create a package-owned WaterFXDefinition asset and assign it."), Water25DInspectorStyles.SmallButton))
            {
                var definition = Water25DInspectorUtility.CreateFxDefinitionAsset(target.name, suffix);
                Water25DInspectorUtility.AssignObjectReference(serializedObject, propertyPath, definition, "Assign Water25D FX Definition");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEventProperty(string propertyPath, string label, string explanation)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label, explanation), true);
            }
        }

        private void DrawGeneratedReference(string label, Transform reference)
        {
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(label, reference, typeof(Transform), true);
            }

            using (new EditorGUI.DisabledScope(reference == null))
            {
                if (GUILayout.Button(new GUIContent("Select", "Select the generated child without exposing it as an editable field."), Water25DInspectorStyles.SmallButton))
                {
                    Water25DInspectorUtility.SelectObject(reference);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void Metric(string label, object value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            EditorGUILayout.LabelField(value != null ? value.ToString() : "—", GUILayout.MinWidth(100f));
            EditorGUILayout.EndHorizontal();
        }

        private void Property(string propertyPath, string label, string tooltip)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip), true);
            }
        }

        private bool GetBool(string propertyPath, bool fallback)
        {
            var property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.boolValue : fallback;
        }

        private float GetFloat(string propertyPath, float fallback)
        {
            var property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.floatValue : fallback;
        }

        private int GetInt(string propertyPath, int fallback)
        {
            var property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.intValue : fallback;
        }

        private WaterReflectionMode GetReflectionMode()
        {
            var property = serializedObject.FindProperty("_reflectionMode");
            return property != null ? (WaterReflectionMode)property.enumValueIndex : WaterReflectionMode.Stylized;
        }

        private WaterSurfaceMode GetSurfaceMode()
        {
            var property = serializedObject.FindProperty("_surfaceMode");
            return property != null ? (WaterSurfaceMode)property.enumValueIndex : WaterSurfaceMode.SimulatedRipples;
        }

        private Camera GetReflectionCamera()
        {
            var property = serializedObject.FindProperty("_reflectionCameraSource");
            return property != null ? property.objectReferenceValue as Camera : null;
        }

        private WaterStyleProfile GetStyleProfile()
        {
            var property = serializedObject.FindProperty("_styleProfile");
            return property != null ? property.objectReferenceValue as WaterStyleProfile : null;
        }

        private WaterQualityProfile GetQualityProfile()
        {
            var property = serializedObject.FindProperty("_qualityProfile");
            return property != null ? property.objectReferenceValue as WaterQualityProfile : null;
        }

        private void RefreshProfileSerializedObjects()
        {
            if (serializedObject == null)
            {
                return;
            }

            serializedObject.UpdateIfRequiredOrScript();
            var style = serializedObject.FindProperty("_styleProfile");
            var quality = serializedObject.FindProperty("_qualityProfile");
            var styleProfile = style != null && !style.hasMultipleDifferentValues ? style.objectReferenceValue : null;
            var qualityProfile = quality != null && !quality.hasMultipleDifferentValues ? quality.objectReferenceValue : null;
            if (styleProfile != _cachedStyleProfile || _styleProfileSerializedObject == null || _styleProfileSerializedObject.targetObject != styleProfile)
            {
                _cachedStyleProfile = styleProfile;
                _styleProfileSerializedObject = styleProfile != null ? new SerializedObject(styleProfile) : null;
            }

            if (qualityProfile != _cachedQualityProfile || _qualityProfileSerializedObject == null || _qualityProfileSerializedObject.targetObject != qualityProfile)
            {
                _cachedQualityProfile = qualityProfile;
                _qualityProfileSerializedObject = qualityProfile != null ? new SerializedObject(qualityProfile) : null;
            }
        }

        private void RefreshValidation()
        {
            var controller = target as Water25DController;
            _validationResults = controller != null
                ? Water25DValidation.Validate(controller)
                : new List<Water25DValidationResult>();
            Repaint();
        }

        private bool HasSafeFixes()
        {
            for (var i = 0; i < _validationResults.Count; i++)
            {
                if (_validationResults[i].HasFix && _validationResults[i].FixAction != Water25DFixAction.SelectObject)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetFixLabel(Water25DFixAction action)
        {
            switch (action)
            {
                case Water25DFixAction.AssignPackageDefaults:
                    return "Assign Defaults";
                case Water25DFixAction.RepairHierarchy:
                    return "Repair Hierarchy";
                case Water25DFixAction.AssignTopMaterial:
                    return "Assign Top Material";
                case Water25DFixAction.AssignFrontMaterial:
                    return "Assign Front Material";
                case Water25DFixAction.AssignRippleMaterial:
                    return "Assign Ripple Material";
                case Water25DFixAction.AssignStyleProfile:
                    return "Assign Style Default";
                case Water25DFixAction.AssignQualityProfile:
                    return "Assign Quality Default";
                default:
                    return "Fix";
            }
        }

        private void ApplyFix(Water25DFixAction action, UnityEngine.Object targetObject)
        {
            switch (action)
            {
                case Water25DFixAction.AssignPackageDefaults:
                    AssignPackageDefaults();
                    break;
                case Water25DFixAction.RepairHierarchy:
                    RepairHierarchy();
                    break;
                case Water25DFixAction.AssignTopMaterial:
                    AssignPackageReference("_topMaterialTemplate", Water25DInspectorUtility.TopMaterialPath, "Assign Top Material");
                    break;
                case Water25DFixAction.AssignFrontMaterial:
                    AssignPackageReference("_frontMaterialTemplate", Water25DInspectorUtility.FrontMaterialPath, "Assign Front Material");
                    break;
                case Water25DFixAction.AssignRippleMaterial:
                    AssignPackageReference("_rippleSimulationMaterialTemplate", Water25DInspectorUtility.RippleMaterialPath, "Assign Ripple Material");
                    break;
                case Water25DFixAction.AssignStyleProfile:
                    AssignPackageReference("_styleProfile", Water25DInspectorUtility.StyleProfilePath, "Assign Style Profile");
                    break;
                case Water25DFixAction.AssignQualityProfile:
                    AssignPackageReference("_qualityProfile", Water25DInspectorUtility.QualityProfilePath, "Assign Quality Profile");
                    break;
                case Water25DFixAction.SelectObject:
                    Water25DInspectorUtility.SelectObject(targetObject);
                    break;
            }

            RefreshValidation();
        }

        private void FixAllSafeIssues()
        {
            AssignPackageDefaults();
            RepairHierarchy();
            RefreshValidation();
        }

        private void AssignPackageDefaults()
        {
            for (var i = 0; i < targets.Length; i++)
            {
                var controller = targets[i] as Water25DController;
                if (controller == null)
                {
                    continue;
                }

                Undo.RegisterFullObjectHierarchyUndo(controller.gameObject, "Assign Water25D Package Defaults");
                Water25DEditorDefaults.AssignDefaults(controller);
            }

            serializedObject.Update();
            RefreshProfileSerializedObjects();
            RefreshValidation();
        }

        private void AssignPackageReference(string propertyPath, string assetPath, string undoName)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            Water25DInspectorUtility.AssignObjectReference(serializedObject, propertyPath, asset, undoName);
            RefreshProfileSerializedObjects();
        }

        private void RepairHierarchy()
        {
            for (var i = 0; i < targets.Length; i++)
            {
                Water25DInspectorUtility.RepairHierarchyWithUndo(targets[i] as Water25DController);
            }

            serializedObject.Update();
            RefreshValidation();
        }

        private void RefreshPreview()
        {
            for (var i = 0; i < targets.Length; i++)
            {
                var controller = targets[i] as Water25DController;
                controller?.RefreshAuthoringPreview();
            }
            SceneView.RepaintAll();
            Repaint();
        }

        private void RebuildGeometry()
        {
            for (var i = 0; i < targets.Length; i++)
            {
                Water25DInspectorUtility.RebuildGeometry(targets[i] as Water25DController);
            }
            RefreshValidation();
        }

        private void MakeProfilesUnique()
        {
            if (targets.Length != 1)
            {
                return;
            }

            var styleProperty = serializedObject.FindProperty("_styleProfile");
            var qualityProperty = serializedObject.FindProperty("_qualityProfile");
            DuplicateAndAssignProfile(styleProperty != null ? styleProperty.objectReferenceValue : null, styleProperty, true);
            DuplicateAndAssignProfile(qualityProperty != null ? qualityProperty.objectReferenceValue : null, qualityProperty, false);
        }

        private void ResetRippleSimulation()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            for (var i = 0; i < targets.Length; i++)
            {
                (targets[i] as Water25DController)?.ResetRippleSimulation();
            }
        }

        private void CreateTestRipple(Water25DController controller)
        {
            if (controller == null || !Application.isPlaying)
            {
                return;
            }

            var size = controller.TopSurfaceSize;
            var localPosition = new Vector3(size.x * 0.5f, controller.WaterlineLocalY, size.y * 0.5f);
            controller.CreateSurfaceImpactAt(controller.transform.TransformPoint(localPosition), 0.75f, true);
        }

        private void OnUndoRedo()
        {
            RefreshProfileSerializedObjects();
            Water25DInspectorUtility.RefreshAllControllers();
            RefreshValidation();
            Repaint();
        }

        private void OnSceneGUI()
        {
            var controller = target as Water25DController;
            if (!Water25DInspectorState.ShowSceneHandles || controller == null || Selection.activeGameObject != controller.gameObject)
            {
                return;
            }

            if (!controller.isActiveAndEnabled)
            {
                return;
            }

            var size = controller.TopSurfaceSize;
            var waterline = controller.WaterlineLocalY;
            var physicalDepth = controller.FrontSurfaceDepth;
            var rootTransform = controller.transform;
            using (new Handles.DrawingScope(rootTransform.localToWorldMatrix))
            {
                Handles.color = new Color(0.1f, 0.75f, 1f, 0.8f);
                Handles.DrawWireCube(new Vector3(size.x * 0.5f, waterline, size.y * 0.5f), new Vector3(size.x, 0.02f, size.y));
                Handles.Label(new Vector3(size.x * 0.5f, waterline, size.y * 0.5f), "Top Surface · XZ");

                Handles.color = new Color(0.1f, 0.35f, 0.8f, 0.6f);
                Handles.DrawWireCube(new Vector3(size.x * 0.5f, waterline - physicalDepth * 0.5f, 0f), new Vector3(size.x, physicalDepth, 0.02f));
                Handles.Label(new Vector3(size.x * 0.5f, waterline - physicalDepth, 0f), "Front Surface · XY");

                var widthPosition = new Vector3(size.x, waterline, 0f);
                Handles.color = new Color(0.2f, 0.9f, 0.95f, 1f);
                EditorGUI.BeginChangeCheck();
                var newWidthPosition = Handles.Slider(widthPosition, Vector3.right, HandleUtility.GetHandleSize(rootTransform.TransformPoint(widthPosition)) * 0.1f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    SetTopSize(new Vector2(Mathf.Max(0.01f, newWidthPosition.x), size.y), "Resize Water25D Width");
                }
                Handles.Label(widthPosition, "Width");

                var visualDepthPosition = new Vector3(0f, waterline, size.y);
                Handles.color = new Color(0.2f, 0.7f, 1f, 1f);
                EditorGUI.BeginChangeCheck();
                var newVisualDepthPosition = Handles.Slider(visualDepthPosition, Vector3.forward, HandleUtility.GetHandleSize(rootTransform.TransformPoint(visualDepthPosition)) * 0.1f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    SetTopSize(new Vector2(size.x, Mathf.Max(0.01f, newVisualDepthPosition.z)), "Resize Water25D Visual Depth");
                }
                Handles.Label(visualDepthPosition, "Visual Depth");

                var physicalDepthPosition = new Vector3(size.x * 0.5f, waterline - physicalDepth, 0f);
                Handles.color = new Color(0.25f, 0.45f, 1f, 1f);
                EditorGUI.BeginChangeCheck();
                var newPhysicalDepthPosition = Handles.Slider(physicalDepthPosition, Vector3.down, HandleUtility.GetHandleSize(rootTransform.TransformPoint(physicalDepthPosition)) * 0.1f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    SetPhysicalDepth(Mathf.Max(0.01f, waterline - newPhysicalDepthPosition.y), "Resize Water25D Physical Depth");
                }
                Handles.Label(physicalDepthPosition, "Physical Depth");

                var waterlinePosition = new Vector3(size.x * 0.5f, waterline, 0f);
                Handles.color = new Color(0.95f, 0.85f, 0.25f, 1f);
                EditorGUI.BeginChangeCheck();
                var newWaterlinePosition = Handles.Slider(waterlinePosition, Vector3.up, HandleUtility.GetHandleSize(rootTransform.TransformPoint(waterlinePosition)) * 0.1f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    SetWaterline(newWaterlinePosition.y, "Move Water25D Waterline");
                }
                Handles.Label(waterlinePosition, "Waterline");
            }
        }

        private void SetTopSize(Vector2 value, string undoName)
        {
            var controller = target as Water25DController;
            if (controller == null)
            {
                return;
            }

            Undo.RecordObject(controller, undoName);
            controller.SetDimensions(value, controller.FrontSurfaceDepth);
            Water25DInspectorUtility.MarkControllerAuthoringChange(controller);
            RefreshValidation();
            SceneView.RepaintAll();
        }

        private void SetPhysicalDepth(float value, string undoName)
        {
            SetFloatProperty("_frontSurfaceDepth", value, undoName);
        }

        private void SetWaterline(float value, string undoName)
        {
            SetFloatProperty("_waterlineLocalY", value, undoName);
        }

        private void SetFloatProperty(string propertyPath, float value, string undoName)
        {
            var controller = target as Water25DController;
            if (controller == null)
            {
                return;
            }

            Undo.RecordObject(controller, undoName);
            if (propertyPath == "_waterlineLocalY")
            {
                controller.SetWaterlineLocalY(value);
            }
            else if (propertyPath == "_frontSurfaceDepth")
            {
                controller.SetDimensions(controller.TopSurfaceSize, value);
            }
            Water25DInspectorUtility.MarkControllerAuthoringChange(controller);
            RefreshValidation();
            SceneView.RepaintAll();
        }
    }
}
