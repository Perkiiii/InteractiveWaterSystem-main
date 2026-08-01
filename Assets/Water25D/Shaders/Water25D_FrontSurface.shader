Shader "Water25D/Front Surface"
{
    Properties
    {
        _FrontSurfaceColor("Surface Color", Color) = (0.08, 0.38, 0.52, 0.84)
        _FrontDeepColor("Deep Color", Color) = (0.025, 0.1, 0.18, 0.94)
        _FoamColor("Foam Color", Color) = (0.78, 0.95, 1, 0.8)
        _FrontDepth("Front Depth", Float) = 10
        _SurfaceMode("Surface Mode", Float) = 0
        _WaterSize("Water Size", Vector) = (20, 6.5, 0, 0)
        _WaterRingCount("Surface Ring Count", Float) = 0
        _WaveAmplitude("Ambient Wave Amplitude", Float) = 0.06
        _WaveLength("Ambient Wave Length", Float) = 3.5
        _WaveSpeed("Ambient Wave Speed", Float) = 0.8
        _WaveDirection("Ambient Wave Direction", Vector) = (1, 0.15, 0, 0)
        _WaveBands("Ambient Wave Bands", Range(1, 4)) = 3
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

        #define WATER_MAX_RINGS 16

        CBUFFER_START(UnityPerMaterial)
            half4 _FrontSurfaceColor;
            half4 _FrontDeepColor;
            half4 _FoamColor;
            float _FrontDepth;
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
                positionOS.y += ambient * input.uv.y;
            }

            output.positionCS = GetVertexPositionInputs(positionOS).positionCS;
            output.uv = input.uv;
            return output;
        }

        half4 Frag(Varyings input) : SV_Target
        {
            half depth = saturate(1.0 - input.uv.y);
            half4 color = lerp(_FrontSurfaceColor, _FrontDeepColor, depth);
            half surfaceFoam = (1.0 - smoothstep(0.0, 0.08, depth)) * _FoamColor.a;
            color.rgb = lerp(color.rgb, _FoamColor.rgb, surfaceFoam * 0.45);

            if (_SurfaceMode > 0.5 && _WaterRingCount > 0.5)
            {
                float localX = input.uv.x * _WaterSize.x;
                float2 localXZ = float2(localX, 0.0);
                int ringCount = min((int)_WaterRingCount, WATER_MAX_RINGS);
                half ringHighlight = 0.0;
                half seamBand = 1.0 - smoothstep(0.0, 0.10, depth);
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
                    float thickness = max(0.0001, ringB.z);
                    float softness = max(0.0001, ringB.w);
                    float distanceToFrontPlane = abs(ringA.y);
                    float reachesFront = smoothstep(0.0, thickness + softness, radius - distanceToFrontPlane);
                    float distanceFromRing = abs(distance(localXZ, ringA.xy) - radius);
                    float annulus = 1.0 - smoothstep(thickness, thickness + softness, distanceFromRing);
                    float fade = (1.0 - age01) * saturate(ringA.w);
                    ringHighlight = saturate(ringHighlight + annulus * reachesFront * fade);
                }

                ringHighlight *= seamBand;
                color.rgb = lerp(color.rgb, _FoamColor.rgb, ringHighlight * 0.55);
                color.a = saturate(color.a + ringHighlight * 0.10);
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
