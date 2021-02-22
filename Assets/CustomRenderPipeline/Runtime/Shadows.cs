using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class Shadows
    {
        private const string bufferName = "Shadows";

        private const int maxShadowedDirectionalLightCount = 1;
        
        private CommandBuffer buffer = new CommandBuffer()
        {
            name = bufferName
        };
        
        ScriptableRenderContext context;

        CullingResults cullingResults;

        ShadowSettings settings;

        private ShadowedDirectionalLight[] shadowedDirectionalLights =
            new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

        private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

        private int shadowedDirectionalLightCount;
        
        public void SetUp(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            this.context = context;
            this.cullingResults = cullingResults;
            this.settings = shadowSettings;

            shadowedDirectionalLightCount = 0;
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
        {
            if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount && 
                light.shadows != LightShadows.None && light.shadowStrength > 0f &&
                cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds bounds))
            {
                shadowedDirectionalLights[shadowedDirectionalLightCount++] = new ShadowedDirectionalLight()
                    {visibleLightIndex = visibleLightIndex};
            }
        }

        public void Render()
        {
            if (shadowedDirectionalLightCount > 0)
            {
                RenderDirectionalShadows();
            }
            else
            {
                buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            }
        }

        void RenderDirectionalShadows()
        {
            int atlasSize = (int) settings.directional.atlasSize;
            buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear,
                RenderTextureFormat.Shadowmap);
            buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            buffer.ClearRenderTarget(true, false, Color.clear);
            buffer.BeginSample(bufferName);
            ExecuteBuffer();

            for (int i = 0; i < shadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadows(i, atlasSize);
            }
        }

        void RenderDirectionalShadows(int index, int atlasSize)
        {
            ShadowedDirectionalLight light = shadowedDirectionalLights[index];
            var shadowSetting = new ShadowDrawingSettings()
                {cullingResults = cullingResults, lightIndex = light.visibleLightIndex};
            
        }

        public void Cleanup()
        {
            buffer.ReleaseTemporaryRT(dirShadowAtlasId);
            ExecuteBuffer();
        }
    }
    
    struct ShadowedDirectionalLight {
        public int visibleLightIndex;
    }
}
