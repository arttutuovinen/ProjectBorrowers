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
        photonView = GetComponentInParent<PhotonView>(); // get root
    }

    // Called from Animation Event (works now!)
    public void PlayFootstep()
    {
        if (!photonView.IsMine)
            return;

        float pitch = Random.Range(minPitch, maxPitch);

        // Play locally
        PlayWithPitch(pitch);

        // Send RPC THROUGH the root PhotonView
        photonView.RPC("RPC_PlayFootstep", RpcTarget.Others, pitch);
    }

    public void PlayWithPitch(float pitch)
    {
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(footstepClip, volume);
    }
}
