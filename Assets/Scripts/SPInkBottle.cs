using UnityEngine;
using Photon.Pun;

public class SPInkBottle : MonoBehaviourPun
{
    public int throwForce = 10;
    public Transform spCamera;
    public Transform throwOrigin;
    public string inkBottlePrefabName = "InkBottleWeapon"; // Must match prefab name in Resources

    public void UseInkBottle()
    {
        // Only allow the local player to throw
        if (!photonView.IsMine) return;

        // Spawn position & rotation
        Vector3 spawnPos = throwOrigin.position;
        Quaternion spawnRot = Quaternion.identity;

        // Instantiate over network
        GameObject bottle = PhotonNetwork.Instantiate(
            inkBottlePrefabName,
            spawnPos,
            spawnRot
        );

        // Get Rigidbody
        Rigidbody rb = bottle.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Throw in camera forward direction
            Vector3 throwDirection = spCamera.forward;

            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }
    }
}
