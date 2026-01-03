using UnityEngine;
using Photon.Pun;
using TMPro;

public class BPScoreManager : MonoBehaviourPun
{
    [Header("Settings")]
    public int maxCaptured = 2;

    private int currentCaptured = 0;
    private TextMeshProUGUI captureText;

    private void Start()
    {
        // Find Capture text at runtime
        GameObject captureObj = GameObject.Find("Canvas/BigPlayerUI/Capture");
        if (captureObj != null)
            captureText = captureObj.GetComponent<TextMeshProUGUI>();
        else
            Debug.LogError("Capture text not found in scene!");

        // Update UI on all clients when joining mid-game
        UpdateText(currentCaptured);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Only Master Client counts
        if (!other.CompareTag("SmallPlayer")) return;

        currentCaptured++;
        currentCaptured = Mathf.Clamp(currentCaptured, 0, maxCaptured);

        // Sync count to all clients
        photonView.RPC("RPC_UpdateCapturedCount", RpcTarget.All, currentCaptured);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Only Master Client counts
        if (!other.CompareTag("SmallPlayer")) return;

        currentCaptured--;
        currentCaptured = Mathf.Clamp(currentCaptured, 0, maxCaptured);

        // Sync count to all clients
        photonView.RPC("RPC_UpdateCapturedCount", RpcTarget.All, currentCaptured);
    }

    [PunRPC]
    private void RPC_UpdateCapturedCount(int newCount)
    {
        currentCaptured = newCount;
        UpdateText(currentCaptured);
    }

    private void UpdateText(int count)
    {
        if (captureText != null)
            captureText.text = $"Captured: {count}/{maxCaptured}";
    }
}
