using System;
using UnityEngine;

namespace Water25D
{
    [Serializable]
    public struct WaterRippleImpact
    {
        public Vector2 CenterUV;
        public float Strength;
        public float Radius;
        public bool InitialUp;

        public WaterRippleImpact(Vector2 centerUV, float strength, float radius, bool initialUp)
        {
            CenterUV = centerUV;
            Strength = strength;
            Radius = radius;
            InitialUp = initialUp;
        }

        public float SignedStrength => InitialUp ? Mathf.Abs(Strength) : -Mathf.Abs(Strength);
    }
}
