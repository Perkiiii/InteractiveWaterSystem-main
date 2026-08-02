#ifndef WATER25D_STYLIZED_SURFACE_INCLUDED
#define WATER25D_STYLIZED_SURFACE_INCLUDED

#define WATER25D_PI 3.14159265359

float2 Water25DSafeDirection(float2 direction)
{
    float lengthSquared = dot(direction, direction);
    return lengthSquared > 0.000001 ? direction * rsqrt(lengthSquared) : float2(1.0, 0.0);
}

float3 Water25DSafeNormal(float3 normal)
{
    float lengthSquared = dot(normal, normal);
    return lengthSquared > 0.000001 ? normal * rsqrt(lengthSquared) : float3(0.0, 1.0, 0.0);
}

float Water25DHash21(float2 value)
{
    value = frac(value * float2(123.34, 456.21));
    value += dot(value, value + 45.32);
    return frac(value.x * value.y);
}

float Water25DValueNoise(float2 value)
{
    float2 cell = floor(value);
    float2 fraction = frac(value);
    fraction = fraction * fraction * (3.0 - 2.0 * fraction);
    float lower = lerp(
        Water25DHash21(cell),
        Water25DHash21(cell + float2(1.0, 0.0)),
        fraction.x);
    float upper = lerp(
        Water25DHash21(cell + float2(0.0, 1.0)),
        Water25DHash21(cell + float2(1.0, 1.0)),
        fraction.x);
    return lerp(lower, upper, fraction.y);
}

float Water25DSurfaceNoise(float2 localXZ, float time)
{
    float first = Water25DValueNoise(localXZ * 0.65 + float2(time * 0.035, -time * 0.021));
    float second = Water25DValueNoise(localXZ * 1.37 + float2(-time * 0.017, time * 0.029));
    return saturate(first * 0.65 + second * 0.35);
}

float3 Water25DProceduralNormal(
    float2 localXZ,
    float time,
    float2 directionA,
    float scaleA,
    float speedA,
    float strengthA,
    float2 directionB,
    float scaleB,
    float speedB,
    float strengthB)
{
    directionA = Water25DSafeDirection(directionA);
    directionB = Water25DSafeDirection(directionB);
    scaleA = max(0.001, abs(scaleA));
    scaleB = max(0.001, abs(scaleB));

    float phaseA = dot(localXZ, directionA) * scaleA + time * speedA;
    float phaseB = dot(localXZ, directionB) * scaleB + time * speedB;
    float2 slope = directionA * (cos(phaseA) * strengthA * scaleA * 0.12) +
                   directionB * (cos(phaseB) * strengthB * scaleB * 0.08);
    float noise = Water25DSurfaceNoise(localXZ * 0.5, time) - 0.5;
    slope += float2(noise, -noise * 0.7) * (strengthA + strengthB) * 0.035;
    return Water25DSafeNormal(float3(-slope.x, 1.0, -slope.y));
}

float3 Water25DAddSurfaceSlope(float3 normalWS, float2 slopeXZ, float strength)
{
    float3 result = normalWS + float3(slopeXZ.x, 0.0, slopeXZ.y) * saturate(strength);
    return Water25DSafeNormal(result);
}

float Water25DDepthGradient(float depth01, float power)
{
    return pow(saturate(depth01), max(0.05, power));
}

float3 Water25DPosterizeColor(float3 color, float steps, float influence)
{
    float safeSteps = max(1.0, floor(steps + 0.5));
    float3 quantized = floor(saturate(color) * safeSteps + 0.5) / safeSteps;
    return lerp(color, quantized, saturate(influence) * step(1.5, safeSteps));
}

float Water25DFresnel(float3 normalWS, float3 viewDirectionWS, float strength, float power)
{
    normalWS = Water25DSafeNormal(normalWS);
    viewDirectionWS = Water25DSafeNormal(viewDirectionWS);
    float grazing = pow(1.0 - saturate(dot(normalWS, viewDirectionWS)), max(0.1, power));
    return saturate(grazing * saturate(strength));
}

float Water25DStylizedHighlight(
    float3 normalWS,
    float3 viewDirectionWS,
    float3 highlightDirectionWS,
    float threshold,
    float softness,
    float breakup,
    float2 localXZ,
    float time)
{
    normalWS = Water25DSafeNormal(normalWS);
    viewDirectionWS = Water25DSafeNormal(viewDirectionWS);
    highlightDirectionWS = Water25DSafeNormal(highlightDirectionWS);
    float3 halfDirection = Water25DSafeNormal(highlightDirectionWS + viewDirectionWS);
    float specular = pow(saturate(dot(normalWS, halfDirection)), 32.0);
    float shaped = smoothstep(
        saturate(threshold - max(0.001, softness)),
        saturate(threshold + max(0.001, softness)),
        specular);
    float noise = 0.75 + 0.25 * sin(dot(localXZ, float2(1.71, 2.23)) + time * 0.18);
    return shaped * lerp(1.0, noise, saturate(breakup));
}

void Water25DProjectReflectionUV(
    float3 worldPosition,
    float4x4 viewProjection,
    out float2 uv,
    out float valid)
{
    float4 clip = mul(viewProjection, float4(worldPosition, 1.0));
    float safeW = max(abs(clip.w), 0.0001);
    uv = clip.xy / safeW * 0.5 + 0.5;
    uv.y = 1.0 - uv.y;
    float2 edgeDistance = min(uv, 1.0 - uv);
    float edgeFade = smoothstep(0.0, 0.035, min(edgeDistance.x, edgeDistance.y));
    valid = step(0.0001, clip.w) * edgeFade;
    uv = saturate(uv);
}

float Water25DBoundaryFoam(
    float2 surfaceUV,
    float width,
    float softness,
    float breakup,
    float time)
{
    float edgeDistance = min(
        min(surfaceUV.x, 1.0 - surfaceUV.x),
        min(surfaceUV.y, 1.0 - surfaceUV.y));
    float edge = 1.0 - smoothstep(
        max(0.0001, width),
        max(0.0002, width + softness),
        edgeDistance);
    float noise = 0.78 + 0.22 * sin(
        surfaceUV.x * 43.0 + surfaceUV.y * 29.0 + time * 0.42);
    return edge * lerp(1.0, saturate(noise), saturate(breakup));
}

float2 Water25DClampScreenUV(float2 uv)
{
    return saturate(uv);
}

#endif
