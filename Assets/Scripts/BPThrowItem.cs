using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BPThrowItem : MonoBehaviourPun
{
    public string prefabThrowItem1 = "ThrowingItemWeapon";
    public string prefabThrowItem2 = "ThrowingItemWeapon_02";
    public string prefabThrowItem3 = "ThrowingItemWeapon_03";

    public Transform throwOrigin;
    public float throwForce = 20f;
    private float destroyTime = 5f;

    public Camera playerCamera;

    // ------------------------------
    // THROW ITEM 1
    // ------------------------------
    public void SpawnThrowItem()
    {
        if (!photonView.IsMine) return;

        GameObject thrownObject = PhotonNetwork.Instantiate(
            prefabThrowItem1,
            throwOrigin.position,
            Quaternion.identity
        );

        ApplyThrowForce(thrownObject);
        StartCoroutine(DestroyAfterDelay(thrownObject));
    }

    // ------------------------------
    // THROW ITEM 2
    // ------------------------------
    public void SpawnThrowItem2()
    {
        if (!photonView.IsMine) return;

        GameObject thrownObject = PhotonNetwork.Instantiate(
            prefabThrowItem2,
            throwOrigin.position,
            Quaternion.identity
        );

        ApplyThrowForce(thrownObject);
        StartCoroutine(DestroyAfterDelay(thrownObject));
    }

    // ------------------------------
    // THROW ITEM 3
    // ------------------------------
    public void SpawnThrowItem3()
    {
        if (!photonView.IsMine) return;

        GameObject thrownObject = PhotonNetwork.Instantiate(
            prefabThrowItem3,
            throwOrigin.position,
            Quaternion.identity
        );

        ApplyThrowForce(thrownObject);
        StartCoroutine(DestroyAfterDelay(thrownObject));
    }

    // ------------------------------
    // APPLY FORCE
    // ------------------------------
    private void ApplyThrowForce(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Vector3 dir = playerCamera.transform.forward;
        rb.AddForce(dir * throwForce, ForceMode.Impulse);
    }

    // ------------------------------
    // NETWORK SAFE AUTO DESTROY
    // ------------------------------
    private IEnumerator DestroyAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(destroyTime);

        if (obj != null)
        {
            PhotonNetwork.Destroy(obj);
        }
    }
}

