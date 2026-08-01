using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Water25D.FX;
using Water25D.Rendering;

namespace Water25D.Editor
{
    /// <summary>
    /// Editor-only checks for authored hierarchy, package resources and conditional runtime
    /// configuration. Validation never repairs automatically when the inspector is opened.
    /// </summary>
    public static class Water25DValidation
    {
        public static List<Water25DValidationResult> Validate(Water25DController controller)
        {
            var results = new List<Water25DValidationResult>(16);
            if (controller == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Controller missing",
                    "The selected object is not a Water25D controller."));
                return results;
            }

            var serializedObject = new SerializedObject(controller);
            serializedObject.Update();
            var styleProfile = GetObject<WaterStyleProfile>(serializedObject, "_styleProfile");
            var qualityProfile = GetObject<WaterQualityProfile>(serializedObject, "_qualityProfile");
            var topMaterialTemplate = GetObject<Material>(serializedObject, "_topMaterialTemplate");
            var frontMaterialTemplate = GetObject<Material>(serializedObject, "_frontMaterialTemplate");
            var rippleMaterialTemplate = GetObject<Material>(serializedObject, "_rippleSimulationMaterialTemplate");
            var enableSurfaceInteraction = GetBool(serializedObject, "_enableSurfaceInteraction", true);
            var enableBuoyancy = GetBool(serializedObject, "_enableBuoyancy", true);
            var enableRippleSimulation = GetBool(serializedObject, "_enableRippleSimulation", true);
            var enableEffects = GetBool(serializedObject, "_enableEffects", true);
            var reflectionMode = GetReflectionMode(serializedObject);

            ValidateHierarchy(controller, serializedObject, results, enableSurfaceInteraction, enableBuoyancy);
            ValidateProfiles(serializedObject, styleProfile, qualityProfile, results);
            ValidateRendering(controller, serializedObject, styleProfile, topMaterialTemplate, frontMaterialTemplate, results);
            ValidateSorting(serializedObject, results);
            ValidatePhysics(controller, serializedObject, results, enableSurfaceInteraction, enableBuoyancy);
            ValidateRipple(controller, serializedObject, qualityProfile, rippleMaterialTemplate, results, enableRippleSimulation);
            ValidateReflection(controller, serializedObject, results, reflectionMode);
            ValidateEffects(serializedObject, results, enableEffects);
            return results;
        }

        private static void ValidateHierarchy(
            Water25DController controller,
            SerializedObject serializedObject,
            List<Water25DValidationResult> results,
            bool enableSurfaceInteraction,
            bool enableBuoyancy)
        {
            var topSurface = FindChild(controller.transform, "TopSurface");
            var frontSurface = FindChild(controller.transform, "FrontSurface");
            var surfaceTrigger = FindChild(controller.transform, "SurfaceCrossingTrigger");
            var buoyancyVolume = FindChild(controller.transform, "BuoyancyVolume");
            var reflectionAnchor = FindChild(controller.transform, "ReflectionAnchor");
            var fxRoot = FindChild(controller.transform, "FXRoot");

            if (topSurface == null || frontSurface == null || surfaceTrigger == null || buoyancyVolume == null || reflectionAnchor == null || fxRoot == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Generated hierarchy incomplete",
                    "The package requires TopSurface, FrontSurface, SurfaceCrossingTrigger, BuoyancyVolume, ReflectionAnchor and FXRoot. Unrelated user children are allowed.",
                    Water25DFixAction.RepairHierarchy,
                    controller.gameObject));
                return;
            }

            ValidateSerializedReference(serializedObject, "_topSurface", topSurface, results);
            ValidateSerializedReference(serializedObject, "_frontSurface", frontSurface, results);
            ValidateSerializedReference(serializedObject, "_surfaceCrossingTrigger", surfaceTrigger, results);
            ValidateSerializedReference(serializedObject, "_buoyancyVolume", buoyancyVolume, results);
            ValidateSerializedReference(serializedObject, "_reflectionAnchor", reflectionAnchor, results);
            ValidateSerializedReference(serializedObject, "_fxRoot", fxRoot, results);

            var topFilter = topSurface.GetComponent<MeshFilter>();
            var topRenderer = topSurface.GetComponent<MeshRenderer>();
            var frontFilter = frontSurface.GetComponent<MeshFilter>();
            var frontRenderer = frontSurface.GetComponent<MeshRenderer>();
            if (topFilter == null || topRenderer == null || frontFilter == null || frontRenderer == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Surface components missing",
                    "TopSurface and FrontSurface each need a MeshFilter and MeshRenderer. Repair Hierarchy can restore generated components.",
                    Water25DFixAction.RepairHierarchy,
                    controller.gameObject));
            }
            else if (!Application.isPlaying && controller.isActiveAndEnabled && (topFilter.sharedMesh == null || frontFilter.sharedMesh == null))
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Preview meshes missing",
                    "The active edit-mode controller has no generated preview mesh. Rebuild the hierarchy or geometry preview.",
                    Water25DFixAction.RepairHierarchy,
                    controller.gameObject));
            }

            var surfaceCollider = surfaceTrigger.GetComponent<BoxCollider2D>();
            var surfaceInteraction = surfaceTrigger.GetComponent<WaterSurfaceInteraction2D>();
            if (surfaceCollider == null || surfaceInteraction == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Surface interaction components missing",
                    "The surface-crossing child needs a BoxCollider2D and WaterSurfaceInteraction2D.",
                    Water25DFixAction.RepairHierarchy,
                    surfaceTrigger.gameObject));
            }
            else if (enableSurfaceInteraction && !surfaceCollider.isTrigger)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Surface collider is not a trigger",
                    "SurfaceCrossingTrigger must remain a thin trigger so it can distinguish surface crossings from underwater volume entry.",
                    Water25DFixAction.RepairHierarchy,
                    surfaceCollider));
            }

            var buoyancyCollider = buoyancyVolume.GetComponent<BoxCollider2D>();
            var physicsVolume = buoyancyVolume.GetComponent<WaterPhysicsVolume2D>();
            var effector = buoyancyVolume.GetComponent<BuoyancyEffector2D>();
            if (buoyancyCollider == null || physicsVolume == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Buoyancy volume components missing",
                    "The buoyancy child needs a BoxCollider2D and WaterPhysicsVolume2D.",
                    Water25DFixAction.RepairHierarchy,
                    buoyancyVolume.gameObject));
            }
            else
            {
                if (enableBuoyancy && !buoyancyCollider.isTrigger)
                {
                    results.Add(new Water25DValidationResult(
                        Water25DValidationSeverity.Error,
                        "Buoyancy collider is not a trigger",
                        "BuoyancyVolume must use a trigger collider so BuoyancyEffector2D can own the gameplay volume.",
                        Water25DFixAction.RepairHierarchy,
                        buoyancyCollider));
                }

                if (enableBuoyancy && effector == null)
                {
                    results.Add(new Water25DValidationResult(
                        Water25DValidationSeverity.Error,
                        "Buoyancy effector missing",
                        "Buoyancy is enabled but the generated volume has no BuoyancyEffector2D.",
                        Water25DFixAction.RepairHierarchy,
                        buoyancyVolume.gameObject));
                }
                else if (enableBuoyancy && !buoyancyCollider.usedByEffector)
                {
                    results.Add(new Water25DValidationResult(
                        Water25DValidationSeverity.Error,
                        "Buoyancy collider is not linked",
                        "The generated collider is not marked as used by its BuoyancyEffector2D.",
                        Water25DFixAction.RepairHierarchy,
                        buoyancyCollider));
                }
            }
        }

        private static void ValidateProfiles(
            SerializedObject serializedObject,
            WaterStyleProfile styleProfile,
            WaterQualityProfile qualityProfile,
            List<Water25DValidationResult> results)
        {
            if (styleProfile == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Style profile missing",
                    "Assign a WaterStyleProfile so surface colors and analytical wave settings are explicit and shareable.",
                    Water25DFixAction.AssignStyleProfile));
            }
            else if (AssetDatabase.GetAssetPath(styleProfile).Length == 0)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Style profile is not an asset",
                    "The assigned style profile is not stored as a project asset and may not survive scene or prefab authoring."));
            }

            if (qualityProfile == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Quality profile missing",
                    "Assign a WaterQualityProfile so ripple resolution, scheduling and geometry density are explicit.",
                    Water25DFixAction.AssignQualityProfile));
            }
            else if (AssetDatabase.GetAssetPath(qualityProfile).Length == 0)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Quality profile is not an asset",
                    "The assigned quality profile is not stored as a project asset and may not survive scene or prefab authoring."));
            }

            var styleProperty = serializedObject.FindProperty("_styleProfile");
            if (styleProperty != null && styleProperty.objectReferenceValue != null && styleProfile == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Style profile reference is invalid",
                    "The style profile reference cannot be loaded. Assign a valid WaterStyleProfile asset.",
                    Water25DFixAction.AssignStyleProfile));
            }
        }

        private static void ValidateRendering(
            Water25DController controller,
            SerializedObject serializedObject,
            WaterStyleProfile styleProfile,
            Material topMaterialTemplate,
            Material frontMaterialTemplate,
            List<Water25DValidationResult> results)
        {
            var topRenderer = controller.TopSurface != null ? controller.TopSurface.GetComponent<MeshRenderer>() : null;
            var frontRenderer = controller.FrontSurface != null ? controller.FrontSurface.GetComponent<MeshRenderer>() : null;
            var resolvedTop = topMaterialTemplate != null
                ? topMaterialTemplate
                : styleProfile != null && styleProfile.TopMaterialTemplate != null
                    ? styleProfile.TopMaterialTemplate
                    : topRenderer != null ? topRenderer.sharedMaterial : null;
            var resolvedFront = frontMaterialTemplate != null
                ? frontMaterialTemplate
                : styleProfile != null && styleProfile.FrontMaterialTemplate != null
                    ? styleProfile.FrontMaterialTemplate
                    : frontRenderer != null ? frontRenderer.sharedMaterial : null;

            ValidateMaterial("Top", resolvedTop, Water25DInspectorUtility.TopShaderName, Water25DFixAction.AssignTopMaterial, controller, results);
            ValidateMaterial("Front", resolvedFront, Water25DInspectorUtility.FrontShaderName, Water25DFixAction.AssignFrontMaterial, controller, results);

            var rippleTemplate = GetObject<Material>(serializedObject, "_rippleSimulationMaterialTemplate");
            if (rippleTemplate == null)
            {
                var packageShader = Shader.Find(Water25DInspectorUtility.RippleShaderName);
                if (packageShader == null || !packageShader.isSupported)
                {
                    results.Add(new Water25DValidationResult(
                        Water25DValidationSeverity.Error,
                        "Ripple material missing",
                        "No ripple material template is assigned and the package ripple shader is unavailable.",
                        Water25DFixAction.AssignRippleMaterial,
                        controller.gameObject));
                }
            }
            else
            {
                ValidateMaterial("Ripple simulation", rippleTemplate, Water25DInspectorUtility.RippleShaderName, Water25DFixAction.AssignRippleMaterial, controller, results);
            }
        }

        private static void ValidateMaterial(
            string label,
            Material material,
            string expectedShader,
            Water25DFixAction fixAction,
            Water25DController controller,
            List<Water25DValidationResult> results)
        {
            var status = Water25DInspectorUtility.GetMaterialStatus(material, expectedShader);
            switch (status)
            {
                case Water25DMaterialStatus.Valid:
                    return;
                case Water25DMaterialStatus.Missing:
                    results.Add(new Water25DValidationResult(
                        Water25DValidationSeverity.Error,
                        label + " material missing",
                        "Assign a persistent material template using the expected Water25D shader.",
                        fixAction,
                        controller.gameObject));
                    break;
                case Water25DMaterialStatus.Unsupported:
                    results.Add(new Water25DValidationResult(
                        Water25DValidationSeverity.Error,
                        label + " shader unsupported",
                        "The assigned material shader exists but is not supported by the current graphics configuration.",
                        fixAction,
                        material));
                    break;
                case Water25DMaterialStatus.Unexpected:
                    results.Add(new Water25DValidationResult(
                        Water25DValidationSeverity.Warning,
                        label + " shader differs from the package shader",
                        "The material may be an intentional project override. Confirm that it supports the Water25D property-block contract.",
                        Water25DFixAction.None,
                        material));
                    break;
            }
        }

        private static void ValidateSorting(SerializedObject serializedObject, List<Water25DValidationResult> results)
        {
            ValidateSortingLayer(serializedObject, "_topSortingLayerName", "Top", results);
            ValidateSortingLayer(serializedObject, "_frontSortingLayerName", "Front", results);
        }

        private static void ValidateSortingLayer(SerializedObject serializedObject, string propertyPath, string label, List<Water25DValidationResult> results)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null || string.IsNullOrEmpty(property.stringValue) || SortingLayer.NameToID(property.stringValue) < 0)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    label + " sorting layer missing",
                    "The configured sorting layer does not exist in this project. Unity will fall back to Default at runtime."));
            }
        }

        private static void ValidatePhysics(
            Water25DController controller,
            SerializedObject serializedObject,
            List<Water25DValidationResult> results,
            bool enableSurfaceInteraction,
            bool enableBuoyancy)
        {
            var surfaceLayers = GetLayerMask(serializedObject, "_surfaceInteractionLayers");
            var triggerLayers = GetLayerMask(serializedObject, "_surfaceTriggerInteractionLayers");
            var buoyancyLayers = GetLayerMask(serializedObject, "_buoyancyLayers");
            if (enableSurfaceInteraction && surfaceLayers.value == 0 && triggerLayers.value == 0)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Surface interaction masks are empty",
                    "No solid or trigger layers can cross the surface trigger, so enter/exit events and contact ripples will not be produced."));
            }

            if (enableBuoyancy && buoyancyLayers.value == 0)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Buoyancy layer mask is empty",
                    "The buoyancy volume is enabled but no Rigidbody2D layers are eligible for buoyancy."));
            }

            if (enableSurfaceInteraction && enableBuoyancy && surfaceLayers.value == buoyancyLayers.value && surfaceLayers.value != 0)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Info,
                    "Surface and buoyancy masks overlap",
                    "This is supported. The thin surface trigger still owns crossings while the full volume owns submerged state and drag."));
            }

            var customDrag = GetBool(serializedObject, "_enableCustomDrag", false);
            var customLinear = GetFloat(serializedObject, "_customLinearDrag", 0f);
            var customAngular = GetFloat(serializedObject, "_customAngularDrag", 0f);
            var effectorLinear = GetFloat(serializedObject, "_buoyancyLinearDamping", 0f);
            var effectorAngular = GetFloat(serializedObject, "_buoyancyAngularDamping", 0f);
            if (customDrag && (customLinear > 1f && effectorLinear > 1f || customAngular > 1f && effectorAngular > 1f))
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Drag may be over-damped",
                    "Strong custom drag and strong BuoyancyEffector2D damping are both enabled. Reduce one source if motion feels excessively slow."));
            }

            if (controller.SurfaceCrossingTrigger != null && !enableSurfaceInteraction)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Info,
                    "Surface interaction disabled",
                    "The crossing trigger is intentionally inactive; its generated components remain available for later authoring."));
            }
        }

        private static void ValidateRipple(
            Water25DController controller,
            SerializedObject serializedObject,
            WaterQualityProfile qualityProfile,
            Material rippleMaterialTemplate,
            List<Water25DValidationResult> results,
            bool enableRippleSimulation)
        {
            if (!enableRippleSimulation)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Info,
                    "Contact ripples disabled",
                    "The ripple state is not allocated while this feature is disabled."));
                return;
            }

            var settings = qualityProfile != null ? qualityProfile.GetSettings() : WaterQualitySettings.Default;
            var resolution = settings.CalculateRippleResolution(controller.TopSurfaceSize);
            if (resolution.x <= 0 || resolution.y <= 0)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Ripple resolution invalid",
                    "The calculated rectangular ripple resolution must be positive.",
                    Water25DFixAction.AssignQualityProfile));
            }

            var minimum = new Vector2Int(2, 2);
            if (qualityProfile != null)
            {
                var qualitySerialized = new SerializedObject(qualityProfile);
                qualitySerialized.Update();
                minimum = GetVector2Int(qualitySerialized, "_minimumRippleResolution", minimum);
                var maximum = GetVector2Int(qualitySerialized, "_maximumRippleResolution", minimum);
                if (minimum.x > maximum.x || minimum.y > maximum.y)
                {
                    results.Add(new Water25DValidationResult(
                        Water25DValidationSeverity.Error,
                        "Ripple resolution limits are incoherent",
                        "Minimum ripple resolution must not exceed maximum ripple resolution."));
                }

                var maximumImpacts = GetInt(qualitySerialized, "_maximumImpactsPerStep", 1);
                var maximumQueued = GetInt(qualitySerialized, "_maximumQueuedImpacts", 1);
                if (maximumQueued < maximumImpacts)
                {
                    results.Add(new Water25DValidationResult(
                        Water25DValidationSeverity.Error,
                        "Ripple queue limit is too small",
                        "Maximum queued impacts must be at least the maximum impacts processed per simulation step."));
                }
            }

            if (rippleMaterialTemplate == null && Shader.Find(Water25DInspectorUtility.RippleShaderName) == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Ripple shader unavailable",
                    "The ripple simulation is enabled but neither a material template nor the package shader is available.",
                    Water25DFixAction.AssignRippleMaterial));
            }

            if (Application.isPlaying && controller.RippleTexture != null && controller.RippleTexture is RenderTexture renderTexture && renderTexture.useMipMap)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Ripple texture has mipmaps",
                    "The optimized ripple state must not allocate or generate mipmaps."));
            }
        }

        private static void ValidateReflection(
            Water25DController controller,
            SerializedObject serializedObject,
            List<Water25DValidationResult> results,
            WaterReflectionMode reflectionMode)
        {
            if (reflectionMode != WaterReflectionMode.Planar)
            {
                return;
            }

            var sourceCamera = GetObject<Camera>(serializedObject, "_reflectionCameraSource");
            if (sourceCamera == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Planar reflection camera missing",
                    "Planar mode falls back to Camera.main when possible, but assigning an explicit source camera makes the reflection group deterministic."));
            }
            else if (!sourceCamera.enabled)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Planar reflection camera disabled",
                    "The selected source camera is disabled and cannot provide the intended reflection view.",
                    Water25DFixAction.SelectObject,
                    sourceCamera));
            }

            var cullingMask = GetLayerMask(serializedObject, "_reflectionCullingMask");
            var surface = controller.TopSurface;
            if (surface != null && (cullingMask.value & (1 << surface.gameObject.layer)) != 0)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Reflection mask may recurse",
                    "The top surface layer is included in the planar reflection culling mask. Exclude the water layer if the renderer recurses.",
                    Water25DFixAction.None,
                    surface.gameObject));
            }

            var scale = GetFloat(serializedObject, "_reflectionResolutionScale", 0.25f);
            var interval = GetInt(serializedObject, "_reflectionUpdateIntervalFrames", 3);
            if (scale < 0.1f || scale > 1f || interval < 1)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Error,
                    "Planar reflection settings invalid",
                    "Reflection resolution scale must be between 0.1 and 1, and update interval must be positive."));
            }

            if (Application.isPlaying && !Water25D.Rendering.WaterReflectionManager.HasInstance)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Planar reflection is not registered",
                    "No WaterReflectionManager exists in Play Mode. The surface will not receive a shared planar reflection until it registers."));
            }
        }

        private static void ValidateEffects(SerializedObject serializedObject, List<Water25DValidationResult> results, bool enableEffects)
        {
            if (!enableEffects)
            {
                return;
            }

            var splash = GetObject<WaterFXDefinition>(serializedObject, "_splashDefinition");
            var bubble = GetObject<WaterFXDefinition>(serializedObject, "_bubbleDefinition");
            if (splash == null || bubble == null)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Optional FX definitions not assigned",
                    "Effects are enabled, but splash and/or bubble definitions are missing. Water25D will use its lightweight fallback effect where supported."));
            }
        }

        private static void ValidateSerializedReference(SerializedObject serializedObject, string propertyPath, Transform expected, List<Water25DValidationResult> results)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property != null && property.objectReferenceValue != null && property.objectReferenceValue != expected)
            {
                results.Add(new Water25DValidationResult(
                    Water25DValidationSeverity.Warning,
                    "Generated reference differs from hierarchy",
                    propertyPath + " does not point to the named generated child. Repair Hierarchy will rewire package references safely.",
                    Water25DFixAction.RepairHierarchy,
                    expected));
            }
        }

        private static Transform FindChild(Transform root, string childName)
        {
            return root != null ? root.Find(childName) : null;
        }

        private static WaterReflectionMode GetReflectionMode(SerializedObject serializedObject)
        {
            var property = serializedObject.FindProperty("_reflectionMode");
            return property != null ? (WaterReflectionMode)property.enumValueIndex : WaterReflectionMode.Stylized;
        }

        private static T GetObject<T>(SerializedObject serializedObject, string propertyPath) where T : UnityEngine.Object
        {
            var property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.objectReferenceValue as T : null;
        }

        private static bool GetBool(SerializedObject serializedObject, string propertyPath, bool fallback)
        {
            var property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.boolValue : fallback;
        }

        private static float GetFloat(SerializedObject serializedObject, string propertyPath, float fallback)
        {
            var property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.floatValue : fallback;
        }

        private static int GetInt(SerializedObject serializedObject, string propertyPath, int fallback)
        {
            var property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.intValue : fallback;
        }

        private static Vector2Int GetVector2Int(SerializedObject serializedObject, string propertyPath, Vector2Int fallback)
        {
            var property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.vector2IntValue : fallback;
        }

        private static LayerMask GetLayerMask(SerializedObject serializedObject, string propertyPath)
        {
            var property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.intValue : 0;
        }
    }
}
