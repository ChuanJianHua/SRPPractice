using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public partial class CameraRenderer
    {
        ScriptableRenderContext context;

        Camera camera;
        
        const string bufferName = "Render Camera";

        CommandBuffer buffer = new CommandBuffer {
            name = bufferName
        };
        
        CullingResults cullingResults;

        private static ShaderTagId unLitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        
        public void Render (ScriptableRenderContext context, Camera camera) {
            this.context = context;
            this.camera = camera;
            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull())
                return;
            
            Setup();
            DrawVisibleGeometry();
            DrawUnsupportedShaders();
            DrawGizmos();
            Submit();
        }

        void Setup()
        {
            context.SetupCameraProperties(camera);
            buffer.ClearRenderTarget(true, true, Color.black);
            buffer.BeginSample(bufferName);
            ExecuteBuffer();
        }

        bool Cull()
        {
            if (camera.TryGetCullingParameters(out var scriptableCullingParameters))
            {
                cullingResults = context.Cull(ref scriptableCullingParameters);
                return true;
            }
            return false;
        }

        void DrawVisibleGeometry()
        {
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(
                unLitShaderTagId, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.all);
            context.DrawRenderers(cullingResults,ref drawingSettings, ref filteringSettings);
            context.DrawSkybox(camera);
    
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
    
            context.DrawRenderers(
            	cullingResults, ref drawingSettings, ref filteringSettings
            );
        }
        
        void Submit () {
            buffer.EndSample(bufferName);
            ExecuteBuffer();
            context.Submit();
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
    }
}
