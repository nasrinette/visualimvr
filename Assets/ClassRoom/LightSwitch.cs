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

    [Header("State")]
    [SerializeField] private bool lightsOn = false;
    public bool IsOn => lightsOn;

    private XRSimpleInteractable interactable;

    private static readonly Color switchOnColor = new Color(0.1f, 0.8f, 0.1f);
    private static readonly Color switchOffColor = new Color(0.8f, 0.1f, 0.1f);

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSwitchPressed);
    }

    void OnDisable()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSwitchPressed);
    }

    void Start()
    {
        ApplyLightState();
    }

    void OnSwitchPressed(SelectEnterEventArgs args)
    {
        lightsOn = !lightsOn;
        ApplyLightState();
        Debug.Log($"Light switch toggled: lights are now {(lightsOn ? "ON" : "OFF")}");
    }

    void ApplyLightState()
    {
        foreach (var light in lights)
        {
            if (light != null)
                light.enabled = lightsOn;
        }

        if (switchRenderer != null)
            switchRenderer.material.color = lightsOn ? switchOnColor : switchOffColor;
    }
}
