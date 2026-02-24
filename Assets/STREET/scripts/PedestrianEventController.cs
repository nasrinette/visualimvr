using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PedestrianEventController : MonoBehaviour
{
    // this class controls the pedestrian that will be spawn and will hit the player when the player press the button and waits for the green light
    public Transform spawnPosition;

    public GameObject pedestrianPrefab;

    private AudioSource source;
    public AudioSource secondSource;
    public AudioClip pedestrianTalking;
    public AudioClip hitSound;

    public Transform xrOrigin;
    public Transform xrCamera;

    Coroutine routine;

    public Transform exitTarget;

    public float approachDurationMax = 6f; // safety so it doesn't loop forever


    public TunnelVisionInput tunnelVisionInput;

    public void StartWaitForGreenMoment()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PedestrianBumpSequence());
    }

    Vector3 GetBumpPoint()
    {
        // retrieves the point where it will hit the player
        // takes the current xr camera position and position it a little forward and on the right
        Vector3 forward = xrCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 bumpPos = xrOrigin.position + forward * 1.2f;
        bumpPos += xrCamera.right * 0.1f;
        bumpPos.y = xrOrigin.position.y;
        return bumpPos;
    }

    IEnumerator RotateYTo(Transform tr, float targetY, float duration)
    {
        // helper function to rotate the pedestrian to say its audio line 
        // rotates smoothly
        float startY = tr.eulerAngles.y;
        float t = 0f;

        while (t < duration)
        {
            float y = Mathf.LerpAngle(startY, targetY, t / duration);
            tr.rotation = Quaternion.Euler(0f, y, 0f);
            t += Time.deltaTime;
            yield return null;
        }

        tr.rotation = Quaternion.Euler(0f, targetY, 0f);
    }

    IEnumerator PedestrianBumpSequence()
    {
        Debug.Log("fired pedestrian");
        yield return new WaitForSeconds(1.0f);

        var newPedestrian = Instantiate(pedestrianPrefab, spawnPosition.position, spawnPosition.rotation);

        var ped = newPedestrian.GetComponent<CharacterWalking>();
        if (ped == null)
        {
            yield break;
        }

        // makes the pedestrian move to bumppoint and stops when it's close to the bumpPoint
        float t = 0f;
        while (t < approachDurationMax)
        {
            Vector3 bumpPoint = GetBumpPoint();
            ped.GoToPosition(bumpPoint);

            // stop when pedestrian is close to the bump point
            if (Vector3.Distance(newPedestrian.transform.position, bumpPoint) <= 0.25f)
                break;

            t += Time.deltaTime;
            yield return null;
        }

        // interpelation part: character turn; stops; say "hey...!" 
        // turns
        yield return StartCoroutine(RotateYTo(newPedestrian.transform, 50f, 0.25f)); 

        // hit sound + say line
        source = newPedestrian.GetComponent<AudioSource>();

        if (secondSource && hitSound)
        {
            secondSource.clip = hitSound;
            secondSource.Play();
        }

        if (source && pedestrianTalking)
        {
            source.clip = pedestrianTalking;
            source.Play();
        }
        // effect on vision 
        if (tunnelVisionInput != null)
        {
            tunnelVisionInput.ReduceBaseRadius(0.02f);

        }
        // stops
        ped.StopMoving();
        yield return new WaitForSeconds(1.0f);
        
        // rotate in right direction to continue
        yield return StartCoroutine(RotateYTo(newPedestrian.transform, 180f, 0.25f));

        // continue moving on its path until the exit Target (a target that I set)
        ped.ResumeMoving();
        if (exitTarget != null)
            ped.GoToPosition(exitTarget.position);

    }
}