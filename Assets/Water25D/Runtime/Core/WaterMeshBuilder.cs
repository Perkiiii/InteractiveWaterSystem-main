using UnityEngine;
using UnityEngine.Rendering;

namespace Water25D
{
    /// <summary>
    /// Builds the two presentation meshes used by a 2.5D water body.
    /// The top surface uses XZ coordinates and the front surface uses XY coordinates.
    /// </summary>
    public static class WaterMeshBuilder
    {
        public static Vector2Int CalculateTopVertexCount(Vector2 topSurfaceSize, float verticesPerUnit)
        {
            var safeSize = new Vector2(Mathf.Max(0.01f, topSurfaceSize.x), Mathf.Max(0.01f, topSurfaceSize.y));
            var safeVerticesPerUnit = Mathf.Max(0.5f, verticesPerUnit);
            return new Vector2Int(
                Mathf.Max(2, Mathf.RoundToInt(safeSize.x * safeVerticesPerUnit) + 1),
                Mathf.Max(2, Mathf.RoundToInt(safeSize.y * safeVerticesPerUnit) + 1));
        }

        public static Mesh BuildTopMesh(Vector2 topSurfaceSize, Vector2Int vertexCount, string meshName)
        {
            var width = Mathf.Max(0.01f, topSurfaceSize.x);
            var depth = Mathf.Max(0.01f, topSurfaceSize.y);
            var verticesX = Mathf.Max(2, vertexCount.x);
            var verticesZ = Mathf.Max(2, vertexCount.y);
            var vertexTotal = verticesX * verticesZ;
            var triangleTotal = (verticesX - 1) * (verticesZ - 1) * 6;

            var mesh = new Mesh
            {
                name = string.IsNullOrEmpty(meshName) ? "Water25D Top Surface" : meshName
            };
            if (vertexTotal > 65535)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }

            var vertices = new Vector3[vertexTotal];
            var uvs = new Vector2[vertexTotal];
            var triangles = new int[triangleTotal];
            var dx = width / (verticesX - 1);
            var dz = depth / (verticesZ - 1);

            for (var z = 0; z < verticesZ; z++)
            {
                var v = (float)z / (verticesZ - 1);
                for (var x = 0; x < verticesX; x++)
                {
                    var u = (float)x / (verticesX - 1);
                    var index = x + z * verticesX;
                    vertices[index] = new Vector3(x * dx, 0f, z * dz);
                    uvs[index] = new Vector2(u, v);
                }
            }

            var triangleIndex = 0;
            for (var z = 0; z < verticesZ - 1; z++)
            {
                for (var x = 0; x < verticesX - 1; x++)
                {
                    var index = x + z * verticesX;
                    triangles[triangleIndex++] = index;
                    triangles[triangleIndex++] = index + verticesX;
                    triangles[triangleIndex++] = index + 1;
                    triangles[triangleIndex++] = index + 1;
                    triangles[triangleIndex++] = index + verticesX;
                    triangles[triangleIndex++] = index + verticesX + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.MarkDynamic();
            return mesh;
        }

        /// <summary>
        /// Builds the intentionally minimal top surface used by FlatStylized.
        /// The vertex and UV order matches the corners of the existing tessellated
        /// builder so mode changes do not change the authored mapping convention.
        /// </summary>
        public static Mesh BuildFlatTopMesh(Vector2 topSurfaceSize, string meshName)
        {
            var width = Mathf.Max(0.01f, topSurfaceSize.x);
            var depth = Mathf.Max(0.01f, topSurfaceSize.y);
            var mesh = new Mesh
            {
                name = string.IsNullOrEmpty(meshName) ? "Water25D Flat Top Surface" : meshName
            };

            mesh.vertices = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(width, 0f, 0f),
                new Vector3(0f, 0f, depth),
                new Vector3(width, 0f, depth)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };
            // The order produces upward-facing normals, matching BuildTopMesh.
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.MarkDynamic();
            return mesh;
        }

        public static Mesh BuildFrontMesh(Vector2 topSurfaceSize, float frontSurfaceDepth, int horizontalVertexCount, string meshName)
        {
            var width = Mathf.Max(0.01f, topSurfaceSize.x);
            var depth = Mathf.Max(0.01f, frontSurfaceDepth);
            var verticesX = Mathf.Max(2, horizontalVertexCount);
            var mesh = new Mesh
            {
                name = string.IsNullOrEmpty(meshName) ? "Water25D Front Surface" : meshName
            };

            var vertices = new Vector3[verticesX * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[(verticesX - 1) * 6];
            var dx = width / (verticesX - 1);

            for (var x = 0; x < verticesX; x++)
            {
                var u = (float)x / (verticesX - 1);
                vertices[x] = new Vector3(x * dx, 0f, 0f);
                uvs[x] = new Vector2(u, 1f);

                var bottomIndex = x + verticesX;
                vertices[bottomIndex] = new Vector3(x * dx, -depth, 0f);
                uvs[bottomIndex] = new Vector2(u, 0f);
            }

            var triangleIndex = 0;
            for (var x = 0; x < verticesX - 1; x++)
            {
                var topA = x;
                var topB = x + 1;
                var bottomA = x + verticesX;
                var bottomB = x + verticesX + 1;

                // Face the front of the XY panel toward negative local Z, matching the reference presentation.
                triangles[triangleIndex++] = topB;
                triangles[triangleIndex++] = bottomA;
                triangles[triangleIndex++] = topA;
                triangles[triangleIndex++] = bottomB;
                triangles[triangleIndex++] = bottomA;
                triangles[triangleIndex++] = topB;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.MarkDynamic();
            return mesh;
        }
    }
}
