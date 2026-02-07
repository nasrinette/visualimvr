using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CharacterInteractOnPoint : MonoBehaviour
{
    [SerializeField] private InputActionReference aButtonAction; // A button input
    [SerializeField] private XRRayInteractor rayInteractor; // Your controller's ray interactor
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
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
        // Check if ray is pointing at this character
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                animator.SetTrigger("Interact");
            }
        }
    }
}
