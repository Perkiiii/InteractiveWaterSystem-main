using UnityEngine;
using UnityEngine.Rendering;
using Water25D.Rendering;

namespace Water25D
{
    /// <summary>
    /// Owns renderer material binding and per-instance property-block state. Project materials
    /// remain immutable; mutable values are written through the block or runtime-owned resources.
    /// </summary>
    internal sealed class WaterRenderingModule
    {
        private readonly MaterialPropertyBlock _propertyBlock = new MaterialPropertyBlock();

        public void Apply(
            WaterRuntimeResources resources,
            MeshRenderer topRenderer,
            SortingGroup topSortingGroup,
            MeshRenderer frontRenderer,
            SortingGroup frontSortingGroup,
            Material topTemplate,
            Material frontTemplate,
            WaterStyleProfile styleProfile,
            Vector2 topSurfaceSize,
            float frontSurfaceDepth,
            float waterlineLocalY,
            WaterQualitySettings qualitySettings,
            WaterSurfaceMode surfaceMode,
            Texture rippleTexture,
            string topSortingLayerName,
            int topSortingOrder,
            string frontSortingLayerName,
            int frontSortingOrder,
            WaterReflectionMode reflectionMode,
            float reflectionStrength,
            WaterSurfaceRenderData surfacePresentationData,
            out Material topMaterial,
            out Material frontMaterial)
        {
            var styleSettings = styleProfile != null ? styleProfile.GetSettings() : WaterStyleSettings.Default;
            styleSettings.Sanitize();

            var resolvedTopTemplate = topTemplate != null ? topTemplate : styleProfile != null ? styleProfile.TopMaterialTemplate : null;
            var resolvedFrontTemplate = frontTemplate != null ? frontTemplate : styleProfile != null ? styleProfile.FrontMaterialTemplate : null;
            topMaterial = resources.ConfigureTopSurfaceMaterial(
                resolvedTopTemplate,
                topRenderer.sharedMaterial,
                Shader.Find("Water25D/Top Surface"));
            frontMaterial = resources.ConfigureFrontSurfaceMaterial(
                resolvedFrontTemplate,
                frontRenderer.sharedMaterial,
                Shader.Find("Water25D/Front Surface"));

            topRenderer.sharedMaterial = topMaterial;
            frontRenderer.sharedMaterial = frontMaterial;
            topSortingGroup.sortingLayerID = GetSortingLayerId(topSortingLayerName);
            topSortingGroup.sortingOrder = topSortingOrder;
            frontSortingGroup.sortingLayerID = GetSortingLayerId(frontSortingLayerName);
            frontSortingGroup.sortingOrder = frontSortingOrder;

            _propertyBlock.Clear();
            styleSettings.Apply(_propertyBlock);
            _propertyBlock.SetFloat(WaterShaderIds.WaveBands, qualitySettings.AmbientWaveBands);
            _propertyBlock.SetVector(WaterShaderIds.WaterSize, new Vector4(topSurfaceSize.x, topSurfaceSize.y, 0f, 0f));
            _propertyBlock.SetFloat(WaterShaderIds.WaterMeshDepth, topSurfaceSize.y);
            _propertyBlock.SetFloat(WaterShaderIds.FrontDepth, frontSurfaceDepth);
            _propertyBlock.SetFloat(WaterShaderIds.Waterline, waterlineLocalY);
            _propertyBlock.SetFloat(WaterShaderIds.SurfaceMode, (float)surfaceMode);
            if (rippleTexture != null)
            {
                _propertyBlock.SetTexture(WaterShaderIds.RippleTexture, rippleTexture);
                _propertyBlock.SetTexture(WaterShaderIds.RippleSimulationTexture, rippleTexture);
            }

            _propertyBlock.SetFloat(WaterShaderIds.RippleEnabled, rippleTexture != null ? 1f : 0f);
            _propertyBlock.SetMatrix(WaterShaderIds.ReflectionViewProjection, Matrix4x4.identity);
            _propertyBlock.SetFloat(WaterShaderIds.ReflectionEnabled, 0f);
            _propertyBlock.SetFloat(WaterShaderIds.ReflectionFallback, reflectionMode == WaterReflectionMode.Stylized ? 1f : 0f);
            _propertyBlock.SetFloat(WaterShaderIds.ReflectionStrength, reflectionStrength);
            ApplySurfacePresentationToBlock(_propertyBlock, surfacePresentationData);
            topRenderer.SetPropertyBlock(_propertyBlock);
            frontRenderer.SetPropertyBlock(_propertyBlock);
        }

        /// <summary>
        /// Applies only transient surface-presentation values. Each renderer's existing
        /// property block is read first so reflection and unrelated instance state survive
        /// ring animation without invoking the full authoring path.
        /// </summary>
        public void ApplySurfacePresentation(
            MeshRenderer topRenderer,
            MeshRenderer frontRenderer,
            WaterSurfaceRenderData renderData)
        {
            if (renderData == null)
            {
                return;
            }

            ApplySurfacePresentation(topRenderer, renderData);
            ApplySurfacePresentation(frontRenderer, renderData);
        }

        private void ApplySurfacePresentation(MeshRenderer renderer, WaterSurfaceRenderData renderData)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.GetPropertyBlock(_propertyBlock);
            ApplySurfacePresentationToBlock(_propertyBlock, renderData);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private static void ApplySurfacePresentationToBlock(MaterialPropertyBlock block, WaterSurfaceRenderData renderData)
        {
            if (block == null || renderData == null)
            {
                return;
            }

            block.SetFloat(WaterShaderIds.SurfaceRingCount, Mathf.Clamp(renderData.ActiveRingCount, 0, renderData.ShaderArrayLength));
            block.SetVectorArray(WaterShaderIds.SurfaceRingsA, renderData.RingsA);
            block.SetVectorArray(WaterShaderIds.SurfaceRingsB, renderData.RingsB);
        }

        private static int GetSortingLayerId(string sortingLayerName)
        {
            var requestedName = string.IsNullOrEmpty(sortingLayerName) ? "Default" : sortingLayerName;
            var requestedId = SortingLayer.NameToID(requestedName);
            return requestedId >= 0 ? requestedId : SortingLayer.NameToID("Default");
        }
    }
}
