using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomRenderPipeline
{
    [Serializable]
    public class ShadowSettings
    {
        [Min(0)]
        public float maxDistance;

        public Directional directional = new Directional() {atlasSize = TextureSize._1024};
    }

    [Serializable]
    public enum TextureSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }
    
    [System.Serializable]
    public struct Directional 
    {
        public TextureSize atlasSize;
    }

}
