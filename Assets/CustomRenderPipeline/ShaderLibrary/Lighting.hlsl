#ifndef CUSTOM_LIGHTING_INCLUDE
#define CUSTOM_LIGHTING_INCLUDE

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction) * light.color);
}

float3 GetLighting (Surface surface, BRDF brdf, Light light) {
    return saturate(IncomingLight(surface, light) * light.attenuation)  * DirectBRDF(surface, brdf, light);
}

float3 GetLighting(Surface surface, BRDF brdf)
{
    ShadowData shadowData = GetShadowData(surface);
    float3 color = 0;
    for(int i = 0; i < GetDirectionalLightCount(); i++)
    {
        color += GetLighting(surface, brdf, GetDirectionLight(i, surface, shadowData));         
    }
    return color;
}

#endif