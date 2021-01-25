#ifndef MYRP_UNLIT_INCLUDED
#define MYRP_UNLIT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(PerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

#define UNITY_MATRIX_M unity_ObjectToWorld

struct VertexInput {
    float4 pos : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
    float4 clipPos : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput UnlitPassVertex(VertexInput vertex_input)
{
    VertexOutput output;
    UNITY_SETUP_INSTANCE_ID(vertex_input);
    UNITY_TRANSFER_INSTANCE_ID(vertex_input, output);
    float4 worldPos = mul(UNITY_MATRIX_M, float4(vertex_input.pos.xyz, 1.0));
    output.clipPos = mul(unity_MatrixVP,  worldPos);
    return output;                   
}

float4 UnlitPassFragment(VertexOutput vertex_output) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(vertex_output);
    return UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);
}

#endif // MYRP_UNLIT_INCLUDED