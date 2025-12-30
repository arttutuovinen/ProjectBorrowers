using UnityEngine;
using Photon.Pun;

public class SPInteractionUI : MonoBehaviourPun
{
    GameObject climbText, releaseText, takeText, returnText;

    void Awake()
    {
        if (!photonView.IsMine) return;
        Canvas canvas = FindObjectOfType<Canvas>();
        climbText = canvas.transform.Find("SmallPlayerUI/Climb")?.gameObject;
        releaseText = canvas.transform.Find("SmallPlayerUI/Release")?.gameObject;
        takeText = canvas.transform.Find("SmallPlayerUI/Take")?.gameObject;
        returnText = canvas.transform.Find("SmallPlayerUI/Return")?.gameObject;

        HideAll();
    }

    public void ShowTake()
    {
        HideAll();
        takeText.SetActive(true);
    }

    public void ShowClimb()
    {
        HideAll();
        climbText.SetActive(true);
    }

    public void ShowRelease()
    {
        HideAll();
        releaseText.SetActive(true);
    }

    public void ShowReturn()
    {
        HideAll();
        returnText.SetActive(true);
    }

    public void HideAll()
    {
        climbText.SetActive(false);
        releaseText.SetActive(false);
        takeText.SetActive(false);
        returnText.SetActive(false);
    }
}
