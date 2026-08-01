using NUnit.Framework;
using UnityEngine;

namespace Water25D.Tests
{
    public sealed class WaterMeshBuilderTests
    {
        [Test]
        public void TopMeshUsesXZCoordinatesAndExpectedBounds()
        {
            var mesh = WaterMeshBuilder.BuildTopMesh(new Vector2(4f, 2f), new Vector2Int(5, 3), "Test Top");
            try
            {
                Assert.AreEqual(15, mesh.vertexCount);
                Assert.AreEqual(48, mesh.triangles.Length);
                Assert.That(mesh.bounds.min, Is.EqualTo(Vector3.zero));
                Assert.That(mesh.bounds.max.x, Is.EqualTo(4f).Within(0.0001f));
                Assert.That(mesh.bounds.max.y, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(mesh.bounds.max.z, Is.EqualTo(2f).Within(0.0001f));
                Assert.That(mesh.normals[0].y, Is.GreaterThan(0.99f));
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }

        [TestCase(4f, 2f)]
        [TestCase(20f, 6.5f)]
        public void FlatTopMeshUsesFourVerticesAndPreservesCornerMapping(float width, float depth)
        {
            var mesh = WaterMeshBuilder.BuildFlatTopMesh(new Vector2(width, depth), "Test Flat Top");
            try
            {
                Assert.AreEqual(4, mesh.vertexCount);
                Assert.AreEqual(6, mesh.triangles.Length);
                Assert.AreEqual(2, mesh.triangles.Length / 3);
                Assert.That(mesh.bounds.min, Is.EqualTo(Vector3.zero));
                Assert.That(mesh.bounds.max, Is.EqualTo(new Vector3(width, 0f, depth)));

                var vertices = mesh.vertices;
                var uvs = mesh.uv;
                var normals = mesh.normals;
                var expectedVertices = new[]
                {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(width, 0f, 0f),
                    new Vector3(0f, 0f, depth),
                    new Vector3(width, 0f, depth)
                };
                var expectedUvs = new[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f)
                };

                for (var i = 0; i < vertices.Length; i++)
                {
                    Assert.That(vertices[i], Is.EqualTo(expectedVertices[i]));
                    Assert.That(vertices[i].y, Is.EqualTo(0f).Within(0.0001f));
                    Assert.That(uvs[i], Is.EqualTo(expectedUvs[i]));
                    Assert.That(normals[i].x, Is.EqualTo(0f).Within(0.0001f));
                    Assert.That(normals[i].y, Is.GreaterThan(0.99f));
                    Assert.That(normals[i].z, Is.EqualTo(0f).Within(0.0001f));
                }

                var triangles = mesh.triangles;
                for (var i = 0; i < triangles.Length; i += 3)
                {
                    var first = vertices[triangles[i]];
                    var second = vertices[triangles[i + 1]];
                    var third = vertices[triangles[i + 2]];
                    Assert.That(Vector3.Cross(second - first, third - first).y, Is.GreaterThan(0f));
                }
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void FlatFrontMeshUsesAStaticFourVertexQuad()
        {
            var mesh = WaterMeshBuilder.BuildFrontMesh(new Vector2(4f, 2f), 6f, 2, "Test Flat Front");
            try
            {
                Assert.AreEqual(4, mesh.vertexCount);
                Assert.AreEqual(6, mesh.triangles.Length);
                Assert.That(mesh.bounds.min.y, Is.EqualTo(-6f).Within(0.0001f));
                Assert.That(mesh.bounds.max.y, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(mesh.bounds.max.x, Is.EqualTo(4f).Within(0.0001f));
                Assert.That(mesh.bounds.size.z, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(mesh.normals[0].z, Is.LessThan(-0.99f));
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void FrontMeshUsesXYCoordinatesAndExpectedDepth()
        {
            var mesh = WaterMeshBuilder.BuildFrontMesh(new Vector2(4f, 2f), 6f, 5, "Test Front");
            try
            {
                Assert.AreEqual(10, mesh.vertexCount);
                Assert.AreEqual(24, mesh.triangles.Length);
                Assert.That(mesh.bounds.min.y, Is.EqualTo(-6f).Within(0.0001f));
                Assert.That(mesh.bounds.max.x, Is.EqualTo(4f).Within(0.0001f));
                Assert.That(mesh.bounds.size.z, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(mesh.normals[0].z, Is.LessThan(-0.99f));
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void VertexDensityProducesRectangularRippleResolution()
        {
            var settings = WaterQualitySettings.Default;
            Assert.AreEqual(new Vector2Int(320, 104), settings.CalculateRippleResolution(new Vector2(20f, 6.5f)));
            Assert.AreEqual(new Vector2Int(161, 53), WaterMeshBuilder.CalculateTopVertexCount(new Vector2(20f, 6.5f), 8f));
        }

        [Test]
        public void SimulatedTopMeshRetainsTessellatedDensityAndCornerUVs()
        {
            var settings = WaterQualitySettings.Default;
            var size = new Vector2(20f, 6.5f);
            var count = WaterMeshBuilder.CalculateTopVertexCount(size, settings.TopVerticesPerUnit);
            var mesh = WaterMeshBuilder.BuildTopMesh(size, count, "Test Simulated Top");
            try
            {
                Assert.AreEqual(count.x * count.y, mesh.vertexCount);
                Assert.AreEqual((count.x - 1) * (count.y - 1) * 6, mesh.triangles.Length);
                Assert.That(mesh.uv[0], Is.EqualTo(new Vector2(0f, 0f)));
                Assert.That(mesh.uv[count.x - 1], Is.EqualTo(new Vector2(1f, 0f)));
                Assert.That(mesh.uv[(count.y - 1) * count.x], Is.EqualTo(new Vector2(0f, 1f)));
                Assert.That(mesh.uv[mesh.vertexCount - 1], Is.EqualTo(new Vector2(1f, 1f)));
                Assert.That(mesh.bounds.max, Is.EqualTo(new Vector3(size.x, 0f, size.y)));
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }
    }
}
