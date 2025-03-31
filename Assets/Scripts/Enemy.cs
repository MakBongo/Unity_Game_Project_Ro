using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed;       // Set by SceneManager
    public int maxHealth;         // Set by SceneManager
    public int damage;            // Set by SceneManager
    public int expValue;          // Set by SceneManager (base value from SceneManager)
    private int currentHealth;
    private Transform player;
    private Rigidbody2D enemyRB;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public Transform groundCheck;
    public float checkRadius = 0.1f;
    public LayerMask whatIsGround;
    private bool isGrounded;
    public float jumpCooldown = 2f;
    private float nextJumpTime;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    void Start()
    {
        player = FindObjectOfType<PlayerController>().transform;
        enemyRB = GetComponent<Rigidbody2D>();
        if (enemyRB == null)
        {
            Debug.LogError("Enemy requires a Rigidbody2D component!");
            enemyRB = gameObject.AddComponent<Rigidbody2D>();
            enemyRB.bodyType = RigidbodyType2D.Dynamic;
            enemyRB.freezeRotation = true;
        }
        nextJumpTime = Time.time;
    }

    public void Initialize()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            enemyRB.velocity = new Vector2(direction.x * moveSpeed, enemyRB.velocity.y);

            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
            if (isGrounded && Time.time >= nextJumpTime && ShouldJump())
            {
                Jump();
                nextJumpTime = Time.time + jumpCooldown;
            }
        }
    }

    void Jump()
    {
        enemyRB.velocity = new Vector2(enemyRB.velocity.x, jumpForce);
        Debug.Log("Enemy jumped!");
    }

    bool ShouldJump()
    {
        if (player != null)
        {
            float verticalDistance = player.position.y - transform.position.y;
            float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
            return verticalDistance > 1f && horizontalDistance < 5f;
        }
        return false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);

            Rigidbody2D playerRB = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRB != null)
            {
                Vector2 knockbackDirection = CalculateKnockbackDirection(collision);
                ApplyKnockback(playerRB, knockbackDirection);
                Debug.Log($"Enemy dealt {damage} damage to Player with knockback.");
            }
        }
    }

    Vector2 CalculateKnockbackDirection(Collision2D collision)
    {
        Vector2 direction = (collision.transform.position - transform.position).normalized;
        return direction;
    }

    void ApplyKnockback(Rigidbody2D rb, Vector2 direction)
    {
        // Apply impulse force in full 2D direction (horizontal and vertical)
        Vector2 knockbackVelocity = direction * knockbackForce;
        rb.AddForce(knockbackVelocity, ForceMode2D.Impulse);
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
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.AddExp(expValue);
        }
        Destroy(gameObject);
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}