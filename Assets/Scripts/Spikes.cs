using UnityEngine;

public class Spikes : MonoBehaviour
{
    [Header("Spike Settings")]
    public int damage = 10;           // Damage dealt to player or enemy
    public float damageInterval = 0.5f; // Time between damage ticks
    public float knockbackForce = 5f;  // Force of the knockback effect
    public float knockbackDuration = 0.2f; // Duration of knockback (brief impulse)
    private float nextDamageTime;     // Tracks when next damage can occur

    private CompositeCollider2D compositeCollider;

    void Start()
    {
        nextDamageTime = Time.time;
        compositeCollider = GetComponent<CompositeCollider2D>();
        if (compositeCollider == null)
        {
            Debug.LogError("Spikes script requires a CompositeCollider2D component!");
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time >= nextDamageTime)
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            Vector2 knockbackDirection = CalculateKnockbackDirection(collision);

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                if (rb != null)
                {
                    ApplyKnockback(rb, knockbackDirection);
                }
                Debug.Log($"Spikes dealt {damage} damage to Player with knockback.");
                nextDamageTime = Time.time + damageInterval;
                return;
            }

            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                if (rb != null)
                {
                    ApplyKnockback(rb, knockbackDirection);
                }
                Debug.Log($"Spikes dealt {damage} damage to Enemy with knockback.");
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }

    Vector2 CalculateKnockbackDirection(Collision2D collision)
    {
        // Use the contact point from the collision
        ContactPoint2D contact = collision.GetContact(0); // Get first contact point
        Vector2 contactPoint = contact.point;
        // Direction away from the contact point
        Vector2 direction = (collision.transform.position - (Vector3)contactPoint).normalized;
        return direction;
    }

    void ApplyKnockback(Rigidbody2D rb, Vector2 direction)
    {
        // Apply an impulse force for immediate knockback
        Vector2 knockbackVelocity = direction * knockbackForce;
        rb.velocity = new Vector2(rb.velocity.x, 0f); // Reset vertical velocity for consistency
        rb.AddForce(knockbackVelocity, ForceMode2D.Impulse);
    }
}