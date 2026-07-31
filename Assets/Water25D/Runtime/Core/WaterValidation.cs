using UnityEngine;

namespace Water25D
{
    public static class WaterValidation
    {
        public static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public static bool TryValidateDimensions(Vector2 topSurfaceSize, float frontSurfaceDepth, out string message)
        {
            if (!IsFinite(topSurfaceSize.x) || !IsFinite(topSurfaceSize.y) || topSurfaceSize.x <= 0f || topSurfaceSize.y <= 0f)
            {
                message = "Top surface width and visual depth must be finite and greater than zero.";
                return false;
            }

            if (!IsFinite(frontSurfaceDepth) || frontSurfaceDepth <= 0f)
            {
                message = "Front surface physical depth must be finite and greater than zero.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool IsLayerIncluded(LayerMask mask, int layer)
        {
            if (layer < 0 || layer > 31)
            {
                return false;
            }

            return (mask.value & (1 << layer)) != 0;
        }

        public static Vector2 ClampSurfaceUV(Vector2 uv)
        {
            return new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));
        }
    }
}
