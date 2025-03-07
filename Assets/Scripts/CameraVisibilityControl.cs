using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVisibilityControl : MonoBehaviour
{
    public Camera bigPlayerCamera;         // Reference to BigPlayer's camera
    public Camera smallPlayerCamera;       // Reference to SmallPlayer's camera
    public GameObject[] objectsToHide;     // Array of objects to hide from BigPlayer's camera

    private int smallPlayerLayer;          // Layer for SmallPlayer
    private int hiddenLayer;             // Custom layer for all the objects that should be hidden from BigPlayer

    void Start()
    {
        // Get the SmallPlayer layer
        smallPlayerLayer = LayerMask.NameToLayer("SmallPlayer");

        // Get the hidden layer or any custom layer where objects will be hidden from BigPlayer
        hiddenLayer = LayerMask.NameToLayer("Hidden");

        // If the hidden layer doesn't exist, log an error
        if (hiddenLayer == -1)
        {
            Debug.LogError("hiddenLayer does not exist! Please create a 'hidden' layer.");
        }

        // Assign all objects in the array to the hiddenLayer
        foreach (GameObject obj in objectsToHide)
        {
            obj.layer = hiddenLayer;
        }

        // Setup camera culling masks to control visibility
        SetupCameras();
    }

    void SetupCameras()
    {
        // Ensure SmallPlayer's camera can see both SmallPlayer and hidden layers
        smallPlayerCamera.cullingMask |= (1 << smallPlayerLayer) | (1 << hiddenLayer);

        // Ensure BigPlayer's camera keeps its current culling mask, but hides the hidden layer
        bigPlayerCamera.cullingMask &= ~(1 << hiddenLayer);
    }
}
