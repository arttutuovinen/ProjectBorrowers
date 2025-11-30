using UnityEngine;
using Photon.Pun;

public class NetworkSettingsInitializer : MonoBehaviour
{
    void Awake()
    {
        PhotonNetwork.SendRate = 60; // updates per second
        // No SendRateOnSerialize in recent versions
    }
}
