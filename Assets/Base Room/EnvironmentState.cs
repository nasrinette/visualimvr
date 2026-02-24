using UnityEngine;
using UnityEngine.Rendering;

public class EnvironmentState : MonoBehaviour
{
    public Material skybox;
    public AmbientMode ambientMode;
    public Color ambientLight;
    public DefaultReflectionMode reflectionMode;

    public bool hasSnapshot;

    public void CaptureCurrent()
    {
        skybox = RenderSettings.skybox;
        ambientMode = RenderSettings.ambientMode;
        ambientLight = RenderSettings.ambientLight;
        reflectionMode = RenderSettings.defaultReflectionMode;
        hasSnapshot = true;
    }

    public void Restore()
    {
        if (!hasSnapshot) return;

        RenderSettings.skybox = skybox;
        RenderSettings.ambientMode = ambientMode;
        RenderSettings.ambientLight = ambientLight;
        RenderSettings.defaultReflectionMode = reflectionMode;

        DynamicGI.UpdateEnvironment();
    }
}