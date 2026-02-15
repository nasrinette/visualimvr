using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

/// <summary>
/// Monitors all curtains and light switches in the scene.
/// When curtains are closed and lights are off, dims the room
/// by adjusting ambient light and post-exposure.
/// </summary>
public class RoomDarknessController : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    public CurtainInteraction[] curtains;
    public LightSwitch[] lightSwitches;

    [Header("Ambient Light")]
    [Tooltip("Ambient intensity when room is fully lit (curtains open or lights on)")]
    public float brightAmbientIntensity = 1.0f;
    [Tooltip("Ambient intensity when room is fully dark (all curtains closed + lights off)")]
    public float darkAmbientIntensity = 0.05f;

    [Header("Post-Exposure")]
    [Tooltip("Adjust post-exposure when dark for extra dimming")]
    public float brightExposure = 0.3f;
    [Tooltip("Post-exposure when fully dark")]
    public float darkExposure = -1.5f;

    [Header("Whiteboard Text")]
    [Tooltip("TMP texts on the whiteboard that should dim with the room")]
    public TextMeshProUGUI[] whiteboardTexts;

    [Header("Transition")]
    [Tooltip("How fast the room darkness changes")]
    public float transitionSpeed = 2f;

    private float currentDarkness;
    private ColorAdjustments colorAdjustments;
    private Color[] originalTextColors;

    void Start()
    {
        if (curtains == null || curtains.Length == 0)
            curtains = FindObjectsOfType<CurtainInteraction>();

        if (lightSwitches == null || lightSwitches.Length == 0)
            lightSwitches = FindObjectsOfType<LightSwitch>();

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
    }

    void Update()
    {
        float targetDarkness = CalculateDarkness();
        currentDarkness = Mathf.Lerp(currentDarkness, targetDarkness, Time.deltaTime * transitionSpeed);

        // Adjust ambient intensity
        RenderSettings.ambientIntensity = Mathf.Lerp(brightAmbientIntensity, darkAmbientIntensity, currentDarkness);

        // Adjust post-exposure
        if (colorAdjustments != null)
            colorAdjustments.postExposure.Override(Mathf.Lerp(brightExposure, darkExposure, currentDarkness));

        // Dim whiteboard text
        if (whiteboardTexts != null && originalTextColors != null)
        {
            float brightness = 1f - currentDarkness;
            for (int i = 0; i < whiteboardTexts.Length; i++)
            {
                if (whiteboardTexts[i] != null)
                    whiteboardTexts[i].color = originalTextColors[i] * brightness;
            }
        }
    }

    /// <summary>
    /// Returns 0 = fully bright, 1 = fully dark.
    /// Room is dark only when ALL curtains are closed AND ALL lights are off.
    /// </summary>
    float CalculateDarkness()
    {
        // Curtain factor: average of how closed each curtain is (0=open, 1=closed)
        float curtainsClosed = 0f;
        if (curtains.Length > 0)
        {
            float total = 0f;
            foreach (var c in curtains)
            {
                if (c != null) total += c.GetClosedAmount();
            }
            curtainsClosed = total / curtains.Length;
        }

        // Light factor: 0 if any light is on, 1 if all lights are off
        float lightsOff = 1f;
        foreach (var ls in lightSwitches)
        {
            if (ls != null && ls.IsOn)
            {
                lightsOff = 0f;
                break;
            }
        }

        // Room is dark when curtains are closed AND lights are off
        return curtainsClosed * lightsOff;
    }
}
