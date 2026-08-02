#ifndef WATER25D_AMEYE_ADAPTATION_INCLUDED
#define WATER25D_AMEYE_ADAPTATION_INCLUDED

// This include is the narrow Water25D adaptation seam around the copied Ameye
// functions. The copied source helpers remain package-owned assets; this file
// supplies the local-coordinate and ownership rules that the Water25D shaders
// need in place of the source package's camera, displacement, and reflection
// systems.
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Assets/Water25D/Shaders/Stylized/Includes/Ameye_DistortUV.hlsl"
#include "Assets/Water25D/Shaders/Stylized/Includes/Ameye_Lighting.hlsl"

float2 Water25DAmeyePanningUV(
    float2 localUV,
    float2 tiling,
    float2 speed,
    float time)
{
    return frac(localUV * tiling + speed * time);
}

float3 Water25DAmeyeDepthColour(
    float depth01,
    float3 shallowColour,
    float3 deepColour,
    float depthPower,
    float bandSteps,
    float bandInfluence)
{
    float shapedDepth = pow(saturate(depth01), max(0.05, depthPower));
    float3 colour = lerp(shallowColour, deepColour, shapedDepth);
    float safeSteps = max(1.0, bandSteps);
    float banded = floor(saturate(shapedDepth) * safeSteps) / safeSteps;
    float bandAmount = saturate(bandInfluence) * 0.35;
    return lerp(colour, lerp(shallowColour, deepColour, banded), bandAmount);
}

float Water25DAmeyeSpecular(
    float3 normalWS,
    float3 viewWS,
    float3 lightDirectionWS,
    float smoothness,
    float hardness)
{
    float sourceSpecular = LightingSpecular(
        SafeNormalize(lightDirectionWS),
        SafeNormalize(normalWS),
        SafeNormalize(viewWS),
        max(1.0, exp2(10.0 * saturate(smoothness) + 1.0)));
    float toonSpecular = smoothstep(0.005, 0.01, sourceSpecular);
    return lerp(sourceSpecular, toonSpecular, saturate(hardness));
}

float Water25DAmeyeFoamBreakup(float2 localUV, float time, float phase)
{
    float waveA = sin(dot(localUV, float2(84.8, 61.7)) + time * 0.57 + phase);
    float waveB = sin(dot(localUV.yx, float2(37.4, 113.2)) - time * 0.31 + phase * 1.7);
    return saturate(0.5 + 0.25 * waveA + 0.25 * waveB);
}

#endif
