using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LightSwitch : MonoBehaviour
{
    [Header("Lights to Control")]
    [Tooltip("Drag all the lights this switch should toggle")]
    public Light[] lights;

    [Header("Light Panel Visuals")]
    [Tooltip("Renderers for the ceiling light panels (will swap materials on toggle)")]
    public Renderer[] lightPanels;
    [Tooltip("Material for panels when lights are ON (emissive)")]
    public Material panelOnMaterial;
    [Tooltip("Material for panels when lights are OFF (dark)")]
    public Material panelOffMaterial;

    [Header("State")]
    [SerializeField] private bool lightsOn = true;

    private XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSwitchPressed);
        }
    }

    void OnDisable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSwitchPressed);
        }
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

        Material panelMat = lightsOn ? panelOnMaterial : panelOffMaterial;
        if (panelMat != null)
        {
            foreach (var panel in lightPanels)
            {
                if (panel != null)
                    panel.material = panelMat;
            }
        }
    }
}
