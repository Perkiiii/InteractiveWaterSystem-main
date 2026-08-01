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
            half edgeDistance = min(min(input.uv.x, 1.0 - input.uv.x), min(input.uv.y, 1.0 - input.uv.y));
            half edgeFoam = 1.0 - smoothstep(0.0, 0.035, edgeDistance);
            half4 color = _BaseColor;
            color.rgb = lerp(color.rgb, _FoamColor.rgb, edgeFoam * _FoamColor.a);
            color.a = saturate(lerp(color.a, 1.0, edgeFoam * 0.35));
            if (_ReflectionFallback > 0.5)
            {
                half fallback = saturate(0.32 + input.uv.y * 0.48);
                color.rgb = lerp(color.rgb, _FoamColor.rgb, fallback * _ReflectionStrength * 0.35);
            }
            if (_ReflectionEnabled > 0.5)
            {
                float4 reflectionClip = mul(_ReflectionViewProjection, float4(input.worldPos, 1.0));
                float2 reflectionUV = reflectionClip.xy / max(reflectionClip.w, 0.0001) * 0.5 + 0.5;
                reflectionUV.y = 1.0 - reflectionUV.y;
                half4 reflection = SAMPLE_TEXTURE2D(_ReflectionTexture, sampler_ReflectionTexture, reflectionUV);
                color.rgb = lerp(color.rgb, reflection.rgb, saturate(_ReflectionStrength) * reflection.a);
            }

            if (_SurfaceMode > 0.5 && _WaterRingCount > 0.5)
            {
                float2 localXZ = input.uv * _WaterSize.xy;
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
