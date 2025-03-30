using UnityEngine;
using System.Collections;

public class TwoWayPlatform : MonoBehaviour
{
    private PlatformEffector2D effector;
    private Collider2D platformCollider; // Store reference to the collider (CompositeCollider2D)
    private float originalRotationalOffset;
    private bool isPlayerDropping = false;

    [Header("Settings")]
    public float dropDelay = 0.2f;

    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
        if (effector == null)
        {
            Debug.LogError("TwoWayPlatform requires a PlatformEffector2D component!");
            return;
        }

        // Get the CompositeCollider2D if present, otherwise fall back to any Collider2D
        platformCollider = GetComponent<CompositeCollider2D>();
        if (platformCollider == null)
        {
            platformCollider = GetComponent<Collider2D>();
            if (platformCollider == null)
            {
                Debug.LogError("TwoWayPlatform requires a Collider2D (e.g., CompositeCollider2D or TilemapCollider2D)!");
                return;
            }
        }

        originalRotationalOffset = effector.rotationalOffset;
        effector.useOneWay = true;
        effector.useSideFriction = false;
        effector.useSideBounce = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            StartCoroutine(AllowDropThrough());
        }
    }

    IEnumerator AllowDropThrough()
    {
        effector.rotationalOffset = 180f;
        isPlayerDropping = true;

        yield return new WaitForSeconds(dropDelay);

        effector.rotationalOffset = originalRotationalOffset;
        isPlayerDropping = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null && isPlayerDropping)
        {
            Physics2D.IgnoreCollision(collision.collider, platformCollider, true);
            Invoke("ResetCollision", dropDelay);
        }
    }

    void ResetCollision()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), platformCollider, false);
        }
    }
}