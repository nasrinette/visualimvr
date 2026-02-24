using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class CarHonkManager : MonoBehaviour
{
    // this script handles when a car should honk (when the player is on the crosswalk but it is red for pedestrians)
   
    public List<CarStopLine> stopLines = new List<CarStopLine>(); // stopLines are all the lines with colliders placed on the road, to know when a car should stop (queue effect)

    public List<AudioClip> hornSounds = new List<AudioClip>(); // this is a list of multiple horn sounds, to choose a random one from that list each time, to sound more natural

    private Coroutine honkRoutine;

    public TunnelVisionInput tunnelVisionInput;

    public void StartHonking()
    {
        if (honkRoutine == null)
            honkRoutine = StartCoroutine(HonkLoop());
    }

    // called when the player leaves the crosswalk (or cars turn red) ; they should stop honking
    public void StopHonking()
    {
        if (honkRoutine != null)
        {
            StopCoroutine(honkRoutine);
            honkRoutine = null;
        }
    }

    IEnumerator HonkLoop()
    {
        // this function chooses a car randomly from the one that stopped, choose a random honk and honk
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3.5f)); // random waiting time between each honk

            List<CinemachineDollyCart> stoppedCars = new List<CinemachineDollyCart>();

            foreach (var line in stopLines)
            {
                if (line == null) continue;

                var car = line.CurrentCar;
                if (car == null) continue;

                if (car.m_Speed <= 0.01f) // only stopped cars ; just in case
                    stoppedCars.Add(car);
            }

            if (stoppedCars.Count == 0) continue;

            var chosenCar = stoppedCars[Random.Range(0, stoppedCars.Count)]; // random car

            var audio = chosenCar.GetComponentInChildren<AudioSource>();
            if (audio != null && hornSounds.Count > 0)
            {
                var randomClip = hornSounds[Random.Range(0, hornSounds.Count)];
                audio.pitch = Random.Range(0.9f, 1.1f); // to make it more random
                audio.PlayOneShot(randomClip);
                audio.pitch = 1f;
            }
            
            if (tunnelVisionInput != null)
                tunnelVisionInput.ReduceBaseRadius(0.02f);
        }
    }



}