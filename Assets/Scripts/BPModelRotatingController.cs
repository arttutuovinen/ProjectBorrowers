using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPModelRotatingController : MonoBehaviour
{
    public Transform cameraTransform;

    public Vector3 positionOffset = new Vector3(0, 0, 0); // Adjustable in Inspector
    public bool useLocalOffset = false; // Whether to apply offset relative to camera rotation



    public Transform targetBone;
    public float minPitch = -30f;       // Minimum pitch angle (down)
    public float maxPitch = 30f;        // Maximum pitch angle (up)
     public float pitchIntensity = 1f;   // 0 = no effect, 1 = full pitch

    public Vector3 rotationAxis = Vector3.right; // Default X-axis for pitch


    void Update()
    {
        if (cameraTransform == null) return;

    // STEP 1: Compute position with offset
    Vector3 targetPosition;
    if (useLocalOffset)
    {
        targetPosition = cameraTransform.position + cameraTransform.rotation * positionOffset;
    }
    else
    {
        targetPosition = cameraTransform.position + positionOffset;
    }

    // STEP 2: Keep current Y position to stay grounded
    targetPosition.y = transform.position.y;

    // Apply the final position
    transform.position = targetPosition;

    // STEP 3: Match Y-axis rotation (only)
    Vector3 currentEuler = transform.eulerAngles;
    currentEuler.y = cameraTransform.eulerAngles.y;
    transform.eulerAngles = currentEuler;

    }
    void LateUpdate()
    {
       if (cameraTransform == null || targetBone == null) return;

        // Get pitch from camera's X-axis rotation
        float pitch = cameraTransform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        float clampedPitch = Mathf.Clamp(pitch, minPitch, maxPitch) * pitchIntensity;
        Quaternion cameraRotation = Quaternion.AngleAxis(clampedPitch, rotationAxis);

        targetBone.localRotation = targetBone.localRotation * cameraRotation;
    }
}
