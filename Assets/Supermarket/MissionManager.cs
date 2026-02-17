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
    
    private bool hasGrabbedGlasses = false;
    private bool supermarketTasksComplete = false;

    [Header("References")]
    [SerializeField] private CharacterInteractOnPoint guideCharacter;

    [Header("Reveal Dialogues")]
    [SerializeField] private AudioClip wrongTomatoDialogue; // "That's a green tomato!"
    [SerializeField] private AudioClip wrongPepperDialogue; // "That's a yellow pepper!"
    [SerializeField] private AudioClip wrongEggplantDialogue; // "That's a zucchini!"
    
    [Header("Error Dialogues")]
    [SerializeField] private AudioClip wrongItemDialogue; // "That's not what I asked for!"
    [SerializeField] private AudioClip noMissionDialogue; // "I didn't ask for anything yet!"

    private SupermarketItem.ItemType currentObjective;
    private bool hasMissionActive = false; // Track if a mission is active

    void Start()
    {
        StartIntroduction();
    }

    void StartIntroduction()
    {
        currentPhase = MissionPhase.Introduction;
        hasMissionActive = false;
        GlassesInteractable.RemoveGlasses();
    }

    public bool HasGrabbedGlasses()
    {
        return hasGrabbedGlasses;
    }

    public bool AreSupermarketTasksComplete()
    {
        return supermarketTasksComplete;
    }

    public void OnGlassesPickedUp(ColorblindTypes glassType)
    {
        hasGrabbedGlasses = true;
        
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
        hasMissionActive = true;
        Debug.Log("Mission 1: Bring me a ripe tomato!");
    }

    void StartMission2()
    {
        currentPhase = MissionPhase.Mission2_GetPepper;
        currentObjective = SupermarketItem.ItemType.BellPepper;
        hasMissionActive = true;
        Debug.Log("Mission 2: Bring me a red bell pepper!");
    }

    void StartMission3()
    {
        currentPhase = MissionPhase.Mission3_GetEggplant;
        currentObjective = SupermarketItem.ItemType.Eggplant;
        hasMissionActive = true;
        Debug.Log("Mission 3: Bring me a purple eggplant!");
    }

    public void OnItemDelivered(SupermarketItem item)
    {
        Debug.Log($"=== ITEM DELIVERED ===");
        Debug.Log($"Current Phase: {currentPhase}");
        Debug.Log($"Expected: {currentObjective}");
        Debug.Log($"Received: {item.itemType}");
        Debug.Log($"Mission Active: {hasMissionActive}");
        
        // Check if there's even a mission active
        if (!hasMissionActive)
        {
            Debug.LogWarning("❌ No mission active! Player brought item without mission.");
            
            if (guideCharacter != null && noMissionDialogue != null)
            {
                guideCharacter.PlayDialogue(noMissionDialogue);
            }
            
            return;
        }
        
        // Verify if the correct item was delivered
        if (item.itemType != currentObjective)
        {
            Debug.LogWarning($"❌ WRONG ITEM! Expected {currentObjective} but got {item.itemType}");
            
            if (guideCharacter != null && wrongItemDialogue != null)
            {
                guideCharacter.PlayDialogue(wrongItemDialogue);
            }
            
            return; // Don't proceed - let player try again
        }
        
        Debug.Log("✓✓✓ CORRECT ITEM DELIVERED! Proceeding with reveal...");
        hasMissionActive = false; // Mission completed
        
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
        
        GlassesInteractable.RemoveGlasses();
        Debug.Log("✓ Glasses state reset - can pick up new glasses now");
    }

    void ShowReveal()
    {
        Debug.Log("=== ShowReveal CALLED ===");
        Debug.Log($"Current Phase in ShowReveal: {currentPhase}");

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
                Debug.LogError("revealDialogue is NULL!");
            if (guideCharacter == null)
                Debug.LogError("guideCharacter is NULL!");
        }

        Invoke("AdvanceToNextMission", 5f);
    }

    void AdvanceToNextMission()
    {
        Debug.Log("=== AdvanceToNextMission CALLED ===");
        switch(currentPhase)
        {
            case MissionPhase.Mission1_GetTomato:
                currentPhase = MissionPhase.Mission1_Reveal;
                GlassesInteractable.RemoveGlasses();
                Debug.Log("Mission 1 complete! ✓ Glasses reset - Ready for Mission 2");
                break;
            case MissionPhase.Mission2_GetPepper:
                currentPhase = MissionPhase.Mission2_Reveal;
                GlassesInteractable.RemoveGlasses();
                Debug.Log("Mission 2 complete! ✓ Glasses reset - Ready for Mission 3");
                break;
            case MissionPhase.Mission3_GetEggplant:
                currentPhase = MissionPhase.Mission3_Reveal;
                supermarketTasksComplete = true;
                GlassesInteractable.RemoveGlasses();
                Debug.Log("All missions complete! ✓ Glasses reset");
                
                EnterRoom enterRoom = FindObjectOfType<EnterRoom>();
                if (enterRoom != null)
                {
                    enterRoom.RefreshDoorBlockers();
                }
                break;
        }
    }
}
