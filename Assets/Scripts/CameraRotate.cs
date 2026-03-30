using UnityEngine;
using Photon.Pun;

public class CameraRotate : MonoBehaviour,IPunObservable
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 90f; // degrees per second

    private PhotonView pv;

    private float targetAngle;
    private float currentY;
    private bool rotating = false;

    private int step = 0; // 0 = first (90), then alternates 180

    void Awake()
    {
        // 🔑 Get PhotonView from parent (root)
        pv = GetComponentInParent<PhotonView>();
    }

    void Start()
    {
        currentY = transform.eulerAngles.y;

        // Only owner controls rotation
        if (pv != null && pv.IsMine)
        {
            SetNextTarget();
        }
    }

    void Update()
    {
        if (pv == null || !pv.IsMine) return;
        if (!rotating) return;

        float stepAmount = rotationSpeed * Time.deltaTime;

        float newY = Mathf.MoveTowardsAngle(currentY, targetAngle, stepAmount);
        transform.rotation = Quaternion.Euler(0, newY, 0);

        currentY = newY;

        if (Mathf.Approximately(currentY, targetAngle))
        {
            rotating = false;
            SetNextTarget();
        }
    }

    void SetNextTarget()
    {
        float rotationAmount;

        if (step == 0)
        {
            rotationAmount = 90f; // first rotation
        }
        else
        {
            rotationAmount = (step % 2 == 1) ? -180f : 180f;
        }

        targetAngle = currentY + rotationAmount;
        rotating = true;
        step++;
    }

    // 🔄 Sync rotation across network
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (pv == null) return;

        if (stream.IsWriting && pv.IsMine)
        {
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.rotation = (Quaternion)stream.ReceiveNext();
            currentY = transform.eulerAngles.y;
        }
    }
}
