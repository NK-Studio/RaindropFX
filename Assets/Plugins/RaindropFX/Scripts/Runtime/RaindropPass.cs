using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace NKStudio
{
    public class RaindropPass : ScriptableRenderPass
    {

        private readonly Material _material;

        // The property block used to set additional properties for the material
        private static MaterialPropertyBlock s_SharedPropertyBlock = new MaterialPropertyBlock();

        // Creating some shader properties in advance as this is slightly more efficient than referencing them by string
        private static readonly int kBlitTexturePropertyId = Shader.PropertyToID("_BlitTexture");
        private static readonly int kBlitScaleBiasPropertyId = Shader.PropertyToID("_BlitScaleBias");
        private static readonly int RainDropAnimationTime = Shader.PropertyToID("_RainDropAnimationTime");
        private static readonly int RaindropIntensity = Shader.PropertyToID("_RaindropIntensity");
        private static readonly int RainAspect = Shader.PropertyToID("_RainAspect");
        private static readonly int RainWobbleStrength = Shader.PropertyToID("_RainWobbleStrength");
        private static readonly int RainSize = Shader.PropertyToID("_RainSize");
        private static readonly int StaticRaindropAnimationTime = Shader.PropertyToID("_StaticRaindropAnimationTime");
        private static readonly int StaticRaindropIntensity = Shader.PropertyToID("_StaticRaindropIntensity");
        private static readonly int Amount = Shader.PropertyToID("_Amount");
        private static readonly int StaticRainSize = Shader.PropertyToID("_StaticRainSize");
        private static readonly int Zoom = Shader.PropertyToID("_Zoom");
        private static readonly int FogIntensity = Shader.PropertyToID("_FogIntensity");
        private static readonly int BlurIntensity = Shader.PropertyToID("_BlurIntensity");

        private const string FogEnable = "_FogEnable";
        private const string BlurEnable = "_BlurEnable";

        public RaindropPass(string passName, Material material)
        {
            profilingSampler = new ProfilingSampler(passName);
            _material = material;

            requiresIntermediateTexture = true;
        }

        private static void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle sourceTexture, Material material)
        {
            s_SharedPropertyBlock.Clear();
            if (sourceTexture != null)
                s_SharedPropertyBlock.SetTexture(kBlitTexturePropertyId, sourceTexture);

            s_SharedPropertyBlock.SetVector(kBlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));

            Raindrop myVolume = VolumeManager.instance.stack?.GetComponent<Raindrop>();
            if (myVolume != null)
            {
                s_SharedPropertyBlock.SetFloat(RainDropAnimationTime, myVolume.RainDropAnimationTime.value);
                s_SharedPropertyBlock.SetFloat(RaindropIntensity, myVolume.Intensity.value);
                s_SharedPropertyBlock.SetVector(RainAspect, myVolume.Aspect.value);
                s_SharedPropertyBlock.SetFloat(RainSize, myVolume.Size.value);
                s_SharedPropertyBlock.SetFloat(RainWobbleStrength, myVolume.WiggleStrength.value);
                
                
                s_SharedPropertyBlock.SetFloat(StaticRaindropAnimationTime, myVolume.StaticRaindropAnimationTime.value);
                s_SharedPropertyBlock.SetFloat(StaticRaindropIntensity, myVolume.StaticRainIntensity.value);
                s_SharedPropertyBlock.SetFloat(Amount, myVolume.StaticRaindropAmount.value);
                s_SharedPropertyBlock.SetFloat(StaticRainSize, myVolume.StaticRaindropSize.value);

                s_SharedPropertyBlock.SetFloat(Zoom, myVolume.Zoom.value);
                
                CoreUtils.SetKeyword(material, FogEnable, myVolume.FogEnable.value);
                if (myVolume.FogEnable.value)
                    s_SharedPropertyBlock.SetFloat(FogIntensity, myVolume.FogIntensity.value);

                // CoreUtils.SetKeyword(material, BlurEnable, myVolume.BlurEnable.value);
                // if (myVolume.BlurEnable.value) 
                //     s_SharedPropertyBlock.SetFloat(BlurIntensity, myVolume.BlurIntensity.value);
            }

            cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
        }

        private class MainPassData
        {
            public Material Material;
            public TextureHandle InputTexture;
        }

        private static void ExecuteMainPass(MainPassData data, RasterGraphContext context)
        {
            ExecuteMainPass(context.cmd, data.InputTexture.IsValid() ? data.InputTexture : null, data.Material);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();

            using (var builder =
                   renderGraph.AddRasterRenderPass<MainPassData>(passName, out var passData, profilingSampler))
            {
                passData.Material = _material;

                var cameraColorDesc = renderGraph.GetTextureDesc(resourcesData.cameraColor);
                cameraColorDesc.name = "_RainDropPostProcessing";
                cameraColorDesc.clearBuffer = false;

                var destination = renderGraph.CreateTexture(cameraColorDesc);
                passData.InputTexture = resourcesData.cameraColor;

                builder.UseTexture(passData.InputTexture);


                builder.SetRenderAttachment(destination, 0);

                builder.SetRenderFunc((MainPassData data, RasterGraphContext context) =>
                    ExecuteMainPass(data, context));

                resourcesData.cameraColor = destination;
            }
        }
    }
}