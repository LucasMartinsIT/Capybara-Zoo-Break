Shader "Custom/URP/InvertedHullOutline"
{
    Properties
    {
        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0.18, 0.07, 0.03, 1)
        _OutlineWidth ("Outline Width (Meters)", Range(0.0, 0.1)) = 0.02
        _ZOffset ("Depth Offset (Push Inside Lines Back)", Range(0.0, 0.5)) = 0.15
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry+1" 
        }

        Pass
        {
            Name "OutlinePass"
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
                float _ZOffset;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Extrude vertices outward along their object-space normals
                float3 extrudedPosOS = input.positionOS.xyz + normalize(input.normalOS) * _OutlineWidth;
                float4 clipPos = TransformObjectToHClip(extrudedPosOS);

                // Push the outline slightly back in depth (Z) so internal details (like eyes/snout creases) are hidden behind the body mesh
                #if UNITY_REVERSED_Z
                    clipPos.z -= _ZOffset * 0.1;
                #else
                    clipPos.z += _ZOffset * 0.1;
                #endif

                output.positionCS = clipPos;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}