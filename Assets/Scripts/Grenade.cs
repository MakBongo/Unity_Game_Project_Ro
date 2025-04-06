using UnityEngine;

public class Grenade : MonoBehaviour
{
    public int damage; // Damage dealt on explosion, set by Shooting script
    public PlayerController player; // Reference to player, set by Shooting script (optional)
    public float explosionRadius = 2f; // Radius of the explosion
    public float explosionDelay = 2f; // Time before explosion
    public GameObject explosionEffectPrefab; // Optional: Visual effect for explosion

    private Rigidbody2D rb;
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Grenade prefab requires a Rigidbody2D component!");
        }
    }

    void OnEnable()
    {
        // Reset state when activated from pool
        hasExploded = false;
        Invoke(nameof(Explode), explosionDelay);
    }

    void OnDisable()
    {
        // Cancel explosion if deactivated (e.g., returned to pool early)
        CancelInvoke(nameof(Explode));
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Deal damage to nearby enemies
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead()) // Check if it's an enemy and not already dead
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Grenade dealt {damage} damage to {hit.name}");
            }
        }

        // Spawn explosion effect if provided
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Deactivate the grenade (returned to pool by Shooting script)
        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Optional: Explode on impact instead of delay
        // Uncomment the line below if you want impact-based explosions
        // Explode();
    }

    // Optional: Visualize explosion radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}