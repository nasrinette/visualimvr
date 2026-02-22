using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CrosswalkButton : MonoBehaviour
{
    public LightTrafficController signal;

    public ScenarioNarration scenario;

    public void OnPressed()
    {
       
        Debug.Log("Crosswalk button pressed with grip!");
        signal.RequestCrossing();
        scenario.OnCrosswalkButtonPressed();
    }
}