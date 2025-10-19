using UnityEngine;
using Unity.Netcode;

public class GameLauncher : MonoBehaviour
{
    void Update()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnGUI()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(10, 10, 150, 40), "Host"))
                NetworkManager.Singleton.StartHost();

            if (GUI.Button(new Rect(10, 60, 150, 40), "Join"))
                NetworkManager.Singleton.StartClient();
        }
    }
}
