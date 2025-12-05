using UnityEngine;
using Photon.Pun;

public class BPCatchCollider : MonoBehaviourPun
{
    public PhotonView teleportPV; // Instead of Transform!

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (!other.CompareTag("SmallPlayer")) return;
        Debug.Log("BP caught SP!"); //This works. 
        PhotonView smallPV = other.GetComponent<PhotonView>();
        if (smallPV != null && teleportPV != null)
        {
            smallPV.RPC("RPC_CapturePlayer", RpcTarget.All, teleportPV.ViewID);
        }
        else
        {
            Debug.LogError("Teleport target not found or SmallPlayer missing PhotonView!");
        }
    }
}
