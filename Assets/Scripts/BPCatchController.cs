using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BPCatchController : MonoBehaviourPun, IPunObservable
{
    [Header("References")]
    public Animator childAnimator;
    public Camera playerCamera;

    public string catchLayerName = "CatchAnim";
    public string armVerticalLayerName = "ArmVertical";

    private PhotonView PV;

    private float minAngle = -50f;
    private float maxAngle = 60f;

    public float catchCooldown = 5.0f;
    private float nextCatchTime = 0f;

    private bool isCatching = false;

    private float syncedArmVert = 0f;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (PV.IsMine)
        {
            UpdateArmVerticalBlend();

            if (Input.GetButtonDown("Fire1"))
            {
                TryCatch();
            }
        }
        else
        {
            childAnimator.SetFloat("ArmVert", syncedArmVert);
        }
    }

    // -----------------------------------------------------------
    // CAMERA → ARM BLEND
    // -----------------------------------------------------------
    private void UpdateArmVerticalBlend()
    {
        float camX = playerCamera.transform.localEulerAngles.x;
        if (camX > 180f) camX -= 360f;

        float t = Mathf.InverseLerp(minAngle, maxAngle, camX);
        float mapped = Mathf.Lerp(1f, -1f, t);

        if (Mathf.Abs(camX) < 0.1f)
            mapped = 0.1f;

        childAnimator.SetFloat("ArmVert", mapped);
    }

    // -----------------------------------------------------------
    // TRY CATCH
    // -----------------------------------------------------------
    private void TryCatch()
    {
        if (isCatching) return;
        if (Time.time < nextCatchTime) return;

        PV.RPC("RPC_PlayCatchAnimation", RpcTarget.All);
    }

    // -----------------------------------------------------------
    // RPC PLAY
    // -----------------------------------------------------------
    [PunRPC]
    private void RPC_PlayCatchAnimation()
    {

        if (isCatching) return;

        isCatching = true;
        nextCatchTime = Time.time + catchCooldown;

        int armLayer = childAnimator.GetLayerIndex(armVerticalLayerName);
        int catchLayer = childAnimator.GetLayerIndex(catchLayerName);

        childAnimator.SetLayerWeight(armLayer, 1f);
        childAnimator.SetLayerWeight(catchLayer, 1f);

        childAnimator.SetBool("IsCatching", true);

        StartCoroutine(ResetAfterCatch(armLayer, catchLayer));
    }

    // -----------------------------------------------------------
    // RESET BOTH LAYERS
    // -----------------------------------------------------------
    private IEnumerator ResetAfterCatch(int armLayer, int catchLayer)
    {
        yield return null;

        AnimatorStateInfo info = childAnimator.GetCurrentAnimatorStateInfo(catchLayer);
        float duration = info.length;

        yield return new WaitForSeconds(duration);

        childAnimator.SetBool("IsCatching", false);

        // FADE BOTH LAYERS OUT TOGETHER
        float fade = 0.25f;
        float t = 0f;

        while (t < fade)
        {
            t += Time.deltaTime;
            float w = 1f - (t / fade);

            childAnimator.SetLayerWeight(armLayer, w);
            childAnimator.SetLayerWeight(catchLayer, w);

            yield return null;
        }

        childAnimator.SetLayerWeight(armLayer, 0f);
        childAnimator.SetLayerWeight(catchLayer, 0f);

        isCatching = false;
    }

    // -----------------------------------------------------------
    // SYNC ARMVERT
    // -----------------------------------------------------------
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(childAnimator.GetFloat("ArmVert"));
        else
            syncedArmVert = (float)stream.ReceiveNext();
    }

    public void PlayCaughtReaction()
    {
        if (childAnimator == null) return;

        int armLayer = childAnimator.GetLayerIndex(armVerticalLayerName);
        int catchLayer = childAnimator.GetLayerIndex(catchLayerName);
        int caughtLayer = childAnimator.GetLayerIndex("CaughtAnim");

        // Disable these two
        childAnimator.SetLayerWeight(armLayer, 0f);
        childAnimator.SetLayerWeight(catchLayer, 0f);

        // Enable caught layer
        childAnimator.SetLayerWeight(caughtLayer, 1f);

        // Trigger caught anim
        childAnimator.SetBool("IsCaught", true);
    }
    // BPCatchController.cs
    public void ResetCaughtReaction()
    {
        int caughtLayer = childAnimator.GetLayerIndex("CaughtAnim");

        childAnimator.SetBool("IsCaught", false);
        childAnimator.SetLayerWeight(caughtLayer, 0f);
    }

    [PunRPC]
    public void RPC_PlayCaughtReactionSP()
    {
        // Only execute locally on remote clients
        PlayCaughtReaction();
    }

    [PunRPC]
    public void RPC_ResetCaughtReactionSP()
    {
        ResetCaughtReaction();
    }
}
