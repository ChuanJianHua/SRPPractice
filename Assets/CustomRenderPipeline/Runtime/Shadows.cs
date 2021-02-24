using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector3 = UnityEngine.Vector3;

namespace CustomRenderPipeline
{
    public class Shadows
    {
        private const string bufferName = "Shadows";

        private const int maxShadowedDirectionalLightCount = 4;
        
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

            int split = shadowedDirectionalLightCount <= 1 ? 1 : 2;
            int tileSize = atlasSize / split;
            
            for (int i = 0; i < shadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadows(i, split, tileSize);
            }
            buffer.EndSample(bufferName);
            ExecuteBuffer();
        }

        void RenderDirectionalShadows(int index, int split, int tileSize)
        {
            ShadowedDirectionalLight light = shadowedDirectionalLights[index];
            var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0, 1,
                Vector3.zero, tileSize, 0f,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData);
            shadowSettings.splitData = splitData;
            SetTileViewport(index, split, tileSize);
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
        }

        public void Cleanup()
        {
            buffer.ReleaseTemporaryRT(dirShadowAtlasId);
            ExecuteBuffer();
        }

        void SetTileViewport(int index, int split, int tileSize)
        {
            var offset = new Vector2(index % split, (int)(index / split));
            buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        }
    }
    
    struct ShadowedDirectionalLight {
        public int visibleLightIndex;
    }
}
