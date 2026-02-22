using System;
using System.Collections;
using UnityEngine;

public class ScenarioNarration : MonoBehaviour
{
    public enum Phase
    {
        Intro, // phase 1
        TryExpandTunnel, // phae 2
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
    public AudioClip overloadClip;
    public AudioClip findButtonClip;
    public AudioClip waitGreenClip;
    public AudioClip crossClip;

    public PedestrianEventController pedestrianEvents;

    public Phase CurrentPhase { get; private set; } = Phase.Intro;
    bool narrationPlaying;

    void Start()
    {


    }

    public void OnEnteredScene()
    {
        Debug.Log("[NAV] Welcome. You're now seeing the world through the eyes of someone with peripheral loss/tunnel vision. Notice how the central sight remains clear, but everything around it fades away? Before moving, take a moment. How much of the street can you really see?");
        StartCoroutine(PlaySound(introClip));
    }

    public void BeginTryExpandPhase()
    {
        if (CurrentPhase != Phase.Intro) return;

        Debug.Log("[NAV] [Phase Intro] END");
        Debug.Log("[NAV] [Phase TryExpandTunnel] START");
        Debug.Log("[NAV] Try to force your vision wider. Stretch your awareness. See if you can see more of the street.");

        GotToNextStep(Phase.TryExpandTunnel);
    }

    public void OnTunnelExpandAttempted()
    {
        if (CurrentPhase != Phase.TryExpandTunnel) return;
        Debug.Log("[NAV] in: tunnel expand attempted; go to: sensoryoverload");
        Debug.Log("[NAV] Tunnel vision doesn’t expand smoothly. You can’t just try harder to see more with this impairment.");
        GotToNextStep(Phase.SensoryOverload);


        StartCoroutine(SensoryOverloadSequence());
    }

    IEnumerator SensoryOverloadSequence()
    {
        Debug.Log("[NAV] Sensory overload starting now...");

        // TODO: increase traffic/pedestrians here
        Debug.Log("[SIM] Increase traffic density");
        Debug.Log("[SIM] Spawn pedestrians from sides");
        Debug.Log("[SIM] Add honk / bike passby sounds");

        yield return new WaitForSeconds(2f);

        Debug.Log("[NAV] Take a moment and listen. Cars, voices, footsteps. When your vision narrows, everything keeps moving, but you just have less time to notice it.");

        yield return new WaitForSeconds(3f);

        Debug.Log("[NAV] Cross the street when you think it’s safe. First, request the crossing.");
        // Don’t auto-advance here — just instruct.
        // Next advance happens when they hit the crosswalk trigger.
        GotToNextStep(Phase.FindCrosswalkButton);
    }



    public void OnCrosswalkButtonPressed()
    {
        if (CurrentPhase != Phase.FindCrosswalkButton) return;
        Debug.Log("[NAV] in: crosswalk button pressed ; go to: waitforgreen ");
        Debug.Log("[NAV] [Action] Crosswalk button pressed");
        Debug.Log("[NAV] Now wait. Listen. People approach from the side.");

        GotToNextStep(Phase.WaitForGreen);

        pedestrianEvents.StartWaitForGreenMoment();
    }


    public void OnTrafficLightTurnedGreen()
    {
        if (CurrentPhase != Phase.WaitForGreen) return;
        Debug.Log("[NAV] in: traffic light turned green; go to: cross street");
        Debug.Log("[NAV] Go when you think it’s safe.");
        GotToNextStep(Phase.CrossStreet);
    }

    public void OnReachedOtherSide()
    {
        if (CurrentPhase != Phase.CrossStreet) return;
        Debug.Log("[NAV] in: reached other side ; go to: done");
        Debug.Log("[NAV] Tunnel vision limits situational awareness. Hazards aren’t invisible, they’re simply unseen until they enter your narrow field of view. " +
                  "[NAV] This makes everyday tasks like crossing a street stressful and dangerous.");

        Debug.Log("[NAV] What you experienced is common for people with peripheral vision loss. Millions navigate daily life with limited vision. " +
                  "[NAV] Thoughtful design, safer crossings, clearer signals, and patience can make streets safer for everyone.");

        Debug.Log("[NAV] Whenever you’re ready, you can head back through the door to return to the initial room.");

        GotToNextStep(Phase.Done);
    }

    void GotToNextStep(Phase next)
    {
        CurrentPhase = next;

        switch (next)
        {
            case Phase.TryExpandTunnel:
                StartCoroutine(PlaySound(tryExpandClip));
                break;

            case Phase.SensoryOverload:
                StartCoroutine(PlaySound(overloadClip));
                // TODO: increase traffic/pedestrians here
                break;

            case Phase.FindCrosswalkButton:
                StartCoroutine(PlaySound(findButtonClip));
                break;

            case Phase.WaitForGreen:
                StartCoroutine(PlaySound(waitGreenClip));
                // TODO: start traffic light logic here
                break;

            case Phase.CrossStreet:
                StartCoroutine(PlaySound(crossClip));
                break;

            case Phase.Done:
                // optional ending line
                break;
        }
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