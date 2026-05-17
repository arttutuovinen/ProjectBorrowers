using UnityEngine;
using Photon.Pun;

public class PlayerRole : MonoBehaviourPun
{
    public static bool IsSmallPlayerClient;

    void Start()
    {
        if (!photonView.IsMine) return;

        // Decide role based on tag (your setup)
        IsSmallPlayerClient = CompareTag("SmallPlayer");
        DebugRole();
    }
    void DebugRole()
    {
        Debug.Log("PlayerRole: " + (CompareTag("SmallPlayer") ? "SmallPlayer" : "BigPlayer"));
    }
}
