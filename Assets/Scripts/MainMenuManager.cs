using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField roomCodeInput;

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void CreateRoom()
    {
        string roomCode = roomCodeInput.text;

        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("Room code is empty!");
            return;
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 3; // 1 Big + 2 Small

        PhotonNetwork.CreateRoom(roomCode, options);
    }

    public void JoinRoom()
    {
        string roomCode = roomCodeInput.text;

        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("Room code is empty!");
            return;
        }

        PhotonNetwork.JoinRoom(roomCode);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room successfully");
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Create room failed: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join room failed: " + message);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}