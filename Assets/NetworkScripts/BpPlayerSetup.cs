using UnityEngine;

public class BpPlayerSetup : MonoBehaviour
{
    public BigPlayerMovement bpMovement;
    public GameObject playerCamera;

    public void IsLocalPlayer()
    {
        bpMovement.enabled = true;
        playerCamera.SetActive(true);
        
    }
}
