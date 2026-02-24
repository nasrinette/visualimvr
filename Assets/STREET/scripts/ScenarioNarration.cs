using System;
using System.Collections;
using UnityEngine;

public class ScenarioNarration : MonoBehaviour
{
    // this class handles the narration and scenario
    // handles the different steps/events and guiding sounds
    public enum Phase
    {
        Intro, // phase 1
        TryExpandTunnel, // phase 2
        SensoryOverload,
        FindCrosswalkButton,
        WaitForGreen,
        CrossStreet,
        Done // conclusion
    }

    [Header("Narration")]
    public AudioSource guideSource;
    public AudioClip introClip;
    public AudioClip tryExpandClip;
    public AudioClip afterTryExpandClip;
    public AudioClip findButtonClip;
    public AudioClip infoTunnelVisionClip;
    public AudioClip recapClip;
    public AudioClip exitDoorClip;

    public PedestrianEventController pedestrianEvents;

    public Phase CurrentPhase { get; private set; } = Phase.Intro;
    bool narrationPlaying;

    bool pendingExpandAttempt;

    public void OnEnteredScene()
    {
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        // AUDIO:  INTRO PART
        CurrentPhase = Phase.Intro;
        // Debug.Log("[NAV] Welcome. You're now seeing the world through the eyes of someone with peripheral loss/tunnel vision. Notice how the central sight remains clear, but everything around it fades away? Before moving, take a moment. How much of the street can you really see?");

        yield return StartCoroutine(PlaySound(introClip));
        yield return new WaitForSeconds(4f);

        CurrentPhase = Phase.TryExpandTunnel;
        // Debug.Log("[NAV] Try to force your vision wider. Stretch your awareness. See if you can see more of the street.");
        if (pendingExpandAttempt) 
        {
            pendingExpandAttempt = false;
            OnTunnelExpandAttempted();
            yield break; 
        }
        // AUDIO: TRY TO EXPAND
        yield return StartCoroutine(PlaySound(tryExpandClip));
    }


    public void OnTunnelExpandAttempted()
    {
        if (CurrentPhase != Phase.TryExpandTunnel) return;
        // Debug.Log("[NAV] Tunnel vision doesn't expand smoothly. You can't just try harder to see more with this impairment.");
        CurrentPhase = Phase.FindCrosswalkButton;
        StartCoroutine(TryExpandSequence());

    }

    public void RequestTunnelExpandAttempt()
    {
        // handles the case where user tries to extend they vision before they are prompted to do so, we just go to next step after the current audio is done
        // If we're already in the right phase, handle immediately
        if (CurrentPhase == Phase.TryExpandTunnel)
        {
            OnTunnelExpandAttempted();
            return;
        }
        if (CurrentPhase == Phase.Intro)
        {
            // Otherwise, remember it happened
            pendingExpandAttempt = true;
            // Debug.Log("[NAV] Expand attempt queued (waiting for TryExpandTunnel phase).");
        }
    }

    IEnumerator TryExpandSequence()
    {
        // AUDIO: REQUEST CROSSING 
        yield return StartCoroutine(PlaySound(afterTryExpandClip));

        yield return new WaitForSeconds(2f);
        // Debug.Log("[NAV] Cross the street when you think it's safe. First, request the crossing.");

        yield return StartCoroutine(PlaySound(findButtonClip));
        CurrentPhase = Phase.WaitForGreen;

    }




    public void OnCrosswalkButtonPressed()
    {
        if (CurrentPhase != Phase.WaitForGreen) return; 
        CurrentPhase = Phase.CrossStreet;
        pedestrianEvents.StartWaitForGreenMoment();
    }


    public void OnReachedOtherSide()
    {
        // AUDIO: FINAL AUDIO (RECAP+EXPLANATION)
        if (CurrentPhase != Phase.CrossStreet) return;
        // Debug.Log("[NAV] Tunnel vision limits situational awareness. Hazards aren't invisible, they're simply unseen until they enter your narrow field of view. " +
        //           "[NAV] This makes everyday tasks like crossing a street stressful and dangerous.");

        // Debug.Log("[NAV] What you experienced is common for people with peripheral vision loss. Millions navigate daily life with limited vision. " +
        //           "[NAV] Thoughtful design, safer crossings, clearer signals, and patience can make streets safer for everyone.");

        // Debug.Log("[NAV] Whenever you're ready, you can head back through the door to return to the initial room.");
        StartCoroutine(FinalAudio());

        CurrentPhase = Phase.Done;
    }

    IEnumerator FinalAudio()
    {
        CurrentPhase = Phase.Done;

        yield return StartCoroutine(PlaySound(infoTunnelVisionClip));
        yield return StartCoroutine(PlaySound(recapClip));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(PlaySound(exitDoorClip));
    }


    IEnumerator PlaySound(AudioClip clip)
    {
        if (clip == null) yield break;

        // prevent overlap
        while (narrationPlaying) yield return null;

        narrationPlaying = true;

        guideSource.clip = clip;
        guideSource.loop = false;
        guideSource.Play();

        yield return new WaitForSeconds(clip.length);

        narrationPlaying = false;
    }
}