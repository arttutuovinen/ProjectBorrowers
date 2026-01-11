using UnityEngine;
using Photon.Pun;
using System.Linq;

public class BPInteractionUI : MonoBehaviourPun
{
    GameObject openText, closeText, takeText, captureSPText;
    public bool captureModeActive;

    void Awake()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        openText = canvas.transform.Find("BigPlayerUI/Open")?.gameObject;
        closeText = canvas.transform.Find("BigPlayerUI/Close")?.gameObject;
        takeText = canvas.transform.Find("BigPlayerUI/Take")?.gameObject;
        captureSPText = canvas.transform.Find("BigPlayerUI/CaptureSP")?.gameObject;
                      
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
        captureModeActive = true;
        HideAll();

        if (captureSPText == null)
        {
            Debug.LogError("BPInteractionUI: captureSPText is null!");
            return;
        }

        captureSPText.SetActive(true);
    }

    public void HideAll()
    {
        if (captureModeActive) return;   // <---- THIS IS THE FIX

        openText.SetActive(false);
        closeText.SetActive(false);
        takeText.SetActive(false);
        captureSPText.SetActive(false);
    }

    public void HideTakeOnly()
    {
        takeText.SetActive(false);
    }

    public void HideCaptureSP()
    {
        if (captureSPText != null)
            captureSPText.SetActive(false);
    }
}
