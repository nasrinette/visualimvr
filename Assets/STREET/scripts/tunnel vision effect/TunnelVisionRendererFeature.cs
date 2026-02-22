using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

// using this tutorial: https://git.wur.nl/farma002/env-urban-vr/-/blob/240042b451574b68849ca8cf6513bc3d41aaa413/Env-Urban-URP/Library/PackageCache/com.unity.render-pipelines.universal@14.0.11/Documentation~/renderer-features/create-custom-renderer-feature.md

public class TunnelVisionRendererFeature : ScriptableRendererFeature
{

    [SerializeField] private TunnelVisionSettings settings = new TunnelVisionSettings();
    [SerializeField] private Material material;
    private TunnelVisionRenderPass pass;


    public override void Create()
    {
        if (material == null)
        {
            Debug.LogError("TunnelVisionRendererFeature: Assign TunnelVision material.");
            return;
        }

        pass = new TunnelVisionRenderPass(material, settings)
        {
            renderPassEvent = settings.passEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null || pass == null) return;

        renderer.EnqueuePass(pass);
    }
    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();

    }

    // Optional: expose the material to other scripts (XR driver)
    public Material GetMaterial() => material;

    // Optional: expose settings to other scripts
    public TunnelVisionSettings GetSettings() => settings;

}
