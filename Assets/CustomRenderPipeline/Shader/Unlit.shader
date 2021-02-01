Shader "Custom Render Pipeline/Unlit"
{
    Properties
    {
    	_BaseColor("BaseColor", Color) = (1,1,1,1)    
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
			#pragma multi_compile_instancing
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			
			#include "../ShaderLibrary/UnlitPass.hlsl"
			ENDHLSL
        }
    }
}
