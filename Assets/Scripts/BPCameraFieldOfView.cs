using UnityEngine;

public class BPCameraFieldOfView : MonoBehaviour
{
    public CameraRotate cameraRotate;

    private void OnTriggerEnter(Collider other)
    {
        cameraRotate.OnChildTriggerEnter(other);
    }
}
