using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Door : MonoBehaviourPun
{
    public float rotationAmount = 80.0f;  // How much the door should rotate
    public float rotationSpeed = 5.0f;    // Speed of the door opening and closing

    private bool isDoorOpen = false;      // Check if the door is open
    private Quaternion initialRotation;   // Store the initial rotation of the door
    private Quaternion targetRotation;    // The target rotation to open/close the door

    private void Awake()
    {
        // Capture initial rotation early (Before any RPC replay)
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
    }

    private void Update()
    {
        // Smoothly rotate the door to the target rotation over time
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // Public method callers should use to toggle the door.
    // This will invoke an RPC so everyone toggles (buffered so late joiners get the state).
    public void ToggleDoor()
    {
        if (PhotonNetwork.InRoom && photonView != null)
        {
            // Broadcast the toggle to everyone (buffered so late-joiners see last state)
            photonView.RPC(nameof(RPC_ToggleDoor), RpcTarget.AllBuffered);
        }
        else
        {
            // Fallback for offline/testing: do local toggle
            LocalToggle();
        }
    }

    // Local toggle (only affects this client)
    private void LocalToggle()
    {
        if (isDoorOpen)
            CloseDoor();
        else
            OpenDoor();

        isDoorOpen = !isDoorOpen;
    }

    // This RPC runs on all clients and toggles the door state locally.
    [PunRPC]
    private void RPC_ToggleDoor()
    {
        // Ensure initialRotation is set (in case RPC comes before Awake/Start on some setups)
        if (initialRotation == null)
            initialRotation = transform.rotation;

        if (isDoorOpen)
            CloseDoor();
        else
            OpenDoor();

        isDoorOpen = !isDoorOpen;
    }

    // Open the door (rotate around the Y-axis relative to the initial rotation)
    private void OpenDoor()
    {
        targetRotation = Quaternion.Euler(0f, rotationAmount, 0f) * initialRotation;
    }

    // Close the door (rotate back to the initial position)
    private void CloseDoor()
    {
        targetRotation = initialRotation;
    }
}
