using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class CarHonkManager : MonoBehaviour
{
    // this script is: a car stops and honks if the player is on the crosswalk when it is red for pedestrians

    // the two stoplines closest to the crosswalk
    // public CarStopLine stopLine1;
    // public CarStopLine stopLine2;

    public List<CarStopLine> stopLines = new List<CarStopLine>();

    public List<AudioClip> hornSounds = new List<AudioClip>();

    private readonly HashSet<CinemachineDollyCart> honkedThisBlock = new HashSet<CinemachineDollyCart>();

    private Coroutine honkRoutine;

    // bool busy;

    public TunnelVisionInput tunnelVisionInput;

    public void StartHonkingIfNeeded()
    {
        if (honkRoutine == null)
            honkRoutine = StartCoroutine(HonkAllStoppedCarsLoop());
    }

    // Call when the player leaves the crosswalk (or cars turn red)
    public void StopHonkingAndReset()
    {
        if (honkRoutine != null)
        {
            StopCoroutine(honkRoutine);
            honkRoutine = null;
        }
        honkedThisBlock.Clear();
    }

    IEnumerator HonkAllStoppedCarsLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3.5f));

            List<CinemachineDollyCart> stoppedCars = new List<CinemachineDollyCart>();

            foreach (var line in stopLines)
            {
                if (line == null) continue;

                var car = line.CurrentCar;
                if (car == null) continue;

                if (car.m_Speed <= 0.01f) // only stopped cars
                    stoppedCars.Add(car);
            }

            if (stoppedCars.Count == 0)
                continue;

            var chosenCar = stoppedCars[Random.Range(0, stoppedCars.Count)];

            var audio = chosenCar.GetComponentInChildren<AudioSource>();


            if (audio != null && hornSounds.Count > 0)
            {
                var randomClip = hornSounds[Random.Range(0, hornSounds.Count)];
                audio.pitch = Random.Range(0.9f, 1.1f); // to make it more random
                audio.PlayOneShot(randomClip);
                audio.pitch = 1f;
                Debug.Log($"[{chosenCar.name}] HONK ({randomClip.name})");
            }

            if (tunnelVisionInput != null)
                tunnelVisionInput.ReduceBaseRadius(0.02f);
        }
    }


    // public bool TryHonkNow()
    // {
    //     // if (busy) return false; // todo fix that??
    //     // we retrieve if a car is inside the stop lines
    //     CarStopLine line = null;
    //     var car = stopLine1?.CurrentCar;
    //     if (car != null) line = stopLine1;
    //     else
    //     {
    //         car = stopLine2?.CurrentCar;
    //         if (car != null) line = stopLine2;
    //     }

    //     // var car = stopLine1?.CurrentCar ?? stopLine2?.CurrentCar;
    //     // if (car == null) return false;
    //     if (car == null || line == null) return false;

    //     StartCoroutine(Honk(line, car));
    //     return true;
    // }

    // IEnumerator Honk(CarStopLine line, CinemachineDollyCart car)
    // {
    //     busy = true;

    //     // car.m_Speed = 0f;
    //     line.Hold(car);
    //     var audio = car.GetComponentInChildren<AudioSource>();
    //     if (audio != null && hornSound != null)
    //     {
    //         audio.PlayOneShot(hornSound);
    //     }
    //     yield return new WaitForSeconds(stopDuration);

    //     busy = false;
    //     if (tunnelVisionInput != null)
    //     {
    //         tunnelVisionInput.ReduceBaseRadius(0.02f);

    //     }
    // }

}