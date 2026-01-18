using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class LobbyMasterManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Button startButton; // Assign the Master-only Start button in Inspector
    public int requiredSmallPlayers = 2;
    public int requiredBigPlayers = 1;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        UpdateStartButton();
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdateStartButton();
    }

    void UpdateStartButton()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        bool canStart = CanStartGame();

        // Change button interactable
        startButton.interactable = canStart;
        ColorBlock cb = startButton.colors; // <-- declare cb here!
        // Change color safely using ColorBlock
        Color whiteColor = new Color32(255, 255, 255, 255);
        cb.normalColor = canStart ? Color.white : Color.gray;
        cb.highlightedColor = canStart ? Color.white : Color.gray;
        cb.pressedColor = canStart ? Color.white : Color.gray;
        cb.disabledColor = Color.gray;
        startButton.colors = cb;
    }

    public void OnStartButtonPressed()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!CanStartGame())
            return; // HARD STOP – cannot cheat start

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("GameScene");
    }

    bool CanStartGame()
    {
        int sp = 0;
        int bp = 0;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.TryGetValue(LobbyKeys.PlayerRole, out object r))
                return false; // someone hasn't chosen yet

            if ((int)r == 0) bp++;
            else sp++;
        }

        return sp == requiredSmallPlayers && bp == requiredBigPlayers;
    }
}