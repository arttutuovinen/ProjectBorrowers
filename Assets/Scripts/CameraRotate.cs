using UnityEngine;
using Photon.Pun;
using System.Collections;

public class CameraRotate : MonoBehaviour,IPunObservable
{
    [Header("References")]
    public Transform cameraHead;     // 👈 child that rotates
    public Collider fieldOfView;     // 👈 trigger collider

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f; // degrees per second

    [SerializeField] private PhotonView pv;

    private float minY = -90f;
    private float maxY = 90f;

    private float currentY;
    private float targetY;

    private bool goingRight = true;
    private bool isLocked = false;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip alarmSound;

    void Start()
    {
        if (cameraHead == null)
        {
            Debug.LogError("CameraHead not assigned!");
            return;
        }

        currentY = cameraHead.localEulerAngles.y;

        if (currentY > 180f)
            currentY -= 360f;

        targetY = maxY;
    }

    void Update()
    {
        if (pv == null || !pv.IsMine) return;
        if (isLocked) return;

        float step = rotationSpeed * Time.deltaTime;

        currentY = Mathf.MoveTowards(currentY, targetY, step);

        cameraHead.localRotation = Quaternion.Euler(0f, currentY, 0f);

        if (Mathf.Approximately(currentY, targetY))
        {
            goingRight = !goingRight;
            targetY = goingRight ? maxY : minY;
        }
    }

    public void OnChildTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.name);

        if (!other.CompareTag("SmallPlayer")) return;

        if (pv.IsMine)
        {
            isLocked = true;

            pv.RPC(nameof(RPC_LockCamera), RpcTarget.All);
            pv.RPC(nameof(RPC_SetFOVColorRed), RpcTarget.All);
            pv.RPC(nameof(RPC_PlaySound), RpcTarget.All);
            StartCoroutine(DestroyAfterDelay(5f));
        }
    }

    [PunRPC]
    void RPC_LockCamera()
    {
        isLocked = true;
    }

    [PunRPC]
    void RPC_SetFOVColorRed()
    {
        if (fieldOfView == null) return;

        Renderer rend = fieldOfView.GetComponent<Renderer>();
        if (rend == null) return;

        foreach (Material mat in rend.materials)
        {
            if (mat.name.Contains("m_hologram"))
            {
                mat.color = Color.red;
            }
        }
    }

    [PunRPC]
    void RPC_PlaySound()
    {
        audioSource.clip = alarmSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (pv != null && pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    // 🔄 Photon Sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (pv == null) return;

        if (stream.IsWriting && pv.IsMine)
        {
            stream.SendNext(cameraHead.localRotation);
            stream.SendNext(isLocked);
        }
        else
        {
            Quaternion targetRot = (Quaternion)stream.ReceiveNext();
            isLocked = (bool)stream.ReceiveNext();

            cameraHead.localRotation = Quaternion.Slerp(
                cameraHead.localRotation,
                targetRot,
                10f * Time.deltaTime
            );

            currentY = cameraHead.localEulerAngles.y;
            if (currentY > 180f) currentY -= 360f;
        }
    }
}
