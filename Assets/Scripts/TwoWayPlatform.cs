using UnityEngine;
using System.Collections;

public class TwoWayPlatform : MonoBehaviour
{
    private PlatformEffector2D effector;
    private float dropDelay = 0.5f;
    private bool isPlayerDropping = false;
    private int playerLayer;
    private int passThroughLayer;

    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
        if (effector == null)
        {
            Debug.LogError("TwoWayPlatform requires a PlatformEffector2D component!");
            return;
        }

        playerLayer = LayerMask.NameToLayer("Player");
        passThroughLayer = LayerMask.NameToLayer("PassThrough");
        if (playerLayer == -1 || passThroughLayer == -1)
        {
            Debug.LogError("Ensure 'Player' and 'PassThrough' layers are defined in the Layer settings!");
            return;
        }

        effector.useOneWay = true;
        effector.useSideFriction = false;
        effector.useSideBounce = false;

        // Allow both Player and Enemies layers to interact with the platform
        effector.colliderMask = LayerMask.GetMask("Player", "Enemies");
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !isPlayerDropping)
        {
            StartCoroutine(AllowPlayerDrop());
        }
    }

    IEnumerator AllowPlayerDrop()
    {
        isPlayerDropping = true;
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.gameObject.layer = passThroughLayer; // Switch to pass-through layer
            Debug.Log("Player dropping through platform!");
        }

        yield return new WaitForSeconds(dropDelay);

        if (player != null)
        {
            player.gameObject.layer = playerLayer; // Restore original layer
        }
        isPlayerDropping = false;
    }
}