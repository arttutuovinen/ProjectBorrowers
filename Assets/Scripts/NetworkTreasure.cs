using UnityEngine;
using Photon.Pun;

public class NetworkTreasure : MonoBehaviourPun
{
    [PunRPC]
    public void RPC_TeleportTreasure(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(pos, rot);
        gameObject.SetActive(true);
    }

    [PunRPC]
    public void RPC_DeactivateTreasure()
    {
        gameObject.SetActive(false);
    }
}
