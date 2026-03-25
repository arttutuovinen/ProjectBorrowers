using UnityEngine;
using Photon.Pun;

public class SPScissors : MonoBehaviour
{
    private GameObject currentTape;
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("SP collided by" + other.gameObject.name); //this works
        // Store tape object if we touch it
        SPCutTape tape = other.GetComponent<SPCutTape>();
        if (tape != null)
        {
            currentTape = other.gameObject;
            Debug.Log("Current tape: " + (currentTape ? currentTape.name : "NULL"));
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentTape)
        {
            currentTape = null;
        }
    }

    public void UseScissors()
    {
        if (!photonView.IsMine)
            return;
        Debug.Log("UseScissors called");
        if (currentTape == null)
            return;

        PhotonView tapeView = currentTape.GetComponent<PhotonView>();
        Debug.Log("TapeView: " + tapeView);
        if (tapeView == null)
            return;

        // Call RPC on tape object
        tapeView.RPC("RPC_CutTape", RpcTarget.All);
        Debug.Log("Sending RPC to: " + currentTape.name);
    }
}
