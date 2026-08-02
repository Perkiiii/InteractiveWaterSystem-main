using UnityEngine;
using UnityEngine.Rendering;
using Water25D.Rendering;

namespace Water25D
{
    /// <summary>
    /// Owns the complete per-instance top/front renderer state. Every presentation, ripple and
    /// reflection update is folded into these cached inputs before the two final property-block
    /// writes, so no other Water25D class can erase an unrelated renderer value.
    /// </summary>
    internal sealed class WaterRenderingModule
    {
        private readonly MaterialPropertyBlock _topPropertyBlock = new MaterialPropertyBlock();
        private readonly MaterialPropertyBlock _frontPropertyBlock = new MaterialPropertyBlock();

        private MeshRenderer _topRenderer;
        private MeshRenderer _frontRenderer;
        private WaterStyleSettings _styleSettings = WaterStyleSettings.Default;
        private WaterQualitySettings _qualitySettings = WaterQualitySettings.Default;
        private Vector2 _topSurfaceSize;
        private float _frontSurfaceDepth;
        private float _waterlineLocalY;
        private WaterSurfaceMode _surfaceMode;
        private Texture _rippleTexture;
        private WaterSurfaceRenderData _surfacePresentationData;
        private bool _includeSurfacePresentation;
        private WaterReflectionRenderState _reflectionState = WaterReflectionRenderState.Disabled;
        private bool _isConfigured;

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
            WaterReflectionRenderState reflectionState,
            WaterSurfaceRenderData surfacePresentationData,
            out Material topMaterial,
            out Material frontMaterial)
        {
            _topRenderer = topRenderer;
            _frontRenderer = frontRenderer;
            _topSurfaceSize = topSurfaceSize;
            _frontSurfaceDepth = frontSurfaceDepth;
            _waterlineLocalY = waterlineLocalY;
            _qualitySettings = qualitySettings;
            _qualitySettings.Sanitize();
            _surfaceMode = surfaceMode;
            _rippleTexture = rippleTexture;
            _surfacePresentationData = surfacePresentationData;
            _includeSurfacePresentation = surfaceMode == WaterSurfaceMode.FlatStylized;
            _reflectionState = reflectionState;
            _styleSettings = styleProfile != null ? styleProfile.GetSettings() : WaterStyleSettings.Default;
            _styleSettings.Sanitize();

            var resolvedTopTemplate = topTemplate != null ? topTemplate : styleProfile != null ? styleProfile.TopMaterialTemplate : null;
            var resolvedFrontTemplate = frontTemplate != null ? frontTemplate : styleProfile != null ? styleProfile.FrontMaterialTemplate : null;
            topMaterial = resources != null && topRenderer != null
                ? resources.ConfigureTopSurfaceMaterial(
                    resolvedTopTemplate,
                    topRenderer.sharedMaterial,
                    Shader.Find("Water25D/Top Surface"))
                : null;
            frontMaterial = resources != null && frontRenderer != null
                ? resources.ConfigureFrontSurfaceMaterial(
                    resolvedFrontTemplate,
                    frontRenderer.sharedMaterial,
                    Shader.Find("Water25D/Front Surface"))
                : null;

            if (topRenderer != null && topMaterial != null)
            {
                topRenderer.sharedMaterial = topMaterial;
            }

            if (frontRenderer != null && frontMaterial != null)
            {
                frontRenderer.sharedMaterial = frontMaterial;
            }

            if (topSortingGroup != null)
            {
                topSortingGroup.sortingLayerID = GetSortingLayerId(topSortingLayerName);
                topSortingGroup.sortingOrder = topSortingOrder;
            }

            if (frontSortingGroup != null)
            {
                frontSortingGroup.sortingLayerID = GetSortingLayerId(frontSortingLayerName);
                frontSortingGroup.sortingOrder = frontSortingOrder;
            }

            _isConfigured = true;
            WriteCompleteState();
        }

        /// <summary>
        /// Publishes the latest fixed-capacity interaction arrays and performs the final complete
        /// top/front writes. The cached reflection and authoring inputs are written again as part
        /// of the same operation; no renderer block is read or modified incrementally.
        /// </summary>
        public void ApplySurfacePresentation(
            MeshRenderer topRenderer,
            MeshRenderer frontRenderer,
            WaterSurfaceRenderData renderData,
            bool includeContactFoam)
        {
            if (renderData == null)
            {
                return;
            }

            _topRenderer = topRenderer != null ? topRenderer : _topRenderer;
            _frontRenderer = frontRenderer != null ? frontRenderer : _frontRenderer;
            _surfacePresentationData = renderData;
            _includeSurfacePresentation = includeContactFoam && _surfaceMode == WaterSurfaceMode.FlatStylized;
            WriteCompleteState();
        }

        /// <summary>
        /// Publishes reflection output from WaterReflectionModule. Reflection cameras and
        /// textures remain manager-owned; this method only routes the immutable snapshot through
        /// the sole final renderer writer.
        /// </summary>
        public void ApplyReflectionState(WaterReflectionRenderState reflectionState)
        {
            _reflectionState = reflectionState;
            WriteCompleteState();
        }

        private void WriteCompleteState()
        {
            if (!_isConfigured)
            {
                return;
            }

            WriteBlock(_topPropertyBlock, true);
            WriteBlock(_frontPropertyBlock, false);
            if (_topRenderer != null)
            {
                _topRenderer.SetPropertyBlock(_topPropertyBlock);
            }

            if (_frontRenderer != null)
            {
                _frontRenderer.SetPropertyBlock(_frontPropertyBlock);
            }
        }

        private void WriteBlock(MaterialPropertyBlock block, bool isTopSurface)
        {
            block.Clear();
            _styleSettings.Apply(block);
            _styleSettings.ApplyPainterlyMaskSettings(block, _qualitySettings);
            block.SetFloat(WaterShaderIds.WaveBands, _qualitySettings.AmbientWaveBands);
            block.SetFloat(
                WaterShaderIds.SecondaryAmbientDetailEnabled,
                _qualitySettings.EnableSecondaryAmbientDetail ? 1f : 0f);
            block.SetFloat(
                WaterShaderIds.StylizedHighlightsEnabled,
                _qualitySettings.EnableStylizedHighlights ? 1f : 0f);
            block.SetFloat(
                WaterShaderIds.RefractionEnabled,
                _qualitySettings.EnableRefraction && _styleSettings.RefractionSourceAvailable ? 1f : 0f);
            block.SetFloat(
                WaterShaderIds.CausticsEnabled,
                _qualitySettings.EnableCaustics && _styleSettings.CausticTexture != null ? 1f : 0f);
            block.SetVector(WaterShaderIds.WaterSize, new Vector4(_topSurfaceSize.x, _topSurfaceSize.y, 0f, 0f));
            block.SetFloat(WaterShaderIds.WaterMeshDepth, _topSurfaceSize.y);
            block.SetFloat(WaterShaderIds.FrontDepth, _frontSurfaceDepth);
            block.SetFloat(WaterShaderIds.Waterline, _waterlineLocalY);
            block.SetFloat(WaterShaderIds.SurfaceMode, (float)_surfaceMode);

            if (_rippleTexture != null)
            {
                block.SetTexture(WaterShaderIds.RippleTexture, _rippleTexture);
                block.SetTexture(WaterShaderIds.RippleSimulationTexture, _rippleTexture);
            }

            block.SetFloat(WaterShaderIds.RippleEnabled, _rippleTexture != null ? 1f : 0f);
            if (_reflectionState.Texture != null)
            {
                block.SetTexture(WaterShaderIds.ReflectionTexture, _reflectionState.Texture);
            }

            block.SetMatrix(WaterShaderIds.ReflectionViewProjection, _reflectionState.ViewProjection);
            block.SetFloat(WaterShaderIds.ReflectionEnabled, _reflectionState.Enabled ? 1f : 0f);
            block.SetFloat(WaterShaderIds.ReflectionFallback, _reflectionState.StylizedFallback ? 1f : 0f);
            block.SetFloat(WaterShaderIds.ReflectionStrength, _reflectionState.Strength);
            ApplySurfacePresentationToBlock(block, _surfacePresentationData, _includeSurfacePresentation);

            // Keep this branch explicit: the two renderers currently share most instance values,
            // but the final writer must be able to diverge their property sets without restoring
            // the old read/modify/write dependency.
            if (!isTopSurface)
            {
                block.SetFloat(WaterShaderIds.RippleEnabled, _rippleTexture != null ? 1f : 0f);
            }
        }

        private static void ApplySurfacePresentationToBlock(
            MaterialPropertyBlock block,
            WaterSurfaceRenderData renderData,
            bool includeSurfacePresentation)
        {
            if (block == null || renderData == null)
            {
                return;
            }

            var ringCount = includeSurfacePresentation
                ? Mathf.Clamp(renderData.ActiveRingCount, 0, renderData.ShaderArrayLength)
                : 0;
            var foamCount = includeSurfacePresentation
                ? Mathf.Clamp(renderData.ActiveContactFoamCount, 0, renderData.FoamShaderArrayLength)
                : 0;
            var wakeCount = includeSurfacePresentation
                ? Mathf.Clamp(renderData.ActiveWakeCount, 0, renderData.WakeShaderArrayLength)
                : 0;
            block.SetFloat(WaterShaderIds.SurfaceRingCount, ringCount);
            block.SetVectorArray(WaterShaderIds.SurfaceRingsA, renderData.RingsA);
            block.SetVectorArray(WaterShaderIds.SurfaceRingsB, renderData.RingsB);
            block.SetVectorArray(WaterShaderIds.SurfaceRingsC, renderData.RingsC);
            block.SetFloat(WaterShaderIds.SurfaceFoamCount, foamCount);
            block.SetVectorArray(WaterShaderIds.SurfaceFoamsA, renderData.FoamsA);
            block.SetVectorArray(WaterShaderIds.SurfaceFoamsB, renderData.FoamsB);
            block.SetVectorArray(WaterShaderIds.SurfaceFoamsC, renderData.FoamsC);
            block.SetFloat(WaterShaderIds.SurfaceWakeCount, wakeCount);
            block.SetVectorArray(WaterShaderIds.SurfaceWakesA, renderData.WakesA);
            block.SetVectorArray(WaterShaderIds.SurfaceWakesB, renderData.WakesB);
            block.SetVectorArray(WaterShaderIds.SurfaceWakesC, renderData.WakesC);
        }

        private static int GetSortingLayerId(string sortingLayerName)
        {
            var requestedName = string.IsNullOrEmpty(sortingLayerName) ? "Default" : sortingLayerName;
            var requestedId = SortingLayer.NameToID(requestedName);
            return requestedId >= 0 ? requestedId : SortingLayer.NameToID("Default");
        }
    }
}
