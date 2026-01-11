using UnityEngine;
using Photon.Pun;
using TMPro;

public class SPScoreManager : MonoBehaviourPun
{
    private TextMeshProUGUI treasuresText;
    private FinishTrigger finish;

    void Start()
    {
        finish = FindObjectOfType<FinishTrigger>();

        Canvas canvas = FindObjectOfType<Canvas>();
        treasuresText = canvas.transform
            .Find("SmallPlayerUI/Treasures")
            .GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (finish == null) return;

        treasuresText.text =
            $"Treasures: {finish.treasuresDelivered}/{finish.totalTreasures}";
    }
}
