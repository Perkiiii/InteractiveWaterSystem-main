using NUnit.Framework;
using UnityEngine;

namespace Water25D.Tests
{
    public sealed class WaterControllerEditModeTests
    {
        [Test]
        public void ControllerRepairsNamedHierarchyWithoutRemovingUnrelatedChild()
        {
            var root = new GameObject("Water Test Root");
            try
            {
                var unrelated = new GameObject("Authored Child");
                unrelated.transform.SetParent(root.transform, false);
                var controller = root.AddComponent<Water25DController>();
                controller.RepairHierarchyAndRebuild();

                Assert.IsNotNull(root.transform.Find("TopSurface"));
                Assert.IsNotNull(root.transform.Find("FrontSurface"));
                Assert.IsNotNull(root.transform.Find("SurfaceCrossingTrigger"));
                Assert.IsNotNull(root.transform.Find("BuoyancyVolume"));
                Assert.IsNotNull(root.transform.Find("ReflectionAnchor"));
                Assert.IsNotNull(root.transform.Find("FXRoot"));
                Assert.IsNotNull(root.transform.Find("Authored Child"));
                Assert.IsNotNull(root.transform.Find("TopSurface").GetComponent<MeshFilter>().sharedMesh);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
