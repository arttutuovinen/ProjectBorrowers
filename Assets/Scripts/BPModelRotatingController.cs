using UnityEngine;
using Photon.Pun;

public class BPModelRotatingController : MonoBehaviourPun, IPunObservable
{
    [Header("Camera & Animator")]
    public Transform cameraTransform;  // local camera
    public Animator animator;          // child object animator

    [Header("Settings")]
    public float minPitch = -50f;      // maps to HeadVert = -1 (looking down)
    public float maxPitch = 60f;       // maps to HeadVert = 1 (looking up)
    public string headParam = "HeadVert"; // Animator float parameter

    private float currentHeadVert = 0f;
    private float networkHeadVert = 0f;
    public float networkLerpSpeed = 8f;

    void Update()
    {
        if (!photonView.IsMine || cameraTransform == null || animator == null) return;

        // Get local camera pitch
        float pitch = cameraTransform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        // Map pitch to -1 .. 1 and **invert** to match blend tree
        currentHeadVert = 1f - (Mathf.InverseLerp(minPitch, maxPitch, pitch) * 2f);

        // Clamp just in case
        currentHeadVert = Mathf.Clamp(currentHeadVert, -1f, 1f);

        // Set parameter locally
        animator.SetFloat(headParam, currentHeadVert);
    }

    void LateUpdate()
    {
        if (photonView.IsMine || animator == null) return;

        // Smoothly apply networked value for remote players
        float value = Mathf.Lerp(animator.GetFloat(headParam), networkHeadVert, Time.deltaTime * networkLerpSpeed);
        animator.SetFloat(headParam, value);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentHeadVert);
        }
        else
        {
            networkHeadVert = (float)stream.ReceiveNext();
        }
    }
}
