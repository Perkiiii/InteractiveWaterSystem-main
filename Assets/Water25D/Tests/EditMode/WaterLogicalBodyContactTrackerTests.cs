using NUnit.Framework;
using UnityEngine;

namespace Water25D.Tests
{
    public sealed class WaterLogicalBodyContactTrackerTests
    {
        [Test]
        public void OneColliderCreatesOneLogicalBodyAndDuplicateMembershipIsRejected()
        {
            var bodyObject = new GameObject("Water25D Tracker Body");
            try
            {
                var body = bodyObject.AddComponent<Rigidbody2D>();
                var collider = bodyObject.AddComponent<BoxCollider2D>();
                var tracker = new WaterLogicalBodyContactTracker(8);

                Assert.IsTrue(tracker.TryAdd(collider));
                Assert.IsFalse(tracker.TryAdd(collider));
                Assert.AreEqual(1, tracker.LogicalBodyCount);
                Assert.IsTrue(tracker.TryGetSampleAt(0, out var sample));
                Assert.AreSame(body, sample.Body);
                Assert.AreEqual(1, sample.ColliderCount);
                Assert.IsTrue(tracker.TryRemove(collider));
                Assert.AreEqual(0, tracker.LogicalBodyCount);
            }
            finally
            {
                Object.DestroyImmediate(bodyObject);
            }
        }

        [Test]
        public void TwoCollidersOnOneBodyRemainLogicalUntilFinalColliderLeaves()
        {
            var bodyObject = new GameObject("Water25D Multi Collider Body");
            try
            {
                bodyObject.AddComponent<Rigidbody2D>();
                var first = bodyObject.AddComponent<BoxCollider2D>();
                var second = bodyObject.AddComponent<BoxCollider2D>();
                second.offset = new Vector2(2f, 0f);
                var tracker = new WaterLogicalBodyContactTracker(8);

                Assert.IsTrue(tracker.TryAdd(first));
                Assert.IsTrue(tracker.TryAdd(second));
                Assert.AreEqual(1, tracker.LogicalBodyCount);
                Assert.IsTrue(tracker.TryGetSampleAt(0, out var sample));
                Assert.AreEqual(2, sample.ColliderCount);
                Assert.That(sample.AggregateBounds.size.x, Is.GreaterThan(2.9f));

                Assert.IsTrue(tracker.TryRemove(first));
                Assert.AreEqual(1, tracker.LogicalBodyCount);
                Assert.IsTrue(tracker.TryRemove(second));
                Assert.AreEqual(0, tracker.LogicalBodyCount);
            }
            finally
            {
                Object.DestroyImmediate(bodyObject);
            }
        }

        [Test]
        public void ColliderAndBodyCapacitiesAreDeterministicAndOverflowIsBounded()
        {
            var bodyObject = new GameObject("Water25D Tracker Overflow Body");
            var otherBodies = new GameObject[2];
            try
            {
                var body = bodyObject.AddComponent<Rigidbody2D>();
                var colliders = new BoxCollider2D[9];
                for (var i = 0; i < colliders.Length; i++)
                {
                    colliders[i] = bodyObject.AddComponent<BoxCollider2D>();
                    colliders[i].offset = new Vector2(i * 2f, 0f);
                }

                var tracker = new WaterLogicalBodyContactTracker(2);
                for (var i = 0; i < colliders.Length; i++)
                {
                    Assert.IsTrue(tracker.TryAdd(colliders[i]));
                }

                Assert.AreEqual(1, tracker.LogicalBodyCount);
                Assert.IsTrue(tracker.TryGetSampleAt(0, out var sample));
                Assert.AreEqual(9, sample.ColliderCount);
                Assert.AreEqual(8, sample.SampledColliderCount);
                Assert.AreEqual(1, tracker.ColliderOverflowCount);

                for (var i = 0; i < otherBodies.Length; i++)
                {
                    otherBodies[i] = new GameObject("Water25D Tracker Capacity Body " + i);
                    otherBodies[i].AddComponent<Rigidbody2D>();
                    var collider = otherBodies[i].AddComponent<BoxCollider2D>();
                    if (i == 0)
                    {
                        Assert.IsTrue(tracker.TryAdd(collider));
                    }
                    else
                    {
                        Assert.IsFalse(tracker.TryAdd(collider));
                    }
                }

                Assert.AreEqual(2, tracker.LogicalBodyCount);
                Assert.AreEqual(1, tracker.DroppedBodyCount);
                for (var i = 0; i < colliders.Length; i++)
                {
                    Assert.IsTrue(tracker.TryRemove(colliders[i]));
                }
            }
            finally
            {
                Object.DestroyImmediate(bodyObject);
                for (var i = 0; i < otherBodies.Length; i++)
                {
                    if (otherBodies[i] != null)
                    {
                        Object.DestroyImmediate(otherBodies[i]);
                    }
                }
            }
        }

        [Test]
        public void DisabledColliderIsCleanedAndClearRemovesAllReferences()
        {
            var bodyObject = new GameObject("Water25D Tracker Cleanup Body");
            try
            {
                bodyObject.AddComponent<Rigidbody2D>();
                var collider = bodyObject.AddComponent<BoxCollider2D>();
                var tracker = new WaterLogicalBodyContactTracker(8);
                Assert.IsTrue(tracker.TryAdd(collider));

                collider.enabled = false;
                tracker.CleanupInvalid();
                Assert.AreEqual(0, tracker.LogicalBodyCount);

                collider.enabled = true;
                Assert.IsTrue(tracker.TryAdd(collider));
                bodyObject.SetActive(false);
                tracker.CleanupInvalid();
                Assert.AreEqual(0, tracker.LogicalBodyCount);
                bodyObject.SetActive(true);
                Assert.IsTrue(tracker.TryAdd(collider));
                tracker.Clear();
                Assert.AreEqual(0, tracker.LogicalBodyCount);
                Assert.IsFalse(tracker.TryGetBodyAt(0, out _));
            }
            finally
            {
                Object.DestroyImmediate(bodyObject);
            }
        }
    }
}
