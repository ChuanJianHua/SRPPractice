using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

namespace CustomRenderPipeline
{
	public class CustomRenderPipeline : RenderPipeline {

		private CameraRenderer cameraRenderer = new CameraRenderer();
		bool useDynamicBatching, useGPUInstancing;
		public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
		{
			this.useDynamicBatching = useDynamicBatching;
			this.useGPUInstancing = useGPUInstancing;
			GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
			GraphicsSettings.lightsUseLinearIntensity = true;
		}
		
		protected override void Render (ScriptableRenderContext renderContext, Camera[] cameras) 
		{
			if (cameras.Length == 0) return;
			BeginFrameRendering(renderContext, cameras);
			
			foreach (var camera in cameras) {
			    BeginCameraRendering(renderContext, camera);
			    cameraRenderer.Render(renderContext, camera, useDynamicBatching, useGPUInstancing);
				EndCameraRendering(renderContext, camera);
			}
			EndFrameRendering(renderContext, cameras);
		}
	}
}

