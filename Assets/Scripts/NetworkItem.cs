using UnityEngine;
using Photon.Pun;

public class NetworkItem : MonoBehaviourPun
{
    [PunRPC]
    void RPC_RequestDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
