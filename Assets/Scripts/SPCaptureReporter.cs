using UnityEngine;
using Photon.Pun;

public class SPCaptureReporter : MonoBehaviourPun
{
    private void OnTriggerEnter(Collider other)
    {
        // Only the owning client reports the trigger
        if (!photonView.IsMine)
            return;

        BPScoreManager scoreManager = other.GetComponent<BPScoreManager>();
        if (scoreManager == null)
            return;

        // Notify Master Client that a Small Player entered the jar
        scoreManager.photonView.RPC(
            "RPC_RequestAddCapture",
            RpcTarget.MasterClient
        );
    }
}
