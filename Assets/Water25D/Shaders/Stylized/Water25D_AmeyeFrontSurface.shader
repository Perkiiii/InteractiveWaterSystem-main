Shader "Water25D/Stylized Ameye Front Surface"
{
    Properties
    {
        _FrontSurfaceColor("Surface Color", Color) = (0.08, 0.38, 0.52, 0.84)
        _FrontDeepColor("Deep Color", Color) = (0.025, 0.1, 0.18, 0.94)
        _FoamColor("Foam Color", Color) = (0.78, 0.95, 1, 0.8)
        _AmeyeIntersectionFoamTexture("Ameye Intersection Foam", 2D) = "white" {}
        _AmeyeSurfaceFoamTexture("Ameye Surface Foam", 2D) = "white" {}
        _TopDepthPower("Top Depth Power", Float) = 0.9
        _ColorBandSteps("Colour Band Steps", Float) = 4
        _ColorBandInfluence("Colour Band Influence", Range(0, 1)) = 0.15
        _FrontOpacity("Front Opacity", Range(0, 1)) = 0.9
        _FrontDepthPower("Front Depth Power", Float) = 1.15
        _WaterlineBandWidth("Waterline Band Width", Range(0.001, 0.5)) = 0.07
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
        _BoundaryFoamWidth("Boundary Foam Width", Range(0.0001, 0.5)) = 0.025
        _BoundaryFoamSoftness("Boundary Foam Softness", Range(0, 0.5)) = 0.04
        _BoundaryFoamBreakup("Boundary Foam Breakup", Range(0, 1)) = 0.25
        _BoundaryFoamIntensity("Boundary Foam Intensity", Range(0, 1)) = 0.45
        _FrontDistortionSourceAvailable("Front Distortion Source Available", Float) = 0
        _FrontDistortionTint("Front Distortion Tint", Color) = (0.8, 0.95, 1, 1)
        _FrontDistortionStrength("Front Distortion Strength", Range(0, 0.01)) = 0.003
        _CausticTexture("Caustic Texture", 2D) = "black" {}
        _CausticTextureValid("Caustic Texture Valid", Float) = 0
        _CausticScale("Caustic Scale", Vector) = (0.16, 0.16, 0, 0)
        _CausticSpeed("Caustic Speed", Vector) = (0.018, -0.012, 0, 0)
        _CausticTint("Caustic Tint", Color) = (0.78, 1, 0.82, 1)
        _CausticIntensity("Caustic Intensity", Range(0, 1)) = 0.2
        _CausticDepthFade("Caustic Depth Fade", Range(0, 1)) = 0.7
        _StylizedHighlightsEnabled("Stylized Highlights Enabled", Float) = 1
        _CausticsEnabled("Caustics Enabled", Float) = 0
        _FrontDepth("Front Depth", Float) = 10
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
        #include "Assets/Water25D/Shaders/Stylized/Includes/Water25D_AmeyeAdaptation.hlsl"

        #define WATER_MAX_RINGS 16
        #define WATER_MAX_CONTACT_FOAMS 8
        #define WATER_MAX_WAKES 16

        TEXTURE2D(_RingMaskAtlas);
        SAMPLER(sampler_RingMaskAtlas);
        TEXTURE2D(_FoamMaskAtlas);
        SAMPLER(sampler_FoamMaskAtlas);
        TEXTURE2D(_WakeMaskAtlas);
        SAMPLER(sampler_WakeMaskAtlas);
        TEXTURE2D(_SurfaceNormalTexture);
        SAMPLER(sampler_SurfaceNormalTexture);
        TEXTURE2D(_SurfaceDetailTexture);
        SAMPLER(sampler_SurfaceDetailTexture);
        TEXTURE2D(_AmeyeIntersectionFoamTexture);
        SAMPLER(sampler_AmeyeIntersectionFoamTexture);
        TEXTURE2D(_AmeyeSurfaceFoamTexture);
        SAMPLER(sampler_AmeyeSurfaceFoamTexture);
        TEXTURE2D(_CausticTexture);
        SAMPLER(sampler_CausticTexture);
        TEXTURE2D(_CameraSortingLayerTexture);
        SAMPLER(sampler_CameraSortingLayerTexture);

        CBUFFER_START(UnityPerMaterial)
            half4 _FrontSurfaceColor;
            half4 _FrontDeepColor;
            half4 _FoamColor;
            float _FrontDepthPower;
            float _FrontOpacity;
            float _WaterlineBandWidth;
            float _ColorBandSteps;
            float _ColorBandInfluence;
            float _SurfaceNormalTextureValid;
            float _SurfaceDetailTextureValid;
            float _SecondaryAmbientDetailEnabled;
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
            float _BoundaryFoamWidth;
            float _BoundaryFoamSoftness;
            float _BoundaryFoamBreakup;
            float _BoundaryFoamIntensity;
            float _FrontDistortionSourceAvailable;
            float4 _FrontDistortionTint;
            float _FrontDistortionStrength;
            float4 _CausticScale;
            float4 _CausticSpeed;
            float4 _CausticTint;
            float _CausticTextureValid;
            float _CausticIntensity;
            float _CausticDepthFade;
            float _StylizedHighlightsEnabled;
            float _CausticsEnabled;
            float _FrontDepth;
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

        float3 EvaluateFrontStylizedNormal(float2 localXZ)
        {
            float noise = Water25DSurfaceNoise(localXZ * 0.42, _Time.y);
            float3 normalWS = Water25DSafeNormal(float3(
                (noise - 0.5) * 0.22,
                (noise - 0.5) * 0.10,
                1.0));
            if (_SurfaceNormalTextureValid > 0.5)
            {
                float2 normalUV = Water25DAmeyePanningUV(
                    localXZ,
                    _NormalLayer1Scale.xy,
                    _NormalLayer1Speed.xy,
                    _Time.y);
                float2 encoded = SAMPLE_TEXTURE2D(
                    _SurfaceNormalTexture,
                    sampler_SurfaceNormalTexture,
                    normalUV).xy * 2.0 - 1.0;
                float3 sampledNormal = Water25DSafeNormal(float3(encoded.x, encoded.y * 0.5, 1.0));
                normalWS = Water25DSafeNormal(lerp(normalWS, sampledNormal, saturate(_NormalLayer1Strength)));
            }
            if (_SecondaryAmbientDetailEnabled > 0.5 && _SurfaceDetailTextureValid > 0.5)
            {
                float2 detailUV = Water25DAmeyePanningUV(
                    localXZ,
                    _NormalLayer2Scale.xy,
                    _NormalLayer2Speed.xy,
                    _Time.y);
                float detail = SAMPLE_TEXTURE2D(
                    _SurfaceDetailTexture,
                    sampler_SurfaceDetailTexture,
                    detailUV).r * 2.0 - 1.0;
                normalWS = Water25DSafeNormal(normalWS + float3(detail, detail * 0.5, 0.0) * _NormalLayer2Strength * 0.08);
            }
            return Water25DSafeNormal(lerp(float3(0.0, 0.0, 1.0), normalWS, saturate(_AmbientNormalStrength)));
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

        float EvaluateFrontContactFoam(float localX)
        {
            if (_SurfaceMode < 0.5 || _WaterFoamCount <= 0.5)
            {
                return 0.0;
            }

            int foamCount = min((int)_WaterFoamCount, WATER_MAX_CONTACT_FOAMS);
            float accumulation = 0.0;
            float2 localXZ = float2(localX, 0.0);
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
                float2 sourceFoamUV = frac(ellipseOffset * 0.5 + 0.5 + _Time.y * float2(0.008, -0.005));
                float sourceFoam = SAMPLE_TEXTURE2D(
                    _AmeyeIntersectionFoamTexture,
                    sampler_AmeyeIntersectionFoamTexture,
                    sourceFoamUV).r;
                ellipse *= lerp(0.72, saturate(sourceFoam), 0.42);
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
                float contribution = ellipse * saturate(breakup) * saturate(foamA.w) * saturate(foamB.y);
                accumulation = saturate(accumulation + contribution);
            }

            return accumulation;
        }

        float EvaluateFrontWake(float localX)
        {
            if (_SurfaceMode < 0.5 || _WaterWakeCount <= 0.5)
            {
                return 0.0;
            }

            int wakeCount = min((int)_WaterWakeCount, WATER_MAX_WAKES);
            float accumulation = 0.0;
            float2 frontPoint = float2(localX, 0.0);
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
                if (frontPoint.x < boundsMin.x || frontPoint.x > boundsMax.x ||
                    0.0 < boundsMin.y || 0.0 > boundsMax.y)
                {
                    continue;
                }

                float2 segment = end - start;
                float segmentLengthSq = dot(segment, segment);
                float along = segmentLengthSq > 0.000001
                    ? saturate(dot(frontPoint - start, segment) / segmentLengthSq)
                    : 0.0;
                float2 closest = start + segment * along;
                float distanceToCapsule = distance(frontPoint, closest);
                float edgeSoftness = max(0.002, halfWidth * 0.45);
                float capsule = 1.0 - smoothstep(halfWidth, halfWidth + edgeSoftness, distanceToCapsule);
                if (_PainterlyMasksEnabled > 0.5 && _WakeMaskAtlasValid > 0.5 && _WakeMaskInfluence > 0.0 && capsule > 0.0)
                {
                    float segmentLength = sqrt(max(segmentLengthSq, 0.000001));
                    float2 tangent = segment / segmentLength;
                    float2 normal = float2(-tangent.y, tangent.x);
                    float alongDistance = dot(frontPoint - start, tangent);
                    float sideDistance = dot(frontPoint - start, normal);
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
                    0.14 * sin((localX - closest.x) * 2.11 + wakeB.w * 6.2831853);
                accumulation = saturate(accumulation + capsule * ageFade * saturate(wakeB.z) * saturate(breakup));
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
                positionOS.y += ambient * input.uv.y;
            }

            output.positionCS = GetVertexPositionInputs(positionOS).positionCS;
            output.uv = input.uv;
            output.worldPos = TransformObjectToWorld(positionOS);
            return output;
        }

        half4 Frag(Varyings input) : SV_Target
        {
            float depth = Water25DDepthGradient(1.0 - input.uv.y, _FrontDepthPower);
            float2 localXZ = float2(input.uv.x * _WaterSize.x, depth * _FrontDepth);
            float3 normalWS = EvaluateFrontStylizedNormal(localXZ);
            float3 viewDirectionWS = Water25DSafeNormal(_WorldSpaceCameraPos - input.worldPos);
            float3 surfaceRGB = Water25DAmeyeDepthColour(
                depth,
                _FrontSurfaceColor.rgb,
                _FrontDeepColor.rgb,
                _FrontDepthPower,
                _ColorBandSteps,
                _ColorBandInfluence);
            float waterlineFoam = 1.0 - smoothstep(0.0, max(0.001, _WaterlineBandWidth), depth);

            float contactFoam = EvaluateFrontContactFoam(input.uv.x * _WaterSize.x);
            float contactSeam = contactFoam * waterlineFoam;
            float ringHighlight = 0.0;

            if (_SurfaceMode > 0.5 && _WaterRingCount > 0.5)
            {
                float localX = input.uv.x * _WaterSize.x;
                float2 ringLocalXZ = float2(localX, 0.0);
                int ringCount = min((int)_WaterRingCount, WATER_MAX_RINGS);
                float seamBand = waterlineFoam;
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
                    float thickness = max(0.0001, ringB.z);
                    float softness = max(0.0001, ringB.w);
                    float2 ringOffset = ringLocalXZ - ringA.xy;
                    float ringBounds = radius + thickness + softness;
                    if (length(ringOffset) > ringBounds)
                    {
                        continue;
                    }

                    float distanceToFrontPlane = abs(ringA.y);
                    float reachesFront = smoothstep(0.0, thickness + softness, radius - distanceToFrontPlane);
                    float distanceFromRing = abs(distance(ringLocalXZ, ringA.xy) - radius);
                    float annulus = 1.0 - smoothstep(thickness, thickness + softness, distanceFromRing);
                    float2 sourceFoamUV = frac(
                        ringOffset / max(0.001, 2.0 * ringBounds) +
                        0.5 +
                        _Time.y * float2(-0.006, 0.009));
                    float sourceFoam = SAMPLE_TEXTURE2D(
                        _AmeyeSurfaceFoamTexture,
                        sampler_AmeyeSurfaceFoamTexture,
                        sourceFoamUV).r;
                    annulus *= lerp(0.74, saturate(sourceFoam), 0.38);
                    if (_PainterlyMasksEnabled > 0.5 && _RingMaskAtlasValid > 0.5 && _RingMaskInfluence > 0.0 && annulus > 0.0)
                    {
                        float2 maskOffset = WaterRotateInteractionUV(ringOffset, ringC.y);
                        float2 maskUV = maskOffset / max(0.001, 2.0 * ringBounds) + 0.5;
                        float painterlyMask = WaterSampleRingMask(maskUV, ringC, age01);
                        annulus = WaterApplyInteractionMask(annulus, painterlyMask, _RingMaskInfluence);
                    }

                    float fade = (1.0 - age01) * saturate(ringA.w);
                    ringHighlight = saturate(ringHighlight + annulus * reachesFront * fade);
                }

                ringHighlight *= seamBand;
            }

            float wake = EvaluateFrontWake(input.uv.x * _WaterSize.x);
            float wakeSeam = wake * waterlineFoam;
            float boundaryFoam = Water25DBoundaryFoam(
                float2(input.uv.x, 1.0 - input.uv.y),
                _BoundaryFoamWidth,
                _BoundaryFoamSoftness,
                _BoundaryFoamBreakup,
                _Time.y);
            float foamAmount = saturate(
                waterlineFoam * _FoamColor.a * 0.45 +
                contactSeam * 0.75 +
                ringHighlight * 0.55 +
                wakeSeam * 0.38 +
                boundaryFoam * _BoundaryFoamIntensity);
            surfaceRGB = lerp(surfaceRGB, _FoamColor.rgb, foamAmount);

            float fresnel = Water25DFresnel(
                normalWS,
                viewDirectionWS,
                _FresnelStrength,
                _FresnelPower);
            surfaceRGB = lerp(surfaceRGB, _FresnelTint.rgb, fresnel * 0.65);

            if (_StylizedHighlightsEnabled > 0.5)
            {
                float ameyeHighlight = Water25DAmeyeSpecular(
                    normalWS,
                    viewDirectionWS,
                    _HighlightDirection.xyz,
                    _HighlightSoftness,
                    _HighlightBreakup);
                float highlight = ameyeHighlight * saturate(1.0 - _HighlightThreshold);
                surfaceRGB += _HighlightColor.rgb * highlight * _HighlightStrength;
            }

            if (_CausticsEnabled > 0.5 && _CausticTextureValid > 0.5)
            {
                float2 causticUV = frac(localXZ * _CausticScale.xy + _Time.y * _CausticSpeed.xy);
                float caustic = SAMPLE_TEXTURE2D(
                    _CausticTexture,
                    sampler_CausticTexture,
                    causticUV).r;
                float causticFade = pow(saturate(1.0 - depth), max(0.05, _CausticDepthFade));
                surfaceRGB += _CausticTint.rgb * caustic * _CausticIntensity * causticFade;
            }

            if (_FrontDistortionSourceAvailable > 0.5)
            {
                float2 screenUV = input.positionCS.xy / max(_ScaledScreenParams.xy, float2(1.0, 1.0));
                float2 sourceDistortionUV;
                DistortUV_float(
                    screenUV,
                    saturate(_FrontDistortionStrength * 100.0),
                    sourceDistortionUV);
                screenUV = lerp(
                    screenUV + normalWS.xy * _FrontDistortionStrength,
                    sourceDistortionUV,
                    saturate(_FrontDistortionStrength * 40.0));
                half4 sceneColor = SAMPLE_TEXTURE2D(
                    _CameraSortingLayerTexture,
                    sampler_CameraSortingLayerTexture,
                    Water25DClampScreenUV(screenUV));
                surfaceRGB = lerp(surfaceRGB, sceneColor.rgb * _FrontDistortionTint.rgb, 0.35);
            }

            half alpha = saturate(lerp(_FrontSurfaceColor.a, _FrontDeepColor.a, depth) * _FrontOpacity);
            alpha = saturate(alpha + contactSeam * 0.14 + ringHighlight * 0.10 + wakeSeam * 0.08);
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
