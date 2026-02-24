using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitStreetRoom : MonoBehaviour
{
    // this class is similar to ExitRoom (classic script used for the other rooms)
    // but here for the street, since the door is placed at another place, we need to re-position the user
    // in the correct place in the base room, when the user exits 
    // to do so, i placed a destination object where i want the user to be re-positionned to
    public GameObject street;

    public GameObject supermarketFrame;
    public GameObject streetFrame;
    public GameObject classroomFrame;


    public GameObject baseRoom;

    public AudioSource audioSource;

    public AudioClip portalSound;

    public TunnelVisionController tunnelController;

    public Transform xrOrigin;           // XR Origin transform
    public Transform destination;        // where player should appear

    public GameObject doorStreet;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("exit");

            if (destination == null || xrOrigin == null) return;

            xrOrigin.position = destination.position;
            xrOrigin.rotation = destination.rotation;

            street.SetActive(false); // hide street

            // display baseroom and all frames
            baseRoom.SetActive(true);
            supermarketFrame.SetActive(true);
            streetFrame.SetActive(true);
            classroomFrame.SetActive(true);

            // portal sound
            audioSource.loop = false;
            audioSource.clip = portalSound;
            audioSource.Play();

            tunnelController.SetTunnelActive(false);

            doorStreet.SetActive(true); 
        }
    }

}
