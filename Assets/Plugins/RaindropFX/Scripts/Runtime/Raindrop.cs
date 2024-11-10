using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NKStudio
{
    [VolumeComponentMenu("NK Studio/Rain drop")]
    [VolumeRequiresRendererFeatures(typeof(RaindropFeature))]
    [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
    public sealed class Raindrop : VolumeComponent, IPostProcessComponent
    {
        public Raindrop()
        {
            displayName = "Rain drop";
        }

        [Header("Rain drop")]
        public ClampedFloatParameter RainDropAnimationTime = new ClampedFloatParameter(1.0f, 0.01f, 2.0f);

        public ClampedFloatParameter Intensity = new ClampedFloatParameter(0f, 0f, 1f);
        public Vector2Parameter Aspect = new Vector2Parameter(new Vector2(4, 1f));
        public ClampedFloatParameter Size = new ClampedFloatParameter(1f, 0f, 5f);
        public ClampedFloatParameter WiggleStrength = new ClampedFloatParameter(0.5f, 0f, 1f);

        [Header("Static Rain drop")]
        public ClampedFloatParameter StaticRaindropAnimationTime = new ClampedFloatParameter(1.0f, 0.01f, 2.0f);

        public ClampedFloatParameter StaticRainIntensity = new ClampedFloatParameter(0.0f, 0f, 1.0f);
        public ClampedIntParameter StaticRaindropAmount = new ClampedIntParameter(2, 1, 3);
        public ClampedFloatParameter StaticRaindropSize = new ClampedFloatParameter(1f, 0f, 1f);

        [Header("Fog")] public BoolParameter FogEnable = new BoolParameter(false);
        public ClampedFloatParameter FogIntensity = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);

        // [Header("Blur")] public BoolParameter BlurEnable = new BoolParameter(false);
        // public ClampedFloatParameter BlurIntensity = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);

        [Header("Zoom")] public ClampedFloatParameter Zoom = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);

        public bool IsActive() => Intensity.GetValue<float>() > 0.0f;
    }
}