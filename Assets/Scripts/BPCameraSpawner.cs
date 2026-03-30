using UnityEngine;
using Photon.Pun;

public class BPCameraSpawner : MonoBehaviourPun
{
    public GameObject cameraItem;
    public Transform throwOrigin;

    public void SpawnCamera()
    {
        if (!photonView.IsMine) return;  // Only owner can spawn

        // Correct network spawn
        GameObject trap = PhotonNetwork.Instantiate(cameraItem.name, throwOrigin.position, Quaternion.identity);
    }
}
