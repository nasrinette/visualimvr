using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CharacterInteractOnPoint : MonoBehaviour
{
    [Header("Point and Click Interaction")]
    [SerializeField] private InputActionReference aButtonAction;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private AudioClip[] dialogueClips; // Array of audio clips for general interaction
    
    private Animator animator;
    private AudioSource audioSource;
    private int currentClipIndex = 0;
    private MissionManager missionManager;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Add AudioSource if it doesn't exist
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find mission manager
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
                // Normal interaction - cycle through dialogues
                animator.SetTrigger("Interact");
                PlayNextDialogue();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if a supermarket item touched the guide
        SupermarketItem item = other.GetComponent<SupermarketItem>();
        
        if (item != null)
        {
            ReceiveItem(item);
        }
    }

    private void ReceiveItem(SupermarketItem item)
    {
        Debug.Log($"Guide received: {item.itemType}");
        
        // Trigger animation
        animator.SetTrigger("Interact");
        
        // Tell mission manager about the delivery
        if (missionManager != null)
        {
            missionManager.OnItemDelivered(item);
        }
        
        // Destroy the item
        Destroy(item.gameObject);
    }

    private void PlayNextDialogue()
    {
        if (dialogueClips.Length > 0)
        {
            audioSource.clip = dialogueClips[currentClipIndex];
            audioSource.Play();
            
            // Move to next clip, loop back to start if at end
            currentClipIndex = (currentClipIndex + 1) % dialogueClips.Length;
        }
    }

    // This method is called by GlassesInteractable when glasses are picked up
    public void PlayDialogue(AudioClip clip)
    {
        // Trigger the interact animation
        if (animator != null)
        {
            animator.SetTrigger("Interact");
        }

        // Play the specific audio clip
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
