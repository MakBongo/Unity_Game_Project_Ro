using UnityEngine;

public class Grenade : Ammunition
{
    public float explosionRadius = 2f;
    public float explosionDelay = 2f;
    public GameObject explosionEffectPrefab;

    private Rigidbody2D rb;
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Grenade prefab requires a Rigidbody2D component!"); // Fixed missing parenthesis
        }
    }

    void OnEnable()
    {
        hasExploded = false;
        Invoke(nameof(Explode), explosionDelay);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Explode));
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(damage);
            }
        }

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Optional: Explode on impact
        // Explode();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}