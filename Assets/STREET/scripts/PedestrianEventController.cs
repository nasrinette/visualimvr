using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PedestrianEventController : MonoBehaviour
{
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

    public float stopDuration = 0.6f;
    public float approachDurationMax = 6f; // safety so it doesn't loop forever


    public TunnelVisionInput tunnelVisionInput;

    public void StartWaitForGreenMoment()
    {
        Debug.Log("StartWaitForGreenMoment");
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PedestrianBumpSequence());
    }

    Vector3 GetBumpPoint()
    {
        Vector3 forward = xrCamera.forward;
        forward.y = 0f;
        forward.Normalize();


        Vector3 bumpPos = xrOrigin.position + forward * 1.2f;
        bumpPos += xrCamera.right * 0.1f;
        bumpPos.y = xrOrigin.position.y;
        // Add roation y axis 110
        return bumpPos;
    }

    IEnumerator RotateYTo(Transform tr, float targetY, float duration)
    {
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
        float t = 0f;
        while (t < approachDurationMax)
        {
            Vector3 bumpPoint = GetBumpPoint();
            ped.GoToPosition(bumpPoint);

            // stop when pedestrian is close to the bump point in front
            if (Vector3.Distance(newPedestrian.transform.position, bumpPoint) <= 0.25f)
                break;

            t += Time.deltaTime;
            yield return null;
        }
        yield return StartCoroutine(RotateYTo(newPedestrian.transform, 50f, 0.25f));

        

        source = newPedestrian.GetComponent<AudioSource>();

        // this is to be sure we wait until the pedestrian is close to user to fire the sounds

        if (secondSource && hitSound)
        {
            secondSource.clip = hitSound;
            secondSource.Play();
            // yield return new WaitForSeconds(hitSound.length);
        }
        if (source && pedestrianTalking)
        {
            source.clip = pedestrianTalking;
            source.Play();
        }
        if (tunnelVisionInput != null)
        {
            tunnelVisionInput.ReduceBaseRadius(0.02f);

        }

        ped.StopMoving();

        yield return new WaitForSeconds(1.0f);
        
        yield return StartCoroutine(RotateYTo(newPedestrian.transform, 180f, 0.25f));

        ped.ResumeMoving();
        if (exitTarget != null)
            ped.GoToPosition(exitTarget.position);

    }
}