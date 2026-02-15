using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using SOHNE.Accessibility.Colorblindness;

public class GlassesInteractable : MonoBehaviour
{
    [Header("Color Blind Type")]
    [Tooltip("Which colorblind mode to activate when these glasses are picked up")]
    public ColorblindTypes colorblindType = ColorblindTypes.Protanopia;

    [Header("Dialogue")]
    [Tooltip("Audio clip to play when these glasses are grabbed")]
    public AudioClip dialogueClip;

    [Header("Behavior")]
    [Tooltip("Should the glasses disappear when grabbed?")]
    public bool hideOnGrab = true;

    private XRGrabInteractable grabInteractable;
    private CharacterInteractOnPoint guideCharacter;
    private MissionManager missionManager;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (grabInteractable == null)
        {
            Debug.LogError($"No XRGrabInteractable found on {gameObject.name}. Please add one.");
            return;
        }

        guideCharacter = FindObjectOfType<CharacterInteractOnPoint>();
        missionManager = FindObjectOfType<MissionManager>();
        
        if (missionManager == null)
        {
            Debug.LogError("MissionManager not found in scene!");
        }
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
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
            Colorblindness.Instance.Change((int)colorblindType);
            Debug.Log($"Applied {colorblindType} filter from {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("Colorblindness system not found in scene!");
        }

        // Tell the guide character to play dialogue
        if (guideCharacter != null && dialogueClip != null)
        {
            guideCharacter.PlayDialogue(dialogueClip);
        }

        // Tell mission manager which glasses were picked up
        if (missionManager != null)
        {
            Debug.Log($"Notifying MissionManager: {colorblindType} glasses picked up");
            missionManager.OnGlassesPickedUp(colorblindType);
        }
        else
        {
            Debug.LogError("Cannot notify MissionManager - it's null!");
        }

        // Hide the glasses
        if (hideOnGrab)
        {
            gameObject.SetActive(false);
        }
    }
}
