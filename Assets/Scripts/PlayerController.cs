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
    public Transform GroundCheck;
    public float CheckRadius;
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

    [Header("Set Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float firePointRadius = 0.5f;
    public Transform gunObject;

    [Header("Gun Data")]
    public int bulletDamage = 10;
    public float bulletSpeed = 20f;
    public float firesPerMinute = 300f;
    public float bulletLifetime = 2f;
    public int magazineSize = 30;
    public float reloadTime = 2f;
    private int poolSize;
    private float nextFireTime = 0f;
    private Queue<GameObject> bulletPool;
    private int currentAmmo;
    private bool isReloading = false;
    private float fireRate;

    [Header("Upgrade Multipliers")]
    private float bulletSpeedUpgrade = 1.1f;
    private float firesPerMinuteUpgrade = 1.1f;
    private float bulletLifetimeUpgrade = 1.1f;
    private float magazineSizeUpgrade = 1.1f;
    private float reloadTimeUpgrade = 0.9f;
    private float healRateUpgrade = 1.1f;

    void Start()
    {
        PlayerRB = GetComponent<Rigidbody2D>();
        bulletPool = new Queue<GameObject>();
        fireRate = 60f / firesPerMinute;
        CalculatePoolSize();
        InitializeBulletPool();
        currentAmmo = magazineSize;
        currentHealth = maxHealth;

        if (gunObject == null)
        {
            Debug.LogWarning("Gun Object not assigned in Inspector!");
        }
    }

    void CalculatePoolSize()
    {
        fireRate = 60f / firesPerMinute;
        float shotsPerSecond = 1f / fireRate;
        int calculatedPoolSize = Mathf.CeilToInt(shotsPerSecond * bulletLifetime) + 5;
        poolSize = Mathf.Max(calculatedPoolSize, magazineSize);
    }

    void InitializeBulletPool()
    {
        while (bulletPool.Count < poolSize)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    void AdjustPoolSize()
    {
        while (bulletPool.Count > poolSize)
        {
            GameObject bullet = bulletPool.Dequeue();
            if (!bullet.activeSelf)
            {
                Destroy(bullet);
            }
            else
            {
                bulletPool.Enqueue(bullet);
                break;
            }
        }
        InitializeBulletPool();
    }

    void Update()
    {
        if (!isReloading)
        {
            if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextFireTime && currentAmmo > 0)
            {
                Shoot();
                fireRate = 60f / firesPerMinute;
                nextFireTime = Time.time + fireRate;
            }
            else if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }
        }

        UpdateGunRotation(); // This now handles flipping
        UpdateFirePoint();

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

        // Removed movement-based flipping logic from here

        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, CheckRadius, WhatIsGround);
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

            // Flip character based on shooting direction
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

    void UpdateFirePoint()
    {
        if (gunObject != null && firePoint != null)
        {
            Vector2 direction = gunObject.right;
            firePoint.position = gunObject.position + (Vector3)(direction * firePointRadius);
            firePoint.rotation = gunObject.rotation;
        }
    }

    void Shoot()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;
            bullet.SetActive(true);

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = bulletDamage;
                bulletScript.player = this;
            }

            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            Vector2 bulletDirection = firePoint.right;
            bulletRB.velocity = bulletDirection * bulletSpeed;

            currentAmmo--;
            StartCoroutine(ReturnBulletToPool(bullet));
        }
    }

    IEnumerator ReturnBulletToPool(GameObject bullet)
    {
        yield return new WaitForSeconds(bulletLifetime);
        if (bullet != null)
        {
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        CalculatePoolSize();
        AdjustPoolSize();
        currentAmmo = magazineSize;
        isReloading = false;
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
        currentExp += exp;
        Debug.Log($"Gained {exp} EXP. Current EXP: {currentExp}/{maxExp}");
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

    public void UpgradeBulletDamage()
    {
        bulletDamage += 2;
        Debug.Log($"Upgraded Bullet Damage to {bulletDamage}");
    }

    public void UpgradeMoveSpeed()
    {
        moveSpeed += 1f;
        Debug.Log($"Upgraded Move Speed to {moveSpeed}");
    }

    public void UpgradeBulletSpeed()
    {
        bulletSpeed *= bulletSpeedUpgrade;
        Debug.Log($"Upgraded Bullet Speed to {bulletSpeed:F2}");
    }

    public void UpgradeFiresPerMinute()
    {
        firesPerMinute *= firesPerMinuteUpgrade;
        fireRate = 60f / firesPerMinute;
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Upgraded Fires Per Minute to {firesPerMinute:F2}");
    }

    public void UpgradeBulletLifetime()
    {
        bulletLifetime *= bulletLifetimeUpgrade;
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Upgraded Bullet Lifetime to {bulletLifetime:F2}");
    }

    public void UpgradeMagazineSize()
    {
        magazineSize = Mathf.RoundToInt(magazineSize * magazineSizeUpgrade);
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Upgraded Magazine Size to {magazineSize}");
    }

    public void UpgradeReloadTime()
    {
        reloadTime *= reloadTimeUpgrade;
        Debug.Log($"Upgraded Reload Time to {reloadTime:F2}");
    }

    public void UpgradeHealRate()
    {
        healRate *= healRateUpgrade;
        Debug.Log($"Upgraded Heal Rate to {healRate:F4}");
    }

    public int GetCurrentHealth() { return Mathf.RoundToInt(currentHealth); }
    public int GetMaxHealth() { return maxHealth; }
    public int GetCurrentAmmo() { return currentAmmo; }
    public bool IsReloading() { return isReloading; }
    public int GetCurrentExp() { return currentExp; }
    public int GetMaxExp() { return maxExp; }
    public int GetLevel() { return level; }
}