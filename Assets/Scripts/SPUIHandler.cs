using UnityEngine;
using Photon.Pun;

public class SPUIHandler : MonoBehaviourPun
{
    [SerializeField] private GameObject smallPlayerUI;

    void Start()
    {
        if (!photonView.IsMine) return;

        // If not assigned, find it even if inactive
        if (smallPlayerUI == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            smallPlayerUI = canvas.transform.Find("SmallPlayerUI")?.gameObject;
        }

        if (smallPlayerUI != null)
            smallPlayerUI.SetActive(true);
        else
            Debug.LogError("SmallPlayerUI not found under Canvas!");
    }
}
