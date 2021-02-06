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
        private static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");

        private const int MaxLightCount = 4;

        private Vector4[] visibleLightColors = new Vector4[MaxLightCount];
        
        public void Render (ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing) {
            this.context = context;
            this.camera = camera;
            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull())
                return;
            
            Setup();
            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
            DrawUnsupportedShaders();
            DrawGizmos();
            Submit();
        }

        void Setup()
        {
            context.SetupCameraProperties(camera);
            var flags = camera.clearFlags;
            buffer.ClearRenderTarget(
        flags <= CameraClearFlags.Depth, 
        flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
            buffer.BeginSample(SampleName);
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

        void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
        {
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(
                unLitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing
            };
            drawingSettings.SetShaderPassName(1, litShaderTagId);
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
            buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        void Light()
        {
            
        }
    }
}
