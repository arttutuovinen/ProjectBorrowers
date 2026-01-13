using UnityEngine;
using Photon.Pun;

public class TreasureScoreManager : MonoBehaviourPun
{
    public static TreasureScoreManager Instance;

    public int treasuresDelivered;
    public int totalTreasures = 3;

    private void Awake()
    {
        Instance = this;
    }

    [PunRPC]
    public void RPC_AddTreasure()
    {
        treasuresDelivered++;

        photonView.RPC(nameof(RPC_UpdateUI), RpcTarget.All, treasuresDelivered);
    }

    [PunRPC]
    public void RPC_UpdateUI(int value)
    {
        foreach (var sp in FindObjectsOfType<SmallPlayerItemCollector>())
        {
            if (sp.photonView.IsMine)
            {
                sp.SetTreasureCount(value, totalTreasures);

                // Show Escape text if all treasures delivered
                if (value >= totalTreasures)
                    sp.SetEscapeActive(true);
            }
        }
    }

}
