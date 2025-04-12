using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Set Movement")]
    public float moveSpeed = 5;
    private float inputX;
    private Rigidbody2D PlayerRB;

    [Header("Set Jump")]
    public float JumpForce = 10;
    private bool canJump = true;

    [Header("Set GroundCheck")]
    public float rayLength = 0.2f;
    public float rayOffset = 0.4f;
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
    public int maxExp = 50;
    public int level = 1;
    private int highestLevel = 1;

    [Header("Set Shooting Reference")]
    public Shooting shooting;

    [Header("Set Money")]
    public int money = 0;
    public float moneyMultiplier = 1f;

    [Header("Set Experience Multiplier")]
    public float expMultiplier = 1f;

    [Header("Set Platform Pass-Through")]
    public float dropDelay = 0.5f;
    private bool isDropping = false;
    private int playerLayer;
    private int passThroughLayer;

    void Awake()
    {
        PlayerRB = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (shooting == null)
        {
            Debug.LogWarning("Shooting script not assigned in PlayerController! Attempting to find it...");
            shooting = GetComponentInChildren<Shooting>();
            if (shooting == null)
            {
                Debug.LogError("Could not find Shooting script in children of PlayerController!");
            }
            else
            {
                Debug.Log("Shooting script found in children!");
            }
        }
    }

    void Start()
    {
        // Cache layer IDs
        playerLayer = LayerMask.NameToLayer("Player");
        passThroughLayer = LayerMask.NameToLayer("PassThrough");
        if (playerLayer == -1 || passThroughLayer == -1)
        {
            Debug.LogError("PlayerController: Ensure 'Player' and 'PassThrough' layers are defined in Layer settings!");
        }
    }

    void Update()
    {
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

        // Check for platform drop, only when not paused
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !isDropping && Time.timeScale > 0f)
        {
            StartCoroutine(DropThroughPlatform());
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

    private IEnumerator DropThroughPlatform()
    {
        isDropping = true;
        gameObject.layer = passThroughLayer; // Switch to pass-through layer
        Debug.Log("PlayerController: Dropping through platform");

        yield return new WaitForSeconds(dropDelay);

        gameObject.layer = playerLayer; // Restore original layer
        isDropping = false;
        Debug.Log("PlayerController: Restored player layer");
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

        if (level % 5 == 0)
        {
            maxExp = Mathf.RoundToInt(maxExp * 1.5f);
            Debug.Log($"Level {level} reached! Added 50% to max EXP. New max EXP: {maxExp}");
        }

        CanvasController canvas = FindObjectOfType<CanvasController>();
        if (canvas != null)
        {
            canvas.QueuePanel("PlayerLevelUp");
        }

        if (level > highestLevel)
        {
            highestLevel = level;
            Debug.Log($"New level record set! Highest Level: {highestLevel}");
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

    public void AddMoney(int amount)
    {
        int scaledMoney = Mathf.RoundToInt(amount * moneyMultiplier);
        money += scaledMoney;
        Debug.Log($"Added {scaledMoney} money (Base: {amount}, Multiplier: {moneyMultiplier:F2}). Total money: {money}");
    }

    public int GetMoney()
    {
        return money;
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
        healRate *= 1.1f;
        Debug.Log($"Upgraded Heal Rate to {healRate:F4}");
    }

    public void UpgradeExpAmount()
    {
        expMultiplier *= 1.1f;
        Debug.Log($"Upgraded EXP Multiplier to {expMultiplier:F2}");
    }

    public void UpgradeMoneyAmount()
    {
        moneyMultiplier *= 1.1f;
        Debug.Log($"Upgraded Money Multiplier to {moneyMultiplier:F2}");
    }

    public int GetCurrentHealth() { return Mathf.RoundToInt(currentHealth); }
    public int GetMaxHealth() { return maxHealth; }
    public void SetCurrentHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
    }
    public int GetCurrentExp() { return currentExp; }
    public int GetMaxExp() { return maxExp; }
    public int GetLevel() { return level; }
    public int GetHighestLevel()
    {
        return highestLevel;
    }
    public void SetHighestLevel(int value)
    {
        highestLevel = value;
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