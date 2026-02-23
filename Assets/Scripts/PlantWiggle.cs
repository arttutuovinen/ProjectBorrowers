using UnityEngine;
using Photon.Pun;

public class PlantWiggle : MonoBehaviourPun
{
    Renderer rend;
    Material mat;

    float currentStrength;
    float targetStrength;

    const float ON_VALUE = 2f;
    const float OFF_VALUE = 0f;

    public AudioSource audioSource;

    [Header("Bush audio Settings")]
    public AudioClip bushClip;
    public float volume = 0.05f;

    [Header("Pitch Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.0f;

    bool isPlayingSound;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rend = GetComponent<Renderer>();
        mat = rend.material; // instance per plant
    }

    void Update()
    {
        float speed = targetStrength > currentStrength ? 10f : 3f;

        currentStrength = Mathf.Lerp(currentStrength, targetStrength, Time.deltaTime * speed);
        mat.SetFloat("_moveStrenght", currentStrength);
    }

    [PunRPC]
    public void SetWiggle(bool active)
    {
        targetStrength = active ? ON_VALUE : OFF_VALUE;

        if (!photonView.IsMine)
            return;

        float randomPitch = GetRandomPitch();

        // Play locally
        PlayWithPitch(randomPitch);

        // Sync pitch to others
        photonView.RPC(nameof(RPC_PlayBush), RpcTarget.Others, randomPitch);
    }

    [PunRPC]
    private void RPC_PlayBush(float pitch)
    {
        PlayWithPitch(pitch);
    }

    private void PlayWithPitch(float pitch)
    {
        if (isPlayingSound) return;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(bushClip, volume);

        StartCoroutine(SoundCooldown());
    }

    private System.Collections.IEnumerator SoundCooldown()
    {
        isPlayingSound = true;
        yield return new WaitForSeconds(bushClip.length);
        isPlayingSound = false;
    }

    private float GetRandomPitch()
    {
        return Random.Range(minPitch, maxPitch);
    }
}
