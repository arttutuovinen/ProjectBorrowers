using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SPThrowItemHit : MonoBehaviourPun
{
    public float knockbackForce = 10f;
    public float upwardForce = 5f;
    public float knockbackDuration = 0.2f;

    private bool isKnockedBack = false;
    private SmallPlayerMovement playerMovement;


    void Start()
    {
        playerMovement = GetComponent<SmallPlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only allow the owner of SmallPlayer to handle being hit
        if (!photonView.IsMine) return;

        if (other.CompareTag("BPThrowItemWeapon"))
        {
            // Broadcast hit to ALL clients
            photonView.RPC("RPC_OnHit", RpcTarget.All);

            // Destroy thrown object across the network
            PhotonNetwork.Destroy(other.gameObject);
        }
    }

    // ------------------------------
    // RPC CALLED ON ALL CLIENTS
    // ------------------------------
    [PunRPC]
    void RPC_OnHit()
    {
        Vector3 randomDir = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        ).normalized;

        Vector3 knockDir = randomDir * knockbackForce + Vector3.up * upwardForce;

        StartCoroutine(KnockbackCoroutine(knockDir));

        if (photonView.IsMine)
        {
            playerMovement.DisableMovement();
            Invoke(nameof(ReEnableMovement), knockbackDuration + 3f);
        }
    }

    // ------------------------------
    // Knockback movement
    // ------------------------------
    private IEnumerator KnockbackCoroutine(Vector3 direction)
    {
        if (isKnockedBack)
            yield break;

        isKnockedBack = true;

        float timer = 0f;

        while (timer < knockbackDuration)
        {
            transform.position += direction * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;

    }

    private void ReEnableMovement()
    {
        playerMovement.EnableMovement();
    }
}


