Shader "Water25D/Top Surface"
{
    Properties
    {
        _BaseColor("Top Color", Color) = (0.2, 0.48, 0.6, 0.92)
        _ShallowColor("Shallow Color", Color) = (0.2, 0.48, 0.6, 0.92)
        _DeepColor("Deep Color", Color) = (0.04, 0.22, 0.36, 0.94)
        _FoamColor("Foam Color", Color) = (0.78, 0.95, 1, 0.8)
        _TopDepthPower("Top Depth Power", Float) = 0.9
        _TopOpacity("Top Opacity", Range(0, 1)) = 0.92
        _ColorBandSteps("Colour Band Steps", Float) = 4
        _ColorBandInfluence("Colour Band Influence", Range(0, 1)) = 0.15
        _SurfaceNormalTexture("Surface Normal Texture", 2D) = "bump" {}
        _SurfaceDetailTexture("Surface Detail Texture", 2D) = "gray" {}
        _NormalLayer1Scale("Normal Layer 1 Scale", Vector) = (0.55, 0.45, 0, 0)
        _NormalLayer1Speed("Normal Layer 1 Speed", Vector) = (0.035, -0.021, 0, 0)
        _NormalLayer1Strength("Normal Layer 1 Strength", Float) = 0.65
        _NormalLayer2Scale("Normal Layer 2 Scale", Vector) = (1.15, 0.9, 0, 0)
        _NormalLayer2Speed("Normal Layer 2 Speed", Vector) = (-0.017, 0.029, 0, 0)
        _NormalLayer2Strength("Normal Layer 2 Strength", Float) = 0.25
        _AmbientNormalStrength("Ambient Normal Strength", Range(0, 1)) = 0.1
        _FresnelTint("Fresnel Tint", Color) = (0.75, 0.95, 1, 1)
        _FresnelStrength("Fresnel Strength", Range(0, 1)) = 0.3
        _FresnelPower("Fresnel Power", Float) = 4
        _HighlightColor("Highlight Color", Color) = (0.88, 0.98, 1, 1)
        _HighlightStrength("Highlight Strength", Range(0, 1)) = 0.18
        _HighlightThreshold("Highlight Threshold", Range(0, 1)) = 0.65
        _HighlightSoftness("Highlight Softness", Range(0.01, 1)) = 0.2
        _HighlightBreakup("Highlight Breakup", Range(0, 1)) = 0.25
        _HighlightDirection("Highlight Direction", Vector) = (-0.3, 0.85, -0.25, 0)
        _StylizedReflectionTint("Stylized Reflection Tint", Color) = (1, 1, 1, 1)
        _StylizedReflectionHorizonColor("Reflection Horizon Color", Color) = (0.22, 0.52, 0.68, 1)
        _StylizedReflectionTopColor("Reflection Top Color", Color) = (0.48, 0.78, 0.88, 1)
        _StylizedReflectionStrength("Stylized Reflection Strength", Range(0, 1)) = 0.3
        _PlanarReflectionTint("Planar Reflection Tint", Color) = (1, 1, 1, 1)
        _PlanarReflectionStrength("Planar Reflection Strength", Range(0, 1)) = 0.35
        _AmbientReflectionDistortion("Ambient Reflection Distortion", Range(0, 0.05)) = 0.0025
        _RingNormalStrength("Ring Normal Strength", Range(0, 1)) = 0.18
        _RingReflectionDistortion("Ring Reflection Distortion", Range(0, 0.05)) = 0.008
        _WakeNormalStrength("Wake Normal Strength", Range(0, 1)) = 0.12
        _WakeReflectionDistortion("Wake Reflection Distortion", Range(0, 0.05)) = 0.006
        _BoundaryFoamWidth("Boundary Foam Width", Range(0.0001, 0.5)) = 0.025
        _BoundaryFoamSoftness("Boundary Foam Softness", Range(0, 0.5)) = 0.04
        _BoundaryFoamBreakup("Boundary Foam Breakup", Range(0, 1)) = 0.25
        _BoundaryFoamIntensity("Boundary Foam Intensity", Range(0, 1)) = 0.45
        _RefractionSourceAvailable("Refraction Source Available", Float) = 0
        _RefractionTint("Refraction Tint", Color) = (1, 1, 1, 1)
        _RefractionStrength("Refraction Strength", Range(0, 0.02)) = 0.003
        _CausticTexture("Caustic Texture", 2D) = "black" {}
        _CausticTextureValid("Caustic Texture Valid", Float) = 0
        _CausticScale("Caustic Scale", Vector) = (0.16, 0.16, 0, 0)
        _CausticSpeed("Caustic Speed", Vector) = (0.018, -0.012, 0, 0)
        _CausticTint("Caustic Tint", Color) = (0.78, 1, 0.82, 1)
        _CausticIntensity("Caustic Intensity", Range(0, 1)) = 0.2
        _CausticDepthFade("Caustic Depth Fade", Range(0, 1)) = 0.7
        _StylizedHighlightsEnabled("Stylized Highlights Enabled", Float) = 1
        _RefractionEnabled("Refraction Enabled", Float) = 0
        _CausticsEnabled("Caustics Enabled", Float) = 0
        _RippleTexture("Ripple Texture", 2D) = "black" {}
        _RippleEnabled("Ripple Enabled", Float) = 0
        _RippleAmplitude("Ripple Amplitude", Float) = 0.18
        _SurfaceMode("Surface Mode", Float) = 0
        _WaterSize("Water Size", Vector) = (20, 6.5, 0, 0)
        _WaterRingCount("Surface Ring Count", Float) = 0
        _WaterFoamCount("Contact Foam Count", Float) = 0
        _WaterFoamSoftness("Contact Foam Softness", Float) = 0.06
        _FoamReflectionOcclusion("Foam Reflection Occlusion", Range(0, 1)) = 0.85
        _WaterWakeCount("Wake Segment Count", Float) = 0
        _WakeFadePower("Wake Fade Power", Float) = 1.25
        _WaveAmplitude("Ambient Wave Amplitude", Float) = 0.06
        _WaveLength("Ambient Wave Length", Float) = 3.5
        _WaveSpeed("Ambient Wave Speed", Float) = 0.8
        _WaveDirection("Ambient Wave Direction", Vector) = (1, 0.15, 0, 0)
        _WaveBands("Ambient Wave Bands", Range(1, 4)) = 3
        _ReflectionTexture("Reflection Texture", 2D) = "black" {}
        _ReflectionEnabled("Reflection Enabled", Float) = 0
        _ReflectionFallback("Stylized Reflection", Float) = 0
        _ReflectionStrength("Reflection Strength", Range(0, 1)) = 0.35
        _RingMaskAtlas("Ring Mask Atlas", 2D) = "white" {}
        _FoamMaskAtlas("Foam Mask Atlas", 2D) = "white" {}
        _WakeMaskAtlas("Wake Mask Atlas", 2D) = "white" {}
        _PainterlyMasksEnabled("Painterly Masks Enabled", Float) = 0
        _PainterlyAgeFrames("Painterly Age Frames", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Assets/Water25D/Shaders/Water25D_AmbientWaves.hlsl"
        #include "Assets/Water25D/Shaders/Water25D_StylizedSurface.hlsl"

        TEXTURE2D(_RippleTexture);
        SAMPLER(sampler_RippleTexture);
        TEXTURE2D(_SurfaceNormalTexture);
        SAMPLER(sampler_SurfaceNormalTexture);
        TEXTURE2D(_SurfaceDetailTexture);
        SAMPLER(sampler_SurfaceDetailTexture);
        TEXTURE2D(_CausticTexture);
        SAMPLER(sampler_CausticTexture);
        TEXTURE2D(_CameraOpaqueTexture);
        SAMPLER(sampler_CameraOpaqueTexture);
        TEXTURE2D(_ReflectionTexture);
        SAMPLER(sampler_ReflectionTexture);
        TEXTURE2D(_RingMaskAtlas);
        SAMPLER(sampler_RingMaskAtlas);
        TEXTURE2D(_FoamMaskAtlas);
        SAMPLER(sampler_FoamMaskAtlas);
        TEXTURE2D(_WakeMaskAtlas);
        SAMPLER(sampler_WakeMaskAtlas);

        #define WATER_MAX_RINGS 16
        #define WATER_MAX_CONTACT_FOAMS 8
        #define WATER_MAX_WAKES 16

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _ShallowColor;
            half4 _DeepColor;
            half4 _FoamColor;
            float _TopDepthPower;
            float _TopOpacity;
            float _ColorBandSteps;
            float _ColorBandInfluence;
            float _SurfaceNormalTextureValid;
            float _SurfaceDetailTextureValid;
            float4 _NormalLayer1Scale;
            float4 _NormalLayer1Speed;
            float _NormalLayer1Strength;
            float4 _NormalLayer2Scale;
            float4 _NormalLayer2Speed;
            float _NormalLayer2Strength;
            float _AmbientNormalStrength;
            float4 _FresnelTint;
            float _FresnelStrength;
            float _FresnelPower;
            float4 _HighlightColor;
            float _HighlightStrength;
            float _HighlightThreshold;
            float _HighlightSoftness;
            float _HighlightBreakup;
            float4 _HighlightDirection;
            float4 _StylizedReflectionTint;
            float4 _StylizedReflectionHorizonColor;
            float4 _StylizedReflectionTopColor;
            float _StylizedReflectionStrength;
            float4 _PlanarReflectionTint;
            float _PlanarReflectionStrength;
            float _AmbientReflectionDistortion;
            float _RingNormalStrength;
            float _RingReflectionDistortion;
            float _WakeNormalStrength;
            float _WakeReflectionDistortion;
            float _BoundaryFoamWidth;
            float _BoundaryFoamSoftness;
            float _BoundaryFoamBreakup;
            float _BoundaryFoamIntensity;
            float _RefractionSourceAvailable;
            float4 _RefractionTint;
            float _RefractionStrength;
            float4 _CausticScale;
            float4 _CausticSpeed;
            float4 _CausticTint;
            float _CausticTextureValid;
            float _CausticIntensity;
            float _CausticDepthFade;
            float _SecondaryAmbientDetailEnabled;
            float _StylizedHighlightsEnabled;
            float _RefractionEnabled;
            float _CausticsEnabled;
            float _RippleEnabled;
            float _RippleAmplitude;
            float _SurfaceMode;
            float4 _WaterSize;
            float _WaterRingCount;
            float4 _WaterRingsA[WATER_MAX_RINGS];
            float4 _WaterRingsB[WATER_MAX_RINGS];
            float4 _WaterRingsC[WATER_MAX_RINGS];
            float _WaterFoamCount;
            float4 _WaterFoamsA[WATER_MAX_CONTACT_FOAMS];
            float4 _WaterFoamsB[WATER_MAX_CONTACT_FOAMS];
            float4 _WaterFoamsC[WATER_MAX_CONTACT_FOAMS];
            float _WaterFoamSoftness;
            float _FoamReflectionOcclusion;
            float _WaterWakeCount;
            float4 _WaterWakesA[WATER_MAX_WAKES];
            float4 _WaterWakesB[WATER_MAX_WAKES];
            float4 _WaterWakesC[WATER_MAX_WAKES];
            float _WakeFadePower;
            float _WaveAmplitude;
            float _WaveLength;
            float _WaveSpeed;
            float4 _WaveDirection;
            float _WaveBands;
            float _ReflectionEnabled;
            float _ReflectionFallback;
            float _ReflectionStrength;
            float4x4 _ReflectionViewProjection;
            float _PainterlyMasksEnabled;
            float _PainterlyAgeFrames;
            float _RingMaskAtlasValid;
            float4 _RingMaskAtlasGrid;
            float _RingMaskVariantCount;
            float _RingMaskFrameCount;
            float _RingMaskInfluence;
            float _FoamMaskAtlasValid;
            float4 _FoamMaskAtlasGrid;
            float _FoamMaskVariantCount;
            float _FoamMaskFrameCount;
            float _FoamMaskInfluence;
            float _WakeMaskAtlasValid;
            float4 _WakeMaskAtlasGrid;
            float _WakeMaskVariantCount;
            float _WakeMaskFrameCount;
            float _WakeMaskInfluence;
        CBUFFER_END

        #include "Assets/Water25D/Shaders/Water25D_InteractionMasks.hlsl"

        float3 EvaluateStylizedSurfaceNormal(float2 localXZ)
        {
            float2 directionA = Water25DSafeDirection(_NormalLayer1Scale.xy);
            float2 directionB = Water25DSafeDirection(_NormalLayer2Scale.xy);
            float3 normalWS = Water25DProceduralNormal(
                localXZ,
                _Time.y,
                directionA,
                max(0.05, length(_NormalLayer1Scale.xy)),
                dot(_NormalLayer1Speed.xy, directionA),
                _NormalLayer1Strength,
                directionB,
                max(0.05, length(_NormalLayer2Scale.xy)),
                dot(_NormalLayer2Speed.xy, directionB),
                _NormalLayer2Strength);

            if (_SurfaceNormalTextureValid > 0.5)
            {
                float2 normalUV = frac(localXZ * _NormalLayer1Scale.xy + _Time.y * _NormalLayer1Speed.xy);
                float2 encoded = SAMPLE_TEXTURE2D(
                    _SurfaceNormalTexture,
                    sampler_SurfaceNormalTexture,
                    normalUV).xy * 2.0 - 1.0;
                float3 sampledNormal = Water25DSafeNormal(float3(encoded.x, 1.0, encoded.y));
                normalWS = Water25DSafeNormal(lerp(normalWS, sampledNormal, saturate(_NormalLayer1Strength)));
            }

            if (_SecondaryAmbientDetailEnabled > 0.5 && _SurfaceDetailTextureValid > 0.5)
            {
                float2 detailUV = frac(localXZ * _NormalLayer2Scale.xy + _Time.y * _NormalLayer2Speed.xy);
                float detail = SAMPLE_TEXTURE2D(
                    _SurfaceDetailTexture,
                    sampler_SurfaceDetailTexture,
                    detailUV).r * 2.0 - 1.0;
                normalWS = Water25DAddSurfaceSlope(
                    normalWS,
                    float2(detail, -detail * 0.75),
                    _NormalLayer2Strength * 0.15);
            }

            return Water25DSafeNormal(lerp(float3(0.0, 1.0, 0.0), normalWS, saturate(_AmbientNormalStrength)));
        }

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 worldPos : TEXCOORD1;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        float EvaluateContactFoam(float2 localXZ)
        {
            if (_SurfaceMode < 0.5 || _WaterFoamCount <= 0.5)
            {
                return 0.0;
            }

            int foamCount = min((int)_WaterFoamCount, WATER_MAX_CONTACT_FOAMS);
            float accumulation = 0.0;
            for (int foamIndex = 0; foamIndex < WATER_MAX_CONTACT_FOAMS; foamIndex++)
            {
                if (foamIndex >= foamCount)
                {
                    break;
                }

                float4 foamA = _WaterFoamsA[foamIndex];
                float4 foamB = _WaterFoamsB[foamIndex];
                float4 foamC = _WaterFoamsC[foamIndex];
                float2 halfSize = max(float2(0.001, 0.001), float2(foamA.z, foamB.x));
                float2 ellipseOffset = (localXZ - foamA.xy) / halfSize;
                float radialDistance = length(ellipseOffset);
                float softness = max(0.001, _WaterFoamSoftness / min(halfSize.x, halfSize.y));
                if (radialDistance > 1.0 + softness)
                {
                    continue;
                }

                float ellipse = 1.0 - smoothstep(1.0, 1.0 + softness, radialDistance);
                if (_PainterlyMasksEnabled > 0.5 && _FoamMaskAtlasValid > 0.5 && _FoamMaskInfluence > 0.0 && ellipse > 0.0)
                {
                    float2 maskOffset = WaterRotateInteractionUV(ellipseOffset, foamC.y);
                    if (foamC.w < 0.0)
                    {
                        maskOffset.y = -maskOffset.y;
                    }

                    float2 maskUV = maskOffset * 0.5 + 0.5;
                    float painterlyMask = WaterSampleFoamMask(maskUV, foamC, 1.0 - saturate(foamB.y));
                    ellipse = WaterApplyInteractionMask(ellipse, painterlyMask, _FoamMaskInfluence);
                }

                float breakup = 0.84 +
                    0.16 * sin(dot(localXZ - foamA.xy, float2(1.73, 2.37)) + foamB.w * 6.2831853 + _Time.y * 0.45);
                float contactAmount = lerp(0.72, 1.0, saturate(foamB.z));
                float contribution = ellipse * saturate(breakup) * saturate(foamA.w) * saturate(foamB.y) * contactAmount;
                accumulation = saturate(accumulation + contribution);
            }

            return accumulation;
        }

        float EvaluateWakeCapsules(float2 localXZ)
        {
            if (_SurfaceMode < 0.5 || _WaterWakeCount <= 0.5)
            {
                return 0.0;
            }

            int wakeCount = min((int)_WaterWakeCount, WATER_MAX_WAKES);
            float accumulation = 0.0;
            for (int wakeIndex = 0; wakeIndex < WATER_MAX_WAKES; wakeIndex++)
            {
                if (wakeIndex >= wakeCount)
                {
                    break;
                }

                float4 wakeA = _WaterWakesA[wakeIndex];
                float4 wakeB = _WaterWakesB[wakeIndex];
                float4 wakeC = _WaterWakesC[wakeIndex];
                float2 start = wakeA.xy;
                float2 end = wakeA.zw;
                float halfWidth = max(0.001, wakeB.x);
                float2 boundsMin = min(start, end) - halfWidth;
                float2 boundsMax = max(start, end) + halfWidth;
                if (localXZ.x < boundsMin.x || localXZ.x > boundsMax.x ||
                    localXZ.y < boundsMin.y || localXZ.y > boundsMax.y)
                {
                    continue;
                }

                float2 segment = end - start;
                float segmentLengthSq = dot(segment, segment);
                float along = segmentLengthSq > 0.000001
                    ? saturate(dot(localXZ - start, segment) / segmentLengthSq)
                    : 0.0;
                float2 closest = start + segment * along;
                float distanceToCapsule = distance(localXZ, closest);
                float edgeSoftness = max(0.002, halfWidth * 0.45);
                float capsule = 1.0 - smoothstep(halfWidth, halfWidth + edgeSoftness, distanceToCapsule);
                if (_PainterlyMasksEnabled > 0.5 && _WakeMaskAtlasValid > 0.5 && _WakeMaskInfluence > 0.0 && capsule > 0.0)
                {
                    float segmentLength = sqrt(max(segmentLengthSq, 0.000001));
                    float2 tangent = segment / segmentLength;
                    float2 normal = float2(-tangent.y, tangent.x);
                    float alongDistance = dot(localXZ - start, tangent);
                    float sideDistance = dot(localXZ - start, normal);
                    float2 maskUV = float2(
                        (alongDistance + halfWidth) / max(0.001, segmentLength + 2.0 * halfWidth),
                        sideDistance / max(0.001, 2.0 * halfWidth) + 0.5);
                    if (wakeC.w < 0.0)
                    {
                        maskUV.y = 1.0 - maskUV.y;
                    }

                    float painterlyMask = WaterSampleWakeMask(maskUV, wakeC, wakeB.y);
                    capsule = WaterApplyInteractionMask(capsule, painterlyMask, _WakeMaskInfluence);
                }

                float ageFade = pow(saturate(1.0 - wakeB.y), max(0.1, _WakeFadePower));
                float breakup = 0.86 +
                    0.14 * sin(dot(localXZ - closest, float2(2.11, 1.37)) + wakeB.w * 6.2831853);
                float taper = lerp(0.78, 1.0, 1.0 - abs(along * 2.0 - 1.0));
                accumulation = saturate(accumulation + capsule * ageFade * saturate(wakeB.z) * saturate(breakup) * taper);
            }

            return accumulation;
        }

        Varyings Vert(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            float3 positionOS = input.positionOS.xyz;
            if (_SurfaceMode < 0.5)
            {
                float3 worldPosition = TransformObjectToWorld(positionOS);
                float ambient = EvaluateWaterAmbientWaves(
                    worldPosition.xz,
                    _WaveDirection.xy,
                    _WaveLength,
                    _WaveAmplitude,
                    _WaveSpeed,
                    _Time.y,
                    _WaveBands);
                float ripple = 0.0;
                if (_RippleEnabled > 0.5)
                {
                    ripple = SAMPLE_TEXTURE2D_LOD(_RippleTexture, sampler_RippleTexture, input.uv, 0).r * _RippleAmplitude;
                }

                positionOS.y += ambient + ripple;
            }

            output.positionCS = GetVertexPositionInputs(positionOS).positionCS;
            output.uv = input.uv;
            output.worldPos = TransformObjectToWorld(positionOS);
            return output;
        }

        half4 Frag(Varyings input) : SV_Target
        {
            float2 localXZ = input.uv * _WaterSize.xy;
            float depth01 = Water25DDepthGradient(1.0 - input.uv.y, _TopDepthPower);
            float3 surfaceRGB = lerp(_ShallowColor.rgb, _DeepColor.rgb, depth01);
            surfaceRGB = Water25DPosterizeColor(surfaceRGB, _ColorBandSteps, _ColorBandInfluence);

            float3 normalWS = EvaluateStylizedSurfaceNormal(localXZ);
            float3 viewDirectionWS = Water25DSafeNormal(_WorldSpaceCameraPos - input.worldPos);
            float2 interactionOffset = normalWS.xz * _AmbientReflectionDistortion;
            half contactFoam = EvaluateContactFoam(localXZ);
            half ringHighlight = 0.0;
            float2 ringNormalXZ = 0.0;
            if (_SurfaceMode > 0.5 && _WaterRingCount > 0.5)
            {
                int ringCount = min((int)_WaterRingCount, WATER_MAX_RINGS);
                for (int ringIndex = 0; ringIndex < WATER_MAX_RINGS; ringIndex++)
                {
                    if (ringIndex >= ringCount)
                    {
                        break;
                    }

                    float4 ringA = _WaterRingsA[ringIndex];
                    float4 ringB = _WaterRingsB[ringIndex];
                    float4 ringC = _WaterRingsC[ringIndex];
                    float age01 = saturate(ringA.z);
                    float radius = lerp(ringB.x, ringB.y, age01);
                    float2 ringOffset = localXZ - ringA.xy;
                    float ringBounds = radius + ringB.z + ringB.w;
                    if (length(ringOffset) > ringBounds)
                    {
                        continue;
                    }

                    float distanceFromRing = abs(distance(localXZ, ringA.xy) - radius);
                    float thickness = max(0.0001, ringB.z);
                    float softness = max(0.0001, ringB.w);
                    float annulus = 1.0 - smoothstep(thickness, thickness + softness, distanceFromRing);
                    if (_PainterlyMasksEnabled > 0.5 && _RingMaskAtlasValid > 0.5 && _RingMaskInfluence > 0.0 && annulus > 0.0)
                    {
                        float2 maskOffset = WaterRotateInteractionUV(ringOffset, ringC.y);
                        float2 maskUV = maskOffset / max(0.001, 2.0 * ringBounds) + 0.5;
                        float painterlyMask = WaterSampleRingMask(maskUV, ringC, age01);
                        annulus = WaterApplyInteractionMask(annulus, painterlyMask, _RingMaskInfluence);
                    }

                    float fade = (1.0 - age01) * saturate(ringA.w);
                    ringHighlight = saturate(ringHighlight + annulus * fade);
                    ringNormalXZ += Water25DSafeDirection(ringOffset) * annulus * fade;
                }
            }

            half wake = EvaluateWakeCapsules(localXZ);
            float2 wakeDetail = float2(
                sin(localXZ.x * 1.7 + _Time.y * 0.15),
                cos(localXZ.y * 1.3 - _Time.y * 0.12)) * wake;
            normalWS = Water25DAddSurfaceSlope(normalWS, ringNormalXZ, _RingNormalStrength);
            normalWS = Water25DAddSurfaceSlope(normalWS, wakeDetail, _WakeNormalStrength);
            interactionOffset += ringNormalXZ * _RingReflectionDistortion + wakeDetail * _WakeReflectionDistortion;

            float boundaryFoam = Water25DBoundaryFoam(
                input.uv,
                _BoundaryFoamWidth,
                _BoundaryFoamSoftness,
                _BoundaryFoamBreakup,
                _Time.y);
            float foamAmount = saturate(
                boundaryFoam * _BoundaryFoamIntensity +
                ringHighlight * 0.55 +
                wake * 0.38 +
                contactFoam * 0.75);
            surfaceRGB = lerp(surfaceRGB, _FoamColor.rgb, foamAmount);

            float fresnel = Water25DFresnel(
                normalWS,
                viewDirectionWS,
                _FresnelStrength,
                _FresnelPower);
            surfaceRGB = lerp(surfaceRGB, _FresnelTint.rgb, fresnel);

            float reflectionOcclusion = 1.0 - contactFoam * saturate(_FoamReflectionOcclusion);
            if (_ReflectionFallback > 0.5)
            {
                float reflectionGradient = saturate(1.0 - input.uv.y);
                float3 stylizedReflection = lerp(
                    _StylizedReflectionHorizonColor.rgb,
                    _StylizedReflectionTopColor.rgb,
                    reflectionGradient) * _StylizedReflectionTint.rgb;
                float stylizedAmount = saturate(_StylizedReflectionStrength) * fresnel * reflectionOcclusion;
                surfaceRGB = lerp(surfaceRGB, stylizedReflection, stylizedAmount);
            }

            if (_ReflectionEnabled > 0.5)
            {
                float2 reflectionUV;
                float reflectionValid;
                Water25DProjectReflectionUV(
                    input.worldPos + float3(interactionOffset.x, 0.0, interactionOffset.y),
                    _ReflectionViewProjection,
                    reflectionUV,
                    reflectionValid);
                half4 reflection = SAMPLE_TEXTURE2D(
                    _ReflectionTexture,
                    sampler_ReflectionTexture,
                    reflectionUV);
                float planarAmount = saturate(_ReflectionStrength * _PlanarReflectionStrength) *
                    fresnel * reflectionOcclusion * reflectionValid * reflection.a;
                surfaceRGB = lerp(surfaceRGB, reflection.rgb * _PlanarReflectionTint.rgb, planarAmount);
            }

            if (_StylizedHighlightsEnabled > 0.5)
            {
                float highlight = Water25DStylizedHighlight(
                    normalWS,
                    viewDirectionWS,
                    _HighlightDirection.xyz,
                    _HighlightThreshold,
                    _HighlightSoftness,
                    _HighlightBreakup,
                    localXZ,
                    _Time.y);
                surfaceRGB += _HighlightColor.rgb * highlight * _HighlightStrength;
            }

            if (_RefractionEnabled > 0.5 && _RefractionSourceAvailable > 0.5)
            {
                float2 screenUV = input.positionCS.xy / max(_ScaledScreenParams.xy, float2(1.0, 1.0));
                screenUV += normalWS.xz * _RefractionStrength;
                half4 sceneColor = SAMPLE_TEXTURE2D(
                    _CameraOpaqueTexture,
                    sampler_CameraOpaqueTexture,
                    Water25DClampScreenUV(screenUV));
                surfaceRGB = lerp(surfaceRGB, sceneColor.rgb * _RefractionTint.rgb, 0.55);
            }

            half alpha = saturate(lerp(_ShallowColor.a, _DeepColor.a, depth01) * _TopOpacity);
            alpha = saturate(alpha + boundaryFoam * 0.20 + ringHighlight * 0.12 + wake * 0.08 + contactFoam * 0.18);
            return half4(saturate(surfaceRGB), alpha);
        }
        ENDHLSL

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
