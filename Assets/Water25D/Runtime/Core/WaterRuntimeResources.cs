using System;
using UnityEngine;

namespace Water25D
{
    /// <summary>
    /// Owns all generated Unity objects for one water instance.
    /// Project assets can be used as templates, but never become mutable simulation state.
    /// </summary>
    public sealed class WaterRuntimeResources : IDisposable
    {
        private Mesh _topMesh;
        private Mesh _frontMesh;
        private Material _topSurfaceMaterial;
        private Material _frontSurfaceMaterial;
        private bool _ownsTopSurfaceMaterial;
        private bool _ownsFrontSurfaceMaterial;
        private Material _rippleMaterial;
        private CustomRenderTexture _rippleTexture;
        private bool _disposed;

        public Mesh TopMesh => _topMesh;
        public Mesh FrontMesh => _frontMesh;
        public Material TopSurfaceMaterial => _topSurfaceMaterial;
        public Material FrontSurfaceMaterial => _frontSurfaceMaterial;
        public Material RippleMaterial => _rippleMaterial;
        public CustomRenderTexture RippleTexture => _rippleTexture;
        public bool OwnsTopSurfaceMaterial => _ownsTopSurfaceMaterial;
        public bool OwnsFrontSurfaceMaterial => _ownsFrontSurfaceMaterial;

        public void ReplaceTopMesh(Mesh mesh)
        {
            ThrowIfDisposed();
            DestroyOwnedObject(_topMesh);
            if (mesh != null)
            {
                mesh.hideFlags = HideFlags.HideAndDontSave;
            }
            _topMesh = mesh;
        }

        public void ReplaceFrontMesh(Mesh mesh)
        {
            ThrowIfDisposed();
            DestroyOwnedObject(_frontMesh);
            if (mesh != null)
            {
                mesh.hideFlags = HideFlags.HideAndDontSave;
            }
            _frontMesh = mesh;
        }

        public Material ConfigureTopSurfaceMaterial(Material preferredTemplate, Material existingRendererMaterial, Shader fallbackShader)
        {
            ThrowIfDisposed();
            return ConfigureSurfaceMaterial(
                preferredTemplate,
                existingRendererMaterial,
                fallbackShader,
                "Water25D Top Material",
                ref _topSurfaceMaterial,
                ref _ownsTopSurfaceMaterial);
        }

        public Material ConfigureFrontSurfaceMaterial(Material preferredTemplate, Material existingRendererMaterial, Shader fallbackShader)
        {
            ThrowIfDisposed();
            return ConfigureSurfaceMaterial(
                preferredTemplate,
                existingRendererMaterial,
                fallbackShader,
                "Water25D Front Material",
                ref _frontSurfaceMaterial,
                ref _ownsFrontSurfaceMaterial);
        }

        public bool TryCreateRippleResources(
            int width,
            int height,
            Material materialTemplate,
            Shader fallbackShader,
            out CustomRenderTexture texture,
            out Material material)
        {
            ThrowIfDisposed();
            ReleaseRippleResources();
            texture = null;
            material = null;

            if (width < 2 || height < 2)
            {
                return false;
            }

            var shader = materialTemplate != null ? materialTemplate.shader : fallbackShader;
            if (shader == null)
            {
                return false;
            }

            try
            {
                material = materialTemplate != null ? new Material(materialTemplate) : new Material(shader);
                material.name = "Water25D Ripple Simulation (Runtime)";
                material.hideFlags = HideFlags.HideAndDontSave;

                var format = RenderTextureFormat.RGHalf;
                if (!SystemInfo.SupportsRenderTextureFormat(format))
                {
                    format = RenderTextureFormat.RGFloat;
                }

                if (!SystemInfo.SupportsRenderTextureFormat(format))
                {
                    DestroyOwnedObject(material);
                    material = null;
                    return false;
                }

                texture = new CustomRenderTexture(width, height, format, RenderTextureReadWrite.Linear)
                {
                    name = "Water25D Ripple Simulation (Runtime)",
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    useMipMap = false,
                    autoGenerateMips = false,
                    doubleBuffered = true,
                    updateMode = CustomRenderTextureUpdateMode.OnDemand,
                    initializationMode = CustomRenderTextureUpdateMode.OnDemand,
                    initializationSource = CustomRenderTextureInitializationSource.TextureAndColor,
                    initializationColor = Color.clear,
                    material = material,
                    shaderPass = 0
                };

                if (!texture.Create())
                {
                    DestroyOwnedObject(texture);
                    DestroyOwnedObject(material);
                    texture = null;
                    material = null;
                    return false;
                }

                texture.Initialize();
                _rippleTexture = texture;
                _rippleMaterial = material;
                return true;
            }
            catch (Exception)
            {
                DestroyOwnedObject(texture);
                DestroyOwnedObject(material);
                texture = null;
                material = null;
                return false;
            }
        }

        public void ReleaseRippleResources()
        {
            if (_rippleTexture != null)
            {
                _rippleTexture.Release();
            }

            DestroyOwnedObject(_rippleTexture);
            DestroyOwnedObject(_rippleMaterial);
            _rippleTexture = null;
            _rippleMaterial = null;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            ReleaseRippleResources();
            DestroyOwnedObject(_topMesh);
            DestroyOwnedObject(_frontMesh);
            if (_ownsTopSurfaceMaterial)
            {
                DestroyOwnedObject(_topSurfaceMaterial);
            }

            if (_ownsFrontSurfaceMaterial)
            {
                DestroyOwnedObject(_frontSurfaceMaterial);
            }

            _topMesh = null;
            _frontMesh = null;
            _topSurfaceMaterial = null;
            _frontSurfaceMaterial = null;
            _ownsTopSurfaceMaterial = false;
            _ownsFrontSurfaceMaterial = false;
        }

        private static Material ConfigureSurfaceMaterial(
            Material preferredTemplate,
            Material existingRendererMaterial,
            Shader fallbackShader,
            string runtimeName,
            ref Material current,
            ref bool ownsCurrent)
        {
            var desired = preferredTemplate != null ? preferredTemplate : existingRendererMaterial;
            if (desired != null)
            {
                if (current == desired)
                {
                    return current;
                }

                if (ownsCurrent && current != desired)
                {
                    DestroyOwnedObject(current);
                }

                current = desired;
                ownsCurrent = false;
                return current;
            }

            if (current != null)
            {
                return current;
            }

            if (fallbackShader == null)
            {
                return null;
            }

            current = new Material(fallbackShader)
            {
                name = runtimeName,
                hideFlags = HideFlags.HideAndDontSave
            };
            ownsCurrent = true;
            return current;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WaterRuntimeResources));
            }
        }

        private static void DestroyOwnedObject(UnityEngine.Object objectToDestroy)
        {
            if (objectToDestroy == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(objectToDestroy);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(objectToDestroy);
            }
        }
    }
}
