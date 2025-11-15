using UnityEngine;
using Photon.Pun;

public class Treasure : MonoBehaviourPun
{
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return; // Only owner triggers it

        if (other.CompareTag("SmallPlayer"))
        {
            // Deactivate treasure across all clients
            photonView.RPC("CollectTreasureRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void CollectTreasureRPC()
    {
        gameObject.SetActive(false);

        // Tell the compass to switch to Finish
        CompassBar compass = FindObjectOfType<CompassBar>();
        if (compass != null)
        {
            compass.photonView.RPC("OnTreasureCollected", RpcTarget.AllBuffered);
        }
    }
}
