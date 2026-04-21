using UnityEngine;
using Photon.Pun;

public class BPGardenCutters : MonoBehaviourPun
{
    public Camera playerCamera;
    public float rayDistance = 11f;
    public float activationDistance = 11f;
    public LayerMask ladderLayer;
    private GameObject cutUIImage;
    private GameObject currentLadder; // Child object (Ladder)
    private bool canUseGardenCutters = false;

    void Start()
    {
        if (!photonView.IsMine) return;
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        cutUIImage = canvas.transform.Find("BigPlayerUI/Cut")?.gameObject;
        cutUIImage.SetActive(false);

    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // ✅ Only raycast when item is active
        if (!canUseGardenCutters)
            return;

        DetectLadder();
    }

    // --------------------------------------------------
    // Enable / Disable from BPItemCollector
    // --------------------------------------------------
    public void SetGardenCuttersActive(bool active)
    {
        canUseGardenCutters = active;

        if (!active)
            currentLadder = null;
    }

    // --------------------------------------------------
    // Detect Ladder (child trigger collider)
    // --------------------------------------------------
    private void DetectLadder()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, ladderLayer, QueryTriggerInteraction.Collide))
        {
            currentLadder = hit.collider.gameObject;
            cutUIImage.SetActive(true);
        }
        else
        {
            currentLadder = null;
            cutUIImage.SetActive(false);
        }
    }

    // --------------------------------------------------
    // Called from BPItemCollector when pressing Fire1
    // --------------------------------------------------
    public bool TryUseGardenCutters()
    {
        if (!photonView.IsMine) return false;

        if (!canUseGardenCutters)
            return false;

        if (currentLadder == null)
            return false;
        cutUIImage.SetActive(false);
        float distance = Vector3.Distance(
            playerCamera.transform.position,
            currentLadder.transform.position
        );

        if (distance > activationDistance)
            return false;

        // Get parent (SPLadder with PhotonView)
        PhotonView ladderRootPV = currentLadder.GetComponentInParent<PhotonView>();

        if (ladderRootPV == null)
        {
            Debug.LogError("SPLadder must have a PhotonView!");
            return false;
        }

        // Sync disable across all clients
        photonView.RPC("RPC_DisableSPLadder", RpcTarget.All, ladderRootPV.ViewID);

        return true;
    }

    // --------------------------------------------------
    // RPC: Disable SPLadder globally
    // --------------------------------------------------
    [PunRPC]
    private void RPC_DisableSPLadder(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv == null) return;

        GameObject spladder = pv.gameObject;

        if (!spladder.activeSelf)
            return;

        spladder.SetActive(false);
    }
}
