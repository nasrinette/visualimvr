using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CataractRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private CataractSettings settings = new CataractSettings();

    private Material material;
    private CataractRenderPass pass;

    public override void Create()
    {
        // Auto-create material from shader (no manual .mat needed)
        var shader = Shader.Find("Hidden/CataractFullscreen");
        if (shader == null)
        {
            Debug.LogError("CataractRendererFeature: Shader 'Hidden/CataractFullscreen' not found.");
            return;
        }

        material = CoreUtils.CreateEngineMaterial(shader);
        pass = new CataractRenderPass(material, settings)
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
        if (material != null)
            CoreUtils.Destroy(material);
    }

    public Material GetMaterial() => material;
    public CataractSettings GetSettings() => settings;
}
