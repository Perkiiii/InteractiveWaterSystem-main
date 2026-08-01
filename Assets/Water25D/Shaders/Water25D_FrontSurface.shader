Shader "Water25D/Front Surface"
{
    Properties
    {
        _FrontSurfaceColor("Surface Color", Color) = (0.08, 0.38, 0.52, 0.84)
        _FrontDeepColor("Deep Color", Color) = (0.025, 0.1, 0.18, 0.94)
        _FoamColor("Foam Color", Color) = (0.78, 0.95, 1, 0.8)
        _FrontDepth("Front Depth", Float) = 10
        _SurfaceMode("Surface Mode", Float) = 0
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

        CBUFFER_START(UnityPerMaterial)
            half4 _FrontSurfaceColor;
            half4 _FrontDeepColor;
            half4 _FoamColor;
            float _FrontDepth;
            float _SurfaceMode;
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
