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

    public TunnelVisionController tunnelController;

    public AudioSource audioSource;


    [Tooltip("Seconds to ignore triggers after room activates (prevents instant exit on entry)")]
    public float entryCooldown = 1f;

    private float activationTime;

    void OnEnable()
    {
        activationTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - activationTime < entryCooldown) return;

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

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;

            }

            if (tunnelController !=null) tunnelController.SetTunnelActive(false);
        }
    }
}
