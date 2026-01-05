Shader "UI/URP_GlassyDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Distortion ("Distortion Strength", Range(0,0.1)) = 0.03
        _Highlight ("Highlight Intensity", Range(0,1)) = 0.3
        _Color ("Tint", Color) = (1,1,1,0.7)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float _Distortion;
            float _Highlight;
            float4 _Color;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 n = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv).xyz * 2 - 1;
                float2 offset = n.xy * _Distortion;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset);

                float highlight = saturate(dot(n, float3(0,0,1)));
                col.rgb += highlight * _Highlight;

                col.rgb *= _Color.rgb;
                col.a *= _Color.a;

                return col;
            }
            ENDHLSL
        }
    }
}
