using UnityEngine;
using Photon.Pun;

public class SPFootstepAudio : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioClip footstepClip;
    public float volume = 1f;

    public AudioSource audioSource;
    private PhotonView photonView;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        photonView = GetComponentInParent<PhotonView>(); // <-- get root PhotonView
    }

    // Called from Animation Event
    public void PlayFootstep()
    {
        if (!photonView.IsMine)
            return;

        // Play locally
        audioSource.PlayOneShot(footstepClip, volume);

        // Tell others to play it
        photonView.RPC(nameof(RPC_PlayFootstep), RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_PlayFootstep()
    {
        audioSource.PlayOneShot(footstepClip, volume);
    }
}
