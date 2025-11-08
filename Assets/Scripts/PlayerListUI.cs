using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerListUI : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerListText;
    public GameObject panel;

    void Start()
    {
        if (panel == null)
            panel = gameObject;
        panel.SetActive(false);
        RefreshPlayerList();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            panel.SetActive(!panel.activeSelf);
    }

    void RefreshPlayerList()
    {
        if (playerListText == null) return;

        playerListText.text = "Players in Room:\n";
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            playerListText.text += "• " + p.NickName + "\n";
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayerList();
    }
}
