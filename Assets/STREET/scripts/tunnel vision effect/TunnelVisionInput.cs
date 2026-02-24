using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class TunnelVisionInput : MonoBehaviour
{
    [Header("References")]
    public Transform leftController;
    public Transform rightController;

    [Header("Input Actions")]
    public InputActionProperty leftGrip;
    public InputActionProperty rightGrip;

    [Header("Material")]
    public Material tunnelMaterial;


    [Header("Tunnel")]
    public float baseRadius = 0.15f;
    public float maxExtraRadius = 0.08f;
    public float expandSpeed = 12f;


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
        // for debuging
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P pressed");
            scenario.OnTunnelExpandAttempted();
        }

        if (!tunnelMaterial || !leftController || !rightController) return;

        // we need left and right back buttons pressed
        var leftAction = GetLeftGripAction();
        var rightAction = GetRightGripAction();

        bool leftHeld = leftAction != null && leftAction.IsPressed();

        bool rightHeld = rightAction != null && rightAction.IsPressed();

        bool gripsHeld = leftHeld && rightHeld;

        // Measure how far the hands moved apart since the grips were first pressed
        float currentDist = Vector3.Distance(leftController.position, rightController.position);

        if (gripsHeld && !wasGripsHeld)
        {
            grabStartDist = currentDist;
        }


        float stretch = 0f;

        if (gripsHeld)
        {
            // we use the grabStartDist and currentDist to impact how much it will stretch
            float delta = currentDist - grabStartDist;
            stretch = Mathf.InverseLerp(0f, maxStretchAmount, delta);
            stretch = Mathf.Clamp01(stretch);
        }

        // trying= grips held AND actually stretched a bit
        bool trying = gripsHeld && stretch > 0.02f;

        float targetRadius = baseRadius;


        if (trying)
        {
            // this fire the scenario event once when the user starts trying
            if (!wasTrying)
            {
                scenario.RequestTunnelExpandAttempt();

            }

            wasTrying = trying;
           
            // resistance : harder to expand the more you pull
            float resisted = 1f - Mathf.Exp(-3f * stretch); // makes expansion harder the more you push; we use exp to do so

            targetRadius = baseRadius + maxExtraRadius * resisted;

            // smooth expand towards the target: moves from currentRadius to targetRadius with t = 1- ..
            currentRadius = Mathf.Lerp(currentRadius, targetRadius, 1f - Mathf.Exp(-expandSpeed * Time.deltaTime));
            radiusVel = 0f;


            strain = Mathf.Lerp(strain, 1f, 1f - Mathf.Exp(-10f * Time.deltaTime));
        }
        else
        {
            // snaps back smoothly to base radius
            // does the same as when trying but "inverse" (from current to base radius) 
            currentRadius = Mathf.SmoothDamp(currentRadius, baseRadius, ref radiusVel, snapTime);
            strain = Mathf.Lerp(strain, 0f, 1f - Mathf.Exp(-18f * Time.deltaTime));
        }

        // apply those parameters to the shader
        tunnelMaterial.SetFloat("_Radius", currentRadius);
        tunnelMaterial.SetFloat("_Feather", 0.24f);
        tunnelMaterial.SetFloat("_Darkness", 1.0f);
        tunnelMaterial.SetVector("_CenterUV", new Vector4(0.5f, 0.5f, 0, 0));
        tunnelMaterial.SetFloat("_BlurStrength", 1.0f);

        // snap when buttons are released
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

        tunnelMaterial.SetFloat("_ShowArrows", showArrows);
    }

    public void ReduceBaseRadius(float amount)
    {
        baseRadius = Mathf.Max(0.1f, baseRadius - amount);
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