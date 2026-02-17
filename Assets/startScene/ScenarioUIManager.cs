using UnityEngine;
using TMPro;
using System.Collections;

public class ScenarioUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject scenarioCard;
    [SerializeField] private TextMeshProUGUI scenarioNameText;
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private TextMeshProUGUI missionStateText; // NEW: feedback text
    
    [Header("References")]
    [SerializeField] private MissionManager missionManager;
    
    [Header("State Text Settings")]
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private float stateFadeDuration = 3f; // How long to show before fading
    
    private string currentScenario = "";
    private Coroutine fadeCoroutine;
    
    void Start()
    {
        // Initially hide the card
        if (scenarioCard != null)
        {
            scenarioCard.SetActive(false);
        }
        
        // Clear mission state text
        if (missionStateText != null)
        {
            missionStateText.text = "";
        }
        
        // Find MissionManager if not assigned
        if (missionManager == null)
        {
            missionManager = FindObjectOfType<MissionManager>();
        }
    }
    
    // Call this from EnterRoom.OnTriggerEnter when entering a door
    public void ShowScenarioInfo(string scenarioName)
    {
        currentScenario = scenarioName;
        
        if (scenarioCard != null)
        {
            scenarioCard.SetActive(true);
        }
        
        // Update scenario name
        if (scenarioNameText != null)
        {
            scenarioNameText.text = scenarioName;
        }
        
        // Update mission text based on scenario
        UpdateMissionText();
    }
    
    public void HideScenarioInfo()
    {
        if (scenarioCard != null)
        {
            scenarioCard.SetActive(false);
        }
        currentScenario = "";
    }
    
    // Update mission information dynamically
    public void UpdateMissionText()
    {
        if (missionText == null || missionManager == null) return;
        
        // Clear state text when mission updates
        ClearStateText();
        
        // Only show mission info for supermarket scenario
        if (currentScenario == "Supermarket")
        {
            string missionInfo = GetCurrentMissionText();
            missionText.text = missionInfo;
        }
        else
        {
            // For other scenarios, hide mission text or show generic info
            missionText.text = "";
        }
    }
    
    private string GetCurrentMissionText()
    {
        MissionManager.MissionPhase phase = missionManager.GetCurrentPhase();
        
        switch (phase)
        {
            case MissionManager.MissionPhase.Introduction:
                return "Pick up the glasses to start";
                
            case MissionManager.MissionPhase.Mission1_GetTomato:
                return "Mission: Bring a ripe tomato to the guide";
                
            case MissionManager.MissionPhase.Mission1_Reveal:
                return "Pick up new glasses for Mission 2";
                
            case MissionManager.MissionPhase.Mission2_GetPepper:
                return "Mission: Bring a red bell pepper to the guide";
                
            case MissionManager.MissionPhase.Mission2_Reveal:
                return "Pick up new glasses for Mission 3";
                
            case MissionManager.MissionPhase.Mission3_GetEggplant:
                return "Mission: Bring a purple eggplant to the guide";
                
            case MissionManager.MissionPhase.Mission3_Reveal:
                return "All missions complete!";
                
            case MissionManager.MissionPhase.Complete:
                return "All missions complete!";
                
            default:
                return "";
        }
    }


    
    // NEW: Show success message in green
    public void ShowSuccessState(string message)
    {
        if (missionStateText != null)
        {
            missionStateText.text = message;
            missionStateText.color = successColor;
            
            // Stop any existing fade coroutine
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            // Start fading out after duration
            fadeCoroutine = StartCoroutine(FadeOutStateText());
        }
    }
    
    // NEW: Show error message in red
    public void ShowErrorState(string message)
    {
        if (missionStateText != null)
        {
            missionStateText.text = message;
            missionStateText.color = errorColor;
            
            // Stop any existing fade coroutine
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            // Start fading out after duration
            fadeCoroutine = StartCoroutine(FadeOutStateText());
        }
    }
    
    // NEW: Clear the state text immediately
    public void ClearStateText()
    {
        if (missionStateText != null)
        {
            // Stop any fade coroutine
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            
            missionStateText.text = "";
        }
    }
    
    // NEW: Coroutine to fade out and clear the state text
    private IEnumerator FadeOutStateText()
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(stateFadeDuration);
        
        // Clear the text
        if (missionStateText != null)
        {
            missionStateText.text = "";
        }
        
        fadeCoroutine = null;
    }

    public void ShowGoToSupermarketInstruction()
    {
        if (scenarioCard != null)
        {
            scenarioCard.SetActive(true);
        }
        
        if (scenarioNameText != null)
        {
            scenarioNameText.text = "Supermarket Mission";
        }
        
        if (missionText != null)
        {
            missionText.text = "Go to the supermarket door";
        }
        
        // Clear any state text
        ClearStateText();
        
        currentScenario = "Supermarket";
    }
}
