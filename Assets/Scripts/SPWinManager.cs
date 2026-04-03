using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine.SceneManagement;

public class SPWinManager : MonoBehaviourPunCallbacks
{
    public static SPWinManager Instance;

    [Header("Win Settings")]
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

    private void CheckWinCondition()
    {
        int totalDone = escapedAmount + capturedAmount;

        if (escapedAmount > 0 && totalDone >= RequiredPlayeramount.RequiredSmallPlayers)
        {
            matchEnded = true;
            photonView.RPC(nameof(RPC_SPWins), RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_SPWins()
    {
        if (spWinsUI != null)
            spWinsUI.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_ResetPlayerRoles), RpcTarget.AllBuffered);
        }

        StartCoroutine(DelayedReturnToLobby());
    }

    private IEnumerator DelayedReturnToLobby()
    {
        yield return new WaitForSeconds(endDelay);

        if (PhotonNetwork.IsMasterClient)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }
    [PunRPC]
    private void RPC_ResetPlayerRoles()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var props = new ExitGames.Client.Photon.Hashtable
        {
            { LobbyKeys.PlayerRole, null } // ← correct Photon way to clear role
        };

            player.SetCustomProperties(props);
        }
    }

}
