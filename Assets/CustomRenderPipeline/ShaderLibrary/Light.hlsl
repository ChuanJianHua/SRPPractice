#ifndef CUSTOM_LIGHT_INCLUDE
#define CUSTOM_LIGHT_INCLUDE

CBUFFER_START(_CustomLight)
    float4 _VisibleLightColors [MAX_VISIBLE_LIGHTS];
    float4 _VisibleLightDirections [MAX_VISIBLE_LIGHTS];                            
CBUFFER_END

struct Light
{
    float color;
    float3 direction;
};

Light GetDirectionLight()
{
    Light light;
    light.color = 1.0;
    light.direction = float3(0.0, 1.0, 0.0); 
    
    return light;
}

#endif