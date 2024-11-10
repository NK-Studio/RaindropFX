using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NKStudio
{
    public sealed class RaindropFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material material;
        [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        private RaindropPass _fullScreenPass;

        public override void Create()
        {
#if UNITY_EDITOR
            if (material == null)
                material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                    "Packages/com.unity.render-pipelines.universal/Runtime/Materials/FullscreenInvertColors.mat");
#endif

            if (material)
                _fullScreenPass = new RaindropPass(name, material);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null || _fullScreenPass == null)
                return;

            if (renderingData.cameraData.cameraType == CameraType.SceneView ||
                renderingData.cameraData.cameraType == CameraType.Preview ||
                renderingData.cameraData.cameraType == CameraType.Reflection)
                return;

            if(renderingData.postProcessingEnabled == false)
                return;
            
            Raindrop myVolume = VolumeManager.instance.stack?.GetComponent<Raindrop>();
            if (myVolume == null || !myVolume.IsActive())
                return;

            _fullScreenPass.renderPassEvent = renderPassEvent;

            _fullScreenPass.ConfigureInput(ScriptableRenderPassInput.None);

            renderer.EnqueuePass(_fullScreenPass);
        }
    }
}