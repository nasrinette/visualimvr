using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitRoom : MonoBehaviour
{

    public GameObject supermarket;
    public GameObject street;
    public GameObject classroom;

    public GameObject supermarketFrame;
    public GameObject streetFrame;
    public GameObject classroomFrame;


    public GameObject baseRoom;

    // Start is called before the first frame update

    public AudioSource supermarketBackgroundSound;
    public AudioSource streetBackgroundSound;
    public AudioSource classroomBackgroundSound;

    public TunnelVisionController tunnelController;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            Debug.Log("exit");
            supermarket.SetActive(false);
            street.SetActive(false);
            classroom.SetActive(false);


            baseRoom.SetActive(true);

            supermarketFrame.SetActive(true);
            streetFrame.SetActive(true);
            classroomFrame.SetActive(true);

            if (streetBackgroundSound != null)
                streetBackgroundSound.enabled = false;
            tunnelController.SetTunnelActive(false);


        }
    }

}
