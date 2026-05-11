using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SPRescueHandler : MonoBehaviourPun
{
    private GameObject escapeBar;
    private Image escapePanel;

    private float rescueValue = 0f;
    private float decreaseSpeed = 0.5f;
    private float increaseAmount = 0.65f;
    private float syncTimer = 0f;

    private bool isActive = false;
    private bool canRescue = false;
    private Transform freePoint;

    void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        escapeBar = canvas.transform.Find("SmallPlayerUI/EscapeBar")?.gameObject;
        escapePanel = canvas.transform.Find("SmallPlayerUI/EscapeBar/Panel")?.GetComponent<Image>();

        if (escapeBar != null)
            escapeBar.SetActive(false);

        freePoint = GameObject.FindWithTag("SPFreePoint")?.transform;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Press interact to START rescue
        if (!isActive && canRescue && Input.GetButtonDown("P1Interact"))
        {
            if (SPCaptureHandler.JailedSPs.Count > 0)
            {
                photonView.RPC(nameof(RPC_StartRescue), RpcTarget.All);
            }
        }

        if (!isActive) return;

        // Decrease over time
        rescueValue -= Time.deltaTime * decreaseSpeed;
        rescueValue = Mathf.Clamp01(rescueValue);

        if (escapePanel != null)
            escapePanel.fillAmount = rescueValue;

        // HOLD to increase
        if (Input.GetButton("P1Interact"))
        {
            rescueValue += increaseAmount * Time.deltaTime;
            rescueValue = Mathf.Clamp01(rescueValue);

            // update UI locally
            if (escapePanel != null)
                escapePanel.fillAmount = rescueValue;
        }

        syncTimer += Time.deltaTime;

        if (syncTimer > 0.2f) // send 5 times/sec
        {
            syncTimer = 0f;
            photonView.RPC(nameof(RPC_UpdateBar), RpcTarget.Others, rescueValue);
        }

        // Complete
        if (rescueValue >= 1f && isActive)
        {
            isActive = false; // STOP immediately
            photonView.RPC(nameof(RPC_CompleteRescue), RpcTarget.All);
        }
    }

    public void ShowRelease()
    {
        if (!photonView.IsMine) return;

        canRescue = true;

        var ui = FindObjectOfType<SPInteractionUI>();
        ui?.ShowRelease();
    }

    public void HideRelease()
    {
        if (!photonView.IsMine) return;

        canRescue = false;

        var ui = FindObjectOfType<SPInteractionUI>();
        ui?.HideAll();

        // ✅ STOP rescue completely if leaving trigger
        if (isActive)
        {
            photonView.RPC(nameof(RPC_StopRescue), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_StartRescue()
    {
        var ui = FindObjectOfType<SPInteractionUI>();
        ui?.HideAll();

        isActive = true;
        rescueValue = 0f;

        if (escapeBar != null)
            escapeBar.SetActive(true);

        if (escapePanel != null)
            escapePanel.fillAmount = 0f;
    }
    
    [PunRPC]
    void RPC_UpdateBar(float value)
    {
        rescueValue = value;

        if (escapePanel != null)
            escapePanel.fillAmount = rescueValue;
    }
    [PunRPC]
    void RPC_CompleteRescue()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var sp in SPCaptureHandler.JailedSPs)
            {
                if (sp != null)
                {
                    sp.photonView.RPC(nameof(SPCaptureHandler.RPC_FreeFromJail), RpcTarget.All, freePoint.position);
                }
            }
            SPWinManager.Instance.photonView.RPC(
    "RPC_RemoveCaptured",
    RpcTarget.MasterClient,
    SPCaptureHandler.JailedSPs.Count
);
            SPCaptureHandler.JailedSPs.Clear();
        }

        // ✅ FORCE UI RESET ON ALL CLIENTS
        isActive = false;
        rescueValue = 0f;

        if (escapeBar != null)
            escapeBar.SetActive(false);

        if (escapePanel != null)
            escapePanel.fillAmount = 0f;
    }

    [PunRPC]
    void RPC_StopRescue()
    {
        isActive = false;
        rescueValue = 0f;

        if (escapeBar != null)
            escapeBar.SetActive(false);

        if (escapePanel != null)
            escapePanel.fillAmount = 0f;
    }
}
