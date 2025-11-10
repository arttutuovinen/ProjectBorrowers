using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BPModelRotatingController : MonoBehaviourPun, IPunObservable
{
    [Header("Camera Settings")]
    public Transform cameraTransform;

    public Vector3 positionOffset = Vector3.zero; // Adjustable in Inspector
    public bool useLocalOffset = false;           // Offset relative to camera rotation

    [Header("Bone Pitch Settings")]
    public Transform targetBone;
    public float minPitch = -30f;       // Minimum pitch angle (down)
    public float maxPitch = 30f;        // Maximum pitch angle (up)
    public float pitchIntensity = 1f;   // 0 = no effect, 1 = full pitch
    public Vector3 rotationAxis = Vector3.right; // Default X-axis for pitch

    // Networked rotation data
    private float networkPitch = 0f;
    private float networkYaw = 0f;

    private Quaternion boneInitialRotation;

    [Header("Smoothing for Remote Players")]
    public float rotationLerpSpeed = 8f;

    void Awake()
    {
        if (targetBone != null)
            boneInitialRotation = targetBone.localRotation;
    }

    void Update()
    {
        if (cameraTransform == null) return;

        if (photonView.IsMine)
        {
            // STEP 1: Compute position with offset
            Vector3 targetPosition = useLocalOffset
                ? cameraTransform.position + cameraTransform.rotation * positionOffset
                : cameraTransform.position + positionOffset;

            // Keep current Y position (stay grounded)
            targetPosition.y = transform.position.y;

            transform.position = targetPosition;

            // STEP 2: Match Y-axis rotation (body turning)
            Vector3 currentEuler = transform.eulerAngles;
            currentEuler.y = cameraTransform.eulerAngles.y;
            transform.eulerAngles = currentEuler;
        }
        else
        {
            // Remote players: smoothly interpolate body rotation
            Vector3 currentEuler = transform.eulerAngles;
            currentEuler.y = Mathf.LerpAngle(currentEuler.y, networkYaw, Time.deltaTime * rotationLerpSpeed);
            transform.eulerAngles = currentEuler;
        }
    }

    void LateUpdate()
    {
        if (targetBone == null || cameraTransform == null) return;

        if (photonView.IsMine)
        {
            // Local player: compute pitch from camera
            float pitch = cameraTransform.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            float clampedPitch = Mathf.Clamp(pitch, minPitch, maxPitch) * pitchIntensity;
            targetBone.localRotation = boneInitialRotation * Quaternion.AngleAxis(clampedPitch, rotationAxis);
        }
        else
        {
            // Remote players: interpolate bone pitch
            Quaternion targetRotation = boneInitialRotation * Quaternion.AngleAxis(networkPitch, rotationAxis);
            targetBone.localRotation = Quaternion.Slerp(targetBone.localRotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        }
    }

    // Photon networking
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send local player rotation data
            stream.SendNext(targetBone.localEulerAngles.x);  // pitch
            stream.SendNext(transform.eulerAngles.y);        // yaw
        }
        else
        {
            // Receive rotation data for remote players
            networkPitch = (float)stream.ReceiveNext();
            networkYaw = (float)stream.ReceiveNext();
        }
    }
}

