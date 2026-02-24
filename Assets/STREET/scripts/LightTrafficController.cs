using UnityEngine;
using System.Collections;

// green for car, red for peds:
// - if user not on crosswalk: car just go, no issue here
// - if user on crosswalk, stop cars at the corresponding line (so they queue if needed) and the first car honks, then if user goes out of the crosswalk, then the cars can start again (basically they are not going to kill the user)

// red for cars, green for peds:
// - if cars come at stop lines, stop cars at the corresponding line (they will queue if needed) until it becomes red for peds again


public enum SignalState
{
    CarsGreen_PedsRed,
    CarsRed_PedsGreen
}

public class LightTrafficController : MonoBehaviour
{
    public GameObject redLightCover;
    public GameObject greenLightCover;
    public GameObject redLightCoverOpposite;
    public GameObject greenLightCoverOpposite;



    public AudioClip beepSound;
    public AudioClip waitSound;
    public AudioClip greenSound;
    public AudioSource audioSource;

    public CrosswalkArea crosswalkArea;
    public CarHonkManager carHonkManager;

    public float waitTime = 6f;
    public float pedsGreenDuration = 4f;

    public SignalState state = SignalState.CarsGreen_PedsRed;

    bool isWaiting;

    // the cars stop if it's red for them OR if it's green but the player is currenlty inside the crosswalk area
    public bool ShouldCarsStop => (state == SignalState.CarsRed_PedsGreen) || ((state == SignalState.CarsGreen_PedsRed) && crosswalkArea != null && crosswalkArea.playerInside);


    void Awake()
    {
        ApplyVisuals();
    }


    public void RequestCrossing()
    {
        if (!isWaiting)
            StartCoroutine(CrossRoutine());
    }



    IEnumerator CrossRoutine()
    {
        // WAITING TIME AFTER ASKED FOR GREEN
        isWaiting = true;

        PlayClip(waitSound);
        yield return new WaitForSeconds(waitTime);

        // GREEN FOR PEDESTRIANS (cars red)
        SetState(SignalState.CarsRed_PedsGreen);

        PlayClip(greenSound);
        yield return new WaitForSeconds(2f);

        // beeps during peds green
        PlayClip(beepSound);
        yield return new WaitForSeconds(pedsGreenDuration);

        // RED FOR PEDESTRIANS (cars green)
        audioSource.Stop();
        SetState(SignalState.CarsGreen_PedsRed);

        isWaiting = false;
    }

    void Update()
    {
        // cars are green but player is still in crosswalk
        bool shouldHonk = (state == SignalState.CarsGreen_PedsRed) && (crosswalkArea != null && crosswalkArea.playerInside);

        if (shouldHonk)
            carHonkManager.StartHonking();
        else
            carHonkManager.StopHonking();


    }

    void SetState(SignalState newState)
    {
        state = newState;
        ApplyVisuals();
    }

    void ApplyVisuals()
    {
        // this function is to cover the red of green pedestrian image for the light traffic corresponding to the state
        bool pedsGreen = (state == SignalState.CarsRed_PedsGreen);

        redLightCover.SetActive(pedsGreen);
        greenLightCover.SetActive(!pedsGreen);

        redLightCoverOpposite.SetActive(pedsGreen);
        greenLightCoverOpposite.SetActive(!pedsGreen);
    }

    void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();
    }

}