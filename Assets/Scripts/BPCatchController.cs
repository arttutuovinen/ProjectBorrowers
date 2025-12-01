using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BPCatchController : MonoBehaviour, IPunObservable
{
    [Header("References")]
    public Animator childAnimator;
    public Camera playerCamera;

    public string catchLayerName = "CatchAnim";
    public string armVerticalLayerName = "ArmVertical";

    private PhotonView PV;

    // Camera → Threshold mapping
    private float minAngle = -50f;
    private float maxAngle = 60f;

    // Catch spam prevention
    private bool isCatching = false;
    public float catchCooldown = 2.0f;
    private float nextCatchTime = 0f;

    // Network-synced arm angle
    private float syncedArmVert = 0f;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (childAnimator == null)
            Debug.LogError("Assign the child Animator in inspector!");

        if (playerCamera == null && PV.IsMine)
            Debug.LogError("Assign playerCamera in inspector!");
    }

    private void Update()
    {
        if (PV.IsMine)
        {
            // Local player updates the arm based on camera
            UpdateArmVerticalBlend();

            // Local player triggers catch
            if (Input.GetButtonDown("Fire1"))
            {
                if (!isCatching && Time.time >= nextCatchTime)
                {
                    PV.RPC("RPC_PlayCatchAnimation", RpcTarget.All);
                }
            }
        }
        else
        {
            // Remote players apply the synced parameter
            childAnimator.SetFloat("ArmVert", syncedArmVert);
        }
    }

    // -----------------------------------------------------------
    // CAM ANGLE → BLEND TREE THRESHOLD (LOCAL ONLY)
    // -----------------------------------------------------------
    private void UpdateArmVerticalBlend()
    {
        float camX = playerCamera.transform.localEulerAngles.x;

        // Convert 0–360 → -180–180
        if (camX > 180f) camX -= 360f;

        // Map (-50 → 60) to (1 → -1) (INVERTED TO MATCH YOUR SETUP)
        float t = Mathf.InverseLerp(minAngle, maxAngle, camX);
        float mapped = Mathf.Lerp(1f, -1f, t);

        // Optional: make 0° = 0.1
        if (Mathf.Abs(camX) < 0.1f)
            mapped = 0.1f;

        childAnimator.SetFloat("ArmVert", mapped);
    }

    // -----------------------------------------------------------
    // CATCH ANIMATION (SYNCED VIA RPC)
    // -----------------------------------------------------------
    [PunRPC]
    private void RPC_PlayCatchAnimation()
    {
        if (isCatching) return; // network safety

        isCatching = true;
        nextCatchTime = Time.time + catchCooldown;

        int armLayer = childAnimator.GetLayerIndex(armVerticalLayerName);

        // Activate ArmVertical layer
        childAnimator.SetLayerWeight(armLayer, 1f);

        // Start catch animation
        childAnimator.SetBool("IsCatching", true);

        StartCoroutine(ResetCatchAfterAnimation(armLayer));
    }

    // -----------------------------------------------------------
    // RESET AFTER ANIMATION
    // -----------------------------------------------------------
    private IEnumerator ResetCatchAfterAnimation(int armLayer)
    {
        int catchLayer = childAnimator.GetLayerIndex(catchLayerName);

        yield return null;

        AnimatorStateInfo info = childAnimator.GetCurrentAnimatorStateInfo(catchLayer);
        float length = info.length;

        // Wait for animation to finish
        yield return new WaitForSeconds(length);

        childAnimator.SetBool("IsCatching", false);

        // Fade layer back to 0
        float fade = 0.2f;
        float t = 0f;

        while (t < fade)
        {
            t += Time.deltaTime;
            float w = Mathf.Lerp(1f, 0f, t / fade);
            childAnimator.SetLayerWeight(armLayer, w);
            yield return null;
        }

        childAnimator.SetLayerWeight(armLayer, 0f);

        // Unlock catching
        isCatching = false;
    }

    // -----------------------------------------------------------
    // PUN 2 ARMVERT SYNC (LOCAL WRITES → REMOTES READ)
    // -----------------------------------------------------------
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Local player sends ArmVert to others
            float armValue = childAnimator.GetFloat("ArmVert");
            stream.SendNext(armValue);
        }
        else
        {
            // Remote players receive ArmVert and apply it
            syncedArmVert = (float)stream.ReceiveNext();
        }
    }
}
