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
        // Detect when audio stops to fire the finished callback
        if (wasPlaying && !IsPlaying)
        {
            wasPlaying = false;
            OnDialogueFinished?.Invoke();
        }
    }

    public void PlayDialogue(AudioClip clip)
    {
        if (clip == null) return;

        if (animator != null)
        {
            animator.SetTrigger("Interact");
        }

        if (audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            wasPlaying = true;
        }
    }
}
