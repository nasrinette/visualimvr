using UnityEngine;
using System.Collections;

public class PedestrianSignal : MonoBehaviour
{
    public GameObject redLightCover;
    public GameObject greenLightCover;

    public GameObject redLightCoverOpposite;
    public GameObject greenLightCoverOpposite;


    public float waitTime = 4f;

    bool isWaiting = false;

    public void RequestCrossing()
    {
        if (!isWaiting)
            StartCoroutine(CrossRoutine());
    }

    IEnumerator CrossRoutine()
    {
        isWaiting = true;

        // redLight.SetActive(true);
        // greenLight.SetActive(false);

        yield return new WaitForSeconds(waitTime);

        redLightCover.SetActive(true);
        greenLightCover.SetActive(false);

        redLightCoverOpposite.SetActive(true);
        greenLightCoverOpposite.SetActive(false);


        yield return new WaitForSeconds(15f);

        redLightCover.SetActive(false);
        greenLightCover.SetActive(true);

        redLightCoverOpposite.SetActive(false);
        greenLightCoverOpposite.SetActive(true);


        isWaiting = false;
    }
}