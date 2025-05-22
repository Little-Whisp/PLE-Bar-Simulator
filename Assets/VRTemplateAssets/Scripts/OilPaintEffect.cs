using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
//will probably be removed later 

public class OilPaintEffect : ScriptableRendererFeature
{
    class OilPaintRenderPass : ScriptableRenderPass
    {
        public Material material;
        private RTHandle tempTexture;

        public OilPaintRenderPass(Material mat)
        {
            material = mat;
            tempTexture = RTHandles.Alloc("_TempOilPaintTex");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            ConfigureClear(ClearFlag.None, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("OilPaintEffect");

            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            Blit(cmd, cameraColorTarget, tempTexture, material);
            Blit(cmd, tempTexture, cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // Clean up if needed
        }
    }

    public Material oilPaintMaterial;
    private OilPaintRenderPass oilPaintPass;

    public override void Create()
    {
        oilPaintPass = new OilPaintRenderPass(oilPaintMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(oilPaintPass);
    }
}
