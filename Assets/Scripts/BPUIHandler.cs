using UnityEngine;
using Photon.Pun;

public class BPUIHandler : MonoBehaviourPun
{
    private GameObject bigPlayerUI;

    void Start()
    {
        if (!photonView.IsMine) return;

        // If not assigned, find it even if inactive
        if (bigPlayerUI == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            bigPlayerUI = canvas.transform.Find("BigPlayerUI")?.gameObject;
        }

        if (bigPlayerUI != null)
            bigPlayerUI.SetActive(true);
        else
            Debug.LogError("SmallPlayerUI not found under Canvas!");
    }
}
