using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;

public class LobbyUiManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text roomCodeText;
    public TMP_InputField nameInput;
    public Button bigPlayerButton;
    public Button smallPlayerButton;
    public Button readyButton;
    public TMP_Text statusText;

    private bool isReady = false;
    private int selectedRole = -1; // 0 = Big, 1 = Small

    void Start()
    {
        roomCodeText.text = $"Room: {PhotonNetwork.CurrentRoom.Name}";
        statusText.text = "Not Ready";

        bigPlayerButton.onClick.AddListener(() => SelectRole(0));
        smallPlayerButton.onClick.AddListener(() => SelectRole(1));
        readyButton.onClick.AddListener(ToggleReady);
    }

    void SelectRole(int role)
    {
        if (isReady) return; // can't change role when ready

        // --- BigPlayer limit check ---
        if (role == 0) // BigPlayer
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.CustomProperties.TryGetValue(LobbyKeys.PlayerRole, out object roleObj))
                {
                    int takenRole = (int)roleObj;
                    if (takenRole == 0) // BigPlayer already taken
                    {
                        statusText.text = "BigPlayer role is already taken!";
                        return; // prevent selection
                    }
                }
            }
        }

        // Store selection in CustomProperties
        var props = new Hashtable
    {
        { LobbyKeys.PlayerRole, role } // 0 = BigPlayer, 1 = SmallPlayer
    };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // Update internal selection & UI
        selectedRole = role;
        bigPlayerButton.image.color = role == 0 ? Color.green : Color.white;
        smallPlayerButton.image.color = role == 1 ? Color.green : Color.white;
    }

    void ToggleReady()
    {
        if (string.IsNullOrWhiteSpace(nameInput.text))
        {
            statusText.text = "Enter name first";
            return;
        }

        if (selectedRole == -1)
        {
            statusText.text = "Select a role";
            return;
        }

        isReady = !isReady;

        statusText.text = isReady ? "READY" : "Not Ready";
        readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Unready" : "Ready";

        // Lock UI when ready
        nameInput.interactable = !isReady;
        bigPlayerButton.interactable = !isReady;
        smallPlayerButton.interactable = !isReady;
    }

    public void OnReadyButtonPressed()
    {
        // 1. Apply name to Photon
        if (!string.IsNullOrWhiteSpace(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text.Trim();
        }

        // 2. Toggle ready state
        bool currentReady = false;
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(LobbyKeys.PlayerReady))
            currentReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties[LobbyKeys.PlayerReady];

        var props = new Hashtable
    {
        { LobbyKeys.PlayerReady, !currentReady }
    };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}
