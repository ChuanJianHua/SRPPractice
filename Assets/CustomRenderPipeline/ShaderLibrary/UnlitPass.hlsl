﻿#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

struct Attribute {
    float4 pos : POSITION;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
    float4 clipPos : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attribute input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 worldPos = TransformObjectToWorld(input.pos.xyz);
    output.clipPos = TransformWorldToHClip(worldPos);
    output.baseUV = TransformBaseUV(input.baseUV);
    return output;                   
}

float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
      float4 color = GetBase(input.baseUV);
    #if defined(_CLIPPING)
        clip(color.a - GetCutoff(input.baseUV));
    #endif

    return color;    
}

#endif // CUSTOM_UNLIT_PASS_INCLUDED                                    