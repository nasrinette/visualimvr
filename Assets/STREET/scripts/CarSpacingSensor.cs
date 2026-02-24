using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarSpacingSensor : MonoBehaviour
{
    CinemachineDollyCart cart;

    float savedSpeed;
    bool hasSaved;

    readonly HashSet<Collider> blockingColliders = new HashSet<Collider>();

    void Awake()
    {
        cart = GetComponentInParent<CinemachineDollyCart>();
    }
    public LayerMask carBodyMask;

    bool IsInMask(int layer)
    {
        return (carBodyMask.value & (1 << layer)) != 0;
    }
    void OnTriggerEnter(Collider other)
    {
        if (!IsInMask(other.gameObject.layer)) return;

        var otherCart = other.GetComponentInParent<CinemachineDollyCart>();
        if (otherCart == null || otherCart == cart) return;

        // Ignore other trigger colliders (like other sensors)
        if (other.isTrigger) return;

        blockingColliders.Add(other);

        if (!hasSaved)
        {
            savedSpeed = cart.m_Speed > 0.01f ? cart.m_Speed : 5f;
            hasSaved = true;
        }

        cart.m_Speed = 0f;
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsInMask(other.gameObject.layer)) return;

        var otherCart = other.GetComponentInParent<CinemachineDollyCart>();
        if (otherCart == null || otherCart == cart) return;
        if (other.isTrigger) return;

        blockingColliders.Remove(other);

        TryResume();
    }

    void FixedUpdate()
    {
        // If a collider is disabled/destroyed while inside, OnTriggerExit won't run
        blockingColliders.RemoveWhere(c => c == null || !c.enabled);

        TryResume();
    }

    void TryResume()
    {
        if (blockingColliders.Count == 0 && hasSaved)
        {
            cart.m_Speed = savedSpeed;
            hasSaved = false;

        }
    }

}