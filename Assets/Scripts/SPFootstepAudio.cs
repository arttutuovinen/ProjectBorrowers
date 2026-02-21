using UnityEngine;
using Photon.Pun;

public class SPFootstepAudio : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioClip footstepClip;
    public float volume = 1f;

    [Header("Pitch Settings")]
    public float minPitch = 0.5f;
    public float maxPitch = 1.0f;

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

        float randomPitch = GetRandomPitch();

        // Play locally
        PlayWithPitch(randomPitch);

        // Sync pitch to others
        photonView.RPC(nameof(RPC_PlayFootstep), RpcTarget.Others, randomPitch);
    }

    [PunRPC]
    private void RPC_PlayFootstep(float pitch)
    {
        PlayWithPitch(pitch);
    }

    private void PlayWithPitch(float pitch)
    {
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(footstepClip, volume);
    }

    private float GetRandomPitch()
    {
        return Random.Range(minPitch, maxPitch);
    }
}
