using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using System.Collections.Generic;

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

    private Light[] overheadLights;
    private float[] originalLightIntensities;

    private float originalAmbientIntensity;
    private float originalPostExposure;

    void Start()
    {
        if (curtains == null || curtains.Length == 0)
            curtains = FindObjectsOfType<CurtainInteraction>();

        if (lightSwitches == null || lightSwitches.Length == 0)
            lightSwitches = FindObjectsOfType<LightSwitch>();

        // cache overhead lights
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

        if (whiteboardTexts == null || whiteboardTexts.Length == 0)
        {
            var canvas = GameObject.Find("classroom/Classroom/WhiteBoard/Canvas");
            if (canvas != null)
                whiteboardTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>();
        }

        if (whiteboardTexts != null)
        {
            originalTextColors = new Color[whiteboardTexts.Length];
            for (int i = 0; i < whiteboardTexts.Length; i++)
            {
                if (whiteboardTexts[i] != null)
                    originalTextColors[i] = whiteboardTexts[i].color;
            }
        }

        originalAmbientIntensity = RenderSettings.ambientIntensity;

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

        float curtainsClosed = CalculateCurtainsClosed();
        float lightsOn = CalculateLightsOnFraction();
        CalculateLightingTargets(curtainsClosed, lightsOn, out currentExposure, out currentAmbient);
        currentLightMultiplier = 1f + (indoorLightBoost - 1f) * curtainsClosed;
    }

    // restore originals
    void OnDisable()
    {
        RenderSettings.ambientIntensity = originalAmbientIntensity;

        if (colorAdjustments != null)
            colorAdjustments.postExposure.Override(originalPostExposure);

        if (overheadLights != null && originalLightIntensities != null)
        {
            for (int i = 0; i < overheadLights.Length; i++)
            {
                if (overheadLights[i] != null)
                    overheadLights[i].intensity = originalLightIntensities[i];
            }
        }

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

        float targetLightMultiplier = Mathf.Lerp(1f, indoorLightBoost, curtainsClosed);

        float dt = Time.deltaTime * transitionSpeed;
        currentExposure = Mathf.Lerp(currentExposure, targetExposure, dt);
        currentAmbient = Mathf.Lerp(currentAmbient, targetAmbient, dt);
        currentLightMultiplier = Mathf.Lerp(currentLightMultiplier, targetLightMultiplier, dt);

        RenderSettings.ambientIntensity = currentAmbient;
        if (colorAdjustments != null)
            colorAdjustments.postExposure.Override(currentExposure);

        for (int i = 0; i < overheadLights.Length; i++)
        {
            if (overheadLights[i] != null)
                overheadLights[i].intensity = originalLightIntensities[i] * currentLightMultiplier;
        }

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

    // blend three tiers
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

    public float GetSceneBrightness()
    {
        float curtainsClosed = CalculateCurtainsClosed();
        float lightsOn = CalculateLightsOnFraction();

        float sunlight = 1f - curtainsClosed;
        float indoorContribution = lightsOn * 0.5f * curtainsClosed;

        return Mathf.Clamp01(sunlight + indoorContribution);
    }

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
