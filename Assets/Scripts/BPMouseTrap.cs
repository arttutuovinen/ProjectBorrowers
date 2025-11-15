using UnityEngine;
using Photon.Pun;

public class BPMouseTrap : MonoBehaviourPun
{
    public GameObject mouseTrap;
    public Transform throwOrigin;

    public void SpawnMouseTrap()
    {
        if (!photonView.IsMine) return;  // Only owner can spawn

        // Correct network spawn
        GameObject trap = PhotonNetwork.Instantiate(mouseTrap.name, throwOrigin.position, Quaternion.identity);

        // Sync to *everyone*, but only the local player will handle camera logic
        photonView.RPC("RPC_SetMouseTrapVisibility", RpcTarget.All, trap.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void RPC_SetMouseTrapVisibility(int trapViewID, PhotonMessageInfo info)
    {
        // Only the LOCAL client should adjust its camera
        if (!photonView.IsMine) return;

        PhotonView trapPv = PhotonView.Find(trapViewID);
        if (trapPv == null) return;

        GameObject trapObject = trapPv.gameObject;

        Camera cam = Camera.main;
        if (cam == null) return;

        int smallPlayerLayer = LayerMask.NameToLayer("SmallPlayer");
        int bigPlayerLayer = LayerMask.NameToLayer("BigPlayer");

        int playerLayer = gameObject.layer;

        // Small player should NOT see the trap
        if (playerLayer == smallPlayerLayer)
        {
            cam.cullingMask &= ~(1 << trapObject.layer);
        }
        // Big player SHOULD see the trap
        else if (playerLayer == bigPlayerLayer)
        {
            cam.cullingMask |= (1 << trapObject.layer);
        }
    }
}
