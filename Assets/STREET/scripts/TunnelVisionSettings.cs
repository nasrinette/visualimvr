
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class TunnelVisionSettings
{
    [Header("Tunnel")]
    [Range(0.05f, 0.5f)] public float radius = 0.18f;
    [Range(0.001f, 0.3f)] public float feather = 0.06f;     // edge softness
    [Range(0f, 1f)] public float darkness = 0.75f;          // vignette darkness

    [Header("Stretch response")]
    [Range(0f, 1f)] public float strain = 0f;               // 0..1 driven by XR
    [Range(0f, 0.05f)] public float warpStrength = 0.0f;    // UV warp at edge
    [Range(0f, 2f)] public float blurStrength = 0.5f;       // blur amount at edge

    [Header("When to run")]
    public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;
}