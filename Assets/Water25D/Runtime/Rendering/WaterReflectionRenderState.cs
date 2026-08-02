using System;
using UnityEngine;

namespace Water25D.Rendering
{
    /// <summary>
    /// Immutable reflection output published by the shared reflection manager. Renderer
    /// property blocks are intentionally outside this contract and remain owned by the
    /// controller's WaterRenderingModule.
    /// </summary>
    internal readonly struct WaterReflectionRenderState : IEquatable<WaterReflectionRenderState>
    {
        public readonly Texture Texture;
        public readonly Matrix4x4 ViewProjection;
        public readonly bool Enabled;
        public readonly bool StylizedFallback;
        public readonly float Strength;
        public readonly int RenderFrame;

        public WaterReflectionRenderState(
            Texture texture,
            Matrix4x4 viewProjection,
            bool enabled,
            bool stylizedFallback,
            float strength,
            int renderFrame)
        {
            Texture = texture;
            ViewProjection = viewProjection;
            Enabled = enabled && texture != null;
            StylizedFallback = stylizedFallback && !Enabled;
            Strength = Mathf.Clamp01(IsFinite(strength) ? strength : 0f);
            RenderFrame = renderFrame;
        }

        public static WaterReflectionRenderState Disabled => new WaterReflectionRenderState(
            null,
            Matrix4x4.identity,
            false,
            false,
            0f,
            -1);

        public static WaterReflectionRenderState ForMode(WaterReflectionMode mode, float strength)
        {
            switch (mode)
            {
                case WaterReflectionMode.Stylized:
                    return new WaterReflectionRenderState(
                        null,
                        Matrix4x4.identity,
                        false,
                        true,
                        strength,
                        -1);
                case WaterReflectionMode.Planar:
                    return new WaterReflectionRenderState(
                        null,
                        Matrix4x4.identity,
                        false,
                        false,
                        strength,
                        -1);
                default:
                    return Disabled;
            }
        }

        public bool Equals(WaterReflectionRenderState other)
        {
            return Texture == other.Texture &&
                   ViewProjection == other.ViewProjection &&
                   Enabled == other.Enabled &&
                   StylizedFallback == other.StylizedFallback &&
                   Mathf.Approximately(Strength, other.Strength) &&
                   RenderFrame == other.RenderFrame;
        }

        public override bool Equals(object obj)
        {
            return obj is WaterReflectionRenderState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Texture != null ? Texture.GetHashCode() : 0;
                hash = hash * 31 + ViewProjection.GetHashCode();
                hash = hash * 31 + Enabled.GetHashCode();
                hash = hash * 31 + StylizedFallback.GetHashCode();
                hash = hash * 31 + Strength.GetHashCode();
                return hash * 31 + RenderFrame;
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
