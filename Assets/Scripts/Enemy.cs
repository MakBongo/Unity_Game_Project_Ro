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
    public float rayLength = 0.2f; // How far down to cast rays
    public float rayOffset = 0.4f; // Distance from center to edge rays
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

            Vector2 originLeft = new Vector2(transform.position.x - rayOffset, transform.position.y);
            Vector2 originRight = new Vector2(transform.position.x + rayOffset, transform.position.y);
            RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, Vector2.down, rayLength, whatIsGround);
            RaycastHit2D hitRight = Physics2D.Raycast(originRight, Vector2.down, rayLength, whatIsGround);
            isGrounded = hitLeft.collider != null || hitRight.collider != null;

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

    void OnDrawGizmos()
    {
        Vector2 originLeft = new Vector2(transform.position.x - rayOffset, transform.position.y);
        Vector2 originRight = new Vector2(transform.position.x + rayOffset, transform.position.y);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(originLeft, originLeft + Vector2.down * rayLength);
        Gizmos.DrawLine(originRight, originRight + Vector2.down * rayLength);
    }
}