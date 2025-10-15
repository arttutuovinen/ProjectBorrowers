using UnityEngine;

public class LeafBillboarding : MonoBehaviour
{
    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0); // flip if backward
    }
}
