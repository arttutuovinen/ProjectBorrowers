using UnityEngine;
using Photon.Pun;

public class SPPlayerInit : MonoBehaviourPun
{
    private PlayerSetup setup;

    void Awake()
    {
        setup = GetComponent<PlayerSetup>();
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            setup.IsLocalPlayer(); // enable controls, camera, etc.
        }
        else
        {
            // Disable local-only features for remote players
            if (setup.playerCamera != null)
                setup.playerCamera.SetActive(false);
        }
    }
}
