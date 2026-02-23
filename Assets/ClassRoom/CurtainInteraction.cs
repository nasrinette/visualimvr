using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Two-handed curtain interaction. Each curtain has a left and right piece with
/// hooks at their inner edges. When both hooks are grabbed simultaneously,
/// pulling hands apart opens the curtains, pushing them together closes them.
/// Curtain pieces animate smoothly and compress (scale) when bunched up at rod ends.
/// </summary>
public class CurtainInteraction : MonoBehaviour
{
    [Header("Curtain Panels")]
    [Tooltip("Left curtain piece (curtain_flannel (1))")]
    public Transform leftCurtain;
    [Tooltip("Right curtain piece (curtain_flannel)")]
    public Transform rightCurtain;

    [Header("Hooks")]
    [Tooltip("Hook at inner edge of left curtain")]
    public XRSimpleInteractable leftHook;
    [Tooltip("Hook at inner edge of right curtain")]
    public XRSimpleInteractable rightHook;

    [Header("Movement Limits (Local X)")]
    [Tooltip("Left curtain X when fully open (pushed to side)")]
    public float leftOpenX = -0.85f;
    [Tooltip("Left curtain X when fully closed (at center)")]
    public float leftClosedX = -0.476f;
    [Tooltip("Right curtain X when fully closed (at center)")]
    public float rightClosedX = 0.334f;
    [Tooltip("Right curtain X when fully open (pushed to side)")]
    public float rightOpenX = 0.533f;

    [Header("Default Positions (Half Open)")]
    public float leftHalfOpenX = -0.680f;
    public float rightHalfOpenX = 0.363f;

    [Header("Scale Settings")]
    [Tooltip("X scale when curtain is fully closed (full width)")]
    public float closedScaleX = 0.679f;
    [Tooltip("X scale when curtain is fully open (bunched up)")]
    public float openScaleX = 0.4f;

    [Header("Hook Settings")]
    [Tooltip("Half-width of each curtain piece in local space (at closedScaleX)")]
    public float curtainHalfWidth = 0.405f;

    [Header("Animation")]
    [Tooltip("How fast the curtains follow the target position")]
    public float animationSpeed = 8f;

    [Header("Settings")]
    [Tooltip("Multiplier for hand movement to curtain movement")]
    public float sensitivity = 1f;

    [Header("Sound")]
    [Tooltip("Sound played while curtains are moving")]
    public AudioClip curtainSound;

    private AudioSource audioSource;
    private IXRSelectInteractor leftInteractor;
    private IXRSelectInteractor rightInteractor;
    private Vector3 leftGrabStartWorld;
    private Vector3 rightGrabStartWorld;
    private float leftCurtainGrabStartX;
    private float rightCurtainGrabStartX;

    private float leftTargetX;
    private float rightTargetX;
    private bool initialized;

    void OnEnable()
    {
        if (leftHook != null)
        {
            leftHook.selectEntered.AddListener(OnLeftHookGrabbed);
            leftHook.selectExited.AddListener(OnLeftHookReleased);
        }
        if (rightHook != null)
        {
            rightHook.selectEntered.AddListener(OnRightHookGrabbed);
            rightHook.selectExited.AddListener(OnRightHookReleased);
        }

        // Reset curtains to fully open on every classroom entry
        if (initialized)
            ResetToOpen();
    }

    void OnDisable()
    {
        if (leftHook != null)
        {
            leftHook.selectEntered.RemoveListener(OnLeftHookGrabbed);
            leftHook.selectExited.RemoveListener(OnLeftHookReleased);
        }
        if (rightHook != null)
        {
            rightHook.selectEntered.RemoveListener(OnRightHookGrabbed);
            rightHook.selectExited.RemoveListener(OnRightHookReleased);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.loop = true;

        if (leftCurtain != null) leftTargetX = leftCurtain.localPosition.x;
        if (rightCurtain != null) rightTargetX = rightCurtain.localPosition.x;
        ApplyScaleFromPosition();
        UpdateHookPositions();
        initialized = true;
    }

    void OnLeftHookGrabbed(SelectEnterEventArgs args)
    {
        leftInteractor = args.interactorObject;
        leftGrabStartWorld = GetInteractorPosition(leftInteractor);
        leftCurtainGrabStartX = leftTargetX;
        PlayCurtainSound();
        Debug.Log("Left curtain hook grabbed");
    }

    void OnLeftHookReleased(SelectExitEventArgs args)
    {
        leftInteractor = null;
        if (rightInteractor == null) StopCurtainSound();
        Debug.Log("Left curtain hook released");
    }

    void OnRightHookGrabbed(SelectEnterEventArgs args)
    {
        rightInteractor = args.interactorObject;
        rightGrabStartWorld = GetInteractorPosition(rightInteractor);
        rightCurtainGrabStartX = rightTargetX;
        PlayCurtainSound();
        Debug.Log("Right curtain hook grabbed");
    }

    void OnRightHookReleased(SelectExitEventArgs args)
    {
        rightInteractor = null;
        if (leftInteractor == null) StopCurtainSound();
        Debug.Log("Right curtain hook released");
    }

    void PlayCurtainSound()
    {
        if (curtainSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = curtainSound;
            audioSource.Play();
        }
    }

    void StopCurtainSound()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    void Update()
    {
        Vector3 localXWorld = transform.TransformDirection(Vector3.right);

        // Update left curtain target from hand movement
        if (leftInteractor != null)
        {
            Vector3 leftCurrentWorld = GetInteractorPosition(leftInteractor);
            float leftDelta = Vector3.Dot(leftCurrentWorld - leftGrabStartWorld, localXWorld) * sensitivity;
            leftTargetX = Mathf.Clamp(leftCurtainGrabStartX + leftDelta, leftOpenX, leftClosedX);
        }

        // Update right curtain target from hand movement
        if (rightInteractor != null)
        {
            Vector3 rightCurrentWorld = GetInteractorPosition(rightInteractor);
            float rightDelta = Vector3.Dot(rightCurrentWorld - rightGrabStartWorld, localXWorld) * sensitivity;
            rightTargetX = Mathf.Clamp(rightCurtainGrabStartX + rightDelta, rightClosedX, rightOpenX);
        }

        // Smoothly animate toward targets
        AnimateCurtains();
        UpdateHookPositions();
    }

    void AnimateCurtains()
    {
        float dt = Time.deltaTime * animationSpeed;

        if (leftCurtain != null)
        {
            Vector3 pos = leftCurtain.localPosition;
            pos.x = Mathf.Lerp(pos.x, leftTargetX, dt);
            leftCurtain.localPosition = pos;

            float t = Mathf.InverseLerp(leftClosedX, leftOpenX, pos.x);
            Vector3 scale = leftCurtain.localScale;
            scale.x = Mathf.Lerp(closedScaleX, openScaleX, t);
            leftCurtain.localScale = scale;
        }

        if (rightCurtain != null)
        {
            Vector3 pos = rightCurtain.localPosition;
            pos.x = Mathf.Lerp(pos.x, rightTargetX, dt);
            rightCurtain.localPosition = pos;

            float t = Mathf.InverseLerp(rightClosedX, rightOpenX, pos.x);
            Vector3 scale = rightCurtain.localScale;
            scale.x = Mathf.Lerp(closedScaleX, openScaleX, t);
            rightCurtain.localScale = scale;
        }
    }

    void ApplyScaleFromPosition()
    {
        if (leftCurtain != null)
        {
            float t = Mathf.InverseLerp(leftClosedX, leftOpenX, leftCurtain.localPosition.x);
            Vector3 scale = leftCurtain.localScale;
            scale.x = Mathf.Lerp(closedScaleX, openScaleX, t);
            leftCurtain.localScale = scale;
        }
        if (rightCurtain != null)
        {
            float t = Mathf.InverseLerp(rightClosedX, rightOpenX, rightCurtain.localPosition.x);
            Vector3 scale = rightCurtain.localScale;
            scale.x = Mathf.Lerp(closedScaleX, openScaleX, t);
            rightCurtain.localScale = scale;
        }
    }

    void UpdateHookPositions()
    {
        if (leftHook != null && leftCurtain != null)
        {
            float scaledHalfWidth = curtainHalfWidth * (leftCurtain.localScale.x / closedScaleX);
            Vector3 hookPos = leftHook.transform.localPosition;
            hookPos.x = leftCurtain.localPosition.x + scaledHalfWidth;
            hookPos.y = leftCurtain.localPosition.y;
            leftHook.transform.localPosition = hookPos;
        }

        if (rightHook != null && rightCurtain != null)
        {
            float scaledHalfWidth = curtainHalfWidth * (rightCurtain.localScale.x / closedScaleX);
            Vector3 hookPos = rightHook.transform.localPosition;
            hookPos.x = rightCurtain.localPosition.x - scaledHalfWidth;
            hookPos.y = rightCurtain.localPosition.y;
            rightHook.transform.localPosition = hookPos;
        }
    }

    Vector3 GetInteractorPosition(IXRSelectInteractor interactor)
    {
        if (interactor is Component comp)
            return comp.transform.position;
        return Vector3.zero;
    }

    public void SetTargets(float leftX, float rightX)
    {
        leftTargetX = leftX;
        rightTargetX = rightX;
    }

    /// <summary>
    /// Performs one animation step with the given delta time. Returns true when done.
    /// Used by the custom editor to animate in edit mode.
    /// </summary>
    public bool AnimateStep(float dt)
    {
        float smoothDt = dt * animationSpeed;
        bool done = true;

        if (leftCurtain != null)
        {
            Vector3 pos = leftCurtain.localPosition;
            pos.x = Mathf.Lerp(pos.x, leftTargetX, smoothDt);
            leftCurtain.localPosition = pos;

            float t = Mathf.InverseLerp(leftClosedX, leftOpenX, pos.x);
            Vector3 scale = leftCurtain.localScale;
            scale.x = Mathf.Lerp(closedScaleX, openScaleX, t);
            leftCurtain.localScale = scale;

            if (Mathf.Abs(pos.x - leftTargetX) > 0.001f) done = false;
        }

        if (rightCurtain != null)
        {
            Vector3 pos = rightCurtain.localPosition;
            pos.x = Mathf.Lerp(pos.x, rightTargetX, smoothDt);
            rightCurtain.localPosition = pos;

            float t = Mathf.InverseLerp(rightClosedX, rightOpenX, pos.x);
            Vector3 scale = rightCurtain.localScale;
            scale.x = Mathf.Lerp(closedScaleX, openScaleX, t);
            rightCurtain.localScale = scale;

            if (Mathf.Abs(pos.x - rightTargetX) > 0.001f) done = false;
        }

        UpdateHookPositions();
        return done;
    }

    /// <summary>
    /// Returns 0 = fully open, 1 = fully closed.
    /// </summary>
    public float GetClosedAmount()
    {
        float leftT = 0f, rightT = 0f;
        if (leftCurtain != null)
            leftT = Mathf.InverseLerp(leftOpenX, leftClosedX, leftCurtain.localPosition.x);
        if (rightCurtain != null)
            rightT = Mathf.InverseLerp(rightOpenX, rightClosedX, rightCurtain.localPosition.x);
        return (leftT + rightT) * 0.5f;
    }

    private void ResetToOpen()
    {
        leftTargetX = leftOpenX;
        rightTargetX = rightOpenX;

        // Snap curtain positions immediately (no animation)
        if (leftCurtain != null)
        {
            Vector3 pos = leftCurtain.localPosition;
            pos.x = leftOpenX;
            leftCurtain.localPosition = pos;
        }
        if (rightCurtain != null)
        {
            Vector3 pos = rightCurtain.localPosition;
            pos.x = rightOpenX;
            rightCurtain.localPosition = pos;
        }

        ApplyScaleFromPosition();
        UpdateHookPositions();
    }

    public void DebugSetOpen() => SetTargets(leftOpenX, rightOpenX);
    public void DebugSetClosed() => SetTargets(leftClosedX, rightClosedX);
    public void DebugSetHalfOpen() => SetTargets(leftHalfOpenX, rightHalfOpenX);
}
