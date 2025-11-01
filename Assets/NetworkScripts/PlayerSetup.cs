using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public SPMovementNET spMovement;
    public GameObject playerCamera;

    public void IsLocalPlayer()
    {
        spMovement.enabled = true;
        playerCamera.SetActive(true);
    }
}
