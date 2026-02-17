using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterRoom : MonoBehaviour
{
    public GameObject supermarket;
    public GameObject street;
    public GameObject classroom;
    public GameObject supermarketFrame;
    public GameObject streetFrame;
    public GameObject classroomFrame;
    public GameObject baseRoom;
    public Collider supermarketCollider;
    public Collider streetCollider;
    public Collider classroomCollider;
    public AudioSource supermarketBackgroundSound;
    public AudioSource streetBackgroundSound;
    public AudioSource classroomBackgroundSound;

    // NEW: Physical blocking colliders for each door
    [Header("Door Blocking Colliders")]
    [Tooltip("Solid collider that blocks supermarket door when inactive")]
    public Collider supermarketBlocker;
    [Tooltip("Solid collider that blocks street door when inactive")]
    public Collider streetBlocker;
    [Tooltip("Solid collider that blocks classroom door when inactive")]
    public Collider classroomBlocker;

    private MissionManager missionManager;
    public AudioSource blockedEntrySound;

    void Start()
    {
        supermarket.SetActive(false);
        street.SetActive(false);
        classroom.SetActive(false);
        baseRoom.SetActive(true);

        missionManager = FindObjectOfType<MissionManager>();
        if (missionManager == null)
        {
            Debug.LogError("MissionManager not found! Room entry checks won't work.");
        }

        // NEW: Initialize blockers - all doors start accessible
        UpdateDoorBlockers();
    }

    // NEW: Update which doors are physically blocked
    void UpdateDoorBlockers()
    {
        if (missionManager == null) return;

        bool hasGlasses = missionManager.HasGrabbedGlasses();
        bool tasksComplete = missionManager.AreSupermarketTasksComplete();

        // Supermarket door: blocked if no glasses grabbed
        if (supermarketBlocker != null)
        {
            supermarketBlocker.enabled = !hasGlasses;
        }

        // Street door: blocked if glasses grabbed but tasks not complete
        if (streetBlocker != null)
        {
            streetBlocker.enabled = (hasGlasses && !tasksComplete);
        }

        // Classroom door: blocked if glasses grabbed but tasks not complete
        if (classroomBlocker != null)
        {
            classroomBlocker.enabled = (hasGlasses && !tasksComplete);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            // SUPERMARKET: Can only enter if glasses have been grabbed
            if (gameObject == supermarketCollider.gameObject)
            {
                if (missionManager != null && !missionManager.HasGrabbedGlasses())
                {
                    Debug.Log("Cannot enter supermarket - need to grab glasses first!");
                    if (blockedEntrySound != null)
                        blockedEntrySound.Play();
                    return;
                }

                supermarket.SetActive(true);
                street.SetActive(false);
                classroom.SetActive(false);
                baseRoom.SetActive(false);
                streetFrame.SetActive(false);
                classroomFrame.SetActive(false);
            }

            // STREET: Can only enter if supermarket tasks are NOT complete yet
            if (gameObject == streetCollider.gameObject)
            {
                if (missionManager != null && missionManager.HasGrabbedGlasses() 
                    && !missionManager.AreSupermarketTasksComplete())
                {
                    Debug.Log("Cannot enter street - finish supermarket tasks first!");
                    if (blockedEntrySound != null)
                        blockedEntrySound.Play();
                    return;
                }

                Debug.Log("entered street");
                supermarket.SetActive(false);
                street.SetActive(true);
                classroom.SetActive(false);
                baseRoom.SetActive(false);
                supermarketFrame.SetActive(false);
                classroomFrame.SetActive(false);
                if(streetBackgroundSound != null)
                    streetBackgroundSound.enabled = true;
            }

            // CLASSROOM: Can only enter if supermarket tasks are NOT complete yet
            if (gameObject == classroomCollider.gameObject)
            {
                if (missionManager != null && missionManager.HasGrabbedGlasses() 
                    && !missionManager.AreSupermarketTasksComplete())
                {
                    Debug.Log("Cannot enter classroom - finish supermarket tasks first!");
                    if (blockedEntrySound != null)
                        blockedEntrySound.Play();
                    return;
                }

                Debug.Log("entered classroom");
                supermarket.SetActive(false);
                street.SetActive(false);
                classroom.SetActive(true);
                baseRoom.SetActive(false);
                supermarketFrame.SetActive(false);
                streetFrame.SetActive(false);
            }
        }
    }

    // NEW: Call this from GlassesInteractable or MissionManager when state changes
    public void RefreshDoorBlockers()
    {
        UpdateDoorBlockers();
    }
}
