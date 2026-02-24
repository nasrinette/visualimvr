using UnityEngine;
using UnityEngine.AI;

public class CharacterWalking : MonoBehaviour
{
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