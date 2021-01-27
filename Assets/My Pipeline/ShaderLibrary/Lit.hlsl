#ifndef MYRP_LIT_INCLUDED
#define MYRP_LIT_INCLUDED

#define MAX_VISIBLE_LIGHTS 4

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

CBUFFER_START(_LightBuffer)
    float4 _VisibleLightColors [MAX_VISIBLE_LIGHTS];
    float4 _VisibleLightDirections [MAX_VISIBLE_LIGHTS];                            
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(PerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

#define UNITY_MATRIX_M unity_ObjectToWorld

struct VertexInput {
    float4 pos : POSITION;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
    float4 clipPos : SV_POSITION;
    float3 normal : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput LitPassVertex(VertexInput input)
{
    VertexOutput output;
    UNITY_SETUP_INSTANCE_ID(vertex_input);
    UNITY_TRANSFER_INSTANCE_ID(vertex_input, output);
    float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
    output.normal = mul((float3x3)UNITY_MATRIX_M, input.normal);
    output.clipPos = mul(unity_MatrixVP,  worldPos);
    return output;                   
}
float3 DiffuseLight(int index , float3 normal)
{
    float3 lightColor = _VisibleLightColors[index].rgb;
    float3 lightDirection = _VisibleLightDirections[index].xyz;
    float3 diffuse = saturate(dot(normal, lightDirection));
    return lightColor * diffuse;
}

float4 LitPassFragment(VertexOutput input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(vertex_output);
    input.normal = normalize(input.normal);
    float3 albedo = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color).rgb;
    float3 diffuseLight = 0;
    for (int i = 0; i < MAX_VISIBLE_LIGHTS; i++) {
        diffuseLight += DiffuseLight(i, input.normal);
    }
    float3 color = albedo * diffuseLight;
    return float4(color, 1);
}



#endif // MYRP_LIT_INCLUDED