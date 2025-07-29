using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPModelRotatingController : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 positionOffset = new Vector3(0, 0, 0); // Adjustable in Inspector
    public bool useLocalOffset = false; // Whether to apply offset relative to camera rotation

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
}
