using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TunnelVisionController : MonoBehaviour
{
    [SerializeField] private UniversalRendererData rendererData;

    private TunnelVisionRendererFeature tunnelFeature;

    void Awake()
    {
        // Find the feature inside the renderer
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is TunnelVisionRendererFeature tv)
            {
                tunnelFeature = tv;
                break;
            }
        }
    }

    public void SetTunnelActive(bool active)
    {
        if (tunnelFeature != null)
        {
            tunnelFeature.SetActive(active);
        }
    }
}