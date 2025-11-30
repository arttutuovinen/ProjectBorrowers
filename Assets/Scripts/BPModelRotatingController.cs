using UnityEngine;
using Photon.Pun;

public class BPModelRotatingController : MonoBehaviourPun, IPunObservable
{
    [Header("Camera & Head")]
    public Transform cameraTransform;  // local player camera
    public Transform headBone;         // ONLY pitch rotates

    [Header("Settings")]
    public float minPitch = -60f;
    public float maxPitch = 30f;
    public float pitchIntensity = 1f;
    public Vector3 rotationAxis = Vector3.right;
    public float rotationLerpSpeed = 8f; // smooth for remote players

    private float currentPitch = 0f;
    private float networkPitch = 0f;
    private Quaternion headInitialRotation;

    void Awake()
    {
        if (headBone != null)
            headInitialRotation = headBone.localRotation;
    }

    void Update()
    {
        if (!cameraTransform) return;

        if (photonView.IsMine)
        {
            // Compute current head pitch from local camera
            float pitch = cameraTransform.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;
            currentPitch = Mathf.Clamp(pitch, minPitch, maxPitch) * pitchIntensity;
        }
        // remote players do nothing in Update
    }

    void LateUpdate()
    {
        if (!headBone) return;

        if (photonView.IsMine)
        {
            // Apply pitch after Animator updates
            headBone.localRotation = headInitialRotation * Quaternion.AngleAxis(currentPitch, rotationAxis);
        }
        else
        {
            // Smoothly interpolate head pitch for remote players
            Quaternion desired = headInitialRotation * Quaternion.AngleAxis(networkPitch, rotationAxis);
            headBone.localRotation = Quaternion.Slerp(headBone.localRotation, desired, Time.deltaTime * rotationLerpSpeed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentPitch);  // send head pitch only
        }
        else
        {
            networkPitch = (float)stream.ReceiveNext();
        }
    }
}

