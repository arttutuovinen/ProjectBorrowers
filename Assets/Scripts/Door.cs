using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class Door : MonoBehaviourPunCallbacks // <- small change to get callbacks
{
    public float rotationAmount = 80f;
    public float rotationSpeed = 5f;
    private bool isDoorOpen = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    private void Awake()
    {
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    public void ToggleDoor()
    {
        if (photonView != null && PhotonNetwork.InRoom)
        {
            photonView.RPC(nameof(RPC_SetDoorState), RpcTarget.AllBuffered, !isDoorOpen);
            return;
        }

        if (isDoorOpen) CloseDoor();
        else OpenDoor();
        isDoorOpen = !isDoorOpen;
    }

    [PunRPC]
    private void RPC_SetDoorState(bool open)
    {
        if (open) OpenDoor();
        else CloseDoor();
        isDoorOpen = open;
    }

    private void OpenDoor()
    {
        targetRotation = Quaternion.Euler(0, rotationAmount, 0) * initialRotation;
    }

    private void CloseDoor()
    {
        targetRotation = initialRotation;
    }
}
