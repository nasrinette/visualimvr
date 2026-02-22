using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TunnelVisionRenderPass : ScriptableRenderPass
{


    private TunnelVisionSettings settings;
    private Material material;

    private RTHandle tempHandle;

    // Shader property IDs (match these in your shader later)
    private static readonly int CenterUvId = Shader.PropertyToID("_CenterUV");
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int FeatherId = Shader.PropertyToID("_Feather");
    private static readonly int DarknessId = Shader.PropertyToID("_Darkness");
    private static readonly int StrainId = Shader.PropertyToID("_Strain");
    private static readonly int WarpStrengthId = Shader.PropertyToID("_WarpStrength");
    private static readonly int BlurStrengthId = Shader.PropertyToID("_BlurStrength");

    public TunnelVisionRenderPass(Material material, TunnelVisionSettings settings)
    {
        this.material = material;
        this.settings = settings;
    }

    // in the tutorial, they use configure, but in new version we use OnCameraSetup
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // Allocate temp texture matching camera target
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

        // Push params (XR driver can also overwrite these each frame)
        // ApplyMaterialParams();
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

    // equivalent UpdateBlurSettings
    private void ApplyMaterialParams()
    {
        if (material == null) return;

        // Center is screen-center for now (eye tracking later could change it)
        material.SetVector(CenterUvId, new Vector4(0.5f, 0.5f, 0f, 0f));

        material.SetFloat(RadiusId, settings.radius);
        material.SetFloat(FeatherId, settings.feather);
        material.SetFloat(DarknessId, settings.darkness);

        material.SetFloat(StrainId, settings.strain);
        material.SetFloat(WarpStrengthId, settings.warpStrength);
        material.SetFloat(BlurStrengthId, settings.blurStrength);
    }

    public void Dispose()
    {
        if (tempHandle != null) tempHandle.Release();

    }

}
