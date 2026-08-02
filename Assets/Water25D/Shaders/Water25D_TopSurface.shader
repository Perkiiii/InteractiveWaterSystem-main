Shader "Water25D/Top Surface"
{
    Properties
    {
        _BaseColor("Top Color", Color) = (0.2, 0.48, 0.6, 0.92)
        _FoamColor("Foam Color", Color) = (0.78, 0.95, 1, 0.8)
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

        TEXTURE2D(_RippleTexture);
        SAMPLER(sampler_RippleTexture);
        TEXTURE2D(_ReflectionTexture);
        SAMPLER(sampler_ReflectionTexture);

        #define WATER_MAX_RINGS 16
        #define WATER_MAX_CONTACT_FOAMS 8
        #define WATER_MAX_WAKES 16

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _FoamColor;
            float _RippleEnabled;
            float _RippleAmplitude;
            float _SurfaceMode;
            float4 _WaterSize;
            float _WaterRingCount;
            float4 _WaterRingsA[WATER_MAX_RINGS];
            float4 _WaterRingsB[WATER_MAX_RINGS];
            float _WaterFoamCount;
            float4 _WaterFoamsA[WATER_MAX_CONTACT_FOAMS];
            float4 _WaterFoamsB[WATER_MAX_CONTACT_FOAMS];
            float _WaterFoamSoftness;
            float _FoamReflectionOcclusion;
            float _WaterWakeCount;
            float4 _WaterWakesA[WATER_MAX_WAKES];
            float4 _WaterWakesB[WATER_MAX_WAKES];
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
        CBUFFER_END

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
                float2 halfSize = max(float2(0.001, 0.001), float2(foamA.z, foamB.x));
                float2 ellipseOffset = (localXZ - foamA.xy) / halfSize;
                float radialDistance = length(ellipseOffset);
                float softness = max(0.001, _WaterFoamSoftness / min(halfSize.x, halfSize.y));
                float ellipse = 1.0 - smoothstep(1.0, 1.0 + softness, radialDistance);
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
            half contactFoam = EvaluateContactFoam(localXZ);
            half edgeDistance = min(min(input.uv.x, 1.0 - input.uv.x), min(input.uv.y, 1.0 - input.uv.y));
            half edgeFoam = 1.0 - smoothstep(0.0, 0.035, edgeDistance);
            half4 color = _BaseColor;
            color.rgb = lerp(color.rgb, _FoamColor.rgb, edgeFoam * _FoamColor.a);
            color.a = saturate(lerp(color.a, 1.0, edgeFoam * 0.35));
            if (_ReflectionFallback > 0.5)
            {
                half fallback = saturate(0.32 + input.uv.y * 0.48);
                half reflectionAmount = saturate(_ReflectionStrength) * (1.0 - contactFoam * saturate(_FoamReflectionOcclusion));
                color.rgb = lerp(color.rgb, _FoamColor.rgb, fallback * reflectionAmount * 0.35);
            }
            if (_ReflectionEnabled > 0.5)
            {
                float4 reflectionClip = mul(_ReflectionViewProjection, float4(input.worldPos, 1.0));
                float2 reflectionUV = reflectionClip.xy / max(reflectionClip.w, 0.0001) * 0.5 + 0.5;
                reflectionUV.y = 1.0 - reflectionUV.y;
                half4 reflection = SAMPLE_TEXTURE2D(_ReflectionTexture, sampler_ReflectionTexture, reflectionUV);
                half reflectionAmount = saturate(_ReflectionStrength) *
                    (1.0 - contactFoam * saturate(_FoamReflectionOcclusion));
                color.rgb = lerp(color.rgb, reflection.rgb, reflectionAmount * reflection.a);
            }

            if (_SurfaceMode > 0.5 && _WaterRingCount > 0.5)
            {
                int ringCount = min((int)_WaterRingCount, WATER_MAX_RINGS);
                half ringHighlight = 0.0;
                for (int ringIndex = 0; ringIndex < WATER_MAX_RINGS; ringIndex++)
                {
                    if (ringIndex >= ringCount)
                    {
                        break;
                    }

                    float4 ringA = _WaterRingsA[ringIndex];
                    float4 ringB = _WaterRingsB[ringIndex];
                    float age01 = saturate(ringA.z);
                    float radius = lerp(ringB.x, ringB.y, age01);
                    float distanceFromRing = abs(distance(localXZ, ringA.xy) - radius);
                    float thickness = max(0.0001, ringB.z);
                    float softness = max(0.0001, ringB.w);
                    float annulus = 1.0 - smoothstep(thickness, thickness + softness, distanceFromRing);
                    float fade = (1.0 - age01) * saturate(ringA.w);
                    ringHighlight = saturate(ringHighlight + annulus * fade);
                }

                color.rgb = lerp(color.rgb, _FoamColor.rgb, ringHighlight * 0.55);
                color.a = saturate(color.a + ringHighlight * 0.12);
            }

            half wake = EvaluateWakeCapsules(localXZ);
            color.rgb = lerp(color.rgb, _FoamColor.rgb, wake * 0.38);
            color.a = saturate(color.a + wake * 0.08);

            color.rgb = lerp(color.rgb, _FoamColor.rgb, contactFoam * 0.75);
            color.a = saturate(color.a + contactFoam * 0.18);
            return color;
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
