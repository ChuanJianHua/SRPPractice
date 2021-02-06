#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#define MAX_VISIBLE_LIGHTS 4

#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct VertexInput {
    float4 pos : POSITION;
    float2 baseUV : TEXCOORD0;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
    float4 clipPos : SV_POSITION;
    float2 baseUV : TEXCOORD1;
    float3 normal : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float3 DiffuseLight(int index, float3 normal)
{
    float3 lightColor = _VisibleLightColors[index].rgb;
    float3 lightDirection = _VisibleLightDirections[index].xyz;
    float3 diffuse = saturate(dot(normal, lightDirection));
    return lightColor * diffuse;
}

VertexOutput LitPassVertex(VertexInput input)
{
    VertexOutput output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 worldPos = TransformObjectToWorld(input.pos.xyz);
    output.clipPos = TransformWorldToHClip(worldPos);
    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    output.baseUV = input.baseUV * baseST.xy +  baseST.zw; 
    output.normal = TransformObjectToWorldNormal(input.normal);
    return output;                   
}

float4 LitPassFragment(VertexOutput input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 albedo = baseMap * baseColor;

    Surface surface;
    surface.normal = normalize(input.normal);
    surface.color = albedo.rgb;
    surface.alpha = albedo.a;
    input.normal = normalize(input.normal);
    float4 diffuseLight = 0;
    for (int i = 0; i < MAX_VISIBLE_LIGHTS; i++) {
        diffuseLight += float4(DiffuseLight(i, input.normal),1);
    }
    float4 color = albedo * diffuseLight;
    #if defined(_CLIPPING)
    clip(color.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    #endif
    return float4(color);
}



#endif // CUSTOM_LIT_PASS_INCLUDED