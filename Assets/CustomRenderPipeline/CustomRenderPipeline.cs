using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

namespace CustomRenderPipeline
{
	public class CustomRenderPipeline : RenderPipeline {

		private CameraRenderer cameraRenderer = new CameraRenderer();
		
		protected override void Render (ScriptableRenderContext renderContext, Camera[] cameras) 
		{
			if (cameras.Length == 0) return;
			BeginFrameRendering(renderContext, cameras);
			
			foreach (var camera in cameras) {
			    BeginCameraRendering(renderContext, camera);
			    cameraRenderer.Render(renderContext, camera);
				EndCameraRendering(renderContext, camera);
			}
			EndFrameRendering(renderContext, cameras);

		}
	}
}

