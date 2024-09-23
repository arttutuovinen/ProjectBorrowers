using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassBar : MonoBehaviour
{
    public GameObject treasure;  // Assign the treasure GameObject in the Inspector

    void Update()
    {
        if (treasure != null)
        {
            // Get the direction to the treasure
            Vector3 directionToTreasure = treasure.transform.position - transform.position;

            // Ignore changes in the y-axis by zeroing it out
            directionToTreasure.y = 0;

            // Check if the direction is not zero (to avoid NaN)
            if (directionToTreasure != Vector3.zero)
            {
                // Calculate the target rotation
                Quaternion targetRotation = Quaternion.LookRotation(directionToTreasure);

                // Set the rotation instantly without any latency
                transform.rotation = targetRotation;
            }
        }
    }
}
