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
    }  
}
