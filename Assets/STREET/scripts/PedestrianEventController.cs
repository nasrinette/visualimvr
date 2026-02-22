using System.Collections;
using UnityEngine;

public class PedestrianEventController : MonoBehaviour
{
    public Transform spawnPosition;

    public GameObject pedestrianPrefab;

    public AudioSource source;
    public AudioClip pedestrianTalking;
    public AudioClip hitSound;

    public Transform xrOrigin;
    public Transform xrCamera;

    Coroutine routine;

    public Transform exitTarget;

    public float stopDuration = 0.8f;
    public float approachDurationMax = 6f; // safety so it doesn't loop forever


    public void StartWaitForGreenMoment()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PedestrianBumpSequence());
    }

    Vector3 GetBumpPoint()
    {
        Vector3 forward = xrCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 bumpPos = xrOrigin.position + forward * 0.6f;
        bumpPos += xrCamera.right * 0.1f;
        bumpPos.y = xrOrigin.position.y;

        return bumpPos;
    }

    IEnumerator PedestrianBumpSequence()
    {
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

        ped.StopMoving(); 

        source = newPedestrian.GetComponent<AudioSource>();

        // this is to be sure we wait until the pedestrian is close to user to fire the sounds
        if (source && hitSound)
        {
            source.clip = hitSound;
            source.Play();
            yield return new WaitForSeconds(hitSound.length);
        }
        if (source && pedestrianTalking)
        {
            source.clip = pedestrianTalking;
            source.Play();
        }

        yield return new WaitForSeconds(stopDuration);

        ped.ResumeMoving(); 
        if (exitTarget != null)
            ped.GoToPosition(exitTarget.position);

    }
}