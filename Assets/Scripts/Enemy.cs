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

    private enum EnemyMode { Patrol, Pursuit }
    [Header("Behavior Settings")]
    private EnemyMode currentMode = EnemyMode.Patrol;
    public float detectionRange = 10f;
    private float timeSincePlayerLeftRange;
    private float patrolSwitchDelay = 15f;

    [Header("Patrol Settings")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    private Vector2 targetPatrolPoint;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float rayLength = 0.2f;
    public float rayOffset = 0.4f;
    public LayerMask whatIsGround;
    private bool isGrounded;
    public float jumpCooldown = 2f;
    private float nextJumpTime;
    public float jumpHorizontalRange = 10f; // New: Increased range for jumping

    [Header("Drop Settings")]
    public float dropDelay = 0.5f;
    private int enemyLayer;
    private int passThroughLayer;
    private bool isDropping = false;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Drop Item List")]
    [SerializeField]
    private DropItem[] dropItems;

    [System.Serializable]
    public struct DropItem
    {
        public GameObject prefab;
        [Range(0f, 1f)]
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

        if (patrolPointA == null || patrolPointB == null)
        {
            Debug.LogError("Patrol points A and B must be assigned in the Inspector!");
            return;
        }
        targetPatrolPoint = patrolPointA.position;
        timeSincePlayerLeftRange = patrolSwitchDelay;
    }

    public void Initialize()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            currentMode = EnemyMode.Pursuit;
            timeSincePlayerLeftRange = 0f;
        }
        else
        {
            timeSincePlayerLeftRange += Time.deltaTime;
            if (timeSincePlayerLeftRange >= patrolSwitchDelay)
            {
                currentMode = EnemyMode.Patrol;
            }
        }

        Debug.Log($"Mode: {currentMode}, Time since player left: {timeSincePlayerLeftRange:F2}");

        switch (currentMode)
        {
            case EnemyMode.Patrol:
                Patrol();
                break;
            case EnemyMode.Pursuit:
                PursuePlayer();
                break;
        }
    }

    void Patrol()
    {
        if (!isDropping)
        {
            Vector2 direction = (targetPatrolPoint - (Vector2)transform.position).normalized;
            enemyRB.velocity = new Vector2(direction.x * moveSpeed, enemyRB.velocity.y);

            UpdateGroundedState();

            if (isGrounded && Time.time >= nextJumpTime)
            {
                if (ShouldJump(targetPatrolPoint))
                {
                    Jump();
                    nextJumpTime = Time.time + jumpCooldown;
                }
                else if (ShouldDrop(targetPatrolPoint))
                {
                    StartCoroutine(DropThroughPlatform());
                }
            }

            float distanceToTarget = Vector2.Distance(transform.position, targetPatrolPoint);
            if (distanceToTarget < 0.5f)
            {
                if (Vector2.Distance(transform.position, patrolPointA.position) < 0.5f)
                {
                    targetPatrolPoint = patrolPointB.position;
                    Debug.Log("Switching to Patrol Point B");
                }
                else if (Vector2.Distance(transform.position, patrolPointB.position) < 0.5f)
                {
                    targetPatrolPoint = patrolPointA.position;
                    Debug.Log("Switching to Patrol Point A");
                }
            }
        }
    }

    void PursuePlayer()
    {
        if (!isDropping)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            enemyRB.velocity = new Vector2(direction.x * moveSpeed, enemyRB.velocity.y);

            UpdateGroundedState();

            if (isGrounded && Time.time >= nextJumpTime)
            {
                if (ShouldJump(player.position))
                {
                    Jump();
                    nextJumpTime = Time.time + jumpCooldown;
                }
                else if (ShouldDrop(player.position))
                {
                    StartCoroutine(DropThroughPlatform());
                }
            }
        }
    }

    void UpdateGroundedState()
    {
        Vector2 originLeft = new Vector2(transform.position.x - rayOffset, transform.position.y);
        Vector2 originRight = new Vector2(transform.position.x + rayOffset, transform.position.y);
        RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, Vector2.down, rayLength, whatIsGround);
        RaycastHit2D hitRight = Physics2D.Raycast(originRight, Vector2.down, rayLength, whatIsGround);
        isGrounded = hitLeft.collider != null || hitRight.collider != null;
    }

    void Jump()
    {
        enemyRB.velocity = new Vector2(enemyRB.velocity.x, jumpForce);
        Debug.Log("Enemy jumped!");
    }

    bool ShouldJump(Vector2 targetPosition)
    {
        float verticalDistance = targetPosition.y - transform.position.y;
        float horizontalDistance = Mathf.Abs(targetPosition.x - transform.position.x);
        return verticalDistance > 1f && horizontalDistance < jumpHorizontalRange; // Updated range
    }

    bool ShouldDrop(Vector2 targetPosition)
    {
        float verticalDistance = targetPosition.y - transform.position.y;
        float horizontalDistance = Mathf.Abs(targetPosition.x - transform.position.x);
        return verticalDistance < -1f && horizontalDistance < 5f && isGrounded;
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
            TryDropItem();
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

        Gizmos.color = Color.green;
        if (patrolPointA != null)
            Gizmos.DrawSphere(patrolPointA.position, 0.2f);
        if (patrolPointB != null)
            Gizmos.DrawSphere(patrolPointB.position, 0.2f);
    }
}