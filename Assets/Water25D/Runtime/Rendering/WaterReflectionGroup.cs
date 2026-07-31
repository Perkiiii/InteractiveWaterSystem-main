using System;
using UnityEngine;

namespace Water25D.Rendering
{
    public enum WaterReflectionMode
    {
        Disabled,
        Stylized,
        Planar
    }

    /// <summary>
    /// Quantized values that determine whether two water surfaces can share one reflection render.
    /// Small height and normal differences intentionally create separate groups instead of
    /// producing a visibly incorrect mirror for one of the surfaces.
    /// </summary>
    public struct WaterReflectionGroupKey : IEquatable<WaterReflectionGroupKey>
    {
        public int CameraId;
        public int PlaneHeight;
        public int NormalX;
        public int NormalY;
        public int NormalZ;
        public int CullingMask;
        public int Mode;
        public int ResolutionScale;
        public int UpdateIntervalFrames;

        public static WaterReflectionGroupKey Create(
            Camera camera,
            Transform plane,
            LayerMask cullingMask,
            WaterReflectionMode mode,
            float resolutionScale,
            int updateIntervalFrames)
        {
            var normal = plane != null ? plane.up.normalized : Vector3.up;
            var planeHeight = plane != null ? Vector3.Dot(normal, plane.position) : 0f;
            return new WaterReflectionGroupKey
            {
                CameraId = camera != null ? camera.GetEntityId().GetHashCode() : 0,
                PlaneHeight = Mathf.RoundToInt(planeHeight / 0.01f),
                NormalX = Mathf.RoundToInt(normal.x * 1000f),
                NormalY = Mathf.RoundToInt(normal.y * 1000f),
                NormalZ = Mathf.RoundToInt(normal.z * 1000f),
                CullingMask = cullingMask.value,
                Mode = (int)mode,
                ResolutionScale = Mathf.RoundToInt(Mathf.Clamp01(resolutionScale) * 100f),
                UpdateIntervalFrames = Mathf.Clamp(updateIntervalFrames, 1, 120)
            };
        }

        public bool Equals(WaterReflectionGroupKey other)
        {
            return CameraId == other.CameraId &&
                   PlaneHeight == other.PlaneHeight &&
                   NormalX == other.NormalX &&
                   NormalY == other.NormalY &&
                   NormalZ == other.NormalZ &&
                   CullingMask == other.CullingMask &&
                   Mode == other.Mode &&
                   ResolutionScale == other.ResolutionScale &&
                   UpdateIntervalFrames == other.UpdateIntervalFrames;
        }

        public override bool Equals(object obj)
        {
            return obj is WaterReflectionGroupKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = CameraId;
                hash = hash * 31 + PlaneHeight;
                hash = hash * 31 + NormalX;
                hash = hash * 31 + NormalY;
                hash = hash * 31 + NormalZ;
                hash = hash * 31 + CullingMask;
                hash = hash * 31 + Mode;
                hash = hash * 31 + ResolutionScale;
                return hash * 31 + UpdateIntervalFrames;
            }
        }
    }
}
