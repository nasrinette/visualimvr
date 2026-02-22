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

    [Header("Table Items (disabled by default in scene)")]
    [SerializeField] private GameObject tableTomato;
    [SerializeField] private GameObject tableBellPepper;
    [SerializeField] private GameObject tableEggplant;

    [Header("Reveal Dialogues")]
    [SerializeField] private AudioClip wrongTomatoDialogue;
    [SerializeField] private AudioClip wrongPepperDialogue;
    [SerializeField] private AudioClip wrongEggplantDialogue;

    [Header("Error Dialogues")]
    [SerializeField] private AudioClip wrongItemDialogue;
    [SerializeField] private AudioClip noMissionDialogue;

    private SupermarketItem.ItemType currentObjective;
    private bool hasMissionActive = false;

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
        switch (glassType)
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
        if (uiManager != null)
            uiManager.ShowGoToSupermarketInstruction();
    }

    void StartMission2()
    {
        currentPhase = MissionPhase.Mission2_GetPepper;
        currentObjective = SupermarketItem.ItemType.BellPepper;
        hasMissionActive = true;
        Debug.Log("Mission 2: Bring me a red bell pepper!");
        if (uiManager != null)
            uiManager.ShowGoToSupermarketInstruction();
    }

    void StartMission3()
    {
        currentPhase = MissionPhase.Mission3_GetEggplant;
        currentObjective = SupermarketItem.ItemType.Eggplant;
        hasMissionActive = true;
        Debug.Log("Mission 3: Bring me a purple eggplant!");
        if (uiManager != null)
            uiManager.ShowGoToSupermarketInstruction();
    }

    public void OnItemDelivered(SupermarketItem item)
    {
        Debug.Log($"=== ITEM DELIVERED ===");
        Debug.Log($"Current Phase: {currentPhase}");
        Debug.Log($"Expected: {currentObjective}");
        Debug.Log($"Received: {item.itemType}");
        Debug.Log($"Mission Active: {hasMissionActive}");

        if (!hasMissionActive)
        {
            Debug.LogWarning("❌ No mission active! Player brought item without mission.");
            if (guideCharacter != null && noMissionDialogue != null)
                guideCharacter.PlayDialogue(noMissionDialogue);
            if (uiManager != null)
                uiManager.ShowErrorState("No mission active!");
            return;
        }

        if (item.itemType != currentObjective)
        {
            Debug.LogWarning($"❌ WRONG ITEM! Expected {currentObjective} but got {item.itemType}");
            if (guideCharacter != null && wrongItemDialogue != null)
                guideCharacter.PlayDialogue(wrongItemDialogue);
            if (uiManager != null)
                uiManager.ShowErrorState("Wrong item! Try again.");
            return;
        }

        Debug.Log("✓✓✓ CORRECT ITEM DELIVERED! Proceeding with reveal...");
        hasMissionActive = false;

        // Reveal the corresponding table item
        switch (item.itemType)
        {
            case SupermarketItem.ItemType.Tomato:
                if (tableTomato != null) tableTomato.SetActive(true);
                break;
            case SupermarketItem.ItemType.BellPepper:
                if (tableBellPepper != null) tableBellPepper.SetActive(true);
                break;
            case SupermarketItem.ItemType.Eggplant:
                if (tableEggplant != null) tableEggplant.SetActive(true);
                break;
        }

        RemoveColorBlindness();
        AdvanceToNextMission();
        ShowReveal();
    }

    void RemoveColorBlindness()
    {
        if (Colorblindness.Instance != null)
        {
            Colorblindness.Instance.Change(0);
            Debug.Log("Glasses removed - normal vision restored");
        }
        GlassesInteractable.RemoveGlasses();
        Debug.Log("✓ Glasses state reset - can pick up new glasses now");
    }

    void ShowReveal()
    {
        Debug.Log("=== ShowReveal CALLED ===");
        AudioClip revealDialogue = null;
        switch (currentPhase)
        {
            case MissionPhase.Mission1_Reveal:
                revealDialogue = wrongTomatoDialogue;
                break;
            case MissionPhase.Mission2_Reveal:
                revealDialogue = wrongPepperDialogue;
                break;
            case MissionPhase.Mission3_Reveal:
                revealDialogue = wrongEggplantDialogue;
                break;
        }
        if (revealDialogue != null && guideCharacter != null)
        {
            Debug.Log("*** PLAYING DIALOGUE NOW ***");
            guideCharacter.PlayDialogue(revealDialogue);
        }
    }

    void AdvanceToNextMission()
    {
        Debug.Log("=== AdvanceToNextMission CALLED ===");
        switch (currentPhase)
        {
            case MissionPhase.Mission1_GetTomato:
                currentPhase = MissionPhase.Mission1_Reveal;
                GlassesInteractable.RemoveGlasses();
                Debug.Log("Mission 1 complete! ✓ Glasses reset - Ready for Mission 2");
                if (uiManager != null)
                    uiManager.UpdateMissionText();
                break;
            case MissionPhase.Mission2_GetPepper:
                currentPhase = MissionPhase.Mission2_Reveal;
                GlassesInteractable.RemoveGlasses();
                Debug.Log("Mission 2 complete! ✓ Glasses reset - Ready for Mission 3");
                if (uiManager != null)
                    uiManager.UpdateMissionText();
                break;
            case MissionPhase.Mission3_GetEggplant:
                currentPhase = MissionPhase.Mission3_Reveal;
                supermarketTasksComplete = true;
                GlassesInteractable.RemoveGlasses();
                Debug.Log("All missions complete! ✓ Glasses reset");
                if (uiManager != null)
                    uiManager.UpdateMissionText();
                EnterRoom enterRoom = FindObjectOfType<EnterRoom>();
                if (enterRoom != null)
                    enterRoom.RefreshDoorBlockers();
                break;
        }
    }

    public MissionPhase GetCurrentPhase()
    {
        return currentPhase;
    }
}
