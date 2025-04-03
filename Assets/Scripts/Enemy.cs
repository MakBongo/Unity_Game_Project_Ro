using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float moveSpeed;
    public int maxHealth;
    public int damage;
    public int expValue;
    private int currentHealth;
    private Transform player;
    private Rigidbody2D enemyRB;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float rayLength = 0.2f;
    public float rayOffset = 0.4f;
    public LayerMask whatIsGround; // Ensure this includes the platform layer (e.g., "Default")
    private bool isGrounded;
    public float jumpCooldown = 2f;
    private float nextJumpTime;

    [Header("Drop Settings")]
    public float dropDelay = 0.5f;
    private int enemyLayer;
    private int passThroughLayer;
    private bool isDropping = false;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Drop Item List")]
    [SerializeField] // Allows editing in Inspector
    private DropItem[] dropItems; // Array of possible drops

    [System.Serializable] // Makes this struct editable in Inspector
    public struct DropItem
    {
        public GameObject prefab; // The object to drop
        [Range(0f, 1f)] // Drop rate between 0 and 1 (0% to 100%)
        public float dropRate;
    }

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

        enemyLayer = LayerMask.NameToLayer("Enemies");
        passThroughLayer = LayerMask.NameToLayer("PassThrough");
        if (enemyLayer == -1 || passThroughLayer == -1)
        {
            Debug.LogError("Ensure 'Enemies' and 'PassThrough' layers are defined in Layer settings!");
            return;
        }
        gameObject.layer = enemyLayer;
    }

    public void Initialize()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player != null && !isDropping)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            enemyRB.velocity = new Vector2(direction.x * moveSpeed, enemyRB.velocity.y);

            Vector2 originLeft = new Vector2(transform.position.x - rayOffset, transform.position.y);
            Vector2 originRight = new Vector2(transform.position.x + rayOffset, transform.position.y);
            RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, Vector2.down, rayLength, whatIsGround);
            RaycastHit2D hitRight = Physics2D.Raycast(originRight, Vector2.down, rayLength, whatIsGround);
            isGrounded = hitLeft.collider != null || hitRight.collider != null;

            if (isGrounded && Time.time >= nextJumpTime)
            {
                if (ShouldJump())
                {
                    Jump();
                    nextJumpTime = Time.time + jumpCooldown;
                }
                else if (ShouldDrop())
                {
                    StartCoroutine(DropThroughPlatform());
                }
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

    bool ShouldDrop()
    {
        if (player != null)
        {
            float verticalDistance = player.position.y - transform.position.y;
            float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
            return verticalDistance < -1f && horizontalDistance < 5f && isGrounded;
        }
        return false;
    }

    IEnumerator DropThroughPlatform()
    {
        isDropping = true;
        gameObject.layer = passThroughLayer;
        Debug.Log("Enemy dropping through platform!");

        yield return new WaitForSeconds(dropDelay);

        gameObject.layer = enemyLayer;
        isDropping = false;
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
            TryDropItem(); // Attempt to drop an item
        }
        Destroy(gameObject);
    }

    void TryDropItem()
    {
        if (dropItems == null || dropItems.Length == 0)
        {
            Debug.Log("No drop items defined for this enemy.");
            return;
        }

        foreach (DropItem item in dropItems)
        {
            if (item.prefab != null && Random.value <= item.dropRate)
            {
                GameObject droppedItem = Instantiate(item.prefab, transform.position, Quaternion.identity);
                Debug.Log($"Enemy dropped {item.prefab.name} with drop rate {item.dropRate * 100}%!");
                // Only drop one item per enemy death (remove this break if you want multiple possible drops)
                break;
            }
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
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