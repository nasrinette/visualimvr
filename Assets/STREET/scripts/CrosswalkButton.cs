using UnityEngine;

public class CrosswalkButton : MonoBehaviour
{
    public PedestrianSignal signal;

    public void OnPressed()
    {
        Debug.Log("Crosswalk button pressed!");
        signal.RequestCrossing();
    }
}