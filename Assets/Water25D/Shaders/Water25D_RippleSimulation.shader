Shader "Water25D/Ripple Simulation"
{
    Properties
    {
        _SpreadX("Propagation X", Range(0.0, 0.25)) = 0.08
        _SpreadZ("Propagation Z", Range(0.0, 0.25)) = 0.08
        _Damping("Damping", Range(0.0, 1.0)) = 0.98
        _ImpactHeight("Impact Height", Range(0.0, 1.0)) = 0.2
        _ImpactCenter("Impact Center", Vector) = (0.5, 0.5, 0, 0)
        _ImpactRadius("Impact Radius", Vector) = (0.01, 0.01, 0, 0)
    }

    CGINCLUDE
    #include "UnityCustomRenderTexture.cginc"

    half _SpreadX;
    half _SpreadZ;
    half _Damping;
    half _ImpactHeight;
    float4 _ImpactCenter;
    float4 _ImpactRadius;

    float4 frag_propagation(v2f_customrendertexture i) : SV_Target
    {
        float2 uv = i.globalTexcoord;
        float2 texel = float2(1.0 / _CustomRenderTextureWidth, 1.0 / _CustomRenderTextureHeight);
        float currentHeight = tex2D(_SelfTexture2D, uv).r;
        float previousHeight = tex2D(_SelfTexture2D, uv).g;
        float laplacianX = tex2D(_SelfTexture2D, uv - float2(texel.x, 0.0)).r +
                           tex2D(_SelfTexture2D, uv + float2(texel.x, 0.0)).r -
                           2.0 * currentHeight;
        float laplacianZ = tex2D(_SelfTexture2D, uv - float2(0.0, texel.y)).r +
                           tex2D(_SelfTexture2D, uv + float2(0.0, texel.y)).r -
                           2.0 * currentHeight;
        float nextHeight = (2.0 * currentHeight - previousHeight + _SpreadX * laplacianX + _SpreadZ * laplacianZ) * _Damping;
        return float4(nextHeight, currentHeight, 0.0, 0.0);
    }

    float4 frag_impact(v2f_customrendertexture i, float sign) : SV_Target
    {
        float2 radius = max(_ImpactRadius.xy, float2(0.00001, 0.00001));
        float2 offset = (i.globalTexcoord - _ImpactCenter.xy) / radius;
        float falloff = saturate(1.0 - dot(offset, offset));
        return float4(sign * _ImpactHeight * falloff, 0.0, 0.0, 0.0);
    }

    float4 frag_up_impact(v2f_customrendertexture i) : SV_Target
    {
        return frag_impact(i, 1.0);
    }

    float4 frag_down_impact(v2f_customrendertexture i) : SV_Target
    {
        return frag_impact(i, -1.0);
    }
    ENDCG

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "Propagation"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag_propagation
            ENDCG
        }

        Pass
        {
            Name "Positive Impact"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag_up_impact
            ENDCG
        }

        Pass
        {
            Name "Negative Impact"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag_down_impact
            ENDCG
        }
    }
}
