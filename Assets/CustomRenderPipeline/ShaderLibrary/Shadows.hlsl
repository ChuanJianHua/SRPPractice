#ifndef CUSTOM_SHADOWS_INCLUDE
#define CUSTOM_SHADOWS_INCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#if defined(_DIRECTIONAL_PCF3)
    #define DIRECTIONAL_FILTER_SAMPLES 4
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_DIRECTIONAL_PCF5)
    #define DIRECTIONAL_FILTER_SAMPLES 9
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_DIRECTIONAL_PCF7)
    #define DIRECTIONAL_FILTER_SAMPLES 16
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif
   
#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4 _CascadeData[MAX_CASCADE_COUNT];
    float4 _ShadowAtlasSize;
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    float4 _ShadowDistanceFade;
CBUFFER_END     

struct DirectionalShadowData
{
    float strength;
    int tileIndex;
    float normalBias;
};

struct ShadowData
{
    int cascadeIndex;
    float strength;
};

float FadedShadowStrength(float distance, float scale, float fade)
{
    return saturate((1.0 - distance * scale) * fade);
}                                                                

ShadowData GetShadowData(Surface surfaceWS)
{
    ShadowData data;
    data.strength = FadedShadowStrength(surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);
    data.cascadeIndex = 0;
    int i = 0;
    for (i = 0; i < _CascadeCount ; i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSquare = DistanceSquare(sphere.xyz, surfaceWS.position);
        if (distanceSquare < sphere.w)
        {
            if (i == _CascadeCount - 1)
            {
                data.strength *= FadedShadowStrength(distanceSquare, _CascadeData[i].x, _ShadowDistanceFade.z);                
            }
            break;   
        }
    }
    if (i == _CascadeCount)
    {
        data.strength = 0.0;
    }
    data.cascadeIndex = i;
    return data;
}

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(
        _DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation(DirectionalShadowData directional, ShadowData global, Surface surfaceWS)
{
    if (directional.strength <= 0.0)
    {
        return 1.0;
    }
    float3 normalBias = surfaceWS.normal * (directional.normalBias * _CascadeData[global.cascadeIndex].y);   
    float3 positionSTS = mul(_DirectionalShadowMatrices[directional.tileIndex], float4(surfaceWS.position + normalBias, 1.0)).xyz;
    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0, shadow, directional.strength);
}
#endif