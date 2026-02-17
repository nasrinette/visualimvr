using UnityEngine;
using SOHNE.Accessibility.Colorblindness;

public class MissionManager : MonoBehaviour
{
    public enum MissionPhase
    {
        Introduction,
        Mission1_GetTomato,
        Mission1_Reveal,
        Mission2_GetPepper,
        Mission2_Reveal,
        Mission3_GetEggplant,
        Mission3_Reveal,
        Complete
    }
    
    [Header("Current State")]
    [SerializeField] private MissionPhase currentPhase = MissionPhase.Introduction;
    
    [Header("References")]
    [SerializeField] private CharacterInteractOnPoint guideCharacter;
    
    [Header("Reveal Dialogues")]
    [SerializeField] private AudioClip wrongTomatoDialogue;    // "Take off glasses... that's a green tomato!"
    [SerializeField] private AudioClip wrongPepperDialogue;    // "That's a yellow pepper!"
    [SerializeField] private AudioClip wrongEggplantDialogue;  // "That's a zucchini!"
    
    private SupermarketItem.ItemType currentObjective;

    void Start()
    {
        StartIntroduction();
    }

    void StartIntroduction()
    {
        currentPhase = MissionPhase.Introduction;
    }

    public void OnGlassesPickedUp(ColorblindTypes glassType)
    {
        switch(glassType)
        {
            case ColorblindTypes.Protanopia:
                StartMission1();
                break;
            case ColorblindTypes.Deuteranopia:
                StartMission2();
                break;
            case ColorblindTypes.Tritanopia:
                StartMission3();
                break;
        }
    }

    void StartMission1()
    {
        currentPhase = MissionPhase.Mission1_GetTomato;
        currentObjective = SupermarketItem.ItemType.Tomato;
        Debug.Log("Mission 1: Bring me a ripe tomato!");
    }

    void StartMission2()
    {
        currentPhase = MissionPhase.Mission2_GetPepper;
        currentObjective = SupermarketItem.ItemType.BellPepper;
        Debug.Log("Mission 2: Bring me a red bell pepper!");
    }

    void StartMission3()
    {
        currentPhase = MissionPhase.Mission3_GetEggplant;
        currentObjective = SupermarketItem.ItemType.Eggplant;
        Debug.Log("Mission 3: Bring me a purple eggplant!");
    }

    public void OnItemDelivered(SupermarketItem item)
    {
        Debug.Log($"Item received: Expected {currentObjective}, Got {item.itemType}");
        Debug.Log($"Current Phase BEFORE reveal: {currentPhase}");
        
        // Remove color blindness effect (simulating taking off glasses)
        RemoveColorBlindness();
        
        // Wait a moment, then show the reveal
        Debug.Log("Invoking ShowReveal in 1 second...");
        Invoke("ShowReveal", 1f);
    }

    void RemoveColorBlindness()
    {
        if (Colorblindness.Instance != null)
        {
            Colorblindness.Instance.Change(0); // 0 = normal vision
            Debug.Log("Glasses removed - normal vision restored");
        }
    }

    void ShowReveal()
    {
        Debug.Log("=== ShowReveal CALLED ===");
        Debug.Log($"Current Phase in ShowReveal: {currentPhase}");
        
        // Guide reacts to wrong item
        AudioClip revealDialogue = null;
        
        switch(currentPhase)
        {
            case MissionPhase.Mission1_GetTomato:
                revealDialogue = wrongTomatoDialogue;
                Debug.Log($"Selected wrongTomatoDialogue: {wrongTomatoDialogue != null}");
                break;
            case MissionPhase.Mission2_GetPepper:
                revealDialogue = wrongPepperDialogue;
                Debug.Log($"Selected wrongPepperDialogue: {wrongPepperDialogue != null}");
                break;
            case MissionPhase.Mission3_GetEggplant:
                revealDialogue = wrongEggplantDialogue;
                Debug.Log($"Selected wrongEggplantDialogue: {wrongEggplantDialogue != null}");
                break;
            default:
                Debug.LogWarning($"Unexpected phase: {currentPhase}");
                break;
        }
        
        Debug.Log($"revealDialogue is null: {revealDialogue == null}");
        Debug.Log($"guideCharacter is null: {guideCharacter == null}");
        
        if (revealDialogue != null && guideCharacter != null)
        {
            Debug.Log("*** PLAYING DIALOGUE NOW ***");
            guideCharacter.PlayDialogue(revealDialogue);
        }
        else
        {
            if (revealDialogue == null)
                Debug.LogError("revealDialogue is NULL! Check MissionManager Inspector - is the audio clip assigned?");
            if (guideCharacter == null)
                Debug.LogError("guideCharacter is NULL! Check MissionManager Inspector - is the character assigned?");
        }
        
        // Move to next phase after dialogue
        Invoke("AdvanceToNextMission", 5f);
    }

    void AdvanceToNextMission()
    {
        Debug.Log("=== AdvanceToNextMission CALLED ===");
        
        switch(currentPhase)
        {
            case MissionPhase.Mission1_GetTomato:
                currentPhase = MissionPhase.Mission1_Reveal;
                Debug.Log("Mission 1 complete! Ready for Mission 2 - Pick up green glasses");
                break;
            case MissionPhase.Mission2_GetPepper:
                currentPhase = MissionPhase.Mission2_Reveal;
                Debug.Log("Mission 2 complete! Ready for Mission 3 - Pick up blue glasses");
                break;
            case MissionPhase.Mission3_GetEggplant:
                currentPhase = MissionPhase.Mission3_Reveal;
                Debug.Log("All missions complete!");
                break;
        }
    }
}
