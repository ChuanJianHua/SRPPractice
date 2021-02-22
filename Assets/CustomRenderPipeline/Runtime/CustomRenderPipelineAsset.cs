using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("shadowSetting")] [SerializeField]
        private ShadowSettings shadowSettings;
        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher, shadowSettings);
        }
    }    
}
