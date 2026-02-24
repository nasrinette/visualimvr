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

        missionManager = FindObjectOfType<MissionManager>();
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
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
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
        animator.SetTrigger("Interact");

        if (missionManager != null)
            missionManager.OnItemDelivered(item);

        Destroy(item.gameObject);

        // Reset after a short delay in case another item is delivered shortly after
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
