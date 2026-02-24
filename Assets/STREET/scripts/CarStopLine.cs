using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class CarStopLine : MonoBehaviour
{
    // this script is attached to each stop line, we have multiple stop lines so that if multiple cars need to stop
    // they stay in queue, one behing each other

    public LightTrafficController signal;

    public bool isFrontStopLine = false;
    public CarStopLine lineAhead;  // it's the line after this one, to check if it's occupied or not (if a car is ahead or not) 


    CinemachineDollyCart currentCar;
    public CinemachineDollyCart CurrentCar => currentCar;  // other scripts can't modify that value

    Dictionary<CinemachineDollyCart, float> saved = new Dictionary<CinemachineDollyCart, float>(); // stores the current speed for each value

    public float fallbackSpeed = 10f;

    public bool inverseSide;

    void OnTriggerEnter(Collider other)
    {
        var cart = other.GetComponentInParent<CinemachineDollyCart>();
        if (cart == null) return;

        currentCar = cart;

    }

    bool HasPassedLine(Transform carT)
    {
        // if car is in front of the stop line plane, it has passed
        Vector3 fwd = transform.forward;
        Vector3 toCar = carT.position - transform.position;
        return Vector3.Dot(fwd, toCar) > 0f;
    }

    void OnTriggerStay(Collider other)
    {
        var cart = other.GetComponentInParent<CinemachineDollyCart>();
        if (cart == null || signal == null) return;

        if (isFrontStopLine && HasPassedLine(cart.transform)) // if the first car has passed the stop line already, it can continue, so that it doesnt stay in the middle of the crosswalk
        {
            Release(cart);
            if (currentCar == cart) currentCar = null;
            return;
        }


        if (!signal.ShouldCarsStop) // if it red for pedestrians
        {
            Release(cart); // the car can just go
            return;
        }

        if (isFrontStopLine || (lineAhead != null && lineAhead.CurrentCar != null)) // if either we are in the 1st line or the lines ahead are already occupied by other cars
        {

            Hold(cart); // we stop the car here 

        }
        else
        {
            // let it keep going forward to the next zone (closer to crosswalk)
            Release(cart);
            if (currentCar == cart) currentCar = null;
        }
    }

    void OnTriggerExit(Collider other)
    {

        var cart = other.GetComponentInParent<CinemachineDollyCart>();
        if (cart == null) return;

        if (currentCar == cart) currentCar = null;
        Release(cart); // car can just go
    }


    public void Hold(CinemachineDollyCart car)
    {
        // saved the speed of the car; or fallback speed if the speed was close to 0
        if (car == null) return;

        if (!saved.ContainsKey(car))
        {
            saved[car] = (car.m_Speed > 0.01f) ? car.m_Speed : fallbackSpeed;
        }
        car.m_Speed = 0f;
    }

    public void Release(CinemachineDollyCart car)
    {
        // we retrieve the speed of the car before it stopped
        if (car == null) return;

        if (saved.TryGetValue(car, out var speed))
        {
            car.m_Speed = speed; // we retrieve the corresponding car speed
            saved.Remove(car);
        }
    }
}
