Shader "Custom/SpriteSheetBillboard"
{
    Properties
    {
        _MainTex ("Sprite Sheet", 2D) = "white" {}
        _Columns ("Columns", Float) = 4
        _Rows ("Rows", Float) = 4
        _CurrentFrame ("Current Frame", Float) = 0
        _HitEffect ("Hit Effect", Range(0, 1)) = 0
        [Toggle] _AlphaClip ("Alpha Clip", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma shader_feature _ALPHACLIP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Columns;
                float _Rows;
                float _CurrentFrame;
                float _HitEffect;
                float _AlphaClip;
                float _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = vertexInput.positionCS;
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float totalFrames = _Columns * _Rows;
                float frame = fmod(_CurrentFrame, totalFrames);
                
                float column = fmod(frame, _Columns);
                float row = floor(frame / _Columns);
                
                float2 uvOffset;
                uvOffset.x = column / _Columns;
                uvOffset.y = 1.0 - (row + 1.0) / _Rows;
                
                float2 uvScale = float2(1.0 / _Columns, 1.0 / _Rows);
                float2 finalUV = input.uv * uvScale + uvOffset;
                
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV);
                
                #ifdef _ALPHACLIP_ON
                    clip(color.a - _Cutoff);
                #endif
                
                color.rgb = lerp(color.rgb, half3(1, 1, 1), _HitEffect);
                
                color.rgb = MixFog(color.rgb, input.fogFactor);
                
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
