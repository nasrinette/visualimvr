using UnityEngine;
using System;
using System.Collections;

public class ClassroomGuideCharacter : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;
    private bool wasPlaying;

    public event Action OnDialogueFinished;

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // detect audio end
        if (wasPlaying && !IsPlaying)
        {
            wasPlaying = false;
            OnDialogueFinished?.Invoke();
        }
    }

    public void PlayDialogue(AudioClip clip)
    {
        if (clip == null) return;

        // trigger talk animation
        if (animator != null)
        {
            animator.SetTrigger("Interact");
        }

        // play clip
        if (audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            wasPlaying = true;
        }
    }
}
