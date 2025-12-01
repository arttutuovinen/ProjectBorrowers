using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class Door : MonoBehaviourPunCallbacks // <- small change to get callbacks
{
    public float rotationAmount = 80.0f;  // How much the door should rotate
    public float rotationSpeed = 5.0f;    // Speed of the door opening and closing
    private bool isDoorOpen = false;      // Check if the door is open
    private Quaternion initialRotation;   // Store the initial rotation of the door
    private Quaternion targetRotation;    // The target rotation to open/close the door

    private void Awake()
    {
        // Ensure initial rotation is captured as early as possible (important for RPC replay on join)
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
    }

    private void Update()
    {
        // Smoothly rotate the door to the target rotation over time
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // Method to toggle the door's state (open/close)
    public void ToggleDoor()
    {
        // If we're in a Photon room and the PhotonView exists, send a buffered RPC that sets the exact new state.
        if (PhotonNetwork.InRoom && photonView != null)
        {
            // compute the new state we want to set
            bool newState = !isDoorOpen;
            // Use AllBuffered so late joiners get the last set state.
            photonView.RPC(nameof(RPC_SetDoorState), RpcTarget.AllBuffered, newState);
            // NOTE: do NOT locally flip isDoorOpen here; RPC will set it on all clients (including this one).
            return;
        }

        // Fallback (offline / no Photon): local toggle only
        if (isDoorOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }

        isDoorOpen = !isDoorOpen;  // Toggle the door state
    }

    // RPC that sets the door state explicitly on each client
    [PunRPC]
    private void RPC_SetDoorState(bool open)
    {
        // Ensure initialRotation is set (in case this RPC runs very early)
        if (initialRotation == null) initialRotation = transform.rotation;

        if (open)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }

        isDoorOpen = open;
    }

    // ---------- NEW: join-time state request/response to handle rejoined host ----------
    // When a client joins, ask the MasterClient for the current state of this door.
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        // If in room and this client is NOT the master (or even if it is — harmless),
        // request the authoritative state from MasterClient so rejoiners get correct state.
        if (PhotonNetwork.InRoom && photonView != null && PhotonNetwork.MasterClient != null)
        {
            int requesterActor = PhotonNetwork.LocalPlayer.ActorNumber;
            // Ask MasterClient to reply with this door's state for this view ID
            // We include the requester's actor number as an int so the master can reply directly.
            photonView.RPC(nameof(RPC_RequestDoorState), RpcTarget.MasterClient, requesterActor);
        }
    }

    // Called on the MasterClient when someone requests the door state
    [PunRPC]
    private void RPC_RequestDoorState(int requesterActorNumber, PhotonMessageInfo info = default)
    {
        // Only the MasterClient should run the reply logic
        if (!PhotonNetwork.IsMasterClient) return;

        // Find the player object for the requester
        var room = PhotonNetwork.CurrentRoom;
        if (room == null || room.Players == null) return;

        if (room.Players.ContainsKey(requesterActorNumber))
        {
            var targetPlayer = room.Players[requesterActorNumber];
            // Send the current door state only to the requester (not buffered)
            photonView.RPC(nameof(RPC_SetDoorState), targetPlayer, isDoorOpen);
        }
    }
    // ---------- end join-time additions ----------

    // Open the door (rotate 55 degrees around the Y-axis)
    private void OpenDoor()
    {
        targetRotation = Quaternion.Euler(0, rotationAmount, 0) * initialRotation;
    }

    // Close the door (rotate back to the initial position)
    private void CloseDoor()
    {
        targetRotation = initialRotation;
    }
}
