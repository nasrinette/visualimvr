using UnityEngine;
using TMPro;
using System.Collections;

public class ScenarioUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject scenarioCard;
    [SerializeField] private TextMeshProUGUI scenarioNameText;
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private TextMeshProUGUI missionStateText;

    [Header("References")]
    [SerializeField] private MissionManager missionManager;

    [Header("State Text Settings")]
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private float stateFadeDuration = 3f;

    private string currentScenario = "";
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (scenarioCard != null)
            scenarioCard.SetActive(false);

        if (missionStateText != null)
            missionStateText.text = "";

        if (missionManager == null)
            missionManager = FindObjectOfType<MissionManager>();
    }

    public void ShowScenarioInfo(string scenarioName)
    {
        currentScenario = scenarioName;

        if (scenarioCard != null)
            scenarioCard.SetActive(true);

        if (scenarioNameText != null)
            scenarioNameText.text = scenarioName;

        UpdateMissionText();
    }

    public void HideScenarioInfo()
    {
        if (scenarioCard != null)
            scenarioCard.SetActive(false);
        currentScenario = "";
    }

    public void UpdateMissionText()
    {
        if (missionText == null || missionManager == null) return;

        ClearStateText();

        if (currentScenario == "Supermarket")
            missionText.text = GetCurrentMissionText();
        else
            missionText.text = "";
    }

    private string GetCurrentMissionText()
    {
        MissionManager.MissionPhase phase = missionManager.GetCurrentPhase();

        switch (phase)
        {
            case MissionManager.MissionPhase.Introduction:
                return "Pick up the glasses to start";

            case MissionManager.MissionPhase.Mission_GetTomato:
                return "Mission: Bring a ripe tomato to the guide";

            case MissionManager.MissionPhase.Mission_GetPepper:
                return "Mission: Bring a green bell pepper to the guide";

            case MissionManager.MissionPhase.Mission_GetGrapes:
                return "Mission: Bring purple grapes to the guide";

            case MissionManager.MissionPhase.AllComplete:
                return "Supermarket scenario completed!";

            default:
                return "Pick up glasses to start a mission";
        }
    }

    public void ShowSuccessState(string message)
    {
        if (missionStateText != null)
        {
            missionStateText.text = message;
            missionStateText.color = successColor;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutStateText());
        }
    }

    public void ShowErrorState(string message)
    {
        if (missionStateText != null)
        {
            missionStateText.text = message;
            missionStateText.color = errorColor;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutStateText());
        }
    }

    public void ClearStateText()
    {
        if (missionStateText != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            missionStateText.text = "";
        }
    }

    private IEnumerator FadeOutStateText()
    {
        yield return new WaitForSeconds(stateFadeDuration);
        if (missionStateText != null)
            missionStateText.text = "";
        fadeCoroutine = null;
    }

    public void ShowGoToSupermarketInstruction()
    {
        if (scenarioCard != null)
            scenarioCard.SetActive(true);

        if (scenarioNameText != null)
            scenarioNameText.text = "Supermarket";

        currentScenario = "Supermarket";
        UpdateMissionText();
        ClearStateText();
    }
}
