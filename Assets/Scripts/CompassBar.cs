using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassBar : MonoBehaviour
{
    public GameObject treasure;  // Assign the treasure GameObject in the Inspector
    public GameObject startFinish;
    private bool hasCollectedTreasure = false;

    void Update()
    {
        // Check if the treasure is collected
        if (!hasCollectedTreasure)
        {
            // Get the direction to the treasure
            Vector3 directionToTreasure = treasure.transform.position - transform.position;

            // Ignore changes in the y-axis by zeroing it out
            directionToTreasure.y = 0;

            // Check if the direction is not zero (to avoid NaN)
            if (directionToTreasure != Vector3.zero)
            {
                // Calculate the target rotation towards the treasure
                Quaternion targetRotation = Quaternion.LookRotation(directionToTreasure);

                // Set the rotation instantly without any latency
                transform.rotation = targetRotation;
            }
        }
        else
        {
            // Get the direction to the startFinish once the treasure is collected
            Vector3 directionToFinish = startFinish.transform.position - transform.position;

            // Ignore changes in the y-axis by zeroing it out
            directionToFinish.y = 0;

            // Check if the direction is not zero (to avoid NaN)
            if (directionToFinish != Vector3.zero)
            {
                // Calculate the target rotation towards the startFinish
                Quaternion targetRotation = Quaternion.LookRotation(directionToFinish);

                // Set the rotation instantly without any latency
                transform.rotation = targetRotation;
            }
        }
    }
    public void CollectTreasure()
    {
        hasCollectedTreasure = true;
    }
}
