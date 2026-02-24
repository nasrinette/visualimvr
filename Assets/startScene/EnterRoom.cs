using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    public GameObject doorStreet;
    public AudioSource audioSource;

    public AudioClip streetBackgroundSound;


    public TunnelVisionController tunnelController;


    // NEW: Physical blocking colliders for each door
    [Header("Door Blocking Colliders")]
    [Tooltip("Solid collider that blocks supermarket door when inactive")]
    public Collider supermarketBlocker;
    [Tooltip("Solid collider that blocks street door when inactive")]
    public Collider streetBlocker;
    [Tooltip("Solid collider that blocks classroom door when inactive")]
    public Collider classroomBlocker;
    [Header("Skybox")]
    [Tooltip("Custom skybox material for the classroom. Leave empty to keep the default.")]
    public Material classroomSkybox;
    [Tooltip("Ambient light color for the classroom (removes blue tint from base skybox)")]
    public Color classroomAmbientColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("UI")]
    public ScenarioUIManager uiManager;

    private MissionManager missionManager;
    private Material baseSkybox;
    private AmbientMode baseAmbientMode;
    private Color baseAmbientColor;
    private DefaultReflectionMode baseReflectionMode;
    public AudioSource blockedEntrySound;

    public AudioSource portalSource;
    public AudioClip portalSound;

    public EnvironmentState envState;

    void Start()
    {
        supermarket.SetActive(false);
        street.SetActive(false);
        classroom.SetActive(false);
        baseRoom.SetActive(true);

        baseSkybox = RenderSettings.skybox;
        baseAmbientMode = RenderSettings.ambientMode;
        baseAmbientColor = RenderSettings.ambientLight;
        baseReflectionMode = RenderSettings.defaultReflectionMode;

        if (envState != null && !envState.hasSnapshot)
{
    envState.CaptureCurrent();
    Debug.Log($"[EnvState] Captured on Start: {RenderSettings.skybox?.name}");
}

        missionManager = FindObjectOfType<MissionManager>();
        if (missionManager == null)
        {
            Debug.LogError("MissionManager not found! Room entry checks won't work.");
        }

        // NEW: Initialize blockers - all doors start accessible
        UpdateDoorBlockers();

        if (tunnelController != null) tunnelController.SetTunnelActive(false);
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
            // SUPERMARKET
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

                RestoreBaseEnvironment();

                // NEW: Update UI to show the specific mission item
                if (uiManager != null)
                {
                    uiManager.ShowScenarioInfo("Supermarket");
                    // This will update to show "Mission: Bring a ripe tomato" etc.
                }
                if (tunnelController != null) tunnelController.SetTunnelActive(false);

                PlayPortalSound();


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

                RestoreBaseEnvironment();

                if (audioSource != null && streetBackgroundSound != null)
                {
                    audioSource.loop = false;
                    audioSource.clip = streetBackgroundSound;
                    audioSource.Play();
                }



                if (doorStreet != null) doorStreet.SetActive(false);

                // NEW: Show scenario UI
                if (uiManager != null)
                {
                    uiManager.ShowScenarioInfo("Street");
                }

                if (tunnelController != null) tunnelController.SetTunnelActive(true);

                PlayPortalSound();
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

                // if (envState != null) envState.CaptureCurrent();

                if (classroomSkybox != null)
                {
                    RenderSettings.skybox = classroomSkybox;
                    RenderSettings.ambientMode = AmbientMode.Flat;
                    RenderSettings.ambientLight = classroomAmbientColor;
                    RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                    DynamicGI.UpdateEnvironment();
                }

                // NEW: Show scenario UI
                if (uiManager != null)
                {
                    uiManager.ShowScenarioInfo("Classroom");
                }
                if (tunnelController != null) tunnelController.SetTunnelActive(false);

                PlayPortalSound();
            }
        }
    }

    void RestoreBaseEnvironment()
    {
        RenderSettings.skybox = baseSkybox;
        RenderSettings.ambientMode = baseAmbientMode;
        RenderSettings.ambientLight = baseAmbientColor;
        RenderSettings.defaultReflectionMode = baseReflectionMode;
        DynamicGI.UpdateEnvironment();
    }

    // NEW: Call this from GlassesInteractable or MissionManager when state changes
    public void RefreshDoorBlockers()
    {
        UpdateDoorBlockers();
    }

    public void PlayPortalSound()
    {
        if (portalSource != null && portalSound != null)
        {
            portalSource.loop = false;
            portalSource.clip = portalSound;
            portalSource.Play();
        }
    }
}
