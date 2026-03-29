using UnityEngine;
using Photon.Pun;

public class MouseTrap : MonoBehaviourPun
{
    [PunRPC]
    public void RPC_DestroyTrap()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
