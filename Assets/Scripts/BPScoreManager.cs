using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BPScoreManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    private int currentCaptured = 0;
    private TMP_Text captureText;
    private GameObject bpWins;
    private bool gameEnded = false;
    public Collider scoreTrigger;
    private HashSet<int> playersInJail = new HashSet<int>();

    private void Start()
    {
        StartCoroutine(InitUI());
    }

    private IEnumerator InitUI()
    {
        yield return null; // wait 1 frame

        Canvas canvas = FindFirstObjectByType<Canvas>();

        captureText = canvas.transform.Find("Divider/Capture")
    ?.GetComponent<TMP_Text>();

        if (captureText == null)
        {
            Debug.LogError("CaptureText is NULL ❌");
        }

        bpWins = canvas.transform.Find("BPWins")?.gameObject;

        UpdateText(currentCaptured);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null) return;

        if (playersInJail.Add(pv.ViewID)) // only if newly added
        {
            UpdateCapturedFromTrigger();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SmallPlayer")) return;

        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null) return;

        if (playersInJail.Remove(pv.ViewID)) // only if actually removed
        {
            UpdateCapturedFromTrigger();
        }
    }

    private void UpdateCapturedFromTrigger()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int count = playersInJail.Count;

        photonView.RPC(nameof(RPC_UpdateCapturedCount), RpcTarget.All, count);
    }

    [PunRPC]
    private void RPC_UpdateCapturedCount(int newCount)
    {
        currentCaptured = newCount;
        UpdateText(currentCaptured);

        if (!gameEnded && currentCaptured >= RequiredPlayeramount.RequiredSmallPlayers)
        {
            gameEnded = true;

            if (bpWins != null)
                bpWins.SetActive(true);

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_ResetPlayerRoles), RpcTarget.AllBuffered);
            }

            StartCoroutine(DelayedReturnToLobby());
        }
    }

    private IEnumerator DelayedReturnToLobby()
    {
        yield return new WaitForSeconds(6f);

        if (PhotonNetwork.IsMasterClient)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }


    private void UpdateText(int count)
    {
        if (captureText != null)
            captureText.text = $"Captured: {count}/{RequiredPlayeramount.RequiredSmallPlayers}";
    }
    [PunRPC]
    private void RPC_ResetPlayerRoles()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var props = new ExitGames.Client.Photon.Hashtable
        {
            { LobbyKeys.PlayerRole, null } // <-- THIS clears the role correctly
        };

            player.SetCustomProperties(props);
        }
    }
    [PunRPC]
    public void RPC_DisableScoreTrigger()
    {
        if (scoreTrigger != null)
        {
            scoreTrigger.enabled = false;
            Debug.Log("Score trigger disabled via RPC!");
        }
    }
    [PunRPC]
    public void RPC_EnableScoreTrigger()
    {
        if (scoreTrigger != null)
        {
            scoreTrigger.enabled = true;
            Debug.Log("Score trigger enabled via RPC!");
        }
    }

    [PunRPC]
    public void RPC_ForceRemovePlayer(int viewID)
    {
        if (playersInJail.Remove(viewID))
        {
            UpdateCapturedFromTrigger();
        }
    }
}
