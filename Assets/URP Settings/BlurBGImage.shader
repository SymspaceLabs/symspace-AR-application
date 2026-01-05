Shader "UI/URP_FrostedGlass"
{
    Properties
    {
        _Blur ("Blur Strength", Range(0,5)) = 1.5
        _Tint ("Glass Tint", Color) = (1,1,1,0.75)
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

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            float4 _CameraOpaqueTexture_TexelSize;

            float _Blur;
            float4 _Tint;

            struct Attributes
            {
                float4 positionOS : POSITION;
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
                o.uv = o.positionHCS.xy / o.positionHCS.w;
                o.uv = o.uv * 0.5 + 0.5;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 texel = _CameraOpaqueTexture_TexelSize.xy * _Blur;

                half4 col = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.uv) * 0.4;
                col += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.uv + texel) * 0.15;
                col += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.uv - texel) * 0.15;
                col += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.uv + texel.yx) * 0.15;
                col += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.uv - texel.yx) * 0.15;

                col.rgb = lerp(col.rgb, _Tint.rgb, 0.4);
                col.a = _Tint.a;

                return col;
            }
            ENDHLSL
        }
    }
}