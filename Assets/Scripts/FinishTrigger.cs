using UnityEngine;
using Photon.Pun;

public class FinishTrigger : MonoBehaviourPun
{
    private TreasureSpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<TreasureSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SmallPlayer"))
            return;

        if (!PhotonNetwork.IsMasterClient)
            return;

        // Teleport & reactivate treasure
        spawner.TeleportTreasure();

        // Notify all compasses to point back to treasure
        CompassBar compass = FindObjectOfType<CompassBar>();
        if (compass != null)
        {
            compass.photonView.RPC("OnTreasureRespawned", RpcTarget.AllBuffered);
        }
    }
}

