using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CataractRenderPass : ScriptableRenderPass
{
    private Material material;
    private RTHandle tempHandle;

    public CataractRenderPass(Material material, CataractSettings settings)
    {
        this.material = material;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
        RenderingUtils.ReAllocateIfNeeded(ref tempHandle, descriptor,
            FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_CataractTemp");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (material == null || tempHandle == null) return;
        if (renderingData.cameraData.isSceneViewCamera) return;
        if (renderingData.cameraData.isPreviewCamera) return;
        if (renderingData.cameraData.cameraType != CameraType.Game) return;
        if (renderingData.cameraData.renderType != CameraRenderType.Base) return;

        CommandBuffer cmd = CommandBufferPool.Get("CataractPass");

        RTHandle cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

        Blit(cmd, cameraTarget, tempHandle, material, 0);
        Blit(cmd, tempHandle, cameraTarget);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        if (tempHandle != null) tempHandle.Release();
    }
}
