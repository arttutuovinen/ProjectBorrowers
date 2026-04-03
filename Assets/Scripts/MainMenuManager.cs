using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField roomCodeInput;

    void Awake()
    {
        // ✅ Always restore cursor when entering LobbyScene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            StartCoroutine(WaitForConnectionThenCreate());
            return;
        }

        CreateRoomInternal();
    }

    IEnumerator WaitForConnectionThenCreate()
    {
        while (!PhotonNetwork.IsConnectedAndReady)
            yield return null;

        CreateRoomInternal();
    }

    void CreateRoomInternal()
    {
        string roomCode = roomCodeInput.text;

        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("Room code is empty!");
            return;
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 3;

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

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            StartCoroutine(WaitForConnectionThenJoin(roomCode));
            return;
        }

        JoinRoomInternal(roomCode);
    }

    IEnumerator WaitForConnectionThenJoin(string roomCode)
    {
        while (!PhotonNetwork.IsConnectedAndReady)
            yield return null;

        JoinRoomInternal(roomCode);
    }

    void JoinRoomInternal(string roomCode)
    {
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