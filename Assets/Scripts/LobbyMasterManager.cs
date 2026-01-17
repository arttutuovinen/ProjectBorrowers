using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class LobbyMasterManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public GameObject startButton; // Assign the Master-only Start button in Inspector

    void Start()
    {
        if (startButton != null)
            startButton.SetActive(PhotonNetwork.IsMasterClient); // only master sees it
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Make sure new master sees the button
        if (startButton != null)
            startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void OnStartButtonPressed()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!AreAllPlayersReady())
        {
            Debug.Log("Cannot start: not all players are ready.");
            return;
        }

        Debug.Log("All players ready! Starting game...");
        PhotonNetwork.CurrentRoom.IsOpen = false; // prevent late joins

        // Load game scene for all players
        PhotonNetwork.LoadLevel("GameScene"); // replace with your actual game scene name
    }

    private bool AreAllPlayersReady()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey(LobbyKeys.PlayerReady)) return false;
            if (!(bool)p.CustomProperties[LobbyKeys.PlayerReady]) return false;
        }
        return true;
    }
}
