using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BPCatchController : MonoBehaviour
{
    [Header("References")]
    public Animator childAnimator; // assign the BP Mesh Animator in inspector
    public string catchLayerName = "CatchAnim"; // your animation layer name

    private PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        if (childAnimator == null)
            Debug.LogError("Assign the child Animator in inspector!");
    }

    private void Update()
    {
        if (!PV.IsMine) return;

        if (Input.GetButtonDown("Fire1"))
        {
            PV.RPC("RPC_PlayCatchAnimation", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_PlayCatchAnimation()
    {
        childAnimator.SetBool("IsCatching", true);
        StartCoroutine(ResetCatchBool());
    }

    private IEnumerator ResetCatchBool()
    {
        int layerIndex = childAnimator.GetLayerIndex(catchLayerName);

        // Wait a frame to make sure Animator updates
        yield return null;

        AnimatorStateInfo stateInfo = childAnimator.GetCurrentAnimatorStateInfo(layerIndex);
        float clipLength = stateInfo.length;

        // Wait for animation to finish
        yield return new WaitForSeconds(clipLength);

        childAnimator.SetBool("IsCatching", false);
    }
}
