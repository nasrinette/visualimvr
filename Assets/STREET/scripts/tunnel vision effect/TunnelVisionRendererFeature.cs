using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

// using this tutorial: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@16.0/manual/renderer-features/create-custom-renderer-feature.html

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


}
