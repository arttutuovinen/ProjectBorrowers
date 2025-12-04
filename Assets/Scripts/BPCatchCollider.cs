using UnityEngine;
using Photon.Pun;

public class BPCatchCollider : MonoBehaviourPun
{
    public PhotonView teleportPV; // Instead of Transform!
    // Drag the PhotonView component here, NOT the Transform.

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return; // Only BP owner should trigger this

        if (other.CompareTag("SmallPlayer"))
        {
            PhotonView smallPlayerPV = other.GetComponent<PhotonView>();

            if (smallPlayerPV != null && teleportPV != null)
            {
                // Safe RPC call with guaranteed valid ViewID
                smallPlayerPV.RPC("RPC_CapturePlayer", RpcTarget.All, teleportPV.ViewID);
            }
            else
            {
                Debug.LogError("Teleport PhotonView is missing or SmallPlayer has no PhotonView!");
            }
        }
    }
}
