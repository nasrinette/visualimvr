using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class CarHonkManager : MonoBehaviour
{
    // this script is: a car stops and honks if the player is on the crosswalk when it is red for pedestrians

    // the two stoplines closest to the crosswalk
    public CarStopLine stopLine1;
    public CarStopLine stopLine2;


    public float stopDuration = 3.0f;

    public AudioClip hornSound;

    bool busy;

    public TunnelVisionInput tunnelVisionInput;

    public bool TryHonkNow()
    {
        // if (busy) return false; // todo fix that??
        // we retrieve if a car is inside the stop lines
        CarStopLine line = null;
        var car = stopLine1?.CurrentCar;
        if (car != null) line = stopLine1;
        else
        {
            car = stopLine2?.CurrentCar;
            if (car != null) line = stopLine2;
        }

        // var car = stopLine1?.CurrentCar ?? stopLine2?.CurrentCar;
        // if (car == null) return false;
 if (car == null || line == null) return false;
 
        StartCoroutine(Honk(line, car));
        return true;
    }

    IEnumerator Honk(CarStopLine line,CinemachineDollyCart car)
    {
        busy = true;

        // car.m_Speed = 0f;
        line.Hold(car);
        var audio = car.GetComponentInChildren<AudioSource>();
        if (audio != null && hornSound != null)
        {
            audio.PlayOneShot(hornSound);
        }
        yield return new WaitForSeconds(stopDuration);

        busy = false;
         if (tunnelVisionInput != null)
        {
            tunnelVisionInput.ReduceBaseRadius(0.02f);

        }
    }

}