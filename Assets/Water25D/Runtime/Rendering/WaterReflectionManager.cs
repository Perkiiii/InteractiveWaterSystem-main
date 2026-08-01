using System.Collections.Generic;
using UnityEngine;

namespace Water25D.Rendering
{
    /// <summary>
    /// Owns adaptive planar reflection resources shared by compatible water surfaces.
    /// A stylized registration never creates a camera or render texture.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WaterReflectionManager : MonoBehaviour
    {
        public sealed class ReflectionRegistration : System.IDisposable
        {
            internal WaterReflectionManager Owner;
            internal Renderer SurfaceRenderer;
            internal Transform Plane;
            internal Camera SourceCamera;
            internal WaterReflectionMode Mode;
            internal LayerMask CullingMask;
            internal float ResolutionScale;
            internal int UpdateIntervalFrames;
            internal float Strength;
            internal readonly MaterialPropertyBlock PropertyBlock = new MaterialPropertyBlock();

            internal ReflectionRegistration(
                WaterReflectionManager owner,
                Renderer surfaceRenderer,
                Transform plane,
                Camera sourceCamera,
                WaterReflectionMode mode,
                LayerMask cullingMask,
                float resolutionScale,
                int updateIntervalFrames,
                float strength)
            {
                Owner = owner;
                SurfaceRenderer = surfaceRenderer;
                Plane = plane;
                SourceCamera = sourceCamera;
                Mode = mode;
                CullingMask = cullingMask;
                ResolutionScale = Mathf.Clamp(resolutionScale, 0.1f, 1f);
                UpdateIntervalFrames = Mathf.Clamp(updateIntervalFrames, 1, 120);
                Strength = Mathf.Clamp01(strength);
            }

            public void Dispose()
            {
                if (Owner == null)
                {
                    return;
                }

                Owner.Unregister(this);
                Owner = null;
            }

            internal void Apply(Texture texture, Matrix4x4 viewProjection, bool enabled, bool fallback)
            {
                if (SurfaceRenderer == null)
                {
                    return;
                }

                SurfaceRenderer.GetPropertyBlock(PropertyBlock);
                if (texture != null)
                {
                    PropertyBlock.SetTexture(WaterShaderIds.ReflectionTexture, texture);
                }
                PropertyBlock.SetMatrix(WaterShaderIds.ReflectionViewProjection, viewProjection);
                PropertyBlock.SetFloat(WaterShaderIds.ReflectionEnabled, enabled ? 1f : 0f);
                PropertyBlock.SetFloat(WaterShaderIds.ReflectionFallback, fallback ? 1f : 0f);
                PropertyBlock.SetFloat(WaterShaderIds.ReflectionStrength, Strength);
                SurfaceRenderer.SetPropertyBlock(PropertyBlock);
            }

            internal WaterReflectionGroupKey GetKey()
            {
                var camera = SourceCamera != null ? SourceCamera : Camera.main;
                return WaterReflectionGroupKey.Create(
                    camera,
                    Plane,
                    CullingMask,
                    Mode,
                    ResolutionScale,
                    UpdateIntervalFrames);
            }
        }

        private sealed class ReflectionGroup
        {
            private readonly List<ReflectionRegistration> _registrations = new List<ReflectionRegistration>(4);
            private readonly WaterReflectionGroupKey _key;
            private Camera _reflectionCamera;
            private RenderTexture _reflectionTexture;
            private Vector3 _lastCameraPosition;
            private Quaternion _lastCameraRotation;
            private int _lastRenderFrame = -1;
            private Matrix4x4 _viewProjection;
            private bool _hasRendered;

            public ReflectionGroup(WaterReflectionGroupKey key)
            {
                _key = key;
            }

            public int Count => _registrations.Count;

            public bool Matches(WaterReflectionGroupKey key)
            {
                return _key.Equals(key);
            }

            public void Add(ReflectionRegistration registration)
            {
                _registrations.Add(registration);
            }

            public void Clear()
            {
                _registrations.Clear();
            }

            public void Update(int frame)
            {
                if (_registrations.Count == 0)
                {
                    return;
                }

                var first = _registrations[0];
                if (first.Mode == WaterReflectionMode.Disabled)
                {
                    ApplyToMembers(null, Matrix4x4.identity, false, false);
                    return;
                }

                var sourceCamera = first.SourceCamera != null ? first.SourceCamera : Camera.main;
                if (first.Mode == WaterReflectionMode.Stylized || sourceCamera == null)
                {
                    ApplyToMembers(null, Matrix4x4.identity, false, true);
                    return;
                }

                var visible = false;
                var excludedLayers = 0;
                for (var i = 0; i < _registrations.Count; i++)
                {
                    var registration = _registrations[i];
                    if (registration.SurfaceRenderer != null && registration.SurfaceRenderer.isVisible)
                    {
                        visible = true;
                    }

                    if (registration.SurfaceRenderer != null)
                    {
                        excludedLayers |= 1 << registration.SurfaceRenderer.gameObject.layer;
                    }
                }

                if (!visible)
                {
                    ApplyToMembers(_reflectionTexture, _viewProjection, _hasRendered, false);
                    return;
                }

                var cameraMoved = !_hasRendered ||
                                  (sourceCamera.transform.position - _lastCameraPosition).sqrMagnitude > 0.0001f ||
                                  Quaternion.Angle(sourceCamera.transform.rotation, _lastCameraRotation) > 0.1f;
                var intervalElapsed = !_hasRendered || frame - _lastRenderFrame >= _key.UpdateIntervalFrames;
                if (cameraMoved || intervalElapsed)
                {
                    Render(sourceCamera, excludedLayers, frame);
                }

                ApplyToMembers(_reflectionTexture, _viewProjection, _hasRendered, false);
            }

            public void Dispose()
            {
                if (_reflectionCamera != null)
                {
                    DestroyOwnedObject(_reflectionCamera.gameObject);
                    _reflectionCamera = null;
                }

                DestroyOwnedObject(_reflectionTexture);
                _reflectionTexture = null;
                _registrations.Clear();
                _hasRendered = false;
            }

            private void Render(Camera sourceCamera, int excludedLayers, int frame)
            {
                EnsureResources(sourceCamera, excludedLayers);
                if (_reflectionCamera == null || _reflectionTexture == null || _registrations.Count == 0)
                {
                    return;
                }

                var plane = _registrations[0].Plane;
                if (plane == null)
                {
                    return;
                }

                var normal = plane.up.normalized;
                var planePoint = plane.position;
                var sourcePosition = sourceCamera.transform.position;
                var reflectedPosition = ReflectPoint(sourcePosition, planePoint, normal);
                var reflectedForward = Vector3.Reflect(sourceCamera.transform.forward, normal);
                var reflectedUp = Vector3.Reflect(sourceCamera.transform.up, normal);
                _reflectionCamera.transform.SetPositionAndRotation(
                    reflectedPosition,
                    Quaternion.LookRotation(reflectedForward, reflectedUp));
                _reflectionCamera.enabled = false;

                var previousInvertCulling = GL.invertCulling;
                GL.invertCulling = !previousInvertCulling;
                try
                {
                    _reflectionCamera.Render();
                }
                finally
                {
                    GL.invertCulling = previousInvertCulling;
                }

                _viewProjection = GL.GetGPUProjectionMatrix(_reflectionCamera.projectionMatrix, true) * _reflectionCamera.worldToCameraMatrix;
                _lastCameraPosition = sourceCamera.transform.position;
                _lastCameraRotation = sourceCamera.transform.rotation;
                _lastRenderFrame = frame;
                _hasRendered = true;
            }

            private void EnsureResources(Camera sourceCamera, int excludedLayers)
            {
                var scale = _key.ResolutionScale / 100f;
                var width = Mathf.Max(16, Mathf.RoundToInt(Mathf.Max(16, sourceCamera.pixelWidth) * scale));
                var height = Mathf.Max(16, Mathf.RoundToInt(Mathf.Max(16, sourceCamera.pixelHeight) * scale));
                if (_reflectionTexture == null || _reflectionTexture.width != width || _reflectionTexture.height != height)
                {
                    DestroyOwnedObject(_reflectionTexture);
                    _reflectionTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
                    {
                        name = "Water25D Shared Reflection (Runtime)",
                        hideFlags = HideFlags.HideAndDontSave,
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp,
                        useMipMap = false,
                        autoGenerateMips = false
                    };
                    _reflectionTexture.Create();
                    _hasRendered = false;
                }

                if (_reflectionCamera == null)
                {
                    var cameraObject = new GameObject("Water25D Reflection Camera")
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    _reflectionCamera = cameraObject.AddComponent<Camera>();
                    _reflectionCamera.enabled = false;
                    _reflectionCamera.useOcclusionCulling = false;
                    _reflectionCamera.allowHDR = false;
                }

                _reflectionCamera.CopyFrom(sourceCamera);
                _reflectionCamera.enabled = false;
                _reflectionCamera.useOcclusionCulling = false;
                _reflectionCamera.targetTexture = _reflectionTexture;
                _reflectionCamera.cullingMask = _key.CullingMask & ~excludedLayers;
                _reflectionCamera.name = "Water25D Reflection Camera";
            }

            private void ApplyToMembers(Texture texture, Matrix4x4 viewProjection, bool enabled, bool fallback)
            {
                for (var i = 0; i < _registrations.Count; i++)
                {
                    _registrations[i].Apply(texture, viewProjection, enabled, fallback);
                }
            }

            private static Vector3 ReflectPoint(Vector3 point, Vector3 planePoint, Vector3 normal)
            {
                return point - 2f * Vector3.Dot(point - planePoint, normal) * normal;
            }

            private static void DestroyOwnedObject(Object objectToDestroy)
            {
                if (objectToDestroy == null)
                {
                    return;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(objectToDestroy);
                }
                else
                {
                    Object.DestroyImmediate(objectToDestroy);
                }
            }
        }

        private static WaterReflectionManager _instance;
        private readonly List<ReflectionRegistration> _registrations = new List<ReflectionRegistration>(8);
        private readonly List<ReflectionGroup> _groups = new List<ReflectionGroup>(4);

        public static bool HasInstance => _instance != null;
        public static int RegisteredSurfaceCount => _instance != null ? _instance._registrations.Count : 0;
        public static int ActiveGroupCount => _instance != null ? _instance._groups.Count : 0;

        public static ReflectionRegistration Register(
            Renderer surfaceRenderer,
            Transform plane,
            Camera sourceCamera,
            WaterReflectionMode mode,
            LayerMask cullingMask,
            float resolutionScale,
            int updateIntervalFrames,
            float strength)
        {
            if (surfaceRenderer == null || plane == null || mode == WaterReflectionMode.Disabled)
            {
                return null;
            }

            var manager = GetOrCreate();
            var registration = new ReflectionRegistration(
                manager,
                surfaceRenderer,
                plane,
                sourceCamera,
                mode,
                cullingMask,
                resolutionScale,
                updateIntervalFrames,
                strength);
            manager._registrations.Add(registration);
            return registration;
        }

        private static WaterReflectionManager GetOrCreate()
        {
            if (_instance != null)
            {
                return _instance;
            }

            var managerObject = new GameObject("Water25D Reflection Manager")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _instance = managerObject.AddComponent<WaterReflectionManager>();
            return _instance;
        }

        private void LateUpdate()
        {
            RebuildGroups();
            for (var i = 0; i < _groups.Count; i++)
            {
                _groups[i].Update(Time.frameCount);
            }
        }

        private void OnDestroy()
        {
            for (var i = 0; i < _groups.Count; i++)
            {
                _groups[i].Dispose();
            }

            _groups.Clear();
            _registrations.Clear();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Unregister(ReflectionRegistration registration)
        {
            var index = _registrations.IndexOf(registration);
            if (index >= 0)
            {
                var lastIndex = _registrations.Count - 1;
                _registrations[index] = _registrations[lastIndex];
                _registrations.RemoveAt(lastIndex);
            }

            if (_registrations.Count == 0)
            {
                DestroyOwnedObject(gameObject);
            }
        }

        private void RebuildGroups()
        {
            for (var i = 0; i < _groups.Count; i++)
            {
                _groups[i].Clear();
            }

            for (var i = 0; i < _registrations.Count; i++)
            {
                var registration = _registrations[i];
                if (registration.SurfaceRenderer == null || registration.Plane == null)
                {
                    continue;
                }

                var key = registration.GetKey();
                var group = FindGroup(key);
                if (group == null)
                {
                    group = new ReflectionGroup(key);
                    _groups.Add(group);
                }

                group.Add(registration);
            }

            for (var i = _groups.Count - 1; i >= 0; i--)
            {
                if (_groups[i].Count != 0)
                {
                    continue;
                }

                _groups[i].Dispose();
                _groups.RemoveAt(i);
            }
        }

        private ReflectionGroup FindGroup(WaterReflectionGroupKey key)
        {
            for (var i = 0; i < _groups.Count; i++)
            {
                if (_groups[i].Matches(key))
                {
                    return _groups[i];
                }
            }

            return null;
        }

        private static void DestroyOwnedObject(Object objectToDestroy)
        {
            if (objectToDestroy == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(objectToDestroy);
            }
            else
            {
                Object.DestroyImmediate(objectToDestroy);
            }
        }
    }
}
