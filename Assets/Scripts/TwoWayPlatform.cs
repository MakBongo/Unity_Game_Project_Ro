using UnityEngine;

public class TwoWayPlatform : MonoBehaviour
{
    private PlatformEffector2D effector;

    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
        if (effector == null)
        {
            Debug.LogError("TwoWayPlatform: PlatformEffector2D component missing!");
            return;
        }

        // Configure one-way platform
        effector.useOneWay = true;
        effector.useSideFriction = false;
        effector.useSideBounce = false;

        // Allow collisions with Player and Enemies
        effector.colliderMask = LayerMask.GetMask("Player", "Enemies");
    }
}