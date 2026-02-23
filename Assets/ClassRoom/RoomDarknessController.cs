using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Controls room brightness based on curtain state and light switches.
/// Three lighting tiers:
///   1. Sunlight (curtains open) - normal brightness
///   2. Indoor only (curtains closed, lights on) - boosted exposure + light intensity
///   3. Dark (curtains closed, lights off) - very dim
/// Also exposes scene brightness for the CataractController.
/// </summary>
public class RoomDarknessController : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    public CurtainInteraction[] curtains;
    public LightSwitch[] lightSwitches;

    [Header("Sunlight (curtains open)")]
    [Tooltip("Ambient intensity when sunlight enters")]
    public float brightAmbientIntensity = 1.0f;
    [Tooltip("Post-exposure with sunlight")]
    public float brightExposure = 0.3f;

    [Header("Indoor Lights Only (curtains closed, lights on)")]
    [Tooltip("Ambient intensity with indoor lighting")]
    public float indoorAmbientIntensity = 2.0f;
    [Tooltip("Post-exposure to compensate for blocked sunlight")]
    public float indoorExposure = 4.0f;
    [Tooltip("Multiplier for overhead light intensity when curtains are fully closed")]
    public float indoorLightBoost = 3f;

    [Header("Dark (curtains closed, lights off)")]
    [Tooltip("Ambient intensity when fully dark")]
    public float darkAmbientIntensity = 0.05f;
    [Tooltip("Post-exposure when fully dark")]
    public float darkExposure = -1.5f;

    [Header("Whiteboard Text")]
    [Tooltip("TMP texts on the whiteboard that should dim with the room")]
    public TextMeshProUGUI[] whiteboardTexts;

    [Header("Transition")]
    [Tooltip("How fast the room lighting changes")]
    public float transitionSpeed = 2f;

    private float currentExposure;
    private float currentAmbient;
    private float currentLightMultiplier = 1f;
    private ColorAdjustments colorAdjustments;
    private Color[] originalTextColors;

    // Cached overhead lights and their original intensities
    private Light[] overheadLights;
    private float[] originalLightIntensities;

    // Original global state for cleanup
    private float originalAmbientIntensity;
    private float originalPostExposure;

    void Start()
    {
        if (curtains == null || curtains.Length == 0)
            curtains = FindObjectsOfType<CurtainInteraction>();

        if (lightSwitches == null || lightSwitches.Length == 0)
            lightSwitches = FindObjectsOfType<LightSwitch>();

        // Cache all overhead lights and their original intensities
        var lights = new List<Light>();
        foreach (var ls in lightSwitches)
        {
            if (ls != null && ls.lights != null)
            {
                foreach (var l in ls.lights)
                    if (l != null) lights.Add(l);
            }
        }
        overheadLights = lights.ToArray();
        originalLightIntensities = new float[overheadLights.Length];
        for (int i = 0; i < overheadLights.Length; i++)
            originalLightIntensities[i] = overheadLights[i].intensity;

        // Auto-find whiteboard texts if not assigned
        if (whiteboardTexts == null || whiteboardTexts.Length == 0)
        {
            var canvas = GameObject.Find("classroom/Classroom/WhiteBoard/Canvas");
            if (canvas != null)
                whiteboardTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>();
        }

        // Store original text colors
        if (whiteboardTexts != null)
        {
            originalTextColors = new Color[whiteboardTexts.Length];
            for (int i = 0; i < whiteboardTexts.Length; i++)
            {
                if (whiteboardTexts[i] != null)
                    originalTextColors[i] = whiteboardTexts[i].color;
            }
        }

        // Save original global state for cleanup
        originalAmbientIntensity = RenderSettings.ambientIntensity;

        // Find ColorAdjustments from the global volume
        var volumes = FindObjectsOfType<Volume>();
        foreach (var vol in volumes)
        {
            if (vol.isGlobal && vol.sharedProfile != null)
            {
                vol.sharedProfile.TryGet(out colorAdjustments);
                if (colorAdjustments != null) break;
            }
        }

        if (colorAdjustments != null)
            originalPostExposure = colorAdjustments.postExposure.value;

        // Initialize to current state (no pop on first frame)
        float curtainsClosed = CalculateCurtainsClosed();
        float lightsOn = CalculateLightsOnFraction();
        CalculateLightingTargets(curtainsClosed, lightsOn, out currentExposure, out currentAmbient);
        currentLightMultiplier = 1f + (indoorLightBoost - 1f) * curtainsClosed;
    }

    void OnDisable()
    {
        // Restore global render settings
        RenderSettings.ambientIntensity = originalAmbientIntensity;

        if (colorAdjustments != null)
            colorAdjustments.postExposure.Override(originalPostExposure);

        // Restore overhead light intensities
        if (overheadLights != null && originalLightIntensities != null)
        {
            for (int i = 0; i < overheadLights.Length; i++)
            {
                if (overheadLights[i] != null)
                    overheadLights[i].intensity = originalLightIntensities[i];
            }
        }

        // Restore whiteboard text colors
        if (whiteboardTexts != null && originalTextColors != null)
        {
            for (int i = 0; i < whiteboardTexts.Length; i++)
            {
                if (whiteboardTexts[i] != null)
                    whiteboardTexts[i].color = originalTextColors[i];
            }
        }
    }

    void Update()
    {
        float curtainsClosed = CalculateCurtainsClosed();
        float lightsOn = CalculateLightsOnFraction();

        float targetExposure, targetAmbient;
        CalculateLightingTargets(curtainsClosed, lightsOn, out targetExposure, out targetAmbient);

        // Boost overhead light intensity as curtains close
        // (simulates indoor lights becoming the primary source)
        float targetLightMultiplier = Mathf.Lerp(1f, indoorLightBoost, curtainsClosed);

        // Smooth transitions
        float dt = Time.deltaTime * transitionSpeed;
        currentExposure = Mathf.Lerp(currentExposure, targetExposure, dt);
        currentAmbient = Mathf.Lerp(currentAmbient, targetAmbient, dt);
        currentLightMultiplier = Mathf.Lerp(currentLightMultiplier, targetLightMultiplier, dt);

        // Apply exposure & ambient
        RenderSettings.ambientIntensity = currentAmbient;
        if (colorAdjustments != null)
            colorAdjustments.postExposure.Override(currentExposure);

        // Apply boosted intensity to overhead lights
        for (int i = 0; i < overheadLights.Length; i++)
        {
            if (overheadLights[i] != null)
                overheadLights[i].intensity = originalLightIntensities[i] * currentLightMultiplier;
        }

        // Dim whiteboard text only when truly dark (curtains closed AND lights off)
        if (whiteboardTexts != null && originalTextColors != null)
        {
            float textDarkness = curtainsClosed * (1f - lightsOn);
            float brightness = 1f - textDarkness;
            for (int i = 0; i < whiteboardTexts.Length; i++)
            {
                if (whiteboardTexts[i] != null)
                    whiteboardTexts[i].color = originalTextColors[i] * brightness;
            }
        }
    }

    /// <summary>
    /// Blends between three lighting tiers based on curtain/light state.
    /// The weights always sum to 1:
    ///   sun = (1 - curtainsClosed)
    ///   indoor = curtainsClosed * lightsOn
    ///   dark = curtainsClosed * (1 - lightsOn)
    /// </summary>
    void CalculateLightingTargets(float curtainsClosed, float lightsOn,
        out float exposure, out float ambient)
    {
        float sun = 1f - curtainsClosed;
        float indoor = curtainsClosed * lightsOn;
        float dark = curtainsClosed * (1f - lightsOn);

        exposure = sun * brightExposure
                 + indoor * indoorExposure
                 + dark * darkExposure;

        ambient = sun * brightAmbientIntensity
                + indoor * indoorAmbientIntensity
                + dark * darkAmbientIntensity;
    }

    /// <summary>
    /// Returns scene brightness for the cataract effect.
    /// 1.0 = bright sunlight (cataracts worst), 0.5 = indoor lights, 0.0 = dark.
    /// </summary>
    public float GetSceneBrightness()
    {
        float curtainsClosed = CalculateCurtainsClosed();
        float lightsOn = CalculateLightsOnFraction();

        float sunlight = 1f - curtainsClosed;
        float indoorContribution = lightsOn * 0.5f * curtainsClosed;

        return Mathf.Clamp01(sunlight + indoorContribution);
    }

    /// <summary>
    /// Returns 0 = fully bright, 1 = fully dark.
    /// Backwards compatible: dark only when curtains closed AND lights off.
    /// </summary>
    public float CalculateDarkness()
    {
        return CalculateCurtainsClosed() * (1f - CalculateLightsOnFraction());
    }

    float CalculateCurtainsClosed()
    {
        if (curtains == null || curtains.Length == 0) return 0f;
        float total = 0f;
        foreach (var c in curtains)
            if (c != null) total += c.GetClosedAmount();
        return total / curtains.Length;
    }

    float CalculateLightsOnFraction()
    {
        if (lightSwitches == null || lightSwitches.Length == 0) return 0f;
        int on = 0, count = 0;
        foreach (var ls in lightSwitches)
        {
            if (ls != null)
            {
                count++;
                if (ls.IsOn) on++;
            }
        }
        return count > 0 ? (float)on / count : 0f;
    }
}
