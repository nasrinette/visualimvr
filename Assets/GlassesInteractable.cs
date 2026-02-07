using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using SOHNE.Accessibility.Colorblindness;

public class GlassesInteractable : MonoBehaviour
{
    [Header("Color Blind Type")]
    [Tooltip("Which colorblind mode to activate when these glasses are picked up")]
    public ColorblindTypes colorblindType = ColorblindTypes.Protanopia;

    [Header("Behavior")]
    [Tooltip("Should the glasses disappear when grabbed?")]
    public bool hideOnGrab = true;

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        // Get or add the XR Grab Interactable component
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (grabInteractable == null)
        {
            Debug.LogError($"No XRGrabInteractable found on {gameObject.name}. Please add one.");
            return;
        }
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            // Subscribe to the select entered event (when grab begins)
            grabInteractable.selectEntered.AddListener(OnGlassesGrabbed);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGlassesGrabbed);
        }
    }

    void OnGlassesGrabbed(SelectEnterEventArgs args)
    {
        // Apply the colorblind filter
        if (Colorblindness.Instance != null)
        {
            // Cast the enum to int to pass the correct filter index
            Colorblindness.Instance.Change((int)colorblindType);
            
            Debug.Log($"Applied {colorblindType} filter from {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("Colorblindness system not found in scene!");
        }

        // Hide/destroy the glasses
        if (hideOnGrab)
        {
            gameObject.SetActive(false);
            // Or use: Destroy(gameObject); if you don't want them to reappear
        }
    }
}
