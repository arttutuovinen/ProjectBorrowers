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

        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found!");
            return;
        }

        // ✅ Update treasure text directly
        var treasureText = canvas.transform.Find("Divider/Treasures")?.GetComponent<TMPro.TextMeshProUGUI>();

        if (treasureText != null)
        {
            treasureText.text = $"Treasures: {value}/{RequiredPlayeramount.TotalTreasures}";
        }
        else
        {
            Debug.LogWarning("Treasures text not found!");
        }

        // ✅ Update escape UI if it exists
        var escape = canvas.transform.Find("SmallPlayerUI/Escape")?.gameObject;
        if (escape != null)
        {
            escape.SetActive(value >= RequiredPlayeramount.TotalTreasures);
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
