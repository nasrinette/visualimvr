using UnityEngine;

public class GuideCharacter : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

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
            // Also configure if it already existed on the GameObject
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
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
