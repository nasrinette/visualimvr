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

    [SerializeField] private ScenarioUIManager uiManager;

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
        
        // NEW: Show "Go to supermarket" instruction
        if (uiManager != null)
        {
            uiManager.ShowGoToSupermarketInstruction();
        }
    }

    void StartMission2()
    {
        currentPhase = MissionPhase.Mission2_GetPepper;
        currentObjective = SupermarketItem.ItemType.BellPepper;
        hasMissionActive = true;
        Debug.Log("Mission 2: Bring me a red bell pepper!");
        
        // NEW: Show "Go to supermarket" instruction
        if (uiManager != null)
        {
            uiManager.ShowGoToSupermarketInstruction();
        }
    }

    void StartMission3()
    {
        currentPhase = MissionPhase.Mission3_GetEggplant;
        currentObjective = SupermarketItem.ItemType.Eggplant;
        hasMissionActive = true;
        Debug.Log("Mission 3: Bring me a purple eggplant!");
        
        // NEW: Show "Go to supermarket" instruction
        if (uiManager != null)
        {
            uiManager.ShowGoToSupermarketInstruction();
        }
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
            
            // NEW: Show error in UI
            if (uiManager != null)
            {
                uiManager.ShowErrorState("No mission active!");
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
            
            // NEW: Show error in UI
            if (uiManager != null)
            {
                uiManager.ShowErrorState("Wrong item! Try again.");
            }
            
            return; // Don't proceed - let player try again
        }
        
         Debug.Log("✓✓✓ CORRECT ITEM DELIVERED! Proceeding with reveal...");
        hasMissionActive = false;
        
        // Remove color blindness effect
        RemoveColorBlindness();
        
        // NEW: Advance to next mission immediately (updates UI)
        AdvanceToNextMission();
        ShowReveal();
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
    
    AudioClip revealDialogue = null;
    
    switch(currentPhase)
    {
        case MissionPhase.Mission1_Reveal:  // Changed from Mission1_GetTomato
            revealDialogue = wrongTomatoDialogue;
            break;
        case MissionPhase.Mission2_Reveal:  // Changed from Mission2_GetPepper
            revealDialogue = wrongPepperDialogue;
            break;
        case MissionPhase.Mission3_Reveal:  // Changed from Mission3_GetEggplant
            revealDialogue = wrongEggplantDialogue;
            break;
    }
    
    if (revealDialogue != null && guideCharacter != null)
    {
        Debug.Log("*** PLAYING DIALOGUE NOW ***");
        guideCharacter.PlayDialogue(revealDialogue);
    }
    
    // REMOVE: Invoke("AdvanceToNextMission", 5f);
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
                
                // NEW: Update UI to show "pick up new glasses"
                if (uiManager != null)
                {
                    uiManager.UpdateMissionText();
                }
                break;
                
            case MissionPhase.Mission2_GetPepper:
                currentPhase = MissionPhase.Mission2_Reveal;
                GlassesInteractable.RemoveGlasses();
                Debug.Log("Mission 2 complete! ✓ Glasses reset - Ready for Mission 3");
                
                // NEW: Update UI to show "pick up new glasses"
                if (uiManager != null)
                {
                    uiManager.UpdateMissionText();
                }
                break;
                
            case MissionPhase.Mission3_GetEggplant:
                currentPhase = MissionPhase.Mission3_Reveal;
                supermarketTasksComplete = true;
                GlassesInteractable.RemoveGlasses();
                Debug.Log("All missions complete! ✓ Glasses reset");
                
                // NEW: Update UI to show completion
                if (uiManager != null)
                {
                    uiManager.UpdateMissionText();
                }
                
                EnterRoom enterRoom = FindObjectOfType<EnterRoom>();
                if (enterRoom != null)
                {
                    enterRoom.RefreshDoorBlockers();
                }
                break;
        }
    }


    // Add this method to MissionManager.cs
    public MissionPhase GetCurrentPhase()
    {
        return currentPhase;
    }

}
