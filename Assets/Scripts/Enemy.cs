using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed;       // Set by SceneManager
    public int maxHealth;         // Set by SceneManager
    public int damage;            // Set by SceneManager
    public int expValue;          // Set by SceneManager (base value from SceneManager)
    private int currentHealth;
    private Transform player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>().transform;
    }

    public void Initialize()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Move towards player
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"Enemy took {damage} damage. Current health: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}