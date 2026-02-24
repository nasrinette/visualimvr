using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LightSwitch : MonoBehaviour
{
    [Header("Lights to Control")]
    [Tooltip("Drag all the lights this switch should toggle")]
    public Light[] lights;

    [Header("Switch Visual")]
    [Tooltip("Renderer for the switch itself (changes color: green=ON, red=OFF)")]
    public Renderer switchRenderer;

    [Header("Switch Toggle Animation")]
    [Tooltip("The toggle part of the switch model (auto-found child named 'Switch.001' if empty)")]
    public Transform switchToggle;
    [Tooltip("Degrees to rotate on local X axis when ON (negative = flip down)")]
    public float toggleFlipAngle = -10f;

    [Header("Sound")]
    [Tooltip("Sound played when switch is toggled")]
    public AudioClip toggleSound;

    [Header("State")]
    [SerializeField] private bool lightsOn = false;
    public bool IsOn => lightsOn;

    private XRSimpleInteractable interactable;
    private Quaternion toggleOriginalRotation;
    private AudioSource audioSource;
    private bool initialized;

    private static readonly Color switchOnColor = new Color(0.1f, 0.8f, 0.1f);
    private static readonly Color switchOffColor = new Color(0.8f, 0.1f, 0.1f);

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    // reset on entry
    void OnEnable()
    {
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSwitchPressed);

        if (initialized)
        {
            lightsOn = false;
            ApplyLightState();
        }
    }

    void OnDisable()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSwitchPressed);
    }

    void Start()
    {
        if (switchToggle == null)
            switchToggle = transform.Find("Switch.001");
        if (switchToggle != null)
            toggleOriginalRotation = switchToggle.localRotation;
        lightsOn = false;
        ApplyLightState();
        initialized = true;
    }

    void OnSwitchPressed(SelectEnterEventArgs args)
    {
        Toggle();
    }

    // flip state
    public void Toggle()
    {
        lightsOn = !lightsOn;
        ApplyLightState();
        if (toggleSound != null && audioSource != null)
            audioSource.PlayOneShot(toggleSound);
        Debug.Log($"Light switch toggled: lights are now {(lightsOn ? "ON" : "OFF")}");
    }

    // update visuals
    void ApplyLightState()
    {
        foreach (var light in lights)
        {
            if (light != null)
                light.enabled = lightsOn;
        }

        if (switchRenderer != null)
            switchRenderer.material.color = lightsOn ? switchOnColor : switchOffColor;

        if (switchToggle != null)
        {
            switchToggle.localRotation = lightsOn
                ? toggleOriginalRotation * Quaternion.Euler(0f, 0f, toggleFlipAngle)
                : toggleOriginalRotation;
        }
    }
}
