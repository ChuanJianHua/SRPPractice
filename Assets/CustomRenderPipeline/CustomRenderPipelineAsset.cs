using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    [CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset {
    
        [ SerializeField ]
        bool dynamicBatching;
        [SerializeField]
        bool instancing;
        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline();
        }
    }    
}
