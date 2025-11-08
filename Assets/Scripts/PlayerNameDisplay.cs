// PlayerNameDisplay.cs
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class PlayerNameDisplay : MonoBehaviourPun
{
    [Tooltip("Optional: assign a name-tag prefab (TextMeshPro UGUI inside a World Space Canvas). If left empty, a 3D TextMeshPro object will be created at runtime.")]
    public GameObject nameTagPrefab;

    [Tooltip("Vertical offset above the player's origin where the tag is placed.")]
    [SerializeField] float verticalOffset = 2f;

    [Tooltip("If a prefab is provided, the script will attempt to use a TextMeshProUGUI inside it (Canvas must be World Space).")]
    [SerializeField] float tryFindCameraInterval = 0.25f;

    GameObject nameTagInstance;
    TMP_Text nameText; // supports both TextMeshPro (3D) and TextMeshProUGUI via base class TMP_Text
    Transform camTransform;

    IEnumerator Start()
    {
        // keep trying to find a camera for a short time (and keep trying later if none found)
        TryFindCamera();

        // Prepare display name
        string displayName = "Player";
        if (photonView.Owner != null && !string.IsNullOrEmpty(photonView.Owner.NickName))
            displayName = photonView.Owner.NickName;
        else if (!string.IsNullOrEmpty(PhotonNetwork.NickName))
            displayName = PhotonNetwork.NickName;

        // Instantiate the name tag
        if (nameTagPrefab != null)
        {
            // If a prefab exists, instantiate as child of the player so it follows the transform
            nameTagInstance = Instantiate(nameTagPrefab, transform, true);

            // Place above head
            nameTagInstance.transform.localPosition = new Vector3(0f, verticalOffset, 0f);
            nameTagInstance.transform.localRotation = Quaternion.identity;
            nameTagInstance.transform.localScale = Vector3.one;

            // Try to find a TMP component on prefab (UGUI or 3D)
            nameText = nameTagInstance.GetComponent<TMP_Text>();
            if (nameText == null)
                nameText = nameTagInstance.GetComponentInChildren<TMP_Text>();

            if (nameText == null)
            {
                Debug.LogWarning("[PlayerNameDisplay] Provided prefab does not contain a TextMeshPro component. Falling back to creating a 3D text object instead.");
                Destroy(nameTagInstance);
                nameTagInstance = Create3DText(displayName);
            }
            else
            {
                nameText.text = displayName;

                // If prefab uses TextMeshProUGUI, ensure its Canvas is World Space
                var canvas = nameTagInstance.GetComponentInChildren<Canvas>();
                if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                {
                    Debug.LogWarning("[PlayerNameDisplay] The PlayerNameUI prefab contains a Canvas that is not World Space. Name tag may not render correctly. Consider converting the Canvas to World Space. Falling back to 3D text.");
                    Destroy(nameTagInstance);
                    nameTagInstance = Create3DText(displayName);
                }
            }
        }
        else
        {
            // No prefab assigned -> create a simple 3D TextMeshPro label
            nameTagInstance = Create3DText(displayName);
        }

        // Slight delay to allow camera to exist (Start is a coroutine)
        yield return new WaitForSeconds(0.05f);

        // Hide local player's own name tag
        if (photonView.IsMine && nameTagInstance != null)
        {
            nameTagInstance.SetActive(false);
        }

        // keep ensuring we have a camera reference
        if (camTransform == null)
            StartCoroutine(EnsureCameraRoutine());
    }

    // Create a simple 3D TextMeshPro label as a child of this player
    private GameObject Create3DText(string displayName)
    {
        var go = new GameObject("NameTag3D");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, verticalOffset, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.1f; // scale down because TMP default units are large

        var text = go.AddComponent<TextMeshPro>();
        text.text = displayName;
        text.fontSize = 25f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.enableCulling = true; // small optimization
        nameText = text;

        return go;
    }

    // Try to find the main camera or camera with AudioListener; assign camTransform if found.
    private void TryFindCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            camTransform = cam.transform;
            return;
        }

        // fallback: camera with enabled AudioListener (local player's camera typically has AudioListener)
        var cameras = Camera.allCameras;
        foreach (var c in cameras)
        {
            var listener = c.GetComponent<AudioListener>();
            if (listener != null && listener.enabled)
            {
                camTransform = c.transform;
                return;
            }
        }

        // fallback: first active camera
        if (Camera.allCamerasCount > 0)
        {
            camTransform = Camera.allCameras[0].transform;
        }
    }

    // Keep trying until a camera appears in the scene
    private IEnumerator EnsureCameraRoutine()
    {
        while (camTransform == null)
        {
            TryFindCamera();
            if (camTransform != null) break;
            yield return new WaitForSeconds(tryFindCameraInterval);
        }
    }

    void LateUpdate()
    {
        if (nameTagInstance == null) return;

        // billboard toward camera if available
        if (camTransform != null)
        {
            Vector3 dir = nameTagInstance.transform.position - camTransform.position;
            if (dir.sqrMagnitude > 0.0001f)
                nameTagInstance.transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
    }

    // Public helper to refresh name later (call if player changes display name after spawn)
    public void RefreshName()
    {
        string displayName = "Player";
        if (photonView.Owner != null && !string.IsNullOrEmpty(photonView.Owner.NickName))
            displayName = photonView.Owner.NickName;
        else if (!string.IsNullOrEmpty(PhotonNetwork.NickName))
            displayName = PhotonNetwork.NickName;

        if (nameText != null)
            nameText.text = displayName;
    }
}
