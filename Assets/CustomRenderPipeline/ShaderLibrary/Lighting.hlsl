#ifndef CUSTOM_LIGHTING_INCLUDE
#define CUSTOM_LIGHTING_INCLUDE

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction) * light.color);
}

float3 GetLighting (Surface surface, Light light) {
    return IncomingLight(surface, light) * surface.color;
}

float3 GetLighting(Surface surface)
{
    GetLighting(surface, GetDirectionLight());         
}

#endif