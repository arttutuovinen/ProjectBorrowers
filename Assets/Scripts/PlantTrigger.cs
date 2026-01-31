using UnityEngine;
using Photon.Pun;

public class PlantTrigger : MonoBehaviourPun
{
    bool isMoving;
    CharacterController controller;
    bool lastSentState;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        isMoving = controller.velocity.sqrMagnitude > 0.0001f;
    }

    void OnTriggerStay(Collider other)
    {
        if (!photonView.IsMine) return;

        PlantWiggle plant = other.GetComponentInChildren<PlantWiggle>();
        if (plant == null) return;

        if (isMoving != lastSentState)
        {
            plant.photonView.RPC("SetWiggle", RpcTarget.All, isMoving);
            lastSentState = isMoving;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine) return;

        PlantWiggle plant = other.GetComponentInChildren<PlantWiggle>();
        if (plant == null) return;

        plant.photonView.RPC("SetWiggle", RpcTarget.All, false);
        lastSentState = false;
    }

}
