using UnityEngine;
using Water25D.Rendering;

namespace Water25D
{
    /// <summary>
    /// Rebuilds generated presentation meshes only when geometry-affecting inputs change.
    /// The resource owner gives each generated mesh an explicit transient lifetime.
    /// </summary>
    internal sealed class WaterGeometryModule
    {
        private bool _applied;
        private Vector2 _appliedTopSurfaceSize;
        private float _appliedFrontSurfaceDepth;
        private float _appliedTopVerticesPerUnit;
        private WaterSurfaceMode _appliedSurfaceMode;

        public void ApplyIfNeeded(
            Vector2 topSurfaceSize,
            float frontSurfaceDepth,
            WaterQualitySettings qualitySettings,
            WaterSurfaceMode surfaceMode,
            WaterRuntimeResources resources,
            MeshFilter topMeshFilter,
            MeshFilter frontMeshFilter)
        {
            var modeChanged = !_applied || _appliedSurfaceMode != surfaceMode;
            var topGeometryChanged = modeChanged ||
                                     _appliedTopSurfaceSize != topSurfaceSize ||
                                     (surfaceMode == WaterSurfaceMode.SimulatedRipples &&
                                      !Mathf.Approximately(_appliedTopVerticesPerUnit, qualitySettings.TopVerticesPerUnit));
            var frontGeometryChanged = modeChanged ||
                                       !_applied ||
                                       !Mathf.Approximately(_appliedTopSurfaceSize.x, topSurfaceSize.x) ||
                                       !Mathf.Approximately(_appliedFrontSurfaceDepth, frontSurfaceDepth) ||
                                       (surfaceMode == WaterSurfaceMode.SimulatedRipples &&
                                        !Mathf.Approximately(_appliedTopVerticesPerUnit, qualitySettings.TopVerticesPerUnit));
            if (!topGeometryChanged && !frontGeometryChanged)
            {
                return;
            }

            var vertexCount = WaterMeshBuilder.CalculateTopVertexCount(topSurfaceSize, qualitySettings.TopVerticesPerUnit);
            if (topGeometryChanged)
            {
                topMeshFilter.sharedMesh = null;
                resources.ReplaceTopMesh(surfaceMode == WaterSurfaceMode.FlatStylized
                    ? WaterMeshBuilder.BuildFlatTopMesh(topSurfaceSize, "Water25D Flat Top Mesh")
                    : WaterMeshBuilder.BuildTopMesh(topSurfaceSize, vertexCount, "Water25D Top Mesh"));
                topMeshFilter.sharedMesh = resources.TopMesh;
            }

            if (frontGeometryChanged)
            {
                frontMeshFilter.sharedMesh = null;
                var frontVertexCount = surfaceMode == WaterSurfaceMode.FlatStylized ? 2 : vertexCount.x;
                resources.ReplaceFrontMesh(WaterMeshBuilder.BuildFrontMesh(
                    topSurfaceSize,
                    frontSurfaceDepth,
                    frontVertexCount,
                    surfaceMode == WaterSurfaceMode.FlatStylized ? "Water25D Flat Front Mesh" : "Water25D Front Mesh"));
                frontMeshFilter.sharedMesh = resources.FrontMesh;
            }

            _appliedTopSurfaceSize = topSurfaceSize;
            _appliedFrontSurfaceDepth = frontSurfaceDepth;
            _appliedTopVerticesPerUnit = qualitySettings.TopVerticesPerUnit;
            _appliedSurfaceMode = surfaceMode;
            _applied = true;
        }

        public void Reset()
        {
            _applied = false;
        }
    }
}
