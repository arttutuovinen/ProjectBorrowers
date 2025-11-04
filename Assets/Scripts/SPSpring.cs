using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SPSpring : MonoBehaviourPun
{
    [Header("Spring Settings")]
    public float knockbackForce = 35f;           // Strength of the upward knockback
    public float knockbackDuration = 1f;       // How long the knockback lasts
    public AnimationCurve jumpCurve;             // Curve to smooth out jump motion

    private bool isKnockedBack = false;
    private CharacterController controller;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void UseSpring()
    {
        if (!photonView.IsMine) return; // Only the local player should perform the knockback

        Debug.Log("SP used SPRING");

        Vector3 knockbackDirection = Vector3.up * knockbackForce;

        // Run locally
        StartCoroutine(KnockbackCoroutine(knockbackDirection));

        // Optionally tell others to play VFX or animation
        photonView.RPC(nameof(PlaySpringEffectRPC), RpcTarget.Others);
    }

    private IEnumerator KnockbackCoroutine(Vector3 direction)
    {
        if (isKnockedBack)
            yield break; // Prevent overlapping knockbacks

        isKnockedBack = true;
        float timer = 0f;

        while (timer < knockbackDuration)
        {
            // Apply upward motion using CharacterController
            controller.Move(direction * jumpCurve.Evaluate(timer / knockbackDuration) * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
    }

    [PunRPC]
    private void PlaySpringEffectRPC()
    {
        // This is called on remote clients to play VFX, sound, or animation.
        // Add effects here, but don't move remote players.
        Debug.Log($"Spring used by {photonView.Owner.NickName}");
    }
}

