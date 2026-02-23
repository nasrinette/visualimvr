using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

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
    public float baseRadius = 0.15f;
    public float maxExtraRadius = 0.08f;
    public float expandSpeed = 12f;
    public float snapSpeed = 18f;

    [Header("Fallback (Keyboard)")]
    public KeyCode debugKey = KeyCode.E;

    float snapTime = 0.03f;
    float currentRadius;

    float strain = 0f;
    float snap = 0f;          // decays to 0 quickly
    float radiusVel = 0f;

    float previousDist;
    float grabStartDist;
    float maxStretchAmount = 0.25f;

    bool wasGripsHeld;

    public ScenarioNarration scenario;

    InputAction runtimeLeftGrip;
    InputAction runtimeRightGrip;
    bool wasTrying;

    void Awake()
    {
        currentRadius = baseRadius;
    }

    void Start()
    {
        // If the Inspector-configured actions have no bindings, create runtime actions
        if (!HasBindings(leftGrip))
        {
            Debug.Log("[TunnelVisionInput] leftGrip has no bindings, creating runtime action for left grip button");
            runtimeLeftGrip = new InputAction("LeftGrip", InputActionType.Button);
            runtimeLeftGrip.AddBinding("<XRController>{LeftHand}/gripButton");
            runtimeLeftGrip.AddBinding("<XRController>{LeftHand}/grip");
            runtimeLeftGrip.Enable();
        }
        else
        {
            leftGrip.action?.Enable();
        }

        if (!HasBindings(rightGrip))
        {
            Debug.Log("[TunnelVisionInput] rightGrip has no bindings, creating runtime action for right grip button");
            runtimeRightGrip = new InputAction("RightGrip", InputActionType.Button);
            runtimeRightGrip.AddBinding("<XRController>{RightHand}/gripButton");
            runtimeRightGrip.AddBinding("<XRController>{RightHand}/grip");
            runtimeRightGrip.Enable();
        }
        else
        {
            rightGrip.action?.Enable();
        }
    }

    bool HasBindings(InputActionProperty prop)
    {
        if (prop.action == null) return false;
        return prop.action.bindings.Count > 0;
    }

    InputAction GetLeftGripAction()
    {
        return runtimeLeftGrip != null ? runtimeLeftGrip : leftGrip.action;
    }

    InputAction GetRightGripAction()
    {
        return runtimeRightGrip != null ? runtimeRightGrip : rightGrip.action;
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P pressed");
            scenario.OnTunnelExpandAttempted();
        }

        if (!tunnelMaterial || !leftController || !rightController) return;

        var leftAction = GetLeftGripAction();
        var rightAction = GetRightGripAction();

        bool leftHeld = leftAction != null && leftAction.IsPressed();
        if (leftHeld) Debug.Log("pressed on left");

        bool rightHeld = rightAction != null && rightAction.IsPressed();
        if (rightHeld) Debug.Log("pressed on right");

        bool gripsHeld = leftHeld && rightHeld;
        if (gripsHeld) Debug.Log("pressed on both");


        float currentDist = Vector3.Distance(leftController.position, rightController.position);
        // Debug.Log("current dist: "+currentDist);
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
            if (!wasTrying)
            {
                scenario.OnTunnelExpandAttempted();

            }

            wasTrying = trying;
            // scenario.OnTunnelExpandAttempted();
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

        // when we are in the try expand audio (explanation), we make the help arrows appear
        // otherwise they disappear
        bool inTryExpand = scenario != null && scenario.CurrentPhase == ScenarioNarration.Phase.TryExpandTunnel;

        float showArrows = (inTryExpand && !trying) ? 1f : 0f;

        float glow = 0f;
        if (inTryExpand) glow = 0.25f;
        // if (gripsHeld) glow = 0.3f;

        tunnelMaterial.SetFloat("_ShowArrows", showArrows);
        tunnelMaterial.SetFloat("_EdgeGlow", glow);
    }

    public void ReduceBaseRadius(float amount)
    {
        baseRadius = Mathf.Max(0f, baseRadius - amount);
        // currentRadius = Mathf.Min(currentRadius, baseRadius);

        Debug.Log($"[TUNNEL] baseRadius reduced -> {baseRadius}");
    }
    void OnDestroy()
    {
        runtimeLeftGrip?.Disable();
        runtimeLeftGrip?.Dispose();
        runtimeRightGrip?.Disable();
        runtimeRightGrip?.Dispose();
    }


}