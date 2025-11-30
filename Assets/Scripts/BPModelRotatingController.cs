using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BPModelRotatingController : MonoBehaviourPun, IPunObservable
{
    [Header("Camera Settings")]
    public Transform cameraTransform;
    public Vector3 positionOffset = Vector3.zero;
    public bool useLocalOffset = false;

    [Header("Bone Pitch Settings")]
    public Transform targetBone;
    public float minPitch = -60f;
    public float maxPitch = 30f;
    public float pitchIntensity = 1f;
    public Vector3 rotationAxis = Vector3.right;

    [HideInInspector] public bool armRotationEnabled = false; // <<< start disabled

    private float networkPitch = 0f;
    private float networkYaw = 0f;
    private Quaternion boneInitialRotation;

    [Header("Smoothing for Remote Players")]
    public float rotationLerpSpeed = 8f;

    void Awake()
    {
        if (targetBone != null)
            boneInitialRotation = targetBone.localRotation;

        // Make sure arm doesn't move before first Fire1
        armRotationEnabled = false;
    }

    void Update()
    {
        if (cameraTransform == null) return;

        if (photonView.IsMine)
        {
            // Step 1: Position with optional offset
            Vector3 targetPosition = useLocalOffset
                ? cameraTransform.position + cameraTransform.rotation * positionOffset
                : cameraTransform.position + positionOffset;

            targetPosition.y = transform.position.y;
            transform.position = targetPosition;

            // Step 2: Match Y rotation
            Vector3 e = transform.eulerAngles;
            e.y = cameraTransform.eulerAngles.y;
            transform.eulerAngles = e;
        }
        else
        {
            // Remote players interpolate
            Vector3 e = transform.eulerAngles;
            e.y = Mathf.LerpAngle(e.y, networkYaw, Time.deltaTime * rotationLerpSpeed);
            transform.eulerAngles = e;
        }
    }

    void LateUpdate()
    {
        if (!armRotationEnabled) return; // <<< critical: stop bone rotation until first Fire1
        if (targetBone == null || cameraTransform == null) return;

        if (photonView.IsMine)
        {
            float pitch = cameraTransform.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;
            float clamped = Mathf.Clamp(pitch, minPitch, maxPitch) * pitchIntensity;
            targetBone.localRotation = boneInitialRotation * Quaternion.AngleAxis(clamped, rotationAxis);
        }
        else
        {
            Quaternion targetRot = boneInitialRotation * Quaternion.AngleAxis(networkPitch, rotationAxis);
            targetBone.localRotation = Quaternion.Slerp(targetBone.localRotation, targetRot,
                Time.deltaTime * rotationLerpSpeed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(targetBone.localEulerAngles.x);
            stream.SendNext(transform.eulerAngles.y);
        }
        else
        {
            networkPitch = (float)stream.ReceiveNext();
            networkYaw = (float)stream.ReceiveNext();
        }
    }
}

