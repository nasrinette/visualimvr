using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CharacterInteractOnPoint : MonoBehaviour
{
    [Header("Point and Click Interaction")]
    [SerializeField] private InputActionReference aButtonAction;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private AudioClip[] dialogueClips;

    private Animator animator;
    private AudioSource audioSource;
    private int currentClipIndex = 0;
    private MissionManager missionManager;
    private bool isReceivingItem = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // ensure there is an AudioSource on this object
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }
        else
        {
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }

        missionManager = FindObjectOfType<MissionManager>(); // locate mission manager in scene for item delivery tracking
    }

    void OnEnable()
    {
        aButtonAction.action.performed += OnAButtonPressed;
    }

    void OnDisable()
    {
        aButtonAction.action.performed -= OnAButtonPressed;
    }

    private void OnAButtonPressed(InputAction.CallbackContext context)
    {
        // when the A button is pressed, check if our ray interactor is pointing at this guide
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                // trigger the talking animation and play dialogue sound
                animator.SetTrigger("Interact");
                PlayNextDialogue();
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        SupermarketItem item = other.GetComponent<SupermarketItem>();
        if (item != null && !isReceivingItem)
        {
            isReceivingItem = true;
            ReceiveItem(item);
        }
    }

    private void ReceiveItem(SupermarketItem item)
    {
        Debug.Log($"Guide received: {item.itemType}");
        animator.SetTrigger("Interact"); // play interaction animation when item is accepted

        if (missionManager != null)
            missionManager.OnItemDelivered(item); // inform mission manager that item was delivered

        Destroy(item.gameObject); // remove the item from the world once received

        Invoke(nameof(ResetReceiving), 1f);
    }

    private void ResetReceiving()
    {
        isReceivingItem = false;
    }


    private void PlayNextDialogue()
    {
        if (dialogueClips.Length > 0)
        {
            audioSource.clip = dialogueClips[currentClipIndex];
            audioSource.Play();
            currentClipIndex = (currentClipIndex + 1) % dialogueClips.Length;
        }
    }

    public void PlayDialogue(AudioClip clip)
    {
        if (animator != null)
            animator.SetTrigger("Interact");

        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
