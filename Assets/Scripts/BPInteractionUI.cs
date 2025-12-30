using UnityEngine;
using Photon.Pun;

public class BPInteractionUI : MonoBehaviourPun
{
    GameObject openText, closeText, takeText;

    void Awake()
    {
        if (!photonView.IsMine) return;
        Canvas canvas = FindObjectOfType<Canvas>();
        openText = canvas.transform.Find("BigPlayerUI/Open")?.gameObject;
        closeText = canvas.transform.Find("BigPlayerUI/Close")?.gameObject;
        takeText = canvas.transform.Find("BigPlayerUI/Take")?.gameObject;

        HideAll();
    }

    public void ShowOpen()
    {
        HideAll();
        openText.SetActive(true);
    }

    public void ShowClose()
    {
        HideAll();
        closeText.SetActive(true);
    }

    public void ShowTake()
    {
        HideAll();
        takeText.SetActive(true);
    }

    public void HideAll()
    {
        openText.SetActive(false);
        closeText.SetActive(false);
        takeText.SetActive(false);
    }
    public void HideTakeOnly()
    {
        takeText.SetActive(false);
    }
}
