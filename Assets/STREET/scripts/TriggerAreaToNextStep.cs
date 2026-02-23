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

    public bool alreadyWentIntoStartTrigger = false;
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
                Debug.Log("in end ");
                scenario.OnReachedOtherSide();
                break;
            // case TriggerType.BeginTryExpand:
            //     scenario.BeginTryExpandPhase();
            //     break;
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