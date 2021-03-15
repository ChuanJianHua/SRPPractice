#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#define MAX_VISIBLE_LIGHTS 4

#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/GI.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

struct VertexInput {
    float3 positionCS : POSITION;
    float2 baseUV : TEXCOORD0;
    float3 normal : NORMAL;
    GI_ATTRIBUTE_DATA                               
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
    float4 positionCS : SV_POSITION;
    float3 positionWS : VAR_POSITION;    
    float2 baseUV : TEXCOORD1;
    float3 normal : TEXCOORD2;
    GI_VARYINGS_DATA    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


VertexOutput LitPassVertex(VertexInput input)
{
    VertexOutput output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    TRANSFER_GI_DATA(input, output);
    float3 worldPos = TransformObjectToWorld(input.positionCS);
    output.positionCS = TransformWorldToHClip(worldPos);
    output.baseUV = TransformBaseUV(input.baseUV); 
    output.normal = TransformObjectToWorldNormal(input.normal);
    output.positionWS = TransformObjectToWorld(input.positionCS.xyz); 
    return output;                   
}

float4 LitPassFragment(VertexOutput input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 base = GetBase(input.baseUV);
    #if defined(_CLIPPING)
    clip(base.a - GetCutoff(input.baseUV));
    #endif                                   
    Surface surface;
    surface.position = input.positionWS;
    surface.normal = normalize(input.normal);
    surface.color = base.rgb;
    surface.alpha = base.a;
    surface.metallic = GetMetallic(input.baseUV);
    surface.smoothness = GetSmoothness(input.baseUV);
    surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    surface.depth = -TransformWorldToView(input.positionWS).z;
    surface.dither = InterleavedGradientNoise(input.positionCS.xy, 0);
    #if defined(_PREMULTIPLY_ALPHA)
        BRDF brdf = GetBRDF(surface, true);
    #else
        BRDF brdf = GetBRDF(surface);
    #endif
    GI gi = GetGI(GI_FRAGMENT_DATA(input), surface);
    float3 color = GetLighting(surface, brdf, gi);
    return float4(color, surface.alpha);
}



#endif // CUSTOM_LIT_PASS_INCLUDED