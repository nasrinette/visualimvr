using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    public EnvironmentState envState;



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

            DynamicGI.UpdateEnvironment();

            if (envState != null) envState.Restore();

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.loop = false;
            }

            if (tunnelController != null) tunnelController.SetTunnelActive(false);
        }
    }
}
