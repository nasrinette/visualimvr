using UnityEngine;


public class CrosswalkButton : MonoBehaviour
{
    // this class if used for when the user press the crosswalk button
    public LightTrafficController signal;

    public ScenarioNarration scenario;

    public void OnPressed()
    {
        Debug.Log("Crosswalk button pressed with grip!");
        signal.RequestCrossing(); // request crossing to the light traffic controller in the scene
        scenario.OnCrosswalkButtonPressed(); // continue the scenario
    }
}