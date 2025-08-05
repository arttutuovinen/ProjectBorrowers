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
        // Apply offset
        if (useLocalOffset)
        {
            // Offset relative to camera's local rotation
            transform.position = cameraTransform.position + cameraTransform.rotation * positionOffset;
        }
        else
        {
            // World-space offset
            transform.position = cameraTransform.position + positionOffset;
        }

        // Match only the Y rotation of the camera
        Vector3 currentEuler = transform.eulerAngles;
        currentEuler.y = cameraTransform.eulerAngles.y;
        transform.eulerAngles = currentEuler;

    }
    void LateUpdate()
    {
       if (cameraTransform == null || targetBone == null) return;

        // Get pitch from camera (Euler X) and convert to -180..180
        float pitch = cameraTransform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        // Clamp and scale pitch
        float clampedPitch = Mathf.Clamp(pitch, minPitch, maxPitch) * pitchIntensity;

        // Create camera pitch rotation on the desired axis
        Quaternion cameraRotation = Quaternion.AngleAxis(clampedPitch, rotationAxis);

        // Apply camera pitch on top of Animator’s rotation
        targetBone.localRotation = targetBone.localRotation * cameraRotation;
    }
}
