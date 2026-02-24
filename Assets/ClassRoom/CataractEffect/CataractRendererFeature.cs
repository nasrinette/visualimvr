using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CataractRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private CataractSettings settings = new CataractSettings();

    private Material material;
    private CataractRenderPass pass;
    private bool effectActive;

    // init material + pass
    public override void Create()
    {
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

    // enqueue if active
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!effectActive || material == null || pass == null) return;
        renderer.EnqueuePass(pass);
    }

    // cleanup resources
    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
        if (material != null)
            CoreUtils.Destroy(material);
    }

    public void SetActive(bool active) => effectActive = active;
    public Material GetMaterial() => material;
    public CataractSettings GetSettings() => settings;
}
