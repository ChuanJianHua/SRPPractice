#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction) * light.color);
}

float3 GetLighting (Surface surface, BRDF brdf, Light light) {
    return saturate(IncomingLight(surface, light) * light.attenuation)  * DirectBRDF(surface, brdf, light);
}

float3 GetLighting(Surface surface, BRDF brdf, GI gi)
{
    ShadowData shadowData = GetShadowData(surface);
    float3 color = gi.diffuse;
    for(int i = 0; i < GetDirectionalLightCount(); i++)
    {
        color += GetLighting(surface, brdf, GetDirectionLight(i, surface, shadowData));         
    }
    return color;
}

#endif