using UnityEngine;

public class TriggerAreaToNextStep : MonoBehaviour
{
    public enum TriggerType
    {
        BeginTryExpand,
        WaitArea,
        StartCrossing,
        MidCrossing,
        EndCrossing,
        StartScene,
    }

    public TriggerType triggerType;
    public ScenarioNarration scenario;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        switch (triggerType)
        {
            case TriggerType.StartCrossing:
                // scenario.OnReachedCrosswalkArea();
                break;
            case TriggerType.EndCrossing:
                scenario.OnReachedOtherSide();
                break;
            case TriggerType.BeginTryExpand:
                scenario.BeginTryExpandPhase();
                break;
            case TriggerType.StartScene:
                scenario.OnEnteredScene();
                break;
        }
    }
}