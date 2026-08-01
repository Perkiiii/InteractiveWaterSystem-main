using System;
using UnityEngine;

namespace Water25D
{
    public interface IWaterRippleSimulator : IDisposable
    {
        bool IsAvailable { get; }
        bool IsSuspended { get; }
        Texture HeightTexture { get; }
        int DroppedImpactCount { get; }

        void EnqueueImpact(WaterRippleImpact impact);
        void Tick(float deltaTime, bool isVisible);
        void ResetSimulation();
    }
}
