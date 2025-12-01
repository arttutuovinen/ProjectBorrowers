using UnityEngine;

public class BpPlayerSetup : MonoBehaviour
{
    public BigPlayerMovement bpMovement;
    public BPItemCollector bpItemCollector;
    public GameObject playerCamera;

    public void IsLocalPlayer()
    {
        bpMovement.enabled = true;
        bpItemCollector.enabled = true;

        playerCamera.SetActive(true);
        
    }
}
