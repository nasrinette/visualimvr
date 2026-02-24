using UnityEngine;
using UnityEngine.AI;

public class CharacterWalking : MonoBehaviour
{
    // this script just sets the animation of the character spawn when the user waits for the green light
    // it also has functions:
    // - to go to a certain position (used to go to the hit position)
    // - to stop moving when they are at the hit position
    // - and then resume moving
    public NavMeshAgent agent;

    Animator animator;

    bool isWalking;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void GoToPosition(Vector3 position)
    {
        agent.SetDestination(position);
        if (!isWalking)
        {
            isWalking = true;
            animator.SetTrigger("Walk");
        }

    }

    public void StopMoving()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (isWalking)
        {
            isWalking = false;
            animator.SetTrigger("Stop");
        }
    }

    public void ResumeMoving()
    {
        agent.isStopped = false;
        if (!isWalking)
        {
            isWalking = true;
            animator.SetTrigger("Walk");
        }
    }
}