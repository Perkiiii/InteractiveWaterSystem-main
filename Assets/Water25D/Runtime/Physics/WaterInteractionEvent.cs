using System;
using UnityEngine;

namespace Water25D
{
    public enum WaterInteractionEventType
    {
        SurfaceEnter,
        SurfaceExit,
        Submerged,
        Resurfaced
    }

    [Serializable]
    public struct WaterInteractionEvent
    {
        public Water25DController Water;
        public Rigidbody2D Rigidbody;
        public Collider2D Collider;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Speed;
        public float RippleStrength;
        public WaterInteractionEventType Type;

        public WaterInteractionEvent(
            Water25DController water,
            Rigidbody2D rigidbody,
            Collider2D collider,
            Vector2 position,
            Vector2 velocity,
            float rippleStrength,
            WaterInteractionEventType type)
        {
            Water = water;
            Rigidbody = rigidbody;
            Collider = collider;
            Position = position;
            Velocity = velocity;
            Speed = velocity.magnitude;
            RippleStrength = rippleStrength;
            Type = type;
        }
    }
}
