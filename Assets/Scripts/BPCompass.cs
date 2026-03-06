using System.Collections;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class BPCompass : MonoBehaviourPun
{
    public GameObject bpCompass; // The visual compass object

    private void Start()
    {
        if (bpCompass != null)
            bpCompass.SetActive(false);
    }

    public void UseCompass()
    {
        if (!photonView.IsMine) return; // Only local BP client can use

        GameObject[] allSPs = GameObject.FindGameObjectsWithTag("SmallPlayer");

        if (allSPs.Length == 0) return; // No SPs in the scene

        // Pick a random SP
        GameObject randomSP = allSPs[Random.Range(0, allSPs.Length)];

        if (bpCompass != null)
        {
            bpCompass.SetActive(true);
            StartCoroutine(UpdateCompass(randomSP));
        }
    }

    private IEnumerator UpdateCompass(GameObject targetSP)
    {
        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration && targetSP != null)
        {
            // Direction from BP to SP, ignore y-axis
            Vector3 direction = targetSP.transform.position - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction); // world rotation
                                                                           // Make local rotation ignore camera rotation
                bpCompass.transform.localRotation = Quaternion.Euler(0f, targetRot.eulerAngles.y - transform.eulerAngles.y, 0f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (bpCompass != null)
            bpCompass.SetActive(false); // Hide after 3 seconds
    }
}



