using UnityEngine;
using Photon.Pun;

public class CompassBar : MonoBehaviourPun
{
    private GameObject treasure;
    private GameObject finish;

    void Start()
    {
        // Find treasure and finish once at start
        treasure = GameObject.FindWithTag("Treasure");
        finish = GameObject.FindWithTag("Finnish");
    }

    void Update()
    {
        GameObject target = GetCurrentTarget();

        if (target == null)
            return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// Returns the treasure if it's still active; otherwise returns Finish.
    /// </summary>
    private GameObject GetCurrentTarget()
    {
        if (treasure != null && treasure.activeInHierarchy)
        {
            return treasure;
        }

        return finish;  // Switch to Finish when treasure is collected/deactivated
    }

    // Call this when the treasure is collected (from the treasure script)
    [PunRPC]
    public void OnTreasureCollected()
    {
        if (treasure != null)
        {
            treasure.SetActive(false);
        }
    }
    [PunRPC]
    public void OnTreasureRespawned()
    {
        if (treasure != null)
            treasure.SetActive(true);
    }
}



