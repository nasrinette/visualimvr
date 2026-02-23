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

    void OnTriggerEnter(Collider other)
    {
        var cart = other.GetComponentInParent<CinemachineDollyCart>();
        if (cart == null) return;

        if (isFrontStopLine)
        {
            currentCar = cart;
        }
            

    }
    void OnTriggerStay(Collider other)
    {
        var cart = other.GetComponentInParent<CinemachineDollyCart>();
        if (cart == null || signal == null) return;

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
        if (car == null) return;
        if (!saved.ContainsKey(car)) saved[car] = car.m_Speed; // we keep the car speed
        car.m_Speed = 0f; // stop car
    }

    public void Release(CinemachineDollyCart car)
    {
        if (car == null) return;
        if (saved.TryGetValue(car, out var speed))
        {
            car.m_Speed = speed; // we retrieve the corresponding car speed
            saved.Remove(car);
        }
    }
}
