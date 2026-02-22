using UnityEngine;
using UnityEngine.AI;

public class CharacterWalking : MonoBehaviour
{
    public NavMeshAgent agent;

    float savedSpeed;
    public void GoToPosition(Vector3 position)
    {
        agent.SetDestination(position);
    }

    public void StopMoving()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    public void ResumeMoving()
    {
        agent.isStopped = false;
    }
}