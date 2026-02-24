using System.Collections;
using UnityEngine;

public class CrosswalkArea : MonoBehaviour
{
    // this script is to detect when the player is in the crosswalk area, this is used in Light Trafic Controller to know when the cars should stop
    // (if the user is in the crosswalk, even if it's red, the cars stops as they are not going to crush the player)
    public bool playerInside;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
}