using UnityEngine;
using Photon.Pun;
using TMPro;

public class SPScoreManager : MonoBehaviourPun
{
    public int treasuresCollected = 0;
    public int totalTreasures = 3;

    private TextMeshProUGUI treasuresText;
    private GameObject treasure;
    private GameObject escapeText;

    void Start()
    {
        if (!photonView.IsMine) return;
        Canvas canvas = FindObjectOfType<Canvas>();
        treasuresText = canvas.transform.Find("SmallPlayerUI/Treasures").GetComponent<TextMeshProUGUI>();
        treasure = GameObject.Find("Treasure");
        escapeText = canvas.transform.Find("SmallPlayerUI/Escape")?.gameObject;
        UpdateTreasuresUI();
    }

    public void AddTreasure()
    {
        if (!photonView.IsMine) return;

        treasuresCollected++;
        UpdateTreasuresUI();

        // Sync with all clients
        photonView.RPC(nameof(RPC_UpdateTreasures), RpcTarget.OthersBuffered, treasuresCollected);

        // Deactivate treasure if max reached
        if (treasuresCollected >= totalTreasures && treasure != null)
        {
            treasure.SetActive(false);
            escapeText.SetActive(true);
        }
    }

    [PunRPC]
    private void RPC_UpdateTreasures(int value)
    {
        treasuresCollected = value;
        UpdateTreasuresUI();
    }

    private void UpdateTreasuresUI()
    {
        if (treasuresText != null)
            treasuresText.text = $"Treasures: {treasuresCollected}/{totalTreasures}";
    }

}
