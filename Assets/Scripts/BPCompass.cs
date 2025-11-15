using System.Collections;
using UnityEngine;
using Photon.Pun;

public class BPCompass : MonoBehaviourPun
{
    public GameObject smallPlayer; // Can be null if no small player exists
    public GameObject bpCompass;

    private void Start()
    {
        bpCompass.SetActive(false);
    }

    public void UseCompass()
    {
        if (!photonView.IsMine) return;   // Only local player can use their compass

        bpCompass.SetActive(true);
        StartCoroutine(UpdateCompass());
    }

    private IEnumerator UpdateCompass()
    {
        float duration = 4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (smallPlayer != null)
            {
                Vector3 directionToSmallPlayer = smallPlayer.transform.position - transform.position;
                directionToSmallPlayer.y = 0;

                if (directionToSmallPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToSmallPlayer);
                    transform.rotation = targetRotation;
                }
            }

            // If smallPlayer is null, compass just stays active but does nothing
            elapsed += Time.deltaTime;
            yield return null;
        }

        bpCompass.SetActive(false); // Hide compass after duration
    }
}



