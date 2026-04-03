using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameStartCountdown : MonoBehaviourPun
{
    [Header("UI")]
    public TextMeshProUGUI countdownText;

    [Header("Timing")]
    public float numberDuration = 1f; // duration per number (3,2,1)
    public float goDuration = 1f;     // GO duration

    private double startTime;
    private bool countdownStarted = false;

    public static bool CanMove = false; // 🔑 GLOBAL movement lock

    void Start()
    {
        CanMove = false;
        if (PhotonNetwork.IsMasterClient)
        {
            double networkTime = PhotonNetwork.Time + 1; // small delay
            photonView.RPC(nameof(RPC_StartCountdown), RpcTarget.All, networkTime);
        }
    }

    [PunRPC]
    void RPC_StartCountdown(double networkStartTime)
    {
        startTime = networkStartTime;
        countdownStarted = true;
        CanMove = false;
    }

    void Update()
    {
        if (!countdownStarted) return;

        double elapsed = PhotonNetwork.Time - startTime;

        float totalNumbersTime = numberDuration * 3;

        if (elapsed < numberDuration)
        {
            countdownText.text = "3";
        }
        else if (elapsed < numberDuration * 2)
        {
            countdownText.text = "2";
        }
        else if (elapsed < numberDuration * 3)
        {
            countdownText.text = "1";
        }
        else if (elapsed < totalNumbersTime + goDuration)
        {
            countdownText.text = "GO!";
            CanMove = true; // ✅ unlock movement
        }
        else
        {
            countdownText.gameObject.SetActive(false);
            countdownStarted = false;
        }
    }
}
