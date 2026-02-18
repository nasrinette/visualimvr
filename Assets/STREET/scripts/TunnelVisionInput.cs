using UnityEngine;
using UnityEngine.InputSystem;

public class TunnelVisionInput : MonoBehaviour
{
    [Header("References")]
    public Transform leftController;
    public Transform rightController;

    [Header("Input Actions (XRI default)")]
    public InputActionProperty leftGrip;
    public InputActionProperty rightGrip;

    [Header("Material to drive")]
    public Material tunnelMaterial;

    // TODO modify this when testing with headset & controllers
    [Header("Gesture")]
    public float gripThreshold = 0.4f;
    public float minDist = 0.20f;
    public float maxDist = 0.60f;

    [Header("Tunnel")]
    public float baseRadius = 0.18f;
    public float maxExtraRadius = 0.08f;
    public float expandSpeed = 12f;
    public float snapSpeed = 18f;

    [Header("Fallback (Keyboard)")]
    public KeyCode debugKey = KeyCode.P;

    float snapTime = 0.03f;
    float currentRadius;

    float strain = 0f;
    float snap = 0f;          // decays to 0 quickly
    float radiusVel = 0f;

    float previousDist;
    float grabStartDist;
    float maxStretchAmount = 0.25f;

    bool wasGripsHeld;


    void Awake()
    {
        currentRadius = baseRadius;
    }

    void Update()
    {
        if (!tunnelMaterial || !leftController || !rightController) return;

        // float lg = leftGrip.action.ReadValue<float>();
        // float rg = rightGrip.action.ReadValue<float>();
        // bool trying = lg > gripThreshold && rg > gripThreshold;//|| Input.GetKey(debugKey);

        bool leftHeld = leftGrip.action != null && leftGrip.action.IsPressed();
        bool rightHeld = rightGrip.action != null && rightGrip.action.IsPressed();
        bool gripsHeld = leftHeld && rightHeld;
        float currentDist = Vector3.Distance(leftController.position, rightController.position);

        if (gripsHeld && !wasGripsHeld)
        {
            grabStartDist = currentDist;
        }


        float stretch = 0f;

        if (gripsHeld)
        {
            float delta = currentDist - grabStartDist;
            stretch = Mathf.InverseLerp(0f, maxStretchAmount, delta);
            stretch = Mathf.Clamp01(stretch);
        }

        // "Trying" means: grips held AND actually stretched a bit
        bool trying = gripsHeld && stretch > 0.02f;

        float targetRadius = baseRadius;



        if (trying)
        {
            Debug.Log("ici");
            // float dist = Vector3.Distance(leftController.position, rightController.position);
            // float stretch = Mathf.InverseLerp(minDist, maxDist, dist);
            // stretch = Mathf.Clamp01(stretch);
            // stretch = 1f; // for now User is stretching at full strength.

            // Resistance (diminishing returns)
            float resisted = 1f - Mathf.Exp(-3f * stretch); // makes expansion harder the more you push; we use exp to do so

            targetRadius = baseRadius + maxExtraRadius * resisted;

            // moves from currentRadius to targetRadius with t =. 1- ..
            currentRadius = Mathf.Lerp(currentRadius, targetRadius, 1f - Mathf.Exp(-expandSpeed * Time.deltaTime));
            radiusVel = 0f;
            strain = Mathf.Lerp(strain, 1f, 1f - Mathf.Exp(-10f * Time.deltaTime));
        }
        else
        {
            // does the same as when trying but "inverse" (from current to base radius) and with snapSpeed > expandSpeed
            // currentRadius = Mathf.Lerp(currentRadius, baseRadius, 1f - Mathf.Exp(-snapSpeed * Time.deltaTime));
            currentRadius = Mathf.SmoothDamp(currentRadius, baseRadius, ref radiusVel, snapTime);
            strain = Mathf.Lerp(strain, 0f, 1f - Mathf.Exp(-18f * Time.deltaTime));
        }

        tunnelMaterial.SetFloat("_Radius", currentRadius);
        tunnelMaterial.SetFloat("_Feather", 0.24f);
        tunnelMaterial.SetFloat("_Darkness", 1.0f);
        tunnelMaterial.SetVector("_CenterUV", new Vector4(0.5f, 0.5f, 0, 0));
        tunnelMaterial.SetFloat("_BlurStrength", 1.0f);

        if (wasGripsHeld && !gripsHeld)
            snap = 1f;

        // snap impulse decays fast
        snap = Mathf.Lerp(snap, 0f, 1f - Mathf.Exp(-35f * Time.deltaTime));

        wasGripsHeld = gripsHeld;
        previousDist = currentDist;


        tunnelMaterial.SetFloat("_Strain", strain);
        tunnelMaterial.SetFloat("_Snap", snap);


    }

}