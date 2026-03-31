using UnityEngine;
using Photon.Pun;

public class BPCameraSpawner : MonoBehaviourPun
{
    public GameObject cameraItem;
    public Transform throwOrigin;

    public void SpawnCamera()
    {
        if (!photonView.IsMine) return;  // Only owner can spawn

        // Get ONLY Y rotation from player
        float yRotation = transform.eulerAngles.y + 180f;
        Quaternion spawnRotation = Quaternion.Euler(0f, yRotation, 0f);

        // Spawn with correct rotation
        GameObject cam = PhotonNetwork.Instantiate(cameraItem.name, throwOrigin.position, spawnRotation);
    }
}
