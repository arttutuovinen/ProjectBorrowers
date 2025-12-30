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
    private bool isAnimating;

    public bool IsOpen => isDoorOpen;

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
        if (!PhotonNetwork.InRoom) return;

        // Only MasterClient actually changes door state
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_SetDoorState), RpcTarget.AllBuffered, !isDoorOpen);
        }
        else
        {
            // Ask MasterClient to toggle the door
            photonView.RPC(nameof(RPC_RequestToggleDoor), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void RPC_RequestToggleDoor()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_SetDoorState), RpcTarget.AllBuffered, !isDoorOpen);
    }

    [PunRPC]
    private void RPC_SetDoorState(bool open)
    {
        if (isAnimating) return;

        isAnimating = true;

        if (open) OpenDoor();
        else CloseDoor();

        isDoorOpen = open;
        StartCoroutine(ResetAnimating());
    }

    private IEnumerator ResetAnimating()
    {
        yield return new WaitForSeconds(0.3f);
        isAnimating = false;
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
