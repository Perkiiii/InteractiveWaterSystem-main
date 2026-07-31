using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Water25D.FX;
using Water25D.Rendering;

namespace Water25D
{
    /// <summary>
    /// Coordinates geometry, presentation, simulation and the two independent 2D physics volumes.
    /// Generated resources belong to this instance and are never taken from the reference systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Water25DController : MonoBehaviour
    {
        private const string TopSurfaceName = "TopSurface";
        private const string FrontSurfaceName = "FrontSurface";
        private const string SurfaceCrossingTriggerName = "SurfaceCrossingTrigger";
        private const string BuoyancyVolumeName = "BuoyancyVolume";
        private const string ReflectionAnchorName = "ReflectionAnchor";
        private const string FxRootName = "FXRoot";

        [Header("Dimensions")]
        [Tooltip("Width and visual depth of the XZ top surface in local units.")]
        [SerializeField] private Vector2 _topSurfaceSize = new Vector2(20f, 6.5f);
        [Tooltip("Physical depth of the XY front surface and buoyancy volume.")]
        [Min(0.01f)] [SerializeField] private float _frontSurfaceDepth = 10f;
        [Tooltip("Local Y coordinate of the waterline. The root can remain at any convenient scene position.")]
        [SerializeField] private float _waterlineLocalY;
        [Range(0f, 1f)] [SerializeField] private float _interactionDepth01 = 0.5f;
        [Min(0.01f)] [SerializeField] private float _surfaceTriggerThickness = 0.25f;

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

        private MeshFilter _topMeshFilter;
        private MeshRenderer _topMeshRenderer;
        private SortingGroup _topSortingGroup;
        private MeshFilter _frontMeshFilter;
        private MeshRenderer _frontMeshRenderer;
        private SortingGroup _frontSortingGroup;
        private BoxCollider2D _surfaceCollider;
        private WaterSurfaceInteraction2D _surfaceInteraction;
        private BoxCollider2D _buoyancyCollider;
        private BuoyancyEffector2D _buoyancyEffector;
        private WaterPhysicsVolume2D _physicsVolume;
        private WaterFXController _fxController;

        [NonSerialized] private WaterRuntimeResources _runtimeResources;
        [NonSerialized] private IWaterRippleSimulator _rippleSimulator;
        [NonSerialized] private MaterialPropertyBlock _materialPropertyBlock;
        [NonSerialized] private WaterQualitySettings _appliedQualitySettings;
        [NonSerialized] private Vector2 _appliedTopSurfaceSize;
        [NonSerialized] private float _appliedFrontSurfaceDepth;
        [NonSerialized] private float _appliedWaterlineLocalY;
        [NonSerialized] private Vector2 _appliedRippleWaterSize;
        [NonSerialized] private bool _geometryApplied;
        [NonSerialized] private bool _hasInitializedOnce;
        [NonSerialized] private bool _isApplyingChanges;
        [NonSerialized] private bool _hasLoggedMissingSurfaceShader;
        [NonSerialized] private bool _hasLoggedMissingRippleShader;
        [NonSerialized] private WaterReflectionManager.ReflectionRegistration _reflectionRegistration;
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
        public WaterStyleProfile StyleProfile => _styleProfile;
        public WaterQualityProfile QualityProfile => _qualityProfile;
        public Transform TopSurface => _topSurface;
        public Transform FrontSurface => _frontSurface;
        public Transform SurfaceCrossingTrigger => _surfaceCrossingTrigger;
        public Transform BuoyancyVolume => _buoyancyVolume;
        public Transform ReflectionAnchor => _reflectionAnchor;
        public Transform FxRoot => _fxRoot;
        public WaterReflectionMode ReflectionMode => _reflectionMode;
        public Camera ReflectionCameraSource => _reflectionCameraSource;
        public Texture RippleTexture => _rippleSimulator != null ? _rippleSimulator.HeightTexture : null;
        public bool IsRippleSimulationSuspended => _rippleSimulator != null && _rippleSimulator.IsSuspended;
        public int DroppedRippleImpactCount => _rippleSimulator != null ? _rippleSimulator.DroppedImpactCount : 0;

        private void Awake()
        {
            ApplyAuthoringChanges();
        }

        private void OnEnable()
        {
            ApplyAuthoringChanges();
        }

        private void Start()
        {
            ApplyAuthoringChanges();
        }

        private void Update()
        {
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

            if (_rippleSimulator == null)
            {
                return;
            }

            var isVisible = (_topMeshRenderer != null && _topMeshRenderer.isVisible) ||
                            (_frontMeshRenderer != null && _frontMeshRenderer.isVisible);
            _rippleSimulator.Tick(Time.deltaTime, isVisible);
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
            if (_isApplyingChanges || !isActiveAndEnabled || (!Application.isPlaying && !_hasInitializedOnce))
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

        public void SetDimensions(Vector2 topSurfaceSize, float frontSurfaceDepth)
        {
            _topSurfaceSize = topSurfaceSize;
            _frontSurfaceDepth = frontSurfaceDepth;
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
            if (!_enableRippleSimulation || !Application.isPlaying)
            {
                return false;
            }

            if (!TryGetSurfaceUV(worldPosition, out var uv))
            {
                return false;
            }

            if (_rippleSimulator == null)
            {
                EnsureRippleSimulator(GetQualitySettings());
            }

            if (_rippleSimulator == null || !_rippleSimulator.IsAvailable)
            {
                return false;
            }

            _rippleSimulator.EnqueueImpact(new WaterRippleImpact(uv, initialStrength, radius, initialUp));
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
            _fxController?.HandleInteraction(eventData);
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
                EnsureHierarchy();
                if (_runtimeResources == null)
                {
                    _runtimeResources = new WaterRuntimeResources();
                }

                var qualitySettings = GetQualitySettings();
                ApplyGeometryIfNeeded(qualitySettings);
                ConfigurePhysicsVolumes();
                _effectsConfigurationPending = true;

                if (Application.isPlaying && _enableRippleSimulation)
                {
                    EnsureRippleSimulator(qualitySettings);
                }
                else
                {
                    DisposeRippleSimulator();
                }

                ApplyRendererBindings();
                _reflectionConfigurationPending = true;
                _hasInitializedOnce = true;
            }
            finally
            {
                _isApplyingChanges = false;
            }
        }

        private void ApplyGeometryIfNeeded(WaterQualitySettings qualitySettings)
        {
            var vertexCount = WaterMeshBuilder.CalculateTopVertexCount(_topSurfaceSize, qualitySettings.TopVerticesPerUnit);
            var geometryChanged = !_geometryApplied ||
                                  _appliedTopSurfaceSize != _topSurfaceSize ||
                                  !Mathf.Approximately(_appliedFrontSurfaceDepth, _frontSurfaceDepth) ||
                                  !Mathf.Approximately(_appliedWaterlineLocalY, _waterlineLocalY) ||
                                  _appliedQualitySettings.TopVerticesPerUnit != qualitySettings.TopVerticesPerUnit;
            if (!geometryChanged)
            {
                return;
            }

            if (_topMeshFilter != null)
            {
                _topMeshFilter.sharedMesh = null;
            }

            if (_frontMeshFilter != null)
            {
                _frontMeshFilter.sharedMesh = null;
            }

            _runtimeResources.ReplaceTopMesh(WaterMeshBuilder.BuildTopMesh(_topSurfaceSize, vertexCount, "Water25D Top Mesh"));
            _runtimeResources.ReplaceFrontMesh(WaterMeshBuilder.BuildFrontMesh(_topSurfaceSize, _frontSurfaceDepth, vertexCount.x, "Water25D Front Mesh"));
            _topMeshFilter.sharedMesh = _runtimeResources.TopMesh;
            _frontMeshFilter.sharedMesh = _runtimeResources.FrontMesh;

            _appliedTopSurfaceSize = _topSurfaceSize;
            _appliedFrontSurfaceDepth = _frontSurfaceDepth;
            _appliedWaterlineLocalY = _waterlineLocalY;
            _appliedQualitySettings = qualitySettings;
            _geometryApplied = true;
        }

        private void ConfigurePhysicsVolumes()
        {
            var width = Mathf.Max(0.01f, _topSurfaceSize.x);
            var depth = Mathf.Max(0.01f, _frontSurfaceDepth);
            var triggerThickness = Mathf.Max(0.01f, _surfaceTriggerThickness);

            _topSurface.localPosition = new Vector3(0f, _waterlineLocalY, 0f);
            _frontSurface.localPosition = new Vector3(0f, _waterlineLocalY, 0f);
            _surfaceCrossingTrigger.localPosition = new Vector3(width * 0.5f, _waterlineLocalY, 0f);
            _buoyancyVolume.localPosition = new Vector3(width * 0.5f, _waterlineLocalY, 0f);
            _reflectionAnchor.localPosition = new Vector3(width * 0.5f, _waterlineLocalY, 0f);
            _fxRoot.localPosition = new Vector3(width * 0.5f, _waterlineLocalY, 0f);

            _surfaceCollider.size = new Vector2(width, triggerThickness);
            _surfaceCollider.offset = Vector2.zero;
            _surfaceCollider.isTrigger = true;
            _surfaceCollider.enabled = _enableSurfaceInteraction;
            _surfaceInteraction.Configure(
                this,
                _surfaceInteractionLayers,
                _surfaceTriggerInteractionLayers,
                _includeTriggerCollidersInSurfaceInteraction);
            _surfaceInteraction.enabled = _enableSurfaceInteraction;

            _buoyancyCollider.size = new Vector2(width, depth);
            _buoyancyCollider.offset = new Vector2(0f, -depth * 0.5f);
            _buoyancyCollider.isTrigger = true;
            _buoyancyCollider.enabled = _enableBuoyancy;
            _physicsVolume.Configure(
                this,
                _buoyancyLayers,
                _enableCustomDrag,
                _customLinearDrag,
                _customAngularDrag);
            _physicsVolume.enabled = _enableBuoyancy;

            if (_enableBuoyancy && _buoyancyEffector == null)
            {
                _buoyancyEffector = _buoyancyVolume.gameObject.AddComponent<BuoyancyEffector2D>();
            }

            if (_buoyancyEffector != null)
            {
                _buoyancyEffector.enabled = _enableBuoyancy;
                _buoyancyEffector.surfaceLevel = 0f;
                _buoyancyEffector.density = Mathf.Max(0f, _buoyancyDensity);
                _buoyancyEffector.linearDamping = Mathf.Max(0f, _buoyancyLinearDamping);
                _buoyancyEffector.angularDamping = Mathf.Max(0f, _buoyancyAngularDamping);
                _buoyancyEffector.flowMagnitude = 0f;
                _buoyancyEffector.useColliderMask = true;
                _buoyancyEffector.colliderMask = _buoyancyLayers.value;
            }

            _buoyancyCollider.usedByEffector = _enableBuoyancy && _buoyancyEffector != null;
        }

        private void EnsureRippleSimulator(WaterQualitySettings qualitySettings)
        {
            var materialTemplate = _rippleSimulationMaterialTemplate;
            var needsNewSimulator = _rippleSimulator == null ||
                                    !_rippleSimulator.IsAvailable ||
                                    _appliedRippleWaterSize != _topSurfaceSize ||
                                    !_appliedQualitySettings.SimulationEquals(qualitySettings) ||
                                    _appliedRippleMaterialTemplate != materialTemplate;
            if (!needsNewSimulator)
            {
                return;
            }

            DisposeRippleSimulator();
            var fallbackShader = Shader.Find("Water25D/Ripple Simulation");
            if (materialTemplate == null && fallbackShader == null)
            {
                if (!_hasLoggedMissingRippleShader)
                {
                    Debug.LogWarning("Water25D ripple simulation could not find its package shader. Assign a ripple material template or reimport the package shader.", this);
                    _hasLoggedMissingRippleShader = true;
                }
                return;
            }

            _rippleSimulator = new CustomRenderTextureRippleSimulator(
                _runtimeResources,
                _topSurfaceSize,
                qualitySettings,
                materialTemplate);
            _appliedRippleMaterialTemplate = materialTemplate;
            _appliedRippleWaterSize = _topSurfaceSize;
            _appliedQualitySettings = qualitySettings;
        }

        private Material _appliedRippleMaterialTemplate;

        private void ApplyRendererBindings()
        {
            var styleSettings = _styleProfile != null ? _styleProfile.GetSettings() : WaterStyleSettings.Default;
            styleSettings.Sanitize();

            var topTemplate = _topMaterialTemplate != null ? _topMaterialTemplate : _styleProfile != null ? _styleProfile.TopMaterialTemplate : null;
            var frontTemplate = _frontMaterialTemplate != null ? _frontMaterialTemplate : _styleProfile != null ? _styleProfile.FrontMaterialTemplate : null;
            var topShader = Shader.Find("Water25D/Top Surface");
            var frontShader = Shader.Find("Water25D/Front Surface");

            var topMaterial = _runtimeResources.ConfigureTopSurfaceMaterial(topTemplate, _topMeshRenderer.sharedMaterial, topShader);
            var frontMaterial = _runtimeResources.ConfigureFrontSurfaceMaterial(frontTemplate, _frontMeshRenderer.sharedMaterial, frontShader);
            if (topMaterial == null && !_hasLoggedMissingSurfaceShader)
            {
                Debug.LogWarning("Water25D has no top material. Assign a template or reimport the package shader.", this);
                _hasLoggedMissingSurfaceShader = true;
            }

            _topMeshRenderer.sharedMaterial = topMaterial;
            _frontMeshRenderer.sharedMaterial = frontMaterial;
            _topSortingGroup.sortingLayerID = GetSortingLayerId(_topSortingLayerName);
            _topSortingGroup.sortingOrder = _topSortingOrder;
            _frontSortingGroup.sortingLayerID = GetSortingLayerId(_frontSortingLayerName);
            _frontSortingGroup.sortingOrder = _frontSortingOrder;

            if (_materialPropertyBlock == null)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }

            _materialPropertyBlock.Clear();
            styleSettings.Apply(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(WaterShaderIds.WaveBands, GetQualitySettings().AmbientWaveBands);
            _materialPropertyBlock.SetVector(WaterShaderIds.WaterSize, new Vector4(_topSurfaceSize.x, _topSurfaceSize.y, 0f, 0f));
            _materialPropertyBlock.SetFloat(WaterShaderIds.WaterMeshDepth, _topSurfaceSize.y);
            _materialPropertyBlock.SetFloat(WaterShaderIds.FrontDepth, _frontSurfaceDepth);
            _materialPropertyBlock.SetFloat(WaterShaderIds.Waterline, _waterlineLocalY);
            var rippleTexture = RippleTexture;
            if (rippleTexture != null)
            {
                _materialPropertyBlock.SetTexture(WaterShaderIds.RippleTexture, rippleTexture);
                _materialPropertyBlock.SetTexture(WaterShaderIds.RippleSimulationTexture, rippleTexture);
            }
            _materialPropertyBlock.SetFloat(WaterShaderIds.RippleEnabled, rippleTexture != null ? 1f : 0f);
            _materialPropertyBlock.SetMatrix(WaterShaderIds.ReflectionViewProjection, Matrix4x4.identity);
            _materialPropertyBlock.SetFloat(WaterShaderIds.ReflectionEnabled, 0f);
            _materialPropertyBlock.SetFloat(WaterShaderIds.ReflectionFallback, _reflectionMode == WaterReflectionMode.Stylized ? 1f : 0f);
            _materialPropertyBlock.SetFloat(WaterShaderIds.ReflectionStrength, _reflectionStrength);
            _topMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            _frontMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        private void RegisterReflectionSurface()
        {
            DisposeReflectionRegistration();
            if (!Application.isPlaying || _reflectionMode == WaterReflectionMode.Disabled || _topMeshRenderer == null || _reflectionAnchor == null)
            {
                return;
            }

            _reflectionRegistration = WaterReflectionManager.Register(
                _topMeshRenderer,
                _reflectionAnchor,
                _reflectionCameraSource,
                _reflectionMode,
                _reflectionCullingMask,
                _reflectionResolutionScale,
                _reflectionUpdateIntervalFrames,
                _reflectionStrength);
        }

        private void ConfigureEffects()
        {
            _fxController?.Configure(this, _enableEffects, _splashDefinition, _bubbleDefinition, _maximumFxPoolSize);
        }

        private void EnsureHierarchy()
        {
            _topSurface = EnsureChild(_topSurface, TopSurfaceName);
            _frontSurface = EnsureChild(_frontSurface, FrontSurfaceName);
            _surfaceCrossingTrigger = EnsureChild(_surfaceCrossingTrigger, SurfaceCrossingTriggerName);
            _buoyancyVolume = EnsureChild(_buoyancyVolume, BuoyancyVolumeName);
            _reflectionAnchor = EnsureChild(_reflectionAnchor, ReflectionAnchorName);
            _fxRoot = EnsureChild(_fxRoot, FxRootName);

            _topMeshFilter = GetOrAddComponent<MeshFilter>(_topSurface.gameObject);
            _topMeshRenderer = GetOrAddComponent<MeshRenderer>(_topSurface.gameObject);
            _topSortingGroup = GetOrAddComponent<SortingGroup>(_topSurface.gameObject);
            _frontMeshFilter = GetOrAddComponent<MeshFilter>(_frontSurface.gameObject);
            _frontMeshRenderer = GetOrAddComponent<MeshRenderer>(_frontSurface.gameObject);
            _frontSortingGroup = GetOrAddComponent<SortingGroup>(_frontSurface.gameObject);

            _surfaceCollider = GetOrAddComponent<BoxCollider2D>(_surfaceCrossingTrigger.gameObject);
            _surfaceInteraction = GetOrAddComponent<WaterSurfaceInteraction2D>(_surfaceCrossingTrigger.gameObject);
            _buoyancyCollider = GetOrAddComponent<BoxCollider2D>(_buoyancyVolume.gameObject);
            _physicsVolume = GetOrAddComponent<WaterPhysicsVolume2D>(_buoyancyVolume.gameObject);
            _fxController = GetOrAddComponent<WaterFXController>(_fxRoot.gameObject);
            _buoyancyVolume.gameObject.TryGetComponent(out _buoyancyEffector);

            if (_synchronizeGeneratedChildLayers)
            {
                var layer = gameObject.layer;
                _topSurface.gameObject.layer = layer;
                _frontSurface.gameObject.layer = layer;
                _surfaceCrossingTrigger.gameObject.layer = layer;
                _buoyancyVolume.gameObject.layer = layer;
                _reflectionAnchor.gameObject.layer = layer;
                _fxRoot.gameObject.layer = layer;
            }
        }

        private Transform EnsureChild(Transform serializedChild, string childName)
        {
            if (serializedChild != null && serializedChild.parent == transform)
            {
                return serializedChild;
            }

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }

            var childObject = new GameObject(childName);
            childObject.transform.SetParent(transform, false);
            return childObject.transform;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        private static int GetSortingLayerId(string sortingLayerName)
        {
            var requestedName = string.IsNullOrEmpty(sortingLayerName) ? "Default" : sortingLayerName;
            var requestedId = SortingLayer.NameToID(requestedName);
            return requestedId >= 0 ? requestedId : SortingLayer.NameToID("Default");
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
        }

        private void DisposeRippleSimulator()
        {
            if (_rippleSimulator == null)
            {
                return;
            }

            _rippleSimulator.Dispose();
            _rippleSimulator = null;
        }

        private void DisposeRuntimeResources()
        {
            DisposeReflectionRegistration();
            _fxController?.DisposeRuntimeResources();
            DisposeRippleSimulator();
            if (_runtimeResources == null)
            {
                return;
            }

            if (_topMeshFilter != null)
            {
                _topMeshFilter.sharedMesh = null;
                if (_topMeshRenderer != null && _runtimeResources.OwnsTopSurfaceMaterial && _topMeshRenderer.sharedMaterial == _runtimeResources.TopSurfaceMaterial)
                {
                    _topMeshRenderer.sharedMaterial = null;
                }
            }

            if (_frontMeshFilter != null)
            {
                _frontMeshFilter.sharedMesh = null;
                if (_frontMeshRenderer != null && _runtimeResources.OwnsFrontSurfaceMaterial && _frontMeshRenderer.sharedMaterial == _runtimeResources.FrontSurfaceMaterial)
                {
                    _frontMeshRenderer.sharedMaterial = null;
                }
            }

            _runtimeResources.Dispose();
            _runtimeResources = null;
            _geometryApplied = false;
        }

        private void DisposeReflectionRegistration()
        {
            if (_reflectionRegistration == null)
            {
                return;
            }

            _reflectionRegistration.Dispose();
            _reflectionRegistration = null;
        }
    }
}
