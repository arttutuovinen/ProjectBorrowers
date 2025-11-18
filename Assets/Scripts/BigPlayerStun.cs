using System.Collections;
using UnityEngine;
using Photon.Pun;

public class BigPlayerStun : MonoBehaviourPun
{
    public float stunDuration = 3f;
    public float stunRadius = 1f; // radius to check for nearby BoppyPins
    private bool isStunned = false;
    private BigPlayerMovement bigPlayerMovement;

    void Start()
    {
        bigPlayerMovement = GetComponent<BigPlayerMovement>();

        // Ensure Rigidbody exists for physics checks (optional)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return; // only run on local player

        // Check for BoppyPins nearby using tag only
        Collider[] hits = Physics.OverlapSphere(transform.position, stunRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("StunItem"))
            {
                // Stun locally
                if (!isStunned)
                    StartCoroutine(StunPlayer());

                // Destroy item on MasterClient
                PhotonView itemPV = hit.GetComponent<PhotonView>();
                if (itemPV != null && PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(itemPV.gameObject);
                }
            }
        }
    }

    IEnumerator StunPlayer()
    {
        isStunned = true;
        bigPlayerMovement.enabled = false;

        yield return new WaitForSeconds(stunDuration);

        bigPlayerMovement.enabled = true;
        isStunned = false;
    }

    // Optional: visualize the stun radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stunRadius);
    }
}

