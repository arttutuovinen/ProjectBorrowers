using UnityEngine;
using Photon.Pun;

public class BPCatchCollider : MonoBehaviourPun
{
    public Transform teleportPosition; // Assign SmallPlayerTeleportPosition in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SmallPlayer"))
        {
            PhotonView smallPlayerPV = other.GetComponent<PhotonView>();

            if (smallPlayerPV != null)
            {
                // CALL THE RPC ON THE SMALL PLAYER'S PHOTONVIEW
                smallPlayerPV.RPC("RPC_CapturePlayer", RpcTarget.All, teleportPosition.GetComponent<PhotonView>().ViewID);

            }
        }
    }
}
