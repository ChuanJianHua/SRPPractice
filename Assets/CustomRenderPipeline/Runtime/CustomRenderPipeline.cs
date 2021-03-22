using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

namespace CustomRenderPipeline
{
	public class CustomRenderPipeline : RenderPipeline {

		private CameraRenderer cameraRenderer = new CameraRenderer();
		bool useDynamicBatching, useGPUInstancing;
		private ShadowSettings _shadowSettings;
		public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings)
		{
			this.useDynamicBatching = useDynamicBatching;
			this.useGPUInstancing = useGPUInstancing;
			this._shadowSettings = shadowSettings;
			GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
			GraphicsSettings.lightsUseLinearIntensity = true;
		}
		
		protected override void Render (ScriptableRenderContext renderContext, Camera[] cameras) 
		{
			// if (cameras.Length == 0) return;
			// BeginFrameRendering(renderContext, cameras);
			
			foreach (var camera in cameras) {
			    // BeginCameraRendering(renderContext, camera);
			    cameraRenderer.Render(renderContext, camera, useDynamicBatching, useGPUInstancing, _shadowSettings);
				// EndCameraRendering(renderContext, camera);
			}
			// EndFrameRendering(renderContext, cameras);
		}
	}
}

