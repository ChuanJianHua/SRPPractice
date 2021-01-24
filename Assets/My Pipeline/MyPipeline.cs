using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline {
	private CullingResults cull;
	private Material errorMaterial;
	private bool enableDynamicBatching;
	private bool enableInstancing;
	private ScriptableCullingParameters cullingParameters;
	CommandBuffer cameraBuffer = new CommandBuffer {
		name = "Render Camera"
	};

	public MyPipeline(bool enableDynamicBatching, bool enableInstancing)
	{
		this.enableDynamicBatching = enableDynamicBatching;
		this.enableInstancing = enableInstancing;
	}
	protected override void Render (ScriptableRenderContext renderContext, Camera[] cameras) 
	{
		foreach (var camera in cameras) {
			Render(renderContext, camera);
		}
	}

	void Render (ScriptableRenderContext context, Camera camera) 
	{
		if (!camera.TryGetCullingParameters(out cullingParameters)) 
		{
			return;
		}

#if UNITY_EDITOR
		if (camera.cameraType == CameraType.SceneView) 
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
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

		SortingSettings sorting = new SortingSettings(camera);
		DrawingSettings drawSettings = new DrawingSettings( new ShaderTagId("SRPDefaultUnlit"), sorting);
		FilteringSettings filterSettings = FilteringSettings.defaultValue;
		
		//Opaque Draw
		sorting.criteria = SortingCriteria.CommonOpaque;
		drawSettings.enableInstancing = enableInstancing;
		drawSettings.enableDynamicBatching = enableDynamicBatching;

		context.DrawRenderers(
			cull, ref drawSettings, ref filterSettings
		);
		context.DrawSkybox(camera);
		
		//Opaque CommonTransparent
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
		var filterSettings = FilteringSettings.defaultValue;
		context.DrawRenderers(
			cull, ref drawSettings, ref filterSettings
		);
	}
}