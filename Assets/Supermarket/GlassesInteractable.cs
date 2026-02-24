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
    private bool hasPlayedDialogue = false; 

    
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
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGlassesGrabbed);
        }
    }

    void Update()
    {
        // Disable interaction if already wearing glasses
        if (grabInteractable != null)
        {
            // Only allow interaction if not wearing glasses 
            grabInteractable.enabled = !isWearingGlasses;
        }
    }


    void ResetWarning()
    {
        hasShownWarning = false;
    }


    void OnGlassesGrabbed(SelectEnterEventArgs args)
    {
        if (isWearingGlasses)
        {
            grabInteractable.interactionManager.CancelInteractableSelection(grabInteractable);
            return;
        }

        isWearingGlasses = true;
        currentGlassesType = colorblindType;

        if (Colorblindness.Instance != null)
        {
            //Activate the colorblind filter in the scene based on the type of glasses picked up
            Colorblindness.Instance.Change((int)colorblindType);
            Debug.Log($"Applied {colorblindType} filter from {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("Colorblindness system not found in scene!");
        }

        // Only play dialogue once per grab
        if (!hasPlayedDialogue && guideCharacter != null && dialogueClip != null)
        {
            guideCharacter.PlayDialogue(dialogueClip);
            hasPlayedDialogue = true;
        }

        if (missionManager != null)
        {
            missionManager.OnGlassesPickedUp(colorblindType);
            EnterRoom enterRoom = FindObjectOfType<EnterRoom>();
            if (enterRoom != null)
                enterRoom.RefreshDoorBlockers();
        }
        else
        {
            Debug.LogError("Cannot notify MissionManager - it's null!");
        }

        if (hideOnGrab)
            gameObject.SetActive(false);
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
