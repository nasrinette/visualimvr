using UnityEngine;
using SOHNE.Accessibility.Colorblindness;

public class MissionManager : MonoBehaviour
{
    public enum MissionPhase
    {
        Introduction,
        Mission_GetTomato,
        Mission_GetPepper,
        Mission_GetGrapes,
        AllComplete
    }

    [Header("Current State")]
    [SerializeField] private MissionPhase currentPhase = MissionPhase.Introduction;

    // Track each mission independently
    private bool tomatoComplete = false;
    private bool pepperComplete = false;
    private bool grapesComplete = false;

    private bool hasGrabbedGlasses = false;
    private bool supermarketTasksComplete = false;

    [Header("References")]
    [SerializeField] private CharacterInteractOnPoint guideCharacter;

    [Header("Table Items (disabled by default in scene)")]
    [SerializeField] private GameObject tableTomato;
    [SerializeField] private GameObject tableBellPepper;
    [SerializeField] private GameObject tableGrapes;

    [Header("Reveal Dialogues")]
    [SerializeField] private AudioClip wrongTomatoDialogue;
    [SerializeField] private AudioClip wrongPepperDialogue;
    [SerializeField] private AudioClip wrongGrapesDialogue;

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

    public bool HasGrabbedGlasses() => hasGrabbedGlasses;
    public bool AreSupermarketTasksComplete() => supermarketTasksComplete;
    public MissionPhase GetCurrentPhase() => currentPhase;
    public bool IsTomatoComplete() => tomatoComplete;
    public bool IsPepperComplete() => pepperComplete;
    public bool IsGrapesComplete() => grapesComplete;

    public void OnGlassesPickedUp(ColorblindTypes glassType)
    {
        hasGrabbedGlasses = true;
        switch (glassType)
        {
            case ColorblindTypes.Protanopia:
                if (!tomatoComplete) StartMissionTomato();
                break;
            case ColorblindTypes.Deuteranopia:
                if (!pepperComplete) StartMissionPepper();
                break;
            case ColorblindTypes.Tritanopia:
                if (!grapesComplete) StartMissionGrapes();
                break;
        }
    }

    void StartMissionTomato()
    {
        currentPhase = MissionPhase.Mission_GetTomato;
        currentObjective = SupermarketItem.ItemType.Tomato;
        hasMissionActive = true;
        Debug.Log("Mission: Bring me a ripe tomato!");
        if (uiManager != null)
            uiManager.ShowGoToSupermarketInstruction();
    }

    void StartMissionPepper()
    {
        currentPhase = MissionPhase.Mission_GetPepper;
        currentObjective = SupermarketItem.ItemType.BellPepper;
        hasMissionActive = true;
        Debug.Log("Mission: Bring me a green bell pepper!");
        if (uiManager != null)
            uiManager.ShowGoToSupermarketInstruction();
    }

    void StartMissionGrapes()
    {
        currentPhase = MissionPhase.Mission_GetGrapes;
        currentObjective = SupermarketItem.ItemType.Grapes;
        hasMissionActive = true;
        Debug.Log("Mission: Bring me purple grapes!");
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

        // Reveal table item and mark mission complete
        switch (item.itemType)
        {
            case SupermarketItem.ItemType.Tomato:
                if (tableTomato != null) tableTomato.SetActive(true);
                tomatoComplete = true;
                ShowReveal(wrongTomatoDialogue);
                break;
            case SupermarketItem.ItemType.BellPepper:
                if (tableBellPepper != null) tableBellPepper.SetActive(true);
                pepperComplete = true;
                ShowReveal(wrongPepperDialogue);
                break;
            case SupermarketItem.ItemType.Grapes:
                if (tableGrapes != null) tableGrapes.SetActive(true);
                grapesComplete = true;
                ShowReveal(wrongGrapesDialogue);
                break;
        }

        RemoveColorBlindness();
        CheckAllMissionsComplete();
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

    void ShowReveal(AudioClip revealDialogue)
    {
        if (revealDialogue != null && guideCharacter != null)
        {
            Debug.Log("*** PLAYING DIALOGUE NOW ***");
            guideCharacter.PlayDialogue(revealDialogue);
        }
    }

    void CheckAllMissionsComplete()
    {
        if (tomatoComplete && pepperComplete && grapesComplete)
        {
            currentPhase = MissionPhase.AllComplete;
            supermarketTasksComplete = true;
            Debug.Log("All missions complete! ✓");
            if (uiManager != null)
                uiManager.UpdateMissionText();
            EnterRoom enterRoom = FindObjectOfType<EnterRoom>();
            if (enterRoom != null)
                enterRoom.RefreshDoorBlockers();
        }
        else
        {
            currentPhase = MissionPhase.Introduction; // Back to idle, waiting for next glasses
            if (uiManager != null)
                uiManager.UpdateMissionText();
        }
    }
}
