using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CharacterInteractOnPoint : MonoBehaviour
{
    [SerializeField] private InputActionReference aButtonAction;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private AudioClip[] dialogueClips; // Array of audio clips
    
    private Animator animator;
    private AudioSource audioSource;
    private int currentClipIndex = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
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
}
