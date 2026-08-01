using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Water25D.FX;
using Water25D.Rendering;

namespace Water25D.Editor
{
    [CustomEditor(typeof(Water25DController))]
    [CanEditMultipleObjects]
    public sealed class Water25DEditor : UnityEditor.Editor
    {
        private const string SetupDocumentationPath = "Assets/Water25D/Documentation/SETUP.md";

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
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            _styleProfileSerializedObject = null;
            _qualityProfileSerializedObject = null;
        }

        public override void OnInspectorGUI()
        {
            Water25DInspectorStyles.Ensure();
            serializedObject.Update();
            RefreshProfileSerializedObjects();

            var controller = target as Water25DController;
            _validationResults = controller != null
                ? Water25DValidation.Validate(controller)
                : new List<Water25DValidationResult>();

            DrawHeader(controller);
            DrawBasicSection();
            DrawRenderingSection();
            DrawAmbientWavesSection();
            DrawContactRipplesSection(controller);
            DrawReflectionSection(controller);
            DrawFxSection();
            DrawPhysicsSection();
            DrawInteractionSection();
            DrawEventsSection();
            DrawPerformanceSection(controller);
            DrawValidationSection(controller);
            DrawAdvancedSection(controller);

            var changed = serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                Water25DInspectorUtility.MarkControllerPropertiesChanged(serializedObject);
                RefreshProfileSerializedObjects();
                SceneView.RepaintAll();
            }
        }

        private void DrawHeader(Water25DController controller)
        {
            EditorGUILayout.BeginVertical(Water25DInspectorStyles.Header);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Water25D", new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 });
            EditorGUILayout.LabelField("2.5D Water Authoring", Water25DInspectorStyles.Subtitle);
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            DrawValidationBadge(GetAggregateSeverity());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(controller != null ? controller.name : "Multiple Water25D objects", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal(Water25DInspectorStyles.StatusRow);
            DrawAssetStatus("Style", serializedObject.FindProperty("_styleProfile"));
            DrawAssetStatus("Quality", serializedObject.FindProperty("_qualityProfile"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(Water25DInspectorStyles.Toolbar);
            if (GUILayout.Button(new GUIContent("Defaults", "Assign only missing package-owned defaults."), EditorStyles.toolbarButton))
            {
                AssignPackageDefaults();
            }

            if (GUILayout.Button(new GUIContent("Repair", "Repair generated children and rebuild the transient preview."), EditorStyles.toolbarButton))
            {
                RepairHierarchy();
            }

            if (GUILayout.Button(new GUIContent("Refresh", "Reapply property blocks and refresh the edit-mode preview."), EditorStyles.toolbarButton))
            {
                RefreshPreview();
            }

            using (new EditorGUI.DisabledScope(targets.Length != 1))
            {
                if (GUILayout.Button(new GUIContent("Top", "Select the generated TopSurface child."), EditorStyles.toolbarButton))
                {
                    Water25DInspectorUtility.SelectObject(controller != null ? controller.TopSurface : null);
                }

                if (GUILayout.Button(new GUIContent("Front", "Select the generated FrontSurface child."), EditorStyles.toolbarButton))
                {
                    Water25DInspectorUtility.SelectObject(controller != null ? controller.FrontSurface : null);
                }
            }

            if (GUILayout.Button(new GUIContent("Setup", "Open the Water25D setup and authoring documentation."), EditorStyles.toolbarButton))
            {
                Water25DInspectorUtility.OpenDocumentation(SetupDocumentationPath);
            }

            EditorGUILayout.EndHorizontal();
            if (targets.Length > 1)
            {
                EditorGUILayout.HelpBox("Multiple Water25D objects are selected. Shared profile edits and single-object authoring actions are guarded below.", MessageType.Info);
            }
        }

        private void DrawBasicSection()
        {
            var open = BeginSection("Basic", "Basic", true);
            if (open)
            {
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

                var showHandles = Water25DInspectorState.ShowSceneHandles;
                var nextShowHandles = EditorGUILayout.ToggleLeft(new GUIContent("Show Scene Handles", "Show optional width, visual-depth, physical-depth and waterline handles in the Scene View."), showHandles);
                if (nextShowHandles != showHandles)
                {
                    Water25DInspectorState.ShowSceneHandles = nextShowHandles;
                    SceneView.RepaintAll();
                }

                EditorGUILayout.HelpBox("Top surface: local XZ. Front surface: local XY. Gameplay remains a flat 2D surface mapped through the explicit Interaction Lane.", MessageType.Info);
            }

            EndSection(open);
        }

        private void DrawRenderingSection()
        {
            var open = BeginSection("Rendering", "Rendering", true);
            if (open)
            {
                EditorGUILayout.LabelField("Appearance Profile", Water25DInspectorStyles.Subsection);
                var styleProperty = serializedObject.FindProperty("_styleProfile");
                if (styleProperty != null)
                {
                    EditorGUILayout.PropertyField(styleProperty, new GUIContent("Style Profile", "Shared colors and analytical wave settings. Changes affect every Water25D object using this asset."));
                }

                var styleProfile = styleProperty != null ? styleProperty.objectReferenceValue as WaterStyleProfile : null;
                DrawProfileActions(styleProperty, styleProfile, true);
                if (styleProfile != null && _styleProfileSerializedObject != null)
                {
                    DrawSharedProfileNotice(styleProfile, true);
                    WaterStyleProfileEditor.DrawSurfaceFields(_styleProfileSerializedObject);
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign a WaterStyleProfile to edit the current appearance inline.", MessageType.Warning);
                }

                EditorGUILayout.Space(3f);
                EditorGUILayout.LabelField("Materials and Sorting", Water25DInspectorStyles.Subsection);
                DrawMaterialRow("Top Material Template", "_topMaterialTemplate", Water25DInspectorUtility.TopShaderName, Water25DInspectorUtility.TopMaterialPath, styleProfile != null ? styleProfile.TopMaterialTemplate : null);
                DrawMaterialRow("Front Material Template", "_frontMaterialTemplate", Water25DInspectorUtility.FrontShaderName, Water25DInspectorUtility.FrontMaterialPath, styleProfile != null ? styleProfile.FrontMaterialTemplate : null);
                DrawMaterialRow("Ripple Simulation Material", "_rippleSimulationMaterialTemplate", Water25DInspectorUtility.RippleShaderName, Water25DInspectorUtility.RippleMaterialPath, null);
                DrawSortingLayerProperty("Top Sorting Layer", "_topSortingLayerName", "Sorting layer for the XZ top renderer.");
                Property("_topSortingOrder", "Top Sorting Order", "Ordering within the selected top sorting layer.");
                DrawSortingLayerProperty("Front Sorting Layer", "_frontSortingLayerName", "Sorting layer for the XY front renderer.");
                Property("_frontSortingOrder", "Front Sorting Order", "Ordering within the selected front sorting layer.");

                if (styleProfile != null && _styleProfileSerializedObject != null)
                {
                    EditorGUILayout.Space(2f);
                    EditorGUILayout.LabelField("Optional Profile Material Templates", Water25DInspectorStyles.Subsection);
                    WaterStyleProfileEditor.DrawMaterialFields(_styleProfileSerializedObject);
                }

                EditorGUILayout.HelpBox("The current shader set provides the top/front presentation, analytical waves, ripple amplitude, foam edge and optional reflection. Distortion, blur, caustics and light shafts are not implemented by Water25D yet.", MessageType.Info);
            }

            EndSection(open);
        }

        private void DrawAmbientWavesSection()
        {
            var open = BeginSection("Ambient Waves", "Ambient Waves", false);
            if (open)
            {
                var styleProfile = GetStyleProfile();
                if (styleProfile == null || _styleProfileSerializedObject == null)
                {
                    EditorGUILayout.HelpBox("Assign a style profile to edit analytical ambient waves.", MessageType.Warning);
                }
                else
                {
                    DrawSharedProfileNotice(styleProfile, true);
                    WaterStyleProfileEditor.DrawAmbientFields(_styleProfileSerializedObject);
                    if (GUILayout.Button(new GUIContent("Reset Wave Settings", "Restore amplitude, wavelength, speed and direction from Water25D_DefaultStyle.asset."), Water25DInspectorStyles.SmallButton))
                    {
                        WaterStyleProfileEditor.ResetAmbientWaveSettings(_styleProfileSerializedObject);
                    }
                }

                var qualityProfile = GetQualityProfile();
                if (qualityProfile != null && _qualityProfileSerializedObject != null)
                {
                    EditorGUILayout.LabelField("Quality Profile", Water25DInspectorStyles.Subsection);
                    WaterQualityProfileEditor.DrawAmbientBandField(_qualityProfileSerializedObject);
                }
                else
                {
                    EditorGUILayout.HelpBox("Ambient band count comes from the assigned quality profile.", MessageType.Info);
                }
            }

            EndSection(open);
        }

        private void DrawContactRipplesSection(Water25DController controller)
        {
            var open = BeginSection("Contact Ripples", "Contact Ripples", false);
            if (open)
            {
                var enabledProperty = serializedObject.FindProperty("_enableRippleSimulation");
                if (enabledProperty != null)
                {
                    EditorGUILayout.PropertyField(enabledProperty, new GUIContent("Ripples Enabled", "Allocate and update the instance-owned CRT ripple state in Play Mode."));
                }

                var enabled = enabledProperty == null || enabledProperty.boolValue;
                if (!enabled)
                {
                    EditorGUILayout.HelpBox("Contact ripple simulation is disabled. Ripple-specific strength controls are hidden, while the assigned quality profile remains available for later use.", MessageType.Info);
                }
                else
                {
                    Property("_impactSpeedForFullStrength", "Full-strength Impact Speed", "Velocity magnitude in world units per second that reaches full impact strength.");
                    Property("_minimumImpactStrength", "Minimum Impact Strength", "Floor applied to non-zero impact strength before the multiplier.");
                    Property("_impactStrengthMultiplier", "Impact Strength Multiplier", "Scales calculated impact strength before it is clamped to 0–1.");
                }

                var qualityProperty = serializedObject.FindProperty("_qualityProfile");
                if (qualityProperty != null)
                {
                    EditorGUILayout.PropertyField(qualityProperty, new GUIContent("Quality Profile", "Shared ripple scheduling, resolution, wave behaviour and geometry settings."));
                }

                var qualityProfile = qualityProperty != null ? qualityProperty.objectReferenceValue as WaterQualityProfile : null;
                DrawProfileActions(qualityProperty, qualityProfile, false);
                if (qualityProfile != null && _qualityProfileSerializedObject != null)
                {
                    DrawSharedProfileNotice(qualityProfile, false);
                    DrawNestedQualitySection(_qualityProfileSerializedObject, "Resolution", "ContactRipples.Resolution", true, WaterQualityProfileEditor.DrawRippleResolutionFields);
                    DrawNestedQualitySection(_qualityProfileSerializedObject, "Scheduling", "ContactRipples.Scheduling", false, WaterQualityProfileEditor.DrawSchedulingFields);
                    DrawNestedQualitySection(_qualityProfileSerializedObject, "Wave Behaviour", "ContactRipples.WaveBehaviour", false, WaterQualityProfileEditor.DrawWaveBehaviourFields);
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign a quality profile to configure the ripple state.", MessageType.Warning);
                }

                var styleProfile = GetStyleProfile();
                if (styleProfile != null && _styleProfileSerializedObject != null)
                {
                    EditorGUILayout.LabelField("Visual Amplitude", Water25DInspectorStyles.Subsection);
                    WaterStyleProfileEditor.DrawRippleFields(_styleProfileSerializedObject);
                }

                DrawRippleRuntimeStatus(controller, enabled);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Reset Ripple Simulation", "Clear the instance-owned runtime ripple state. Available in Play Mode."), Water25DInspectorStyles.SmallButton))
                {
                    ResetRippleSimulation();
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying || controller == null || !enabled))
                {
                    if (GUILayout.Button(new GUIContent("Test Ripple at Center", "Queue a test impact at the centre of the water in Play Mode."), Water25DInspectorStyles.SmallButton))
                    {
                        CreateTestRipple(controller);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EndSection(open);
        }

        private void DrawReflectionSection(Water25DController controller)
        {
            var open = BeginSection("Reflection", "Reflection", false);
            if (open)
            {
                var modeProperty = serializedObject.FindProperty("_reflectionMode");
                if (modeProperty != null)
                {
                    EditorGUILayout.PropertyField(modeProperty, new GUIContent("Reflection Mode", "Disabled, camera-free Stylized fallback, or shared adaptive Planar reflection."));
                }

                var mode = modeProperty != null ? (WaterReflectionMode)modeProperty.enumValueIndex : WaterReflectionMode.Stylized;
                if (mode == WaterReflectionMode.Disabled)
                {
                    EditorGUILayout.HelpBox("Reflection rendering is disabled and no reflection camera or texture is created.", MessageType.Info);
                }
                else if (mode == WaterReflectionMode.Stylized)
                {
                    Property("_reflectionStrength", "Reflection Strength", "Strength of the current camera-free stylized reflection fallback.");
                    EditorGUILayout.HelpBox("Stylized mode uses no reflection camera. Planar camera, mask, resolution and update controls are intentionally hidden.", MessageType.Info);
                }
                else
                {
                    Property("_reflectionCameraSource", "Camera Source", "Camera used to define the shared planar reflection group. Assign explicitly for deterministic grouping.");
                    Property("_reflectionCullingMask", "Planar Culling Mask", "Layers rendered by the shared reflection camera. Exclude water and reflection-helper layers when recursion is possible.");
                    Property("_reflectionResolutionScale", "Resolution Scale", "Fraction of the source camera resolution used by the shared reflection texture.");
                    Property("_reflectionUpdateIntervalFrames", "Update Interval", "Minimum frame interval between adaptive reflection renders unless the camera moves.");
                    Property("_reflectionStrength", "Reflection Strength", "Blend strength applied to the planar reflection.");
                    DrawReflectionEstimate(GetReflectionCamera());
                    if (Application.isPlaying)
                    {
                        if (WaterReflectionManager.HasInstance)
                        {
                            EditorGUILayout.LabelField("Registration", WaterReflectionManager.RegisteredSurfaceCount + " surface(s), " + WaterReflectionManager.ActiveGroupCount + " active group(s)");
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("No WaterReflectionManager is present in Play Mode yet.", MessageType.Warning);
                        }
                    }
                }
            }

            EndSection(open);
        }

        private void DrawFxSection()
        {
            var open = BeginSection("FX", "FX", false);
            if (open)
            {
                var enabledProperty = serializedObject.FindProperty("_enableEffects");
                if (enabledProperty != null)
                {
                    EditorGUILayout.PropertyField(enabledProperty, new GUIContent("Effects Enabled", "Enable pooled splash and bubble presentation effects for interaction events."));
                }

                var enabled = enabledProperty == null || enabledProperty.boolValue;
                if (!enabled)
                {
                    EditorGUILayout.HelpBox("Pooled splash and bubble effects are disabled. Definition assignments are hidden until this feature is enabled.", MessageType.Info);
                }
                else
                {
                    DrawFxDefinitionRow("Splash Definition", "_splashDefinition", "Splash", "Definition used for surface enter and exit effects.");
                    DrawFxDefinitionRow("Bubble Definition", "_bubbleDefinition", "Bubble", "Definition used for submerged effects.");
                    Property("_maximumFxPoolSize", "Maximum Pool Size", "Upper bound for each fixed-capacity effect pool. Exhaustion rejects requests instead of instantiating during gameplay.");
                    var splash = serializedObject.FindProperty("_splashDefinition");
                    var bubble = serializedObject.FindProperty("_bubbleDefinition");
                    if (splash != null && bubble != null && (splash.objectReferenceValue == null || bubble.objectReferenceValue == null))
                    {
                        EditorGUILayout.HelpBox("FX definitions are optional. Missing assignments produce the lightweight package fallback effect and are reported as a warning by Validation.", MessageType.Warning);
                    }
                }
            }

            EndSection(open);
        }

        private void DrawPhysicsSection()
        {
            var open = BeginSection("Physics", "Physics", false);
            if (open)
            {
                EditorGUILayout.LabelField("Buoyancy", Water25DInspectorStyles.Subsection);
                Property("_enableBuoyancy", "Enable Buoyancy", "Enable the full underwater trigger and its BuoyancyEffector2D.");
                var buoyancyEnabled = GetBool("_enableBuoyancy", true);
                if (buoyancyEnabled)
                {
                    Property("_buoyancyDensity", "Density", "Density used by the generated BuoyancyEffector2D.");
                    Property("_buoyancyLayers", "Buoyancy Layers", "Rigidbody2D layers eligible for the full underwater volume.");
                    Property("_buoyancyLinearDamping", "Effector Linear Damping", "Linear damping applied by BuoyancyEffector2D when custom drag is disabled or kept modest.");
                    Property("_buoyancyAngularDamping", "Effector Angular Damping", "Angular damping applied by BuoyancyEffector2D when custom drag is disabled or kept modest.");
                }

                EditorGUILayout.LabelField("Drag", Water25DInspectorStyles.Subsection);
                Property("_enableCustomDrag", "Enable Custom Drag", "Apply optional per-Rigidbody2D drag from WaterPhysicsVolume2D in addition to effector damping.");
                if (GetBool("_enableCustomDrag", false))
                {
                    Property("_customLinearDrag", "Custom Linear Drag", "Additional linear drag applied once per logical Rigidbody2D contact.");
                    Property("_customAngularDrag", "Custom Angular Drag", "Additional angular drag applied once per logical Rigidbody2D contact.");
                    if (GetFloat("_customLinearDrag", 0f) > 1f && GetFloat("_buoyancyLinearDamping", 0f) > 1f || GetFloat("_customAngularDrag", 0f) > 1f && GetFloat("_buoyancyAngularDamping", 0f) > 1f)
                    {
                        EditorGUILayout.HelpBox("Strong custom drag and strong effector damping may over-damp motion. Tune one source down if needed.", MessageType.Warning);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Custom drag is disabled. BuoyancyEffector2D damping values provide the configured drag response.", MessageType.Info);
                }

                EditorGUILayout.HelpBox("Generated buoyancy collider and effector state are shown in Validation; their references are intentionally not editable fields here.", MessageType.Info);
            }

            EndSection(open);
        }

        private void DrawInteractionSection()
        {
            var open = BeginSection("Interaction", "Interaction", false);
            if (open)
            {
                Property("_enableSurfaceInteraction", "Surface Interaction", "Enable the thin surface-crossing trigger and logical enter/exit tracking.");
                if (GetBool("_enableSurfaceInteraction", true))
                {
                    Property("_surfaceInteractionLayers", "Surface Interaction Layers", "Non-trigger collider layers eligible for surface crossings and contact ripples.");
                    Property("_surfaceTriggerInteractionLayers", "Trigger Interaction Layers", "Trigger collider layers eligible for surface crossings when trigger colliders are included.");
                    Property("_includeTriggerCollidersInSurfaceInteraction", "Include Trigger Colliders", "Allow eligible trigger colliders to produce one logical surface contact per Rigidbody2D.");
                    EditorGUILayout.LabelField("Crossing Band", "Configured in Basic", Water25DInspectorStyles.MetricLabel);
                    EditorGUILayout.LabelField("Interaction Lane", "Configured in Basic", Water25DInspectorStyles.MetricLabel);
                    EditorGUILayout.HelpBox("Impact strength, full-speed threshold and multiplier are grouped in Contact Ripples so they are authored once.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Surface interaction is disabled. The crossing trigger will not produce enter, exit, ripple or splash events.", MessageType.Info);
                }
            }

            EndSection(open);
        }

        private void DrawEventsSection()
        {
            var open = BeginSection("Events", "Events", false);
            if (open)
            {
                DrawEventProperty("_onSurfaceEnter", "Surface Entered", "Fires once when a logical Rigidbody2D first crosses the thin surface trigger.");
                DrawEventProperty("_onSurfaceExit", "Surface Exited", "Fires once when the last collider of a logical Rigidbody2D leaves the thin surface trigger.");
                DrawEventProperty("_onSubmerged", "Submerged", "Fires once when a logical Rigidbody2D first enters the full buoyancy volume.");
                DrawEventProperty("_onResurfaced", "Resurfaced", "Fires once when the last collider of a logical Rigidbody2D leaves the full buoyancy volume.");
                EditorGUILayout.HelpBox("The runtime also exposes C# events for systems that cannot be wired through the Inspector. Splash and ripple requests remain internal presentation actions rather than persistent UnityEvents.", MessageType.Info);
            }

            EndSection(open);
        }

        private void DrawPerformanceSection(Water25DController controller)
        {
            var open = BeginSection("Performance", "Performance", false);
            if (open)
            {
                var metrics = Water25DInspectorUtility.CalculateMetrics(controller);
                EditorGUILayout.LabelField("Geometry (calculated)", Water25DInspectorStyles.Subsection);
                Metric("Top Mesh Vertices", metrics.TopVertexCount.x * metrics.TopVertexCount.y);
                Metric("Top Mesh Triangles", metrics.TopTriangleCount);
                Metric("Front Mesh Vertices", metrics.FrontVertexCount);
                Metric("Front Mesh Triangles", metrics.FrontTriangleCount);
                Metric("Top Vertices Per Unit", metrics.TopVerticesPerUnit.ToString("0.##"));

                EditorGUILayout.LabelField("Ripple State (estimated)", Water25DInspectorStyles.Subsection);
                Metric("Calculated Resolution", metrics.RippleResolution.x + " x " + metrics.RippleResolution.y);
                Metric("One State Texture", Water25DInspectorUtility.FormatBytes(metrics.RippleStateBytes));
                Metric("Expected Format", metrics.UsesRgHalf ? "RGHalf" : "RGFloat fallback");
                Metric("Mipmaps", "Disabled");
                Metric("Simulation Updates / Second", metrics.SimulationFrequency.ToString("0.##"));
                Metric("Propagation Substeps", metrics.PropagationSubsteps);
                Metric("Propagated Cells / Second", metrics.PropagatedCellsPerSecond.ToString("N0"));
                Metric("Maximum Impact Injections / Update", metrics.MaximumImpactsPerStep);
                Metric("Queue Capacity", metrics.MaximumQueuedImpacts);

                if (Application.isPlaying && controller != null)
                {
                    EditorGUILayout.LabelField("Runtime", Water25DInspectorStyles.Subsection);
                    Metric("Ripple Simulator", controller.RippleSimulationAvailable ? "Available" : "Unavailable");
                    Metric("Ripple State", controller.IsRippleSimulationSuspended ? "Suspended" : "Active");
                    Metric("Dropped Impacts", controller.DroppedRippleImpactCount);
                    Metric("Top Renderer", IsRendererVisible(controller.TopSurface) ? "Visible" : "Not visible");
                    Metric("Front Renderer", IsRendererVisible(controller.FrontSurface) ? "Visible" : "Not visible");
                    Metric("Reflection Mode", controller.ReflectionMode.ToString());
                }

                var mode = GetReflectionMode();
                if (mode == WaterReflectionMode.Planar)
                {
                    EditorGUILayout.LabelField("Reflection (estimated)", Water25DInspectorStyles.Subsection);
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
                    Metric("Update Interval", GetInt("_reflectionUpdateIntervalFrames", 3) + " frame(s)");
                    Metric("Estimated Maximum Frequency", (60f / Mathf.Max(1, GetInt("_reflectionUpdateIntervalFrames", 3))).ToString("0.##") + " Hz at 60 FPS");
                }

                EditorGUILayout.HelpBox("All values in this dashboard are calculated estimates derived from authored settings. Use the Unity Profiler and Frame Debugger on target hardware before recording performance claims.", MessageType.Info);
            }

            EndSection(open);
        }

        private void DrawValidationSection(Water25DController controller)
        {
            var defaultOpen = HasSeverity(Water25DValidationSeverity.Error);
            var open = BeginSection("Validation", "Validation", defaultOpen);
            if (open)
            {
                if (targets.Length > 1)
                {
                    EditorGUILayout.HelpBox("Validation details are shown for the first selected controller. Serialized fields remain multi-object aware.", MessageType.Info);
                }

                DrawValidationGroup(Water25DValidationSeverity.Error, "Errors");
                DrawValidationGroup(Water25DValidationSeverity.Warning, "Warnings");
                DrawValidationGroup(Water25DValidationSeverity.Info, "Information");

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Validate", "Refresh validation findings without changing authored data."), Water25DInspectorStyles.SmallButton))
                {
                    RefreshValidation();
                }

                using (new EditorGUI.DisabledScope(!HasSafeFixes()))
                {
                    if (GUILayout.Button(new GUIContent("Fix All Safe Issues", "Assign missing package defaults and repair generated hierarchy. No destructive fixes are performed."), Water25DInspectorStyles.SmallButton))
                    {
                        FixAllSafeIssues();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EndSection(open);
        }

        private void DrawAdvancedSection(Water25DController controller)
        {
            var open = BeginSection("Advanced", "Advanced", false);
            if (open)
            {
                Property("_synchronizeGeneratedChildLayers", "Synchronize Generated Child Layers", "Keep generated children on the controller's layer. Disable only when a deliberate layer layout is authored manually.");
                EditorGUILayout.LabelField("Generated Hierarchy", Water25DInspectorStyles.Subsection);
                DrawGeneratedReference("Top Surface", controller != null ? controller.TopSurface : null);
                DrawGeneratedReference("Front Surface", controller != null ? controller.FrontSurface : null);
                DrawGeneratedReference("Surface Crossing Trigger", controller != null ? controller.SurfaceCrossingTrigger : null);
                DrawGeneratedReference("Buoyancy Volume", controller != null ? controller.BuoyancyVolume : null);
                DrawGeneratedReference("Reflection Anchor", controller != null ? controller.ReflectionAnchor : null);
                DrawGeneratedReference("FX Root", controller != null ? controller.FxRoot : null);

                EditorGUILayout.LabelField("Runtime Resource Summary", Water25DInspectorStyles.Subsection);
                EditorGUILayout.HelpBox("Meshes, property blocks, ripple textures, runtime ripple materials, reflection resources and pooled FX belong to the water instance or shared reflection group. Persistent template assets are not mutated at runtime.", MessageType.Info);
                Metric("Top Shader", Water25DInspectorUtility.TopShaderName);
                Metric("Front Shader", Water25DInspectorUtility.FrontShaderName);
                Metric("Ripple Shader", Water25DInspectorUtility.RippleShaderName);

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField("Authoring Actions", Water25DInspectorStyles.Subsection);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Assign Package Defaults", "Assign only missing package-owned materials and profiles."), Water25DInspectorStyles.SmallButton))
                {
                    AssignPackageDefaults();
                }
                if (GUILayout.Button(new GUIContent("Make Profiles Unique", "Duplicate assigned style and quality profiles and assign them to this controller."), Water25DInspectorStyles.SmallButton))
                {
                    MakeProfilesUnique();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Repair Hierarchy", "Repair generated children without deleting unrelated authored children."), Water25DInspectorStyles.SmallButton))
                {
                    RepairHierarchy();
                }
                if (GUILayout.Button(new GUIContent("Rebuild Geometry", "Rebuild transient top/front preview meshes using current dimensions and quality density."), Water25DInspectorStyles.SmallButton))
                {
                    RebuildGeometry();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Reset Ripple", "Clear runtime ripple state in Play Mode."), Water25DInspectorStyles.SmallButton))
                {
                    ResetRippleSimulation();
                }
                if (GUILayout.Button(new GUIContent("Open Setup", "Open package setup documentation."), Water25DInspectorStyles.SmallButton))
                {
                    Water25DInspectorUtility.OpenDocumentation(SetupDocumentationPath);
                }
                EditorGUILayout.EndHorizontal();
            }

            EndSection(open);
        }

        private bool BeginSection(string key, string label, bool defaultOpen)
        {
            var open = Water25DInspectorState.GetFoldout(key, defaultOpen);
            var next = EditorGUILayout.BeginFoldoutHeaderGroup(open, label);
            if (next != open)
            {
                Water25DInspectorState.SetFoldout(key, next);
            }

            if (next)
            {
                EditorGUILayout.BeginVertical(Water25DInspectorStyles.SectionCard);
            }

            return next;
        }

        private static void EndSection(bool open)
        {
            if (open)
            {
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawNestedQualitySection(SerializedObject profile, string label, string stateKey, bool defaultOpen, System.Func<SerializedObject, bool> draw)
        {
            var open = Water25DInspectorState.GetFoldout(stateKey, defaultOpen);
            var next = EditorGUILayout.Foldout(open, label, true, Water25DInspectorStyles.Foldout);
            if (next != open)
            {
                Water25DInspectorState.SetFoldout(stateKey, next);
            }

            if (next)
            {
                EditorGUILayout.BeginVertical(Water25DInspectorStyles.SectionCard);
                draw(profile);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSharedProfileNotice(UnityEngine.Object profile, bool style)
        {
            var users = Water25DInspectorUtility.CountProfileUsers(profile);
            var isDefault = style
                ? Water25DInspectorUtility.IsPackageDefaultStyle(profile as WaterStyleProfile)
                : Water25DInspectorUtility.IsPackageDefaultQuality(profile as WaterQualityProfile);
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
                UnityEngine.Object defaultProfile = style
                    ? Water25DInspectorUtility.LoadPackageAsset<WaterStyleProfile>(Water25DInspectorUtility.StyleProfilePath)
                    : Water25DInspectorUtility.LoadPackageAsset<WaterQualityProfile>(Water25DInspectorUtility.QualityProfilePath);
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

        private void DrawMaterialRow(string label, string propertyPath, string expectedShader, string packagePath, Material profileFallback)
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

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property, new GUIContent(label, "Persistent material template. Runtime property blocks carry per-water values without mutating this asset."));
            DrawStatusBadge(Water25DInspectorUtility.GetMaterialStatus(resolved, expectedShader));
            EditorGUILayout.EndHorizontal();

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

        private void DrawRippleRuntimeStatus(Water25DController controller, bool enabled)
        {
            EditorGUILayout.LabelField("Runtime Status", Water25DInspectorStyles.Subsection);
            if (!Application.isPlaying || controller == null)
            {
                EditorGUILayout.HelpBox("Runtime-only simulator status appears in Play Mode.", MessageType.Info);
                return;
            }

            Metric("Simulator", enabled && controller.RippleSimulationAvailable ? "Available" : "Unavailable");
            Metric("State", controller.IsRippleSimulationSuspended ? "Suspended" : "Active");
            if (Water25DInspectorUtility.TryGetTextureDimensions(controller.RippleTexture, out var dimensions))
            {
                Metric("Texture", dimensions.x + " x " + dimensions.y);
            }
            Metric("Dropped Impacts", controller.DroppedRippleImpactCount);
        }

        private void DrawReflectionEstimate(Camera camera)
        {
            if (camera == null || camera.pixelWidth <= 0 || camera.pixelHeight <= 0)
            {
                EditorGUILayout.LabelField("Estimated Texture", "Camera dimensions unavailable", Water25DInspectorStyles.MetricLabel);
                return;
            }

            var scale = GetFloat("_reflectionResolutionScale", 0.25f);
            var width = Mathf.Max(1, Mathf.RoundToInt(camera.pixelWidth * scale));
            var height = Mathf.Max(1, Mathf.RoundToInt(camera.pixelHeight * scale));
            EditorGUILayout.LabelField("Estimated Texture", width + " x " + height, Water25DInspectorStyles.MetricLabel);
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
            if (property == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label, explanation), true);
            EditorGUILayout.LabelField(explanation, EditorStyles.miniLabel);
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

        private void DrawValidationGroup(Water25DValidationSeverity severity, string label)
        {
            var count = CountSeverity(severity);
            if (count == 0)
            {
                return;
            }

            EditorGUILayout.LabelField(label + " (" + count + ")", Water25DInspectorStyles.Subsection);
            for (var i = 0; i < _validationResults.Count; i++)
            {
                var result = _validationResults[i];
                if (result.Severity != severity)
                {
                    continue;
                }

                EditorGUILayout.BeginVertical(Water25DInspectorStyles.StatusRow);
                EditorGUILayout.BeginHorizontal();
                DrawValidationBadge(result.Severity);
                EditorGUILayout.LabelField(result.Title, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField(result.Message, EditorStyles.wordWrappedMiniLabel);
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
                    if (GUILayout.Button(new GUIContent("Select", "Select the object associated with this validation result."), Water25DInspectorStyles.SmallButton))
                    {
                        Water25DInspectorUtility.SelectObject(result.TargetObject);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawAssetStatus(string label, SerializedProperty property)
        {
            var value = property != null ? property.objectReferenceValue : null;
            var previous = GUI.color;
            GUI.color = value != null ? Water25DInspectorStyles.ValidColor : Water25DInspectorStyles.ErrorColor;
            EditorGUILayout.LabelField(label + ": " + (value != null ? "Assigned" : "Missing"), EditorStyles.miniBoldLabel);
            GUI.color = previous;
        }

        private void DrawValidationBadge(Water25DValidationSeverity severity)
        {
            var previous = GUI.color;
            GUI.color = GetSeverityColor(severity);
            var label = severity == Water25DValidationSeverity.Error ? "Errors" : severity == Water25DValidationSeverity.Warning ? "Warnings" : "Valid";
            GUILayout.Label(label, EditorStyles.miniBoldLabel, GUILayout.Width(68f));
            GUI.color = previous;
        }

        private void DrawValidationBadge(Water25DMaterialStatus status)
        {
            var previous = GUI.color;
            GUI.color = Water25DInspectorUtility.GetMaterialStatusColor(status);
            GUILayout.Label(Water25DInspectorUtility.GetMaterialStatusText(status), EditorStyles.miniBoldLabel, GUILayout.Width(92f));
            GUI.color = previous;
        }

        private static Color GetSeverityColor(Water25DValidationSeverity severity)
        {
            switch (severity)
            {
                case Water25DValidationSeverity.Error:
                    return Water25DInspectorStyles.ErrorColor;
                case Water25DValidationSeverity.Warning:
                    return Water25DInspectorStyles.WarningColor;
                default:
                    return Water25DInspectorStyles.ValidColor;
            }
        }

        private void DrawStatusBadge(Water25DValidationSeverity severity)
        {
            var previous = GUI.color;
            GUI.color = GetSeverityColor(severity);
            GUILayout.Label(severity == Water25DValidationSeverity.Error ? "ERROR" : severity == Water25DValidationSeverity.Warning ? "WARNINGS" : "VALID", EditorStyles.boldLabel);
            GUI.color = previous;
        }

        private void DrawStatusBadge(Water25DMaterialStatus status)
        {
            var previous = GUI.color;
            GUI.color = Water25DInspectorUtility.GetMaterialStatusColor(status);
            GUILayout.Label(Water25DInspectorUtility.GetMaterialStatusText(status), EditorStyles.miniBoldLabel);
            GUI.color = previous;
        }

        private void Metric(string label, object value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.MinWidth(150f));
            EditorGUILayout.LabelField(value != null ? value.ToString() : "—", Water25DInspectorStyles.MetricLabel);
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

        private static bool IsRendererVisible(Transform surface)
        {
            var renderer = surface != null ? surface.GetComponent<MeshRenderer>() : null;
            return renderer != null && renderer.isVisible;
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
            _validationResults = controller != null ? Water25DValidation.Validate(controller) : new List<Water25DValidationResult>();
            Repaint();
        }

        private Water25DValidationSeverity GetAggregateSeverity()
        {
            if (HasSeverity(Water25DValidationSeverity.Error))
            {
                return Water25DValidationSeverity.Error;
            }

            return HasSeverity(Water25DValidationSeverity.Warning) ? Water25DValidationSeverity.Warning : Water25DValidationSeverity.Info;
        }

        private bool HasSeverity(Water25DValidationSeverity severity)
        {
            for (var i = 0; i < _validationResults.Count; i++)
            {
                if (_validationResults[i].Severity == severity)
                {
                    return true;
                }
            }

            return false;
        }

        private int CountSeverity(Water25DValidationSeverity severity)
        {
            var count = 0;
            for (var i = 0; i < _validationResults.Count; i++)
            {
                if (_validationResults[i].Severity == severity)
                {
                    count++;
                }
            }

            return count;
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
            controller.CreateContactRippleAt(controller.transform.TransformPoint(localPosition), 0.75f, true);
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
            SceneView.RepaintAll();
        }
    }
}
