using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;

public class BPScoreManager : MonoBehaviourPun
{
    [Header("Settings")]
    public int maxCaptured = 1;

    private int currentCaptured = 0;
    private TextMeshProUGUI captureText;
    private GameObject bpWins;
    private bool gameEnded = false;

    private void Start()
    {
        captureText = GameObject.Find("Canvas/BigPlayerUI/Capture")
            ?.GetComponent<TextMeshProUGUI>();

        bpWins = GameObject.Find("Canvas/BPWins");
        UpdateText(currentCaptured);
    }

    [PunRPC]
    private void RPC_RequestAddCapture()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        currentCaptured = Mathf.Clamp(currentCaptured + 1, 0, maxCaptured);
        photonView.RPC("RPC_UpdateCapturedCount", RpcTarget.All, currentCaptured);
    }

    [PunRPC]
    private void RPC_UpdateCapturedCount(int newCount)
    {
        currentCaptured = newCount;
        UpdateText(currentCaptured);

        if (!gameEnded && currentCaptured >= maxCaptured)
        {
            gameEnded = true;

            if (bpWins != null)
                bpWins.SetActive(true);

            StartCoroutine(WinTimer());
        }
    }

    private IEnumerator WinTimer()
    {
        yield return new WaitForSeconds(3f);

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("MainMenu");
    }

    private void UpdateText(int count)
    {
        if (captureText != null)
            captureText.text = $"Captured: {count}/{maxCaptured}";
    }
}
