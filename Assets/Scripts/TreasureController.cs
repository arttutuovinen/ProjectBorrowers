using UnityEngine;
using Photon.Pun;

public class TreasureController : MonoBehaviourPun
{
    [PunRPC]
    public void RPC_TeleportTreasure(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
}

