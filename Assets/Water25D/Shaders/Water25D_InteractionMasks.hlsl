#ifndef WATER25D_INTERACTION_MASKS_INCLUDED
#define WATER25D_INTERACTION_MASKS_INCLUDED

float2 WaterRotateInteractionUV(float2 value, float angle)
{
    float sine = sin(angle);
    float cosine = cos(angle);
    return float2(
        value.x * cosine - value.y * sine,
        value.x * sine + value.y * cosine);
}

float2 WaterBuildInteractionAtlasUV(
    float2 localUV,
    float4 metadata,
    float age01,
    float4 grid,
    float variantCount,
    float frameCount)
{
    float columns = max(1.0, floor(grid.x + 0.5));
    float rows = max(1.0, floor(grid.y + 0.5));
    float cellCount = max(1.0, columns * rows);
    float safeVariantCount = clamp(floor(variantCount + 0.5), 1.0, cellCount);
    float maximumFrameCount = max(1.0, floor(cellCount / safeVariantCount));
    float safeFrameCount = clamp(floor(frameCount + 0.5), 1.0, maximumFrameCount);
    float variantIndex = clamp(floor(metadata.x + 0.5), 0.0, safeVariantCount - 1.0);
    float frameIndex = _PainterlyAgeFrames > 0.5
        ? min(floor(saturate(age01) * safeFrameCount), safeFrameCount - 1.0)
        : 0.0;
    float frameOffset = clamp(floor(metadata.z + 0.5), 0.0, safeFrameCount - 1.0);
    frameIndex = fmod(frameIndex + frameOffset, safeFrameCount);

    float cellIndex = frameIndex * safeVariantCount + variantIndex;
    float cellX = fmod(cellIndex, columns);
    float cellY = floor(cellIndex / columns);
    // Keep bilinear filtering inside the selected cell. This fixed inset also makes a one-cell
    // atlas behave identically to an atlas with multiple variants or age frames.
    float2 safeLocalUV = saturate(localUV) * 0.96 + 0.02;
    return (float2(cellX, cellY) + safeLocalUV) / float2(columns, rows);
}

float WaterApplyInteractionMask(float analytical, float mask, float influence)
{
    return analytical * lerp(1.0, saturate(mask), saturate(influence));
}

float WaterSampleRingMask(float2 localUV, float4 metadata, float age01)
{
    float2 atlasUV = WaterBuildInteractionAtlasUV(
        localUV,
        metadata,
        age01,
        _RingMaskAtlasGrid,
        _RingMaskVariantCount,
        _RingMaskFrameCount);
    return SAMPLE_TEXTURE2D(_RingMaskAtlas, sampler_RingMaskAtlas, atlasUV).r;
}

float WaterSampleFoamMask(float2 localUV, float4 metadata, float age01)
{
    float2 atlasUV = WaterBuildInteractionAtlasUV(
        localUV,
        metadata,
        age01,
        _FoamMaskAtlasGrid,
        _FoamMaskVariantCount,
        _FoamMaskFrameCount);
    return SAMPLE_TEXTURE2D(_FoamMaskAtlas, sampler_FoamMaskAtlas, atlasUV).r;
}

float WaterSampleWakeMask(float2 localUV, float4 metadata, float age01)
{
    float2 atlasUV = WaterBuildInteractionAtlasUV(
        localUV,
        metadata,
        age01,
        _WakeMaskAtlasGrid,
        _WakeMaskVariantCount,
        _WakeMaskFrameCount);
    return SAMPLE_TEXTURE2D(_WakeMaskAtlas, sampler_WakeMaskAtlas, atlasUV).r;
}

#endif
