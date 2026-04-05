using UnityEngine;
using Photon.Pun;
using System.Collections;

public class SPInkBottleDestroy : MonoBehaviourPun
{
    private bool hasCollided = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Prevent multiple triggers
        if (hasCollided) return;

        hasCollided = true;

        // Start destroy timer
        StartCoroutine(DestroyAfterDelay(3f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Only the owner should destroy it over the network
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
