using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BPCatchController : MonoBehaviour
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
    public float catchCooldown = 1.0f;
    private float nextCatchTime = 0f;

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
        if (!PV.IsMine) return;

        UpdateArmVerticalBlend();

        // Only allow catch if cooldown is done & not already catching
        if (Input.GetButtonDown("Fire1"))
        {
            if (!isCatching && Time.time >= nextCatchTime)
            {
                PV.RPC("RPC_PlayCatchAnimation", RpcTarget.All);
            }
        }
    }

    // -----------------------------------------------------------
    // CAM ANGLE → BLEND TREE THRESHOLD
    // -----------------------------------------------------------
    private void UpdateArmVerticalBlend()
    {
        float camX = playerCamera.transform.localEulerAngles.x;

        // Convert 0–360 → -180–180
        if (camX > 180f) camX -= 360f;

        // Map angle (-50 → 60) to (1 → -1)  **INVERTED**
        float t = Mathf.InverseLerp(minAngle, maxAngle, camX);
        float mapped = Mathf.Lerp(1f, -1f, t);

        // Optional: make 0° ≈ 0.1
        if (Mathf.Abs(camX) < 0.1f)
            mapped = 0.1f;

        childAnimator.SetFloat("ArmVert", mapped);
    }

    // -----------------------------------------------------------
    // CATCH ANIMATION (RPC SYNCED)
    // -----------------------------------------------------------
    [PunRPC]
    private void RPC_PlayCatchAnimation()
    {
        // Prevent overlap even on network delay
        if (isCatching) return;

        isCatching = true;
        nextCatchTime = Time.time + catchCooldown;

        int armLayer = childAnimator.GetLayerIndex(armVerticalLayerName);

        // Turn Arm layer ON
        childAnimator.SetLayerWeight(armLayer, 1f);

        // Trigger animation
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

        // Wait until animation finishes
        yield return new WaitForSeconds(length);

        childAnimator.SetBool("IsCatching", false);

        // Fade ArmVertical layer weight to 0
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

        // Allow next catch
        isCatching = false;
    }
}
