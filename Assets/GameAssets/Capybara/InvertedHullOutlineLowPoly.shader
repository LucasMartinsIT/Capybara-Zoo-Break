Shader "Custom/URP/InvertedHullOutlineLowPoly"
{
    Properties
    {
        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0.18, 0.07, 0.03, 1)
        _OutlineWidth ("Outline Width (Meters)", Range(0.0, 0.1)) = 0.02
        _ZOffset ("Depth Offset (Push Inside Lines Back)", Range(0.0, 0.5)) = 0.15

        [Header(Low Poly Hard Edge Fix)]
        _HardEdgeBlend ("Hard Edge Fix Blend (0=Normal, 1=Radial)", Range(0.0, 1.0)) = 0.75
        _CenterOffset ("Pivot Center Offset (XYZ)", Vector) = (0, 0.6, 0, 0)
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
                float _HardEdgeBlend;
                float4 _CenterOffset;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Calculate radial direction from the visual center of the mesh to prevent split gaps on sharp edges
                float3 radialDir = normalize(input.positionOS.xyz - _CenterOffset.xyz);
                float3 normalDir = normalize(input.normalOS);

                // Blend between standard normals and radial direction to close gaps on low-poly corners
                float3 extrudeDir = normalize(lerp(normalDir, radialDir, _HardEdgeBlend));

                // Extrude vertices outward
                float3 extrudedPosOS = input.positionOS.xyz + extrudeDir * _OutlineWidth;
                float4 clipPos = TransformObjectToHClip(extrudedPosOS);

                // Push the outline slightly back in depth (Z) so internal details are clean
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