using System;
using UnityEngine.Rendering.Universal;

[Serializable]
public class CataractSettings
{
    [UnityEngine.Header("When to run")]
    public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;
}
