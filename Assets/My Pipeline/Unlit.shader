Shader "My Pipeline/Unlit"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
            #ifndef MYRP_UNLIT_INCLUDE
            #define MYRP_UNLIT_INCLUDE
            #endif

            struct VertexInput
            {
                float4 pos : POSITION;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
            };
            ENDHLSL
        }
    }
}
