using UnityEngine;
using Photon.Pun;

public class BPNetworkController : MonoBehaviour
{
    public Transform networkSync;

    void Update()
    {
        if (networkSync != null && !GetComponent<PhotonView>().IsMine)
        {
            transform.position = networkSync.position;
            transform.rotation = Quaternion.Euler(0, networkSync.eulerAngles.y, 0);
        }
    }
}
