using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline {

	
	CullingResults cull;

	Material errorMaterial;

	CommandBuffer cameraBuffer = new CommandBuffer {
		name = "Render Camera"
	};

	protected override void Render (ScriptableRenderContext renderContext, Camera[] cameras) 
	{
		foreach (var camera in cameras) {
			Render(renderContext, camera);
		}
	}

	void Render (ScriptableRenderContext context, Camera camera) {
		ScriptableCullingParameters cullingParameters;
		if (!camera.TryGetCullingParameters(out cullingParameters)) {
			return;
		}

#if UNITY_EDITOR
		if (camera.cameraType == CameraType.SceneView) {
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
		}
#endif

		cull = context.Cull(ref cullingParameters);

		context.SetupCameraProperties(camera);

		CameraClearFlags clearFlags = camera.clearFlags;
		cameraBuffer.ClearRenderTarget(
			(clearFlags & CameraClearFlags.Depth) != 0,
			(clearFlags & CameraClearFlags.Color) != 0,
			camera.backgroundColor
		);
		//cameraBuffer.ClearRenderTarget(true, false, Color.clear);
		cameraBuffer.BeginSample("Render Camera");
		context.ExecuteCommandBuffer(cameraBuffer);
		cameraBuffer.Clear();

		var sorting = new SortingSettings(camera);
		sorting.criteria = SortingCriteria.CommonOpaque;
		var drawSettings = new DrawingSettings( new ShaderTagId("SRPDefaultUnlit"), sorting);
	
		var filterSettings = FilteringSettings.defaultValue;

		context.DrawRenderers(
			cull, ref drawSettings, ref filterSettings
		);

		context.DrawSkybox(camera);
		sorting.criteria = SortingCriteria.CommonTransparent;
		drawSettings.sortingSettings = sorting;
		filterSettings.renderQueueRange = RenderQueueRange.transparent;
		
		context.DrawRenderers(
			cull, ref drawSettings, ref filterSettings
		);

		DrawDefaultPipeline(context, camera);

		cameraBuffer.EndSample("Render Camera");
		context.ExecuteCommandBuffer(cameraBuffer);
		cameraBuffer.Clear();

		context.Submit();
	}

	[Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
	void DrawDefaultPipeline (ScriptableRenderContext context, Camera camera) {
		if (errorMaterial == null) {
			Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
			errorMaterial = new Material(errorShader) {
				hideFlags = HideFlags.HideAndDontSave
			};
		}

		var drawSettings = new DrawingSettings(new ShaderTagId("ForwardBase"), new SortingSettings());
		drawSettings.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
		drawSettings.SetShaderPassName(2, new ShaderTagId("Always"));
		drawSettings.SetShaderPassName(3, new ShaderTagId("Vertex"));
		drawSettings.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
		drawSettings.SetShaderPassName(5, new ShaderTagId("VertexLM"));
		drawSettings.overrideMaterial = errorMaterial;

		var filterSettings = new FilteringSettings();

		context.DrawRenderers(
			cull, ref drawSettings, ref filterSettings
		);
	}
}