using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FinishTrigger : MonoBehaviourPun
{
    [Header("Door Settings")]
    public Transform door;
    public float doorCloseAngle = 0f;   // Final rotation when closed
    public float doorOpenAngle = 108f;  // Initial rotation
    public float doorCloseSpeed = 2f;   // Smooth rotation speed

    private bool treasureDelivered = false;

    private void OnTriggerStay(Collider other)
    {
        if (treasureDelivered) return;
        if (!other.CompareTag("SmallPlayer")) return;

        var spCollector = other.GetComponent<SmallPlayerItemCollector>();
        if (spCollector == null) return;

        // Check if player is holding Treasure
        if (spCollector.GetCurrentItem() != "Treasure") return;

        // Deliver treasure on Fire1 press
        if (Input.GetButtonDown("Fire1"))
        {
            DeliverTreasure(spCollector);
        }
    }

    private void DeliverTreasure(SmallPlayerItemCollector spCollector)
    {
        treasureDelivered = true;

        // Disable this trigger so it cannot be reused
        GetComponent<Collider>().enabled = false;

        // Consume the Treasure from the SP client
        spCollector.ConsumeItem("Treasure");

        // Close the door smoothly
        if (door != null)
            StartCoroutine(CloseDoor());

        Debug.Log("Treasure delivered to SPFinish!");

        // Notify MasterClient to spawn/teleport new Treasure
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_RequestNewTreasure), RpcTarget.MasterClient);
        }
        else
        {
            SpawnNewTreasure();
        }
    }

    [PunRPC]
    private void RPC_RequestNewTreasure()
    {
        SpawnNewTreasure();
    }

    private void SpawnNewTreasure()
    {
        TreasureSpawner spawner = FindObjectOfType<TreasureSpawner>();
        if (spawner != null)
        {
            // Reactivate the Treasure prefab instead of instantiating
            if (spawner.treasure != null)
                spawner.treasure.SetActive(true);

            // Teleport it to a random spawn point
            spawner.TeleportTreasure();

            Debug.Log("Treasure reactivated and teleported by MasterClient.");
        }
    }


    private IEnumerator CloseDoor()
    {
        float t = 0f;
        Quaternion startRot = door.localRotation;
        Quaternion targetRot = Quaternion.Euler(0f, doorCloseAngle, 0f);

        while (t < 1f)
        {
            t += Time.deltaTime * doorCloseSpeed;
            door.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        door.localRotation = targetRot;
    }
}

