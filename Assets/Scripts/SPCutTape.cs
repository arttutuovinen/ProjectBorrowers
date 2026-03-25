using UnityEngine;
using Photon.Pun;

public class SPCutTape : MonoBehaviourPun
{
    [PunRPC]
    public void RPC_CutTape()
    {
        Debug.Log("RPC_CutTape called on " + gameObject.name);
        Transform mesh1 = transform.Find("TapeMesh_1");
        Transform mesh2 = transform.Find("TapeMesh_2");

        if (mesh1 != null && mesh1.gameObject.activeSelf)
            mesh1.gameObject.SetActive(false);

        if (mesh2 != null && mesh2.gameObject.activeSelf)
            mesh2.gameObject.SetActive(false);
    }
}
