using UnityEngine;
using Photon.Pun;

public class CameraRotate : MonoBehaviour,IPunObservable
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 90f; // degrees per second

    [SerializeField] private PhotonView pv;

    private float minY = -90f;
    private float maxY = 90f;

    private float currentY;
    private float targetY;

    private bool goingRight = true;

    void Start()
    {
        currentY = transform.localEulerAngles.y;

        // Convert from 0–360 to -180–180
        if (currentY > 180f)
            currentY -= 360f;

        targetY = maxY;

        if (!pv.IsMine) return;
    }

    void Update()
    {
        if (pv == null || !pv.IsMine) return;

        float step = rotationSpeed * Time.deltaTime;

        currentY = Mathf.MoveTowards(currentY, targetY, step);
        transform.localRotation = Quaternion.Euler(0f, currentY, 0f);

        if (Mathf.Approximately(currentY, targetY))
        {
            // Switch direction
            goingRight = !goingRight;
            targetY = goingRight ? maxY : minY;
        }
    }

    // 🔄 Photon Sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (pv == null) return;

        if (stream.IsWriting && pv.IsMine)
        {
            stream.SendNext(transform.localRotation);
        }
        else
        {
            Quaternion targetRot = (Quaternion)stream.ReceiveNext();

            // Smooth for other clients
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRot,
                10f * Time.deltaTime
            );

            currentY = transform.localEulerAngles.y;
            if (currentY > 180f) currentY -= 360f;
        }
    }
}
