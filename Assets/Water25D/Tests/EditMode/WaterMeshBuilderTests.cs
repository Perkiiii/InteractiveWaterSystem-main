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
    }
}
