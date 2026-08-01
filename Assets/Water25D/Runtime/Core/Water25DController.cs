using System;
using UnityEngine;
using UnityEngine.Events;
using Water25D.FX;
using Water25D.Rendering;

namespace Water25D
{
    /// <summary>
    /// Coordinates geometry, presentation, simulation and the two independent 2D physics volumes.
    /// Generated resources belong to this instance and are never taken from the reference systems.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class Water25DController : MonoBehaviour
    {
        [Header("Dimensions")]
        [Tooltip("Width and visual depth of the XZ top surface in local units.")]
        [SerializeField] private Vector2 _topSurfaceSize = new Vector2(20f, 6.5f);
        [Tooltip("Physical depth of the XY front surface and buoyancy volume.")]
        [Min(0.01f)] [SerializeField] private float _frontSurfaceDepth = 10f;
        [Tooltip("Local Y coordinate of the waterline. The root can remain at any convenient scene position.")]
        [SerializeField] private float _waterlineLocalY;
        [Range(0f, 1f)] [SerializeField] private float _interactionDepth01 = 0.5f;
        [Min(0.01f)] [SerializeField] private float _surfaceTriggerThickness = 0.25f;

        [Header("Surface")]
        [SerializeField] private WaterSurfaceMode _surfaceMode;

        [Header("Profiles")]
        [SerializeField] private WaterStyleProfile _styleProfile;
        [SerializeField] private WaterQualityProfile _qualityProfile;

        [Header("Optional Material Templates")]
        [Tooltip("Optional template for the top mesh. The asset is only read at runtime.")]
        [SerializeField] private Material _topMaterialTemplate;
        [Tooltip("Optional template for the front mesh. The asset is only read at runtime.")]
        [SerializeField] private Material _frontMaterialTemplate;
        [Tooltip("Optional template for the runtime ripple material. It is cloned before simulation parameters are changed.")]
        [SerializeField] private Material _rippleSimulationMaterialTemplate;

        [Header("Sorting")]
        [SerializeField] private string _topSortingLayerName = "Default";
        [SerializeField] private int _topSortingOrder;
        [SerializeField] private string _frontSortingLayerName = "Default";
        [SerializeField] private int _frontSortingOrder;

        [Header("Reflection")]
        [SerializeField] private WaterReflectionMode _reflectionMode = WaterReflectionMode.Stylized;
        [SerializeField] private Camera _reflectionCameraSource;
        [SerializeField] private LayerMask _reflectionCullingMask = ~0;
        [Range(0.1f, 1f)] [SerializeField] private float _reflectionResolutionScale = 0.25f;
        [Range(1, 120)] [SerializeField] private int _reflectionUpdateIntervalFrames = 3;
        [Range(0f, 1f)] [SerializeField] private float _reflectionStrength = 0.35f;

        [Header("FX")]
        [SerializeField] private bool _enableEffects = true;
        [SerializeField] private WaterFXDefinition _splashDefinition;
        [SerializeField] private WaterFXDefinition _bubbleDefinition;
        [Range(1, 64)] [SerializeField] private int _maximumFxPoolSize = 16;

        [Header("Physics and Interaction")]
        [SerializeField] private bool _enableSurfaceInteraction = true;
        [SerializeField] private bool _enableBuoyancy = true;
        [SerializeField] private LayerMask _surfaceInteractionLayers = ~0;
        [SerializeField] private LayerMask _surfaceTriggerInteractionLayers = ~0;
        [SerializeField] private LayerMask _buoyancyLayers = ~0;
        [SerializeField] private bool _includeTriggerCollidersInSurfaceInteraction = true;
        [Min(0f)] [SerializeField] private float _buoyancyDensity = 1f;
        [Min(0f)] [SerializeField] private float _buoyancyLinearDamping = 0.1f;
        [Min(0f)] [SerializeField] private float _buoyancyAngularDamping = 0.1f;
        [SerializeField] private bool _enableCustomDrag;
        [Tooltip("Keep this modest when BuoyancyEffector2D linear damping is enabled.")]
        [Min(0f)] [SerializeField] private float _customLinearDrag;
        [Tooltip("Keep this modest when BuoyancyEffector2D angular damping is enabled.")]
        [Min(0f)] [SerializeField] private float _customAngularDrag;

        [Header("Ripple Simulation")]
        [SerializeField] private bool _enableRippleSimulation = true;
        [Min(0.01f)] [SerializeField] private float _impactSpeedForFullStrength = 6f;
        [Range(0f, 1f)] [SerializeField] private float _minimumImpactStrength = 0.08f;
        [Range(0f, 2f)] [SerializeField] private float _impactStrengthMultiplier = 1f;

        [Header("Authoring")]
        [SerializeField] private bool _synchronizeGeneratedChildLayers = true;

        [Header("Events")]
        [SerializeField] private UnityEvent _onSurfaceEnter = new UnityEvent();
        [SerializeField] private UnityEvent _onSurfaceExit = new UnityEvent();
        [SerializeField] private UnityEvent _onSubmerged = new UnityEvent();
        [SerializeField] private UnityEvent _onResurfaced = new UnityEvent();

        [Header("Generated Hierarchy References")]
        [SerializeField, HideInInspector] private Transform _topSurface;
        [SerializeField, HideInInspector] private Transform _frontSurface;
        [SerializeField, HideInInspector] private Transform _surfaceCrossingTrigger;
        [SerializeField, HideInInspector] private Transform _buoyancyVolume;
        [SerializeField, HideInInspector] private Transform _reflectionAnchor;
        [SerializeField, HideInInspector] private Transform _fxRoot;

        [NonSerialized] private WaterRuntimeResources _runtimeResources;
        [NonSerialized] private WaterHierarchyModule _hierarchy;
        [NonSerialized] private WaterGeometryModule _geometry;
        [NonSerialized] private WaterRenderingModule _rendering;
        [NonSerialized] private WaterPhysicsModule _physics;
        [NonSerialized] private WaterRippleModule _ripple;
        [NonSerialized] private WaterReflectionModule _reflection;
        [NonSerialized] private bool _isApplyingChanges;
        [NonSerialized] private bool _hasLoggedMissingSurfaceShader;
        [NonSerialized] private bool _effectsConfigurationPending;
        [NonSerialized] private bool _reflectionConfigurationPending;

        public event Action<WaterInteractionEvent> SurfaceEntered;
        public event Action<WaterInteractionEvent> SurfaceExited;
        public event Action<WaterInteractionEvent> Submerged;
        public event Action<WaterInteractionEvent> Resurfaced;

        public Vector2 TopSurfaceSize => _topSurfaceSize;
        public float FrontSurfaceDepth => _frontSurfaceDepth;
        public float WaterlineLocalY => _waterlineLocalY;
        public float WaterlineWorldY => transform.TransformPoint(new Vector3(0f, _waterlineLocalY, 0f)).y;
        public float InteractionDepth01 => _interactionDepth01;
        public WaterSurfaceMode SurfaceMode => _surfaceMode;
        public WaterStyleProfile StyleProfile => _styleProfile;
        public WaterQualityProfile QualityProfile => _qualityProfile;
        public Transform TopSurface => _hierarchy != null ? _hierarchy.TopSurface : _topSurface;
        public Transform FrontSurface => _hierarchy != null ? _hierarchy.FrontSurface : _frontSurface;
        public Transform SurfaceCrossingTrigger => _hierarchy != null ? _hierarchy.SurfaceCrossingTrigger : _surfaceCrossingTrigger;
        public Transform BuoyancyVolume => _hierarchy != null ? _hierarchy.BuoyancyVolume : _buoyancyVolume;
        public Transform ReflectionAnchor => _hierarchy != null ? _hierarchy.ReflectionAnchor : _reflectionAnchor;
        public Transform FxRoot => _hierarchy != null ? _hierarchy.FxRoot : _fxRoot;
        public WaterReflectionMode ReflectionMode => _reflectionMode;
        public Camera ReflectionCameraSource => _reflectionCameraSource;
        public Texture RippleTexture => _ripple != null ? _ripple.HeightTexture : null;
        public bool RippleSimulationAvailable => _ripple != null && _ripple.IsAvailable;
        public bool IsRippleSimulationSuspended => _ripple != null && _ripple.IsSuspended;
        public int DroppedRippleImpactCount => _ripple != null ? _ripple.DroppedImpactCount : 0;

        private void OnEnable()
        {
            EnsureModules();
            ApplyAuthoringChanges();
        }

        private void Reset()
        {
            // Reset is called for a newly added component (and for an explicit Inspector
            // reset). Existing serialized controllers omit this field and therefore keep
            // the enum's zero-valued legacy-compatible mode.
            _surfaceMode = WaterSurfaceMode.FlatStylized;
            EnsureModules();
            ApplyAuthoringChanges();
        }

        private void Update()
        {
            EnsureModules();
            if (_effectsConfigurationPending)
            {
                ConfigureEffects();
                _effectsConfigurationPending = false;
            }

            if (_reflectionConfigurationPending)
            {
                RegisterReflectionSurface();
                _reflectionConfigurationPending = false;
            }

            var isVisible = (_hierarchy.TopMeshRenderer != null && _hierarchy.TopMeshRenderer.isVisible) ||
                            (_hierarchy.FrontMeshRenderer != null && _hierarchy.FrontMeshRenderer.isVisible);
            if (_surfaceMode == WaterSurfaceMode.SimulatedRipples && _enableRippleSimulation)
            {
                _ripple.Tick(_surfaceMode, Time.deltaTime, isVisible);
            }
        }

        private void OnDisable()
        {
            DisposeRuntimeResources();
        }

        private void OnDestroy()
        {
            DisposeRuntimeResources();
        }

        private void OnValidate()
        {
            SanitizeSerializedValues();
            if (_isApplyingChanges || !isActiveAndEnabled)
            {
                return;
            }

            // Unity may invoke OnValidate while a serialized property is still being
            // applied. Replacing transient meshes from that callback is destructive and
            // is rejected by the Editor. The package editor and Undo/Redo path explicitly
            // call RefreshAuthoringPreview after serialization has completed; runtime
            // validation can apply immediately.
            if (!Application.isPlaying)
            {
                return;
            }

            // Changes are applied incrementally. Style changes only touch property blocks;
            // geometry and simulation resources are rebuilt only when their inputs changed.
            ApplyAuthoringChanges();
        }

        [ContextMenu("Repair Hierarchy and Rebuild")]
        public void RepairHierarchyAndRebuild()
        {
            ApplyAuthoringChanges();
        }

        /// <summary>
        /// Reapplies property-block, hierarchy and module configuration without forcing a
        /// geometry rebuild when the geometry inputs are unchanged. Editor tooling uses this
        /// after a shared profile asset changes so the preview stays current without marking
        /// the containing scene dirty.
        /// </summary>
        public void RefreshAuthoringPreview()
        {
            EnsureModules();
            ApplyAuthoringChanges();
        }

        /// <summary>
        /// Invalidates the geometry cache and rebuilds the transient preview meshes.
        /// </summary>
        public void RebuildGeometryPreview()
        {
            EnsureModules();
            _geometry.Reset();
            ApplyAuthoringChanges();
        }

        /// <summary>
        /// Clears the instance-owned ripple state. Runtime ripple state is intentionally not
        /// available in edit mode, so this is a safe no-op while authoring.
        /// </summary>
        public void ResetRippleSimulation()
        {
            if (!Application.isPlaying || !_enableRippleSimulation || _surfaceMode != WaterSurfaceMode.SimulatedRipples)
            {
                return;
            }

            EnsureModules();
            _ripple.ResetSimulation(_surfaceMode);
        }

        public void SetDimensions(Vector2 topSurfaceSize, float frontSurfaceDepth)
        {
            _topSurfaceSize = topSurfaceSize;
            _frontSurfaceDepth = frontSurfaceDepth;
            SanitizeSerializedValues();
            ApplyAuthoringChanges();
        }

        public void SetWaterlineLocalY(float waterlineLocalY)
        {
            _waterlineLocalY = waterlineLocalY;
            ApplyAuthoringChanges();
        }

        /// <summary>
        /// Changes the presentation mode and immediately applies its resource-ownership
        /// contract. In particular, switching to FlatStylized releases any active CRT.
        /// </summary>
        public void SetSurfaceMode(WaterSurfaceMode surfaceMode)
        {
            if (_surfaceMode == surfaceMode)
            {
                return;
            }

            _surfaceMode = surfaceMode;
            SanitizeSerializedValues();
            ApplyAuthoringChanges();
        }

        /// <summary>
        /// Queues a world-space impact. The point is rejected when it lies outside the XZ top surface.
        /// </summary>
        public bool CreateContactRippleAt(Vector3 worldPosition, float initialStrength, bool initialUp = true)
        {
            var settings = GetQualitySettings();
            return CreateContactRippleAt(worldPosition, initialStrength, initialUp, settings.ImpactRadius);
        }

        public bool CreateContactRippleAt(Vector3 worldPosition, float initialStrength, bool initialUp, float radius)
        {
            if (!_enableRippleSimulation || !Application.isPlaying || _surfaceMode != WaterSurfaceMode.SimulatedRipples)
            {
                return false;
            }

            if (!TryGetSurfaceUV(worldPosition, out var uv))
            {
                return false;
            }

            EnsureModules();
            if (!_ripple.IsAvailable)
            {
                return false;
            }

            _ripple.EnqueueImpact(new WaterRippleImpact(uv, initialStrength, radius, initialUp));
            return true;
        }

        public bool TryGetSurfaceUV(Vector3 worldPosition, out Vector2 uv)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            var width = Mathf.Max(0.01f, _topSurfaceSize.x);
            var depth = Mathf.Max(0.01f, _topSurfaceSize.y);
            if (localPosition.x < 0f || localPosition.x > width || localPosition.z < 0f || localPosition.z > depth)
            {
                uv = default;
                return false;
            }

            uv = new Vector2(localPosition.x / width, localPosition.z / depth);
            return true;
        }

        public Vector3 GetInteractionWorldPosition(Vector2 worldPosition)
        {
            // 2D gameplay supplies X/Y. The explicit lane depth keeps that gameplay plane
            // independent from the visual top-surface resolution.
            var localPosition = transform.InverseTransformPoint(new Vector3(worldPosition.x, transform.position.y, transform.position.z));
            localPosition.x = Mathf.Clamp(localPosition.x, 0f, Mathf.Max(0.01f, _topSurfaceSize.x));
            localPosition.y = _waterlineLocalY;
            localPosition.z = Mathf.Clamp01(_interactionDepth01) * Mathf.Max(0.01f, _topSurfaceSize.y);
            return transform.TransformPoint(localPosition);
        }

        public float CalculateImpactStrength(Vector2 velocity)
        {
            var speed = velocity.magnitude;
            var normalizedSpeed = speed / Mathf.Max(0.01f, _impactSpeedForFullStrength);
            return Mathf.Clamp(Mathf.Max(_minimumImpactStrength, normalizedSpeed) * _impactStrengthMultiplier, 0f, 1f);
        }

        public float GetImpactRadius(Vector2 velocity)
        {
            var settings = GetQualitySettings();
            var speedFactor = Mathf.Clamp01(velocity.magnitude / Mathf.Max(0.01f, _impactSpeedForFullStrength));
            return settings.ImpactRadius * (0.75f + speedFactor * 0.5f);
        }

        internal void NotifyInteraction(WaterInteractionEvent eventData)
        {
            _hierarchy?.FxController?.HandleInteraction(eventData);
            switch (eventData.Type)
            {
                case WaterInteractionEventType.SurfaceEnter:
                    SurfaceEntered?.Invoke(eventData);
                    _onSurfaceEnter?.Invoke();
                    break;
                case WaterInteractionEventType.SurfaceExit:
                    SurfaceExited?.Invoke(eventData);
                    _onSurfaceExit?.Invoke();
                    break;
                case WaterInteractionEventType.Submerged:
                    Submerged?.Invoke(eventData);
                    _onSubmerged?.Invoke();
                    break;
                case WaterInteractionEventType.Resurfaced:
                    Resurfaced?.Invoke(eventData);
                    _onResurfaced?.Invoke();
                    break;
            }
        }

        private void ApplyAuthoringChanges()
        {
            if (_isApplyingChanges)
            {
                return;
            }

            _isApplyingChanges = true;
            try
            {
                SanitizeSerializedValues();
                EnsureModules();
                _hierarchy.Initialise(
                    transform,
                    _topSurface,
                    _frontSurface,
                    _surfaceCrossingTrigger,
                    _buoyancyVolume,
                    _reflectionAnchor,
                    _fxRoot,
                    _synchronizeGeneratedChildLayers);
                _topSurface = _hierarchy.TopSurface;
                _frontSurface = _hierarchy.FrontSurface;
                _surfaceCrossingTrigger = _hierarchy.SurfaceCrossingTrigger;
                _buoyancyVolume = _hierarchy.BuoyancyVolume;
                _reflectionAnchor = _hierarchy.ReflectionAnchor;
                _fxRoot = _hierarchy.FxRoot;
                if (_runtimeResources == null)
                {
                    _runtimeResources = new WaterRuntimeResources();
                }

                var qualitySettings = GetQualitySettings();
                _geometry.ApplyIfNeeded(
                    _topSurfaceSize,
                    _frontSurfaceDepth,
                    qualitySettings,
                    _surfaceMode,
                    _runtimeResources,
                    _hierarchy.TopMeshFilter,
                    _hierarchy.FrontMeshFilter);
                _physics.Apply(
                    this,
                    _hierarchy,
                    _topSurfaceSize,
                    _frontSurfaceDepth,
                    _waterlineLocalY,
                    _surfaceTriggerThickness,
                    _enableSurfaceInteraction,
                    _enableBuoyancy,
                    _surfaceInteractionLayers,
                    _surfaceTriggerInteractionLayers,
                    _buoyancyLayers,
                    _includeTriggerCollidersInSurfaceInteraction,
                    _buoyancyDensity,
                    _buoyancyLinearDamping,
                    _buoyancyAngularDamping,
                    _enableCustomDrag,
                    _customLinearDrag,
                    _customAngularDrag);
                _effectsConfigurationPending = true;

                if (Application.isPlaying && _enableRippleSimulation && _surfaceMode == WaterSurfaceMode.SimulatedRipples)
                {
                    _ripple.Ensure(_runtimeResources, _topSurfaceSize, qualitySettings, _rippleSimulationMaterialTemplate, this, _surfaceMode);
                }
                else
                {
                    _ripple.Dispose();
                }

                _rendering.Apply(
                    _runtimeResources,
                    _hierarchy.TopMeshRenderer,
                    _hierarchy.TopSortingGroup,
                    _hierarchy.FrontMeshRenderer,
                    _hierarchy.FrontSortingGroup,
                    _topMaterialTemplate,
                    _frontMaterialTemplate,
                    _styleProfile,
                    _topSurfaceSize,
                    _frontSurfaceDepth,
                    _waterlineLocalY,
                    qualitySettings,
                    _surfaceMode,
                    RippleTexture,
                    _topSortingLayerName,
                    _topSortingOrder,
                    _frontSortingLayerName,
                    _frontSortingOrder,
                    _reflectionMode,
                    _reflectionStrength,
                    out var topMaterial,
                    out var frontMaterial);
                if ((topMaterial == null || frontMaterial == null) && !_hasLoggedMissingSurfaceShader)
                {
                    Debug.LogWarning("Water25D is missing a top or front surface material. Assign package defaults or reimport the package shaders.", this);
                    _hasLoggedMissingSurfaceShader = true;
                }
                _reflectionConfigurationPending = true;
            }
            finally
            {
                _isApplyingChanges = false;
            }
        }

        private void RegisterReflectionSurface()
        {
            _reflection.Configure(
                _hierarchy.TopMeshRenderer,
                _hierarchy.ReflectionAnchor,
                _reflectionCameraSource,
                _reflectionMode,
                _reflectionCullingMask,
                _reflectionResolutionScale,
                _reflectionUpdateIntervalFrames,
                _reflectionStrength);
        }

        private void ConfigureEffects()
        {
            _hierarchy.FxController?.Configure(this, _enableEffects, _splashDefinition, _bubbleDefinition, _maximumFxPoolSize);
        }

        private WaterQualitySettings GetQualitySettings()
        {
            return _qualityProfile != null ? _qualityProfile.GetSettings() : WaterQualitySettings.Default;
        }

        private void SanitizeSerializedValues()
        {
            _topSurfaceSize.x = Mathf.Max(0.01f, _topSurfaceSize.x);
            _topSurfaceSize.y = Mathf.Max(0.01f, _topSurfaceSize.y);
            _frontSurfaceDepth = Mathf.Max(0.01f, _frontSurfaceDepth);
            _surfaceTriggerThickness = Mathf.Max(0.01f, _surfaceTriggerThickness);
            _interactionDepth01 = Mathf.Clamp01(_interactionDepth01);
            _buoyancyDensity = Mathf.Max(0f, _buoyancyDensity);
            _buoyancyLinearDamping = Mathf.Max(0f, _buoyancyLinearDamping);
            _buoyancyAngularDamping = Mathf.Max(0f, _buoyancyAngularDamping);
            _customLinearDrag = Mathf.Max(0f, _customLinearDrag);
            _customAngularDrag = Mathf.Max(0f, _customAngularDrag);
            _impactSpeedForFullStrength = Mathf.Max(0.01f, _impactSpeedForFullStrength);
            _minimumImpactStrength = Mathf.Clamp01(_minimumImpactStrength);
            _impactStrengthMultiplier = Mathf.Clamp(_impactStrengthMultiplier, 0f, 2f);
            _reflectionResolutionScale = Mathf.Clamp(_reflectionResolutionScale, 0.1f, 1f);
            _reflectionUpdateIntervalFrames = Mathf.Clamp(_reflectionUpdateIntervalFrames, 1, 120);
            _reflectionStrength = Mathf.Clamp01(_reflectionStrength);
            if (_surfaceMode != WaterSurfaceMode.SimulatedRipples && _surfaceMode != WaterSurfaceMode.FlatStylized)
            {
                _surfaceMode = WaterSurfaceMode.SimulatedRipples;
            }
        }

        private void EnsureModules()
        {
            if (_hierarchy == null)
            {
                _hierarchy = new WaterHierarchyModule();
            }

            if (_geometry == null)
            {
                _geometry = new WaterGeometryModule();
            }

            if (_rendering == null)
            {
                _rendering = new WaterRenderingModule();
            }

            if (_physics == null)
            {
                _physics = new WaterPhysicsModule();
            }

            if (_ripple == null)
            {
                _ripple = new WaterRippleModule();
            }

            if (_reflection == null)
            {
                _reflection = new WaterReflectionModule();
            }
        }

        private void DisposeRuntimeResources()
        {
            _reflection?.Dispose();
            _hierarchy?.FxController?.DisposeRuntimeResources();
            _ripple?.Dispose();
            if (_runtimeResources == null)
            {
                _geometry?.Reset();
                return;
            }

            if (_hierarchy?.TopMeshFilter != null)
            {
                _hierarchy.TopMeshFilter.sharedMesh = null;
                if (_hierarchy.TopMeshRenderer != null && _runtimeResources.OwnsTopSurfaceMaterial && _hierarchy.TopMeshRenderer.sharedMaterial == _runtimeResources.TopSurfaceMaterial)
                {
                    _hierarchy.TopMeshRenderer.sharedMaterial = null;
                }
            }

            if (_hierarchy?.FrontMeshFilter != null)
            {
                _hierarchy.FrontMeshFilter.sharedMesh = null;
                if (_hierarchy.FrontMeshRenderer != null && _runtimeResources.OwnsFrontSurfaceMaterial && _hierarchy.FrontMeshRenderer.sharedMaterial == _runtimeResources.FrontSurfaceMaterial)
                {
                    _hierarchy.FrontMeshRenderer.sharedMaterial = null;
                }
            }

            _runtimeResources.Dispose();
            _runtimeResources = null;
            _geometry?.Reset();
        }
    }
}
