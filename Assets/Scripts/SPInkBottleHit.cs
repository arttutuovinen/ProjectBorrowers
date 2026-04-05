using UnityEngine;
using Photon.Pun;

public class SPInkBottleHit : MonoBehaviourPun
{
    private BPInkSplatUI uiController;

    private void Start()
    {
        uiController = FindObjectOfType<BPInkSplatUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only react if THIS is the local player
        if (!photonView.IsMine) return;

        if (other.CompareTag("InkBottleWeapon"))
        {
            if (uiController != null)
            {
                uiController.ShowInkSplat();
            }
        }
    }
}
