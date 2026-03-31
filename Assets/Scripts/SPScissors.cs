using UnityEngine;
using Photon.Pun;

public class SPScissors : MonoBehaviourPun
{
    private GameObject currentTape;
    private SPInteractionUI spInteractionUI;
    private bool hasScissors = false;

    void Start()
    {
        spInteractionUI = FindObjectOfType<SPInteractionUI>();
        hasScissors = false;
    }

    // Helper: find the nearest parent (or self) that has SPCutTape
    private GameObject FindTapeParent(Transform t)
    {
        while (t != null)
        {
            if (t.GetComponent<SPCutTape>() != null) // found the tape object
                return t.gameObject;

            t = t.parent; // move up
        }
        return null; // nothing found
    }

    private void OnTriggerEnter(Collider other)
    {

        // Find the closest TapeWeapon object
        GameObject tape = FindTapeParent(other.transform);

        if (tape != null)
        {
            currentTape = tape;
            

            // Only show cut UI if the player has scissors
            if (hasScissors)
            {
                SPCutTape cutTape = tape.GetComponent<SPCutTape>();
                if (cutTape != null)
                {
                    // Check if TapeMesh_1 is active (always exists)
                    Transform mesh1 = tape.transform.Find("TapeMesh_1");
                    bool mesh1Active = mesh1 != null && mesh1.gameObject.activeSelf;

                    // Check if TapeMesh_2 exists and is active (only BothSides)
                    Transform mesh2 = tape.transform.Find("TapeMesh_2");
                    bool mesh2Active = mesh2 != null && mesh2.gameObject.activeSelf;

                    // Show the UI only if at least one relevant mesh is active
                    if (mesh1Active || mesh2Active)
                    {
                        spInteractionUI?.ShowCut();
                        Debug.Log("ShowCut activated for tape: " + tape.name);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hasScissors == true)
        {
            spInteractionUI?.HideAll();
        }
        
        GameObject tape = FindTapeParent(other.transform);
        if (tape != null && tape == currentTape)
        {
            Debug.Log("Exited tape: " + currentTape.name);
            currentTape = null;
        }
    }

    public void UseScissors()
    {
        if (!photonView.IsMine)
            return;
        spInteractionUI?.HideAll();
        Debug.Log("UseScissors called");
        if (currentTape == null)
            return;

        PhotonView tapeView = currentTape.GetComponent<PhotonView>();
        Debug.Log("TapeView: " + (tapeView ? tapeView.name : "NULL"));
        if (tapeView == null)
            return;

        // Call RPC to cut the tape on all clients
        tapeView.RPC("RPC_CutTape", RpcTarget.All);
        Debug.Log("Sending RPC to: " + currentTape.name);
    }

    public void GotScissors()
    {
        hasScissors = true;
    }
    public void UsedScissors()
    {
        hasScissors = false;
    }
}
