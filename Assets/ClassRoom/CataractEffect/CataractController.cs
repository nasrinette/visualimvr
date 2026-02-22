using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Drives cataract shader parameters based on scene brightness from RoomDarknessController.
///
/// Real cataract behaviour:
///   - Bright sunlight: WORST vision (heavy glare, scatter, veiling, reduced contrast)
///   - Moderate indoor light: BEST vision (minimal scatter, just base cloudiness)
///   - Dark: poor vision (dilated pupil exposes more clouded lens, extra blur)
///
/// The effect is always present (the lens is physically clouded) but the
/// brightness-dependent symptoms scale dramatically with light level.
/// </summary>
public class CataractController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] private RoomDarknessController roomDarkness;

    [Header("Base Severity (always present regardless of light)")]
    [Tooltip("Poisson blur radius - the permanent cloudiness of the lens")]
    [Range(0.001f, 0.01f)] public float baseBlur = 0.003f;
    [Tooltip("Baseline contrast loss from the clouded lens")]
    [Range(0f, 0.3f)] public float baseContrastLoss = 0.10f;
    [Tooltip("Yellow/brown discolouration of the aged lens")]
    [Range(0f, 0.4f)] public float yellowTint = 0.15f;

    [Header("Bright Light Effects (sunlight)")]
    [Tooltip("Extra blur in bright conditions (whole lens illuminated)")]
    [Range(0f, 0.01f)] public float brightBlurBoost = 0.002f;
    [Tooltip("Max light scatter strength in bright conditions")]
    [Range(0f, 1f)] public float maxScatter = 0.4f;
    [Tooltip("Max contrast loss in bright conditions")]
    [Range(0f, 0.5f)] public float maxContrastLoss = 0.25f;
    [Tooltip("Max veiling glare (milky haze) in bright conditions")]
    [Range(0f, 0.4f)] public float maxVeilingGlare = 0.20f;

    [Header("Dark Conditions")]
    [Tooltip("Extra blur when dark (dilated pupil uses cloudy lens periphery)")]
    [Range(0f, 0.01f)] public float darkBlurBoost = 0.003f;

    [Header("Bloom (halos around light sources)")]
    [Tooltip("Bloom intensity in bright sunlight")]
    [Range(0f, 5f)] public float brightBloomIntensity = 2.5f;
    [Tooltip("Bloom intensity with indoor lights")]
    [Range(0f, 3f)] public float indoorBloomIntensity = 0.8f;
    [Tooltip("Bloom intensity in darkness")]
    [Range(0f, 1f)] public float darkBloomIntensity = 0.2f;
    [Tooltip("Bloom threshold in bright light (lower = more halos)")]
    [Range(0f, 2f)] public float brightBloomThreshold = 0.5f;
    [Tooltip("Bloom threshold with indoor lights")]
    [Range(0f, 2f)] public float indoorBloomThreshold = 0.9f;

    [Header("Transition")]
    public float transitionSpeed = 2f;

    private Material cataractMaterial;
    private Bloom bloom;
    private float originalBloomIntensity;
    private float originalBloomThreshold;
    private bool hadBloom;

    // Smoothed current values
    private float curBlur, curScatter, curContrast, curVeil, curBloomInt, curBloomThresh;

    void Start()
    {
        // Find the CataractRendererFeature and its material
        if (rendererData != null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is CataractRendererFeature cf)
                {
                    cataractMaterial = cf.GetMaterial();
                    break;
                }
            }
        }

        if (cataractMaterial == null)
            Debug.LogWarning("CataractController: Could not find CataractRendererFeature material. " +
                             "Assign the RendererData and add CataractRendererFeature to the renderer.");

        if (roomDarkness == null)
            roomDarkness = FindObjectOfType<RoomDarknessController>();

        // Find Bloom override
        var volumes = FindObjectsOfType<Volume>();
        foreach (var vol in volumes)
        {
            if (vol.isGlobal && vol.sharedProfile != null)
            {
                vol.sharedProfile.TryGet(out bloom);
                if (bloom != null) break;
            }
        }

        if (bloom != null)
        {
            hadBloom = true;
            originalBloomIntensity = bloom.intensity.value;
            originalBloomThreshold = bloom.threshold.value;
        }

    }

    void Update()
    {
        if (cataractMaterial == null || roomDarkness == null) return;

        float brightness = roomDarkness.GetSceneBrightness();
        float dt = Time.deltaTime * transitionSpeed;

        // --- Calculate target values ---

        // Blur: U-shaped curve. Best at ~0.5 brightness (indoor lights).
        // Worse in bright light (whole lens flooded) AND in dark (dilated pupil).
        float optBright = 0.5f;
        float deviation = Mathf.Abs(brightness - optBright) / Mathf.Max(optBright, 0.01f);
        float targetBlur = baseBlur;
        if (brightness > optBright)
            targetBlur += brightBlurBoost * Mathf.Pow(deviation, 2f);
        else
            targetBlur += darkBlurBoost * Mathf.Pow(deviation, 2f);

        // Scatter: steep exponential with brightness.
        // Negligible below ~0.3, ramps up sharply toward 1.0.
        float scatterCurve = Mathf.Pow(Mathf.Max(brightness - 0.3f, 0f) / 0.7f, 2.5f);
        float targetScatter = maxScatter * scatterCurve;

        // Contrast loss: base + brightness-driven component
        float targetContrast = baseContrastLoss
            + (maxContrastLoss - baseContrastLoss) * Mathf.Pow(brightness, 1.5f);

        // Veiling glare: proportional to brightness
        float targetVeil = maxVeilingGlare * Mathf.Pow(brightness, 1.5f);

        // Bloom intensity & threshold
        float targetBloomInt, targetBloomThresh;
        if (brightness > 0.7f)
        {
            float t = (brightness - 0.7f) / 0.3f;
            targetBloomInt = Mathf.Lerp(indoorBloomIntensity, brightBloomIntensity, t);
            targetBloomThresh = Mathf.Lerp(indoorBloomThreshold, brightBloomThreshold, t);
        }
        else if (brightness > 0.2f)
        {
            targetBloomInt = indoorBloomIntensity;
            targetBloomThresh = indoorBloomThreshold;
        }
        else
        {
            float t = brightness / 0.2f;
            targetBloomInt = Mathf.Lerp(darkBloomIntensity, indoorBloomIntensity, t);
            targetBloomThresh = indoorBloomThreshold;
        }

        // --- Smooth transitions ---
        curBlur = Mathf.Lerp(curBlur, targetBlur, dt);
        curScatter = Mathf.Lerp(curScatter, targetScatter, dt);
        curContrast = Mathf.Lerp(curContrast, targetContrast, dt);
        curVeil = Mathf.Lerp(curVeil, targetVeil, dt);
        curBloomInt = Mathf.Lerp(curBloomInt, targetBloomInt, dt);
        curBloomThresh = Mathf.Lerp(curBloomThresh, targetBloomThresh, dt);

        // --- Apply to shader ---
        cataractMaterial.SetFloat("_BlurRadius", curBlur);
        cataractMaterial.SetFloat("_ScatterStrength", curScatter);
        cataractMaterial.SetFloat("_ContrastLoss", curContrast);
        cataractMaterial.SetFloat("_YellowTint", yellowTint);
        cataractMaterial.SetFloat("_VeilingGlare", curVeil);

        // --- Apply Bloom ---
        if (bloom != null)
        {
            bloom.intensity.Override(curBloomInt);
            bloom.threshold.Override(curBloomThresh);
        }

    }

    void OnDisable()
    {
        // Restore original Bloom values so edits don't persist on shared profile
        if (bloom != null && hadBloom)
        {
            bloom.intensity.Override(originalBloomIntensity);
            bloom.threshold.Override(originalBloomThreshold);
        }
    }
}
