using UnityEngine;
using Photon.Pun;

public class BPTapeManager : MonoBehaviourPun
{
    public Camera playerCamera;
    public float rayDistance = 10f;
    public float activationDistance = 3f;
    public LayerMask tapeAreaLayer;

    private GameObject currentTapeArea;

    void Update()
    {
        if (!photonView.IsMine) return;   // Only local player does raycasts
        DetectTapeArea();
    }

    private void DetectTapeArea()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, tapeAreaLayer))
        {
            currentTapeArea = hit.collider.gameObject;
        }
        else
        {
            currentTapeArea = null;
        }
    }

    /// <summary>
    /// Tries to activate the tape area the player is looking at.
    /// Returns true if a tape was successfully placed, false otherwise.
    /// </summary>
    public bool TryActivateTape()
    {
        if (!photonView.IsMine) return false;  // Only the owner triggers activation

        if (currentTapeArea == null)
        {
            return false;
        }

        // Check distance
        float distance = Vector3.Distance(
            playerCamera.transform.position,
            currentTapeArea.transform.position
        );

        if (distance > activationDistance)
        {
            return false;
        }

        if (!currentTapeArea.TryGetComponent(out TapeArea areaComponent))
        {
            return false;
        }

        // Already activated?
        if (areaComponent.IsActivated)
        {
            return false;
        }

        // Activate across the network using RPC
        PhotonView areaPv = currentTapeArea.GetComponent<PhotonView>();
        if (areaPv == null)
        {
            Debug.LogError("TapeArea object MUST have a PhotonView to sync!");
            return false;
        }

        photonView.RPC("RPC_ActivateTapeArea", RpcTarget.All, areaPv.ViewID);

        return true;
    }

    [PunRPC]
    private void RPC_ActivateTapeArea(int areaID)
    {
        PhotonView areaPv = PhotonView.Find(areaID);
        if (areaPv == null) return;

        GameObject areaObj = areaPv.gameObject;

        if (!areaObj.TryGetComponent(out TapeArea areaComponent))
            return;

        if (areaComponent.IsActivated)
            return; // Avoid double-activating

        // Activate all tape meshes (children)
        foreach (Transform child in areaObj.transform)
        {
            child.gameObject.SetActive(true);
        }

        areaComponent.IsActivated = true;
    }
}

