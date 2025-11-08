using UnityEngine;
using Photon.Pun;

public class SPPersists : MonoBehaviourPun
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject); // keep this player across scene loads
    }
}
