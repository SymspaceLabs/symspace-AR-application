Shader "UI/BlurImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _BlurSize;

                fixed4 col = tex2D(_MainTex, i.uv) * 0.36;
                col += tex2D(_MainTex, i.uv + texel) * 0.16;
                col += tex2D(_MainTex, i.uv - texel) * 0.16;
                col += tex2D(_MainTex, i.uv + texel * 2) * 0.16;
                col += tex2D(_MainTex, i.uv - texel * 2) * 0.16;

                return col;
            }
            ENDCG
        }
    }
}
