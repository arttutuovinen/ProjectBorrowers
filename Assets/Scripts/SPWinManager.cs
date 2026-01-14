using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine.SceneManagement;

public class SPWinManager : MonoBehaviourPunCallbacks
{
    public static SPWinManager Instance;

    [Header("Win Settings")]
    public int escapedMaxAmount = 2; // adjustable
    public float endDelay = 5f;

    private int escapedAmount;
    private int capturedAmount;
    private bool matchEnded;

    private GameObject spWinsUI;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        spWinsUI = canvas.transform.Find("SPWins")?.gameObject;
        if (spWinsUI != null)
            spWinsUI.SetActive(false);
    }

    // ======================
    // REPORTS (called by SPs)
    // ======================

    [PunRPC]
    public void RPC_ReportEscaped()
    {
        if (!PhotonNetwork.IsMasterClient || matchEnded) return;

        escapedAmount++;
        CheckWinCondition();
    }

    [PunRPC]
    public void RPC_ReportCaptured()
    {
        if (!PhotonNetwork.IsMasterClient || matchEnded) return;

        capturedAmount++;
        CheckWinCondition();
    }

    // ======================
    // WIN CHECK
    // ======================

    private void CheckWinCondition()
    {
        int totalDone = escapedAmount + capturedAmount;

        if (escapedAmount > 0 && totalDone >= escapedMaxAmount)
        {
            matchEnded = true;
            photonView.RPC(nameof(RPC_SPWins), RpcTarget.All);
        }
    }

    // ======================
    // MATCH END
    // ======================

    [PunRPC]
    private void RPC_SPWins()
    {
        if (spWinsUI != null)
            spWinsUI.SetActive(true);

        StartCoroutine(DelayedLeaveRoom());
    }

    // ======================
    // PHOTON-SAFE ROOM LEAVE
    // ======================

    private IEnumerator DelayedLeaveRoom()
    {
        // Wait the endDelay for player to see SPWins text
        yield return new WaitForSeconds(endDelay);

        // Start leaving the room
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); // async → OnLeftRoom() will trigger
        }
        else if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect(); // async → OnDisconnected() will trigger
        }
        else
        {
            // Already disconnected, safe to load scene
            LoadMainMenu();
        }
    }

    public override void OnLeftRoom()
    {
        // After leaving room, safely disconnect
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            // Already disconnected, safe to load scene
            LoadMainMenu();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // Fully safe to load the main menu
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
