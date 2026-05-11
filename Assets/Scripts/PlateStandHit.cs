using UnityEngine;
using Photon.Pun;

public class PlateStandHit : MonoBehaviourPun
{
    [SerializeField] private GameObject plateMesh;
    [SerializeField] private ParticleSystem hitParticles;

    private bool hasBeenHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit)
            return;

        if (other.CompareTag("BPThrowItemWeapon"))
        {
            hasBeenHit = true;

            photonView.RPC(nameof(RPC_HitPlate), RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_HitPlate()
    {
        plateMesh.SetActive(false);

        hitParticles.Play();
    }
}
