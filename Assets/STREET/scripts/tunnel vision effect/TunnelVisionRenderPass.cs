using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// i used https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@16.0/manual/renderer-features/create-custom-renderer-feature.html

public class TunnelVisionRenderPass : ScriptableRenderPass
{


    private TunnelVisionSettings settings;
    private Material material;

    private RTHandle tempHandle;
   
    public TunnelVisionRenderPass(Material material, TunnelVisionSettings settings)
    {
        this.material = material;
        this.settings = settings;
    }

    // in the tutorial, they use configure, but in new version we use OnCameraSetup
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // this allocate temp texture matching camera target
        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        RenderingUtils.ReAllocateIfNeeded(ref tempHandle, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TunnelVisionTemp");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (material == null) return;

        // skip SceneView camera
        if (renderingData.cameraData.isSceneViewCamera) return;
        if (renderingData.cameraData.isPreviewCamera) return;
        if (renderingData.cameraData.cameraType != CameraType.Game) return;
        if (renderingData.cameraData.renderType != CameraRenderType.Base) return;

        CommandBuffer cmd = CommandBufferPool.Get("TunnelVisionPass");

        // Grab camera color
        RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

        if (tempHandle == null) return;
        // Blit from the camera target to the temporary render texture,
        // using the first shader pass.
        Blit(cmd, cameraTargetHandle, tempHandle, material, 0);
        // we just copy the processed image back
        Blit(cmd, tempHandle, cameraTargetHandle);

        //Execute the command buffer and release it back to the pool.
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        if (tempHandle != null) tempHandle.Release();

    }

}
