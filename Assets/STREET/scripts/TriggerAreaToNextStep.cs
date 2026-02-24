using UnityEngine;

public class TriggerAreaToNextStep : MonoBehaviour
{
    // this class is a helper for the scenario for the street scene, to go to corresponding next step
    public enum TriggerType
    {
        BeginTryExpand,
        WaitArea,
        StartCrossing,
        MidCrossing,
        EndCrossing,
        StartScene,
    }

    public bool alreadyWentIntoStartTrigger = false;
    public TriggerType triggerType;
    public ScenarioNarration scenario;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        switch (triggerType)
        {
            case TriggerType.EndCrossing:
                scenario.OnReachedOtherSide();
                break;
            case TriggerType.StartScene:
                {
                    if (alreadyWentIntoStartTrigger) return;
                    alreadyWentIntoStartTrigger = true;
                    scenario.OnEnteredScene();
                    break;
                }

        }
    }
}