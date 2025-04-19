using UnityEngine;
using System.Collections;

public class TrackingMissile : Ammunition
{
    [Header("Explosion Settings")]
    public float explosionRadius = 2f; // Matches Grenade
    public GameObject explosionEffectPrefab; // Same as Grenade

    [Header("Tracking Settings")]
    public float trackingStrength = 0.5f; // Turn speed (0-1)
    public float detectionRadius = 3f; // Only track enemies within 3 units

    private Rigidbody2D rb;
    private GameObject target;
    private bool hasExploded = false;
    private Coroutine explosionCoroutine;
    private float explosionDelay;
    private bool isPaused;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("TrackingMissile: Rigidbody2D component missing!");
        }
    }

    void OnEnable()
    {
        hasExploded = false;
        isPaused = false;
        FindTarget();
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
            if (Time.timeScale > 0f)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
        Explode();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (!isPaused && rb != null)
            {
                rb.velocity = Vector2.zero; // Pause movement
                isPaused = true;
            }
            return;
        }
        if (isPaused && rb != null)
        {
            // Resume velocity
            Vector2 direction = target != null ?
                (target.transform.position - transform.position).normalized :
                rb.velocity.normalized;
            rb.velocity = direction * (rb.velocity.magnitude > 0 ? rb.velocity.magnitude : 8f);
            isPaused = false;
        }

        if (target == null || !target.activeInHierarchy)
        {
            FindTarget();
        }
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f || rb == null || hasExploded) return;

        if (target != null)
        {
            // Verify target is still in range
            if (Vector2.Distance(transform.position, target.transform.position) > detectionRadius)
            {
                target = null;
                return;
            }

            Vector2 directionToTarget = (target.transform.position - transform.position).normalized;
            Vector2 currentDirection = rb.velocity.normalized;
            Vector2 newDirection = Vector2.Lerp(currentDirection, directionToTarget,
                trackingStrength * Time.fixedDeltaTime).normalized;
            rb.velocity = newDirection * rb.velocity.magnitude;
            float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        // If no target, maintain velocity (no change needed, rb.velocity persists)
    }

    private void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemies");
        float closestDistance = detectionRadius; // Limit to detectionRadius
        GameObject closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        target = closestEnemy;
        Debug.Log(target != null ?
            $"TrackingMissile: Targeting {target.name} at distance {closestDistance:F2}" :
            $"TrackingMissile: No target within {detectionRadius} units");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == player?.gameObject) return; // Ignore player
        Explode();
    }

    private void Explode()
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
                Debug.Log($"TrackingMissile: Dealt {damage} damage to {enemy.name}");
            }
        }

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        gameObject.SetActive(false);
    }

    void OnDrawGizmos()
    {
        // Explosion radius
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, explosionRadius);

        // Detection radius
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, detectionRadius);
    }

    void OnDrawGizmosSelected()
    {
        // Explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        // Detection radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}