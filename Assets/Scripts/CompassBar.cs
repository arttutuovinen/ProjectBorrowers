using UnityEngine;
using Photon.Pun;

public class CompassBar : MonoBehaviourPun
{
    private GameObject treasure;

    void Start()
    {
        treasure = GameObject.FindWithTag("Treasure");
    }

    void Update()
    {
        if (treasure == null || !treasure.activeInHierarchy)
            return;

        Vector3 direction = treasure.transform.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}



