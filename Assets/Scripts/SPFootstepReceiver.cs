using UnityEngine;
using Photon.Pun;

public class SPFootstepReceiver : MonoBehaviourPun
{
    private SPFootstepAudio footstepAudio;

    private void Awake()
    {
        footstepAudio = GetComponentInChildren<SPFootstepAudio>();
    }

    [PunRPC]
    private void RPC_PlayFootstep(float pitch)
    {
        footstepAudio.PlayWithPitch(pitch);
    }
}
