using UnityEngine;
using Photon.Pun;

public class TreasureScoreManager : MonoBehaviourPun
{
    public static TreasureScoreManager Instance;

    public int treasuresDelivered;

    private void Awake()
    {
        Instance = this;
    }

    public int GetTreasureCount() => treasuresDelivered;

    [PunRPC]
    public void RPC_SetTreasureCount(int value)
    {
        treasuresDelivered = value;

        foreach (var sp in FindObjectsOfType<SmallPlayerItemCollector>())
        {
            sp.SetTreasureCount(value, RequiredPlayeramount.TotalTreasures);
            sp.SetEscapeActive(value >= RequiredPlayeramount.TotalTreasures);
        }
    }

    // ONLY MasterClient calls this
    public void AddTreasure_Master()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        treasuresDelivered++;
        photonView.RPC(nameof(RPC_SetTreasureCount), RpcTarget.All, treasuresDelivered);
    }
}
