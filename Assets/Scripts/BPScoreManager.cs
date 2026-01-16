using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class BPScoreManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    public int maxCaptured = 1;

    private int currentCaptured = 0;
    private TextMeshProUGUI captureText;
    private GameObject bpWins;
    private bool gameEnded = false;
    public Collider scoreTrigger;

    private void Start()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        captureText = GameObject.Find("Canvas/BigPlayerUI/Capture")
            ?.GetComponent<TextMeshProUGUI>();

        bpWins = canvas.transform.Find("BPWins")?.gameObject;
        UpdateText(currentCaptured);
    }

    // ======================
    // SCORE
    // ======================

    [PunRPC]
    private void RPC_RequestAddCapture()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        currentCaptured = Mathf.Clamp(currentCaptured + 1, 0, maxCaptured);
        photonView.RPC(nameof(RPC_UpdateCapturedCount), RpcTarget.All, currentCaptured);
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

            StartCoroutine(DelayedLeaveRoom());
        }
    }

    // ======================
    // MATCH END (SAFE EXIT)
    // ======================

    private IEnumerator DelayedLeaveRoom()
    {
        // Show BPWins UI for a few seconds
        yield return new WaitForSeconds(6f);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); // async
        }
        else if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            LoadMainMenu();
        }
    }

    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            LoadMainMenu();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // ======================
    // UI
    // ======================

    private void UpdateText(int count)
    {
        if (captureText != null)
            captureText.text = $"Captured: {count}/{maxCaptured}";
    }

    // ======================
    // SCORE TRIGGER CONTROL
    // ======================

    [PunRPC]
    public void RPC_DisableScoreTrigger()
    {
        scoreTrigger.enabled = false;
    }

    [PunRPC]
    public void RPC_EnableScoreTrigger()
    {
        scoreTrigger.enabled = true;
    }
}
