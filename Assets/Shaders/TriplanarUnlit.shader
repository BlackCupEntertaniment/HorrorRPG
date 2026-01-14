Shader "Custom/TriplanarUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tiling ("Tiling", Float) = 1.0
        _BlendSharpness ("Blend Sharpness", Range(1, 10)) = 5.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Tiling;
                float _BlendSharpness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInputs.normalWS;
                
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 blendWeights = abs(input.normalWS);
                blendWeights = pow(blendWeights, _BlendSharpness);
                blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);

                float3 worldPos = input.positionWS * _Tiling;

                half4 xProjection = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, worldPos.zy);
                half4 yProjection = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, worldPos.xz);
                half4 zProjection = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, worldPos.xy);

                half4 color = xProjection * blendWeights.x + 
                             yProjection * blendWeights.y + 
                             zProjection * blendWeights.z;

                color.rgb = MixFog(color.rgb, input.fogFactor);
                
                return color;
            }
            ENDHLSL
        }
    }
}
