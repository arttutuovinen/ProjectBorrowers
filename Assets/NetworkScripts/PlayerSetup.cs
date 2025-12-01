using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public SPMovementNET spMovement;

    public GameObject playerCamera;
    public SmallPlayerItemCollector spItemCollector;
    public SPBoppyPin spBobbyPin;
    public SPFlashbang spFlashbang;

    public void IsLocalPlayer()
    {
        spMovement.enabled = true;
        playerCamera.SetActive(true);
        spItemCollector.enabled = true;
        spBobbyPin.enabled = true;
        spFlashbang.enabled = true;
        
    }
}
