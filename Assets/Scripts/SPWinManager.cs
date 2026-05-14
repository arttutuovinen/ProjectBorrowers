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
        Debug.Log("ESCAPED = " + escapedAmount);
        Debug.Log("JAILED = " + SPCaptureHandler.JailedSPs.Count);
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        int totalSPs = RequiredPlayeramount.RequiredSmallPlayers;

        // TRUE current jailed count
        int jailedCount = SPCaptureHandler.JailedSPs.Count;
        Debug.Log("=== WIN CHECK ===");
        Debug.Log("Total SPs: " + totalSPs);
        Debug.Log("Escaped: " + escapedAmount);
        Debug.Log("Jailed: " + jailedCount);

        // If anyone currently jailed → only free players must escape
        if (jailedCount > 0)
        {
            int freePlayers = totalSPs - jailedCount;

            if (freePlayers > 0 && escapedAmount >= freePlayers)
            {
                matchEnded = true;
                photonView.RPC(nameof(RPC_SPWins), RpcTarget.All);
            }
        }
        else
        {
            // Nobody jailed → ALL players must escape
            if (escapedAmount >= totalSPs)
            {
                matchEnded = true;
                photonView.RPC(nameof(RPC_SPWins), RpcTarget.All);
            }
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
