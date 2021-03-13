﻿Shader "Custom Render Pipeline/Lit"
{
    Properties
    {
    	_BaseMap("Texture", 2D) = "white" {}
    	_BaseColor("BaseColor", Color) = (1,1,1,1)  
    	_Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    	[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
    	[KeywordEnum(On, Clip, Dither, Off)] _Shadows("Shadows", float) = 0
    	_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5

    	[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0  
    	[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
    	[Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha ("Premultiply Alpha", Float) = 0
    	[Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows ("Receive Shadows", Float) = 1
    }
    SubShader
    {
        Pass
        {
        	Tags { "LightMode" = "CustomLit" }
        	Blend [_SrcBlend] [_DstBlend]
        	ZWrite [_ZWrite] 
            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _RECEIVE_SHADOWS

            #pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_instancing
            #pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
            #pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER

			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
			#pragma shader_feature _PREMULTIPLY_ALPHA
			#include "../ShaderLibrary/LitPass.hlsl"
			ENDHLSL
        }
    	
    	
    	Pass
        {
        	Tags { "LightMode" = "ShadowCaster" }
        	
        	colormask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
            #pragma multi_compile _ _SHADOWS_CLIP _SHADOWS_DITHER
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment 
			#pragma shader_feature _PREMULTIPLY_ALPHA
			#include "../ShaderLibrary/ShadowCasterPass.hlsl"
			ENDHLSL
        }
    }
	
	CustomEditor "CustomShaderGUI"
}


