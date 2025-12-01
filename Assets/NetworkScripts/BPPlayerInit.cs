using UnityEngine;
using Photon.Pun;

public class BPPlayerInit : MonoBehaviourPun
{
    private BpPlayerSetup setup;

    void Awake()
    {
        setup = GetComponent<BpPlayerSetup>();
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            setup.IsLocalPlayer(); // enable controls, camera, etc.
        }
        else
        {
            if (setup.playerCamera != null)
                setup.playerCamera.SetActive(false);
        }
    }
}
