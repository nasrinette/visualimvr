using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarSpacingSensor : MonoBehaviour
{
    // this script is for each car to detect if a car is close to it
    // the sensor has a collider and is placed at the front of the car
    // this script was needed so that if 2 cars are close they don't collide, and a distance is maintained

    CinemachineDollyCart cart;

    float savedSpeed;
    bool hasSaved;

    readonly HashSet<Collider> blockingColliders = new HashSet<Collider>();

    public LayerMask carBodyMask; // on a separate layer

    void Awake()
    {
        cart = GetComponentInParent<CinemachineDollyCart>();
    }

    bool IsInMask(int layer)
    {
        // check if the layer is included in the layer mask
        return (carBodyMask.value & (1 << layer)) != 0;
    }


    void OnTriggerEnter(Collider other)
    {
        if (!IsInMask(other.gameObject.layer)) return;

        var otherCart = other.GetComponentInParent<CinemachineDollyCart>();
        if (otherCart == null || otherCart == cart) return;

        // to ignore other trigger colliders (like other sensors)
        if (other.isTrigger) return;

        blockingColliders.Add(other);

        if (!hasSaved) // if we saved the speed already
        {
            savedSpeed = cart.m_Speed > 0.01f ? cart.m_Speed : 5f; // default value if the speed was 0
            hasSaved = true;
        }

        cart.m_Speed = 0f; // stops
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsInMask(other.gameObject.layer)) return;

        var otherCart = other.GetComponentInParent<CinemachineDollyCart>();
        if (otherCart == null || otherCart == cart) return;
        if (other.isTrigger) return;

        blockingColliders.Remove(other);

        ResumeSpeed(); // resume the speed of the car
    }

    void FixedUpdate()
    {
        // if a collider is disabled/destroyed while inside, OnTriggerExit won't run
        blockingColliders.RemoveWhere(c => c == null || !c.enabled);

        ResumeSpeed();
    }

    void ResumeSpeed()
    {
        // this retrieves the speed of that car and continue 
        if (blockingColliders.Count == 0 && hasSaved)
        {
            cart.m_Speed = savedSpeed;
            hasSaved = false;

        }
    }

}