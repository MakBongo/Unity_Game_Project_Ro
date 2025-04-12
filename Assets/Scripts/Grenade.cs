using UnityEngine;
using System.Collections;

public class Grenade : Ammunition
{
    [Header("Explosion Settings")]
    public float explosionRadius = 2f;
    private float explosionDelay; // Set by Shooting
    public GameObject explosionEffectPrefab;

    private Rigidbody2D rb;
    private bool hasExploded = false;
    private Coroutine explosionCoroutine;

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
        hasExploded = false;
        // Explosion is started via SetExplosionDelay
    }

    void OnDisable()
    {
        if (explosionCoroutine != null)
        {
            StopCoroutine(explosionCoroutine);
            explosionCoroutine = null;
        }
    }

    public void SetExplosionDelay(float delay)
    {
        explosionDelay = delay;
        Debug.Log($"Grenade: SetExplosionDelay called with delay = {delay}");
        if (explosionCoroutine != null)
        {
            StopCoroutine(explosionCoroutine);
        }
        explosionCoroutine = StartCoroutine(ExplosionTimer());
    }

    private IEnumerator ExplosionTimer()
    {
        float elapsed = 0f;
        while (elapsed < explosionDelay)
        {
            // Only increment timer when game is not paused
            if (Time.timeScale > 0f)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
        Explode();
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
        // Optional: Explode on impact (commented out as per original)
        // Explode();
    }

    void OnDrawGizmos()
    {
        // Draw gizmo even when not selected for better visibility
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, explosionRadius); // Faint fill for clarity
    }

    void OnDrawGizmosSelected()
    {
        // Emphasize when selected
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}