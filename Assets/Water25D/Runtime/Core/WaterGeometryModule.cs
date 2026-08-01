using UnityEngine;

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
        private float _appliedWaterlineLocalY;
        private float _appliedTopVerticesPerUnit;

        public void ApplyIfNeeded(
            Vector2 topSurfaceSize,
            float frontSurfaceDepth,
            float waterlineLocalY,
            WaterQualitySettings qualitySettings,
            WaterRuntimeResources resources,
            MeshFilter topMeshFilter,
            MeshFilter frontMeshFilter)
        {
            var geometryChanged = !_applied ||
                                  _appliedTopSurfaceSize != topSurfaceSize ||
                                  !Mathf.Approximately(_appliedFrontSurfaceDepth, frontSurfaceDepth) ||
                                  !Mathf.Approximately(_appliedWaterlineLocalY, waterlineLocalY) ||
                                  !Mathf.Approximately(_appliedTopVerticesPerUnit, qualitySettings.TopVerticesPerUnit);
            if (!geometryChanged)
            {
                return;
            }

            topMeshFilter.sharedMesh = null;
            frontMeshFilter.sharedMesh = null;

            var vertexCount = WaterMeshBuilder.CalculateTopVertexCount(topSurfaceSize, qualitySettings.TopVerticesPerUnit);
            resources.ReplaceTopMesh(WaterMeshBuilder.BuildTopMesh(topSurfaceSize, vertexCount, "Water25D Top Mesh"));
            resources.ReplaceFrontMesh(WaterMeshBuilder.BuildFrontMesh(topSurfaceSize, frontSurfaceDepth, vertexCount.x, "Water25D Front Mesh"));
            topMeshFilter.sharedMesh = resources.TopMesh;
            frontMeshFilter.sharedMesh = resources.FrontMesh;

            _appliedTopSurfaceSize = topSurfaceSize;
            _appliedFrontSurfaceDepth = frontSurfaceDepth;
            _appliedWaterlineLocalY = waterlineLocalY;
            _appliedTopVerticesPerUnit = qualitySettings.TopVerticesPerUnit;
            _applied = true;
        }

        public void Reset()
        {
            _applied = false;
        }
    }
}
