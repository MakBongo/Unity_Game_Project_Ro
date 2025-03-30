using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed;       // Set by SceneManager
    public int maxHealth;         // Set by SceneManager
    public int damage;            // Set by SceneManager
    public int expValue;          // Set by SceneManager (base value from SceneManager)
    private int currentHealth;
    private Transform player;
    private Rigidbody2D enemyRB;  // Added for physics-based movement and jumping

    [Header("Jump Settings")]
    public float jumpForce = 8f;  // Jump height (less than player¡¦s 10 by default)
    public Transform groundCheck; // Position to check if grounded
    public float checkRadius = 0.1f; // Radius for ground detection
    public LayerMask whatIsGround; // Layer for ground detection
    private bool isGrounded;      // Track if enemy is on ground
    public float jumpCooldown = 2f; // Time between jumps
    private float nextJumpTime;   // Time when next jump is allowed

    void Start()
    {
        player = FindObjectOfType<PlayerController>().transform;
        enemyRB = GetComponent<Rigidbody2D>();
        if (enemyRB == null)
        {
            Debug.LogError("Enemy requires a Rigidbody2D component!");
            enemyRB = gameObject.AddComponent<Rigidbody2D>();
            enemyRB.bodyType = RigidbodyType2D.Dynamic;
            enemyRB.freezeRotation = true; // Prevent tipping
        }
        nextJumpTime = Time.time; // Allow jumping immediately
    }

    public void Initialize()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player != null)
        {
            // Horizontal movement toward player
            Vector2 direction = (player.position - transform.position).normalized;
            enemyRB.velocity = new Vector2(direction.x * moveSpeed, enemyRB.velocity.y);

            // Jump logic
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
            if (isGrounded && Time.time >= nextJumpTime && ShouldJump())
            {
                Jump();
                nextJumpTime = Time.time + jumpCooldown; // Set cooldown
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
        // Jump if player is above or at a certain distance
        if (player != null)
        {
            float verticalDistance = player.position.y - transform.position.y;
            float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
            // Jump if player is above and within a reasonable horizontal range
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
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.AddExp(expValue); // Award EXP to player on death
        }
        Destroy(gameObject);
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}