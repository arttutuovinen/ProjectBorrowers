using UnityEngine;
using Photon.Pun;
using System.Linq;

public class BPInteractionUI : MonoBehaviourPun
{
    GameObject openText, closeText, takeText, captureSPText;

    void Awake()
    {
        if (!photonView.IsMine) return;
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        openText = canvas.transform.Find("BigPlayerUI/Open")?.gameObject;
        closeText = canvas.transform.Find("BigPlayerUI/Close")?.gameObject;
        takeText = canvas.transform.Find("BigPlayerUI/Take")?.gameObject;
        captureSPText = canvas.GetComponentsInChildren<Transform>(true) // 'true' includes inactive
                     .FirstOrDefault(t => t.name == "CaptureSP")?.gameObject;

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

    public void ShowCaptureSP()
    {
        HideAll();
        captureSPText.SetActive(true);
    }

    public void HideAll()
    {
        openText.SetActive(false);
        closeText.SetActive(false);
        takeText.SetActive(false);
        captureSPText.SetActive(false);
    }
    public void HideTakeOnly()
    {
        takeText.SetActive(false);
    }
}
