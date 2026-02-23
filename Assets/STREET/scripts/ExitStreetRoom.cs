using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitStreetRoom : MonoBehaviour
{
    public GameObject street;

    public GameObject supermarketFrame;
    public GameObject streetFrame;
    public GameObject classroomFrame;


    public GameObject baseRoom;

    // Start is called before the first frame update

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

            street.SetActive(false);


            baseRoom.SetActive(true);

            supermarketFrame.SetActive(true);
            streetFrame.SetActive(true);
            classroomFrame.SetActive(true);

            audioSource.loop = false;
            audioSource.clip = portalSound;
            audioSource.Play();

            tunnelController.SetTunnelActive(false);

            doorStreet.SetActive(true);
        }
    }

}
