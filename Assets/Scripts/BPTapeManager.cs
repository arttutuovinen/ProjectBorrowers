using UnityEngine;

public class BPTapeManager : MonoBehaviour
{
    public Camera playerCamera;
    public float rayDistance = 10f;
    public float activationDistance = 3f;
    public LayerMask tapeAreaLayer; // Assign "Tape" layer in Inspector (for TapeWeapon prefabs)

    private GameObject currentTapeArea;

    void Update()
    {
        DetectTapeArea();
    }

    void DetectTapeArea()
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
        if (currentTapeArea == null)
        {
            Debug.Log("No tape area in sight.");
            return false;
        }

        // Check distance
        float distance = Vector3.Distance(playerCamera.transform.position, currentTapeArea.transform.position);
        if (distance > activationDistance)
        {
            Debug.Log("Too far from tape area to activate.");
            return false;
        }

        // Check if area already used
        if (!currentTapeArea.TryGetComponent(out TapeArea areaComponent))
        {
            Debug.LogWarning($"Tape area '{currentTapeArea.name}' missing TapeArea component!");
            return false;
        }

        if (areaComponent.IsActivated)
        {
            Debug.Log($"Tape area '{currentTapeArea.name}' already has tape placed.");
            return false;
        }

        // Activate children (the actual tape meshes)
        foreach (Transform child in currentTapeArea.transform)
        {
            child.gameObject.SetActive(true);
        }

        areaComponent.IsActivated = true;
        Debug.Log($"Activated tape area: {currentTapeArea.name}");
        return true;
    }
}
