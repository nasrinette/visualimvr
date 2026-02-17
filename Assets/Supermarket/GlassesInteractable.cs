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
    
    [Tooltip("Audio clip to play when trying to grab while already wearing glasses")]
    public AudioClip alreadyWearingGlassesClip;

    [Header("Behavior")]
    [Tooltip("Should the glasses disappear when grabbed?")]
    public bool hideOnGrab = true;

    private XRGrabInteractable grabInteractable;
    private CharacterInteractOnPoint guideCharacter;
    private MissionManager missionManager;
    
    private static bool isWearingGlasses = false;
    private static ColorblindTypes currentGlassesType;
    
    // Track hover attempts
    private bool hasShownWarning = false;

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
            grabInteractable.hoverEntered.AddListener(OnGlassesHovered);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGlassesGrabbed);
            grabInteractable.hoverEntered.RemoveListener(OnGlassesHovered);
        }
    }

    void Update()
    {
        // Disable interaction if already wearing glasses
        if (grabInteractable != null)
        {
            // Only allow interaction if not wearing glasses OR these are the glasses being worn
            grabInteractable.enabled = !isWearingGlasses;
        }
    }

    void OnGlassesHovered(HoverEnterEventArgs args)
    {
        // Show warning when hovering over glasses while wearing another pair
        if (isWearingGlasses && !hasShownWarning)
        {
            Debug.Log($"Cannot pick up {colorblindType} glasses - already wearing {currentGlassesType} glasses!");
            
            if (guideCharacter != null && alreadyWearingGlassesClip != null)
            {
                guideCharacter.PlayDialogue(alreadyWearingGlassesClip);
            }
            
            hasShownWarning = true;
            Invoke(nameof(ResetWarning), 3f); // Reset after 3 seconds
        }
    }

    void ResetWarning()
    {
        hasShownWarning = false;
    }

    void OnGlassesGrabbed(SelectEnterEventArgs args)
    {
        // Double check - should not reach here if Update() is working
        if (isWearingGlasses)
        {
            Debug.LogWarning("Glasses grab attempted while already wearing - this shouldn't happen!");
            grabInteractable.interactionManager.CancelInteractableSelection(grabInteractable);
            return;
        }

        // Mark glasses as being worn
        isWearingGlasses = true;
        currentGlassesType = colorblindType;

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
            
            EnterRoom enterRoom = FindObjectOfType<EnterRoom>();
            if (enterRoom != null)
            {
                enterRoom.RefreshDoorBlockers();
            }
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

    public static void RemoveGlasses()
    {
        isWearingGlasses = false;
        Debug.Log($"Glasses removed - can now pick up new glasses");
    }

    public static bool IsWearingGlasses()
    {
        return isWearingGlasses;
    }

    public static ColorblindTypes GetCurrentGlassesType()
    {
        return currentGlassesType;
    }
}
