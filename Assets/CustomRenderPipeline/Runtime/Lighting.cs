using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class Lighting
    {
        const int maxDirLightCount = 4;
        
        private const string bufferName = "Lighting";

        private CommandBuffer buffer = new CommandBuffer()
        {
            name = bufferName
        };

        private static int
            dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
            dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
            dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections"),
            dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");


        private static Vector4[]
            dirLightColors = new Vector4[maxDirLightCount],
            dirLightDirections = new Vector4[maxDirLightCount],
            dirLightShadowData = new Vector4[maxDirLightCount];
        
        private Shadows shadows = new Shadows();

        public void SetUp(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            buffer.BeginSample(bufferName);
            shadows.SetUp(context, cullingResults, shadowSettings);
            SetupLight(cullingResults);
            shadows.Render();
            buffer.EndSample(bufferName);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        void SetupLight(CullingResults cullingResults)
        {
            NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
            int dirLightCount = 0;
            for (int i = 0; i < visibleLights.Length; i++) {
                VisibleLight visibleLight = visibleLights[i];
                if (visibleLight.light.type == LightType.Directional)
                {
                    SetupDirectionalLight(dirLightCount++, ref visibleLight);
                    if (dirLightCount >= maxDirLightCount) break;
                }
            }

            buffer.SetGlobalInt(dirLightCountId, dirLightCount);
            buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
            buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
            buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
        }

        void SetupDirectionalLight(int index, ref VisibleLight visibleLights)
        {
            dirLightColors[index] = visibleLights.finalColor;
            dirLightDirections[index] = -visibleLights.localToWorldMatrix.GetColumn(2);
            dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLights.light, index);
        }

        public void Cleanup()
        {
            shadows.Cleanup();  
        }
    }
}
