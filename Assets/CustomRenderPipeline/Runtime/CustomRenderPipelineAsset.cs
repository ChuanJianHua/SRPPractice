using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    [CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset {
    
        [ SerializeField ]
        bool useDynamicBatching;
        [SerializeField]
        bool useGPUInstancing;
        [SerializeField]
        bool useSRPBatcher;
        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher);
        }
    }    
}
