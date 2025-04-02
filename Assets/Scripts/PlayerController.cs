using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Set Movement")]
    public float moveSpeed = 5;
    private float inputX;
    private Rigidbody2D PlayerRB;
    private bool faceRight = true;

    [Header("Set Jump")]
    public float JumpForce = 10;
    private bool canJump = true;

    [Header("Set GroundCheck")]
    public float rayLength = 0.2f; // How far down to cast rays
    public float rayOffset = 0.4f; // Distance from center to edge rays
    public LayerMask WhatIsGround;
    private bool isGrounded;

    [Header("Set Health")]
    public int maxHealth = 100;
    private float currentHealth;

    [Header("Healing")]
    public float healRate = 0.001f;
    private float healTimer = 0f;
    private float healInterval = 1f;

    [Header("Set Experience")]
    public int currentExp = 0;
    public int maxExp = 100;
    public int level = 1;
    public float expMultiplier = 1f; // Multiplier for EXP gain

    [Header("Set Shooting References")]
    public Transform gunObject; // Kept for rotation and flipping
    public Shooting shooting;   // Reference to the Shooting script

    void Start()
    {
        PlayerRB = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (gunObject == null)
        {
            Debug.LogWarning("Gun Object not assigned in Inspector!");
        }

        shooting = GetComponent<Shooting>();
        if (shooting == null)
        {
            Debug.LogWarning("Shooting script not found on PlayerController!");
        }
    }

    void Update()
    {
        UpdateGunRotation();

        if (currentHealth <= 0)
        {
            Die();
        }

        HealOverTime();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canJump)
        {
            PlayerRB.velocity = new Vector2(PlayerRB.velocity.x, JumpForce);
            canJump = false;
        }
    }

    void FixedUpdate()
    {
        inputX = Input.GetAxis("Horizontal");
        PlayerRB.velocity = new Vector2(inputX * moveSpeed, PlayerRB.velocity.y);

        bool wasGrounded = isGrounded;
        Vector2 originLeft = new Vector2(transform.position.x - rayOffset, transform.position.y);
        Vector2 originRight = new Vector2(transform.position.x + rayOffset, transform.position.y);
        RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, Vector2.down, rayLength, WhatIsGround);
        RaycastHit2D hitRight = Physics2D.Raycast(originRight, Vector2.down, rayLength, WhatIsGround);
        isGrounded = hitLeft.collider != null || hitRight.collider != null;

        if (!wasGrounded && isGrounded)
        {
            canJump = true;
        }
    }

    void Flip()
    {
        faceRight = !faceRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    void UpdateGunRotation()
    {
        if (gunObject != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Vector2 direction = (mousePos - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gunObject.rotation = Quaternion.Euler(0f, 0f, angle);

            if (direction.x > 0 && !faceRight)
            {
                Flip();
            }
            else if (direction.x < 0 && faceRight)
            {
                Flip();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");
    }

    void Die()
    {
        Debug.Log("Player died!");
        gameObject.SetActive(false);
    }

    public void AddExp(int exp)
    {
        int scaledExp = Mathf.RoundToInt(exp * expMultiplier);
        currentExp += scaledExp;
        Debug.Log($"Gained {scaledExp} EXP (Base: {exp}, Multiplier: {expMultiplier:F2}). Current EXP: {currentExp}/{maxExp}");
        while (currentExp >= maxExp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        currentExp -= maxExp;
        maxExp = Mathf.RoundToInt(maxExp * 1.5f);
        CanvasController canvas = FindObjectOfType<CanvasController>();
        if (canvas != null)
        {
            canvas.QueuePanel("PlayerLevelUp");
        }
        Debug.Log($"Leveled up to {level}! New max EXP: {maxExp}");
    }

    void HealOverTime()
    {
        if (currentHealth < maxHealth)
        {
            healTimer += Time.deltaTime;
            if (healTimer >= healInterval)
            {
                float healAmount = maxHealth * healRate;
                currentHealth += healAmount;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                healTimer = 0f;
                Debug.Log($"Player healed by {healAmount:F2}. Current health: {currentHealth}");
            }
        }
    }

    public void UpgradeMaxHealth()
    {
        maxHealth += 10;
        Debug.Log($"Upgraded Max Health to {maxHealth}");
    }

    public void UpgradeMoveSpeed()
    {
        moveSpeed += 1f;
        Debug.Log($"Upgraded Move Speed to {moveSpeed}");
    }

    public void UpgradeHealRate()
    {
        healRate *= 1.1f; // Using a direct multiplier here since upgrade fields are moved
        Debug.Log($"Upgraded Heal Rate to {healRate:F4}");
    }

    public void UpgradeExpAmount()
    {
        expMultiplier *= 1.1f; // Using a direct multiplier here
        Debug.Log($"Upgraded EXP Multiplier to {expMultiplier:F2}");
    }

    public int GetCurrentHealth() { return Mathf.RoundToInt(currentHealth); }
    public int GetMaxHealth() { return maxHealth; }
    public int GetCurrentExp() { return currentExp; }
    public int GetMaxExp() { return maxExp; }
    public int GetLevel() { return level; }

    void OnDrawGizmos()
    {
        Vector2 originLeft = new Vector2(transform.position.x - rayOffset, transform.position.y);
        Vector2 originRight = new Vector2(transform.position.x + rayOffset, transform.position.y);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(originLeft, originLeft + Vector2.down * rayLength);
        Gizmos.DrawLine(originRight, originRight + Vector2.down * rayLength);
    }
}