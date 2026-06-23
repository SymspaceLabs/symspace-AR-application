Shader "Custom/URP/InvisibleDepthOnlyExpanded"
{
    Properties
    {
        _Expand ("Expand Amount", Float) = 0.005
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry-1"
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            // Property from material
            float _Expand;

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Expand vertex position along its normal
                float3 expandedPos = input.positionOS.xyz + input.normalOS * _Expand;

                output.positionHCS = TransformObjectToHClip(float4(expandedPos, 1.0));
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                return float4(0, 0, 0, 0); // Invisible
            }
            ENDHLSL
        }
    }

    FallBack Off
}