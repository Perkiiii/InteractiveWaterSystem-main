using UnityEngine;

namespace Water25D
{
    /// <summary>
    /// A value snapshot of one logical body's currently eligible surface colliders.
    /// The tracker owns the sampled collider storage; this value only describes the
    /// aggregate needed by crossing and presentation code.
    /// </summary>
    internal struct WaterSurfaceContactSample
    {
        public Rigidbody2D Body;
        public Collider2D RepresentativeCollider;
        public Bounds AggregateBounds;
        public int BodyKey;
        public int ColliderCount;
        public int SampledColliderCount;

        public Vector2 ContactCenter => new Vector2(AggregateBounds.center.x, AggregateBounds.center.y);
        public float ContactWidth => AggregateBounds.size.x;
    }
}
