using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [Header("Set Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float firePointRadius = 0.5f;

    [Header("Weapon Data")]
    public WeaponData weaponData; // Template ScriptableObject
    private WeaponData runtimeData; // Runtime copy

    private int poolSize;
    private float nextFireTime = 0f;
    private Queue<GameObject> bulletPool;
    private int currentAmmo;
    private bool isReloading = false;
    private float fireRate;
    private bool faceRight = true;

    [Header("Upgrade Multipliers")]
    public float bulletSpeedUpgrade = 1.1f;
    public float firesPerMinuteUpgrade = 1.1f;
    public float bulletLifetimeUpgrade = 1.1f;
    public float magazineSizeUpgrade = 1.1f;
    public float reloadTimeUpgrade = 0.9f;

    [Header("Player Reference")]
    public PlayerController player;

    void Start()
    {
        if (weaponData == null)
        {
            Debug.LogError("WeaponData not assigned in Shooting script!");
            return;
        }

        runtimeData = Instantiate(weaponData);

        bulletPool = new Queue<GameObject>();
        fireRate = 60f / runtimeData.firesPerMinute;
        CalculatePoolSize();
        InitializeBulletPool();
        currentAmmo = runtimeData.magazineSize;

        if (player == null)
        {
            player = GetComponentInParent<PlayerController>();
            if (player == null)
            {
                Debug.LogError("PlayerController not assigned and not found in parent!");
            }
        }
    }

    void Update()
    {
        UpdateGunRotation();

        if (!isReloading)
        {
            if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextFireTime && currentAmmo > 0)
            {
                Shoot();
                fireRate = 60f / runtimeData.firesPerMinute;
                nextFireTime = Time.time + fireRate;
            }
            else if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }

            if (Input.GetKeyDown(KeyCode.R) && currentAmmo < runtimeData.magazineSize)
            {
                StartCoroutine(Reload());
            }
        }

        UpdateFirePoint();
    }

    void CalculatePoolSize()
    {
        fireRate = 60f / runtimeData.firesPerMinute;
        float shotsPerSecond = 1f / fireRate;
        int calculatedPoolSize = Mathf.CeilToInt(shotsPerSecond * runtimeData.bulletLifetime) + 5;
        poolSize = Mathf.Max(calculatedPoolSize, runtimeData.magazineSize);
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

    void UpdateGunRotation()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (direction.x > 0 && !faceRight)
        {
            Flip();
        }
        else if (direction.x < 0 && faceRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        faceRight = !faceRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void UpdateFirePoint()
    {
        if (firePoint != null)
        {
            Vector2 direction = transform.right;
            firePoint.position = transform.position + (Vector3)(direction * firePointRadius);
            firePoint.rotation = transform.rotation;
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
                bulletScript.damage = runtimeData.bulletDamage;
                bulletScript.player = player;
            }

            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            Vector2 bulletDirection = transform.right;
            bulletRB.velocity = bulletDirection * runtimeData.bulletSpeed;

            currentAmmo--;
            StartCoroutine(ReturnBulletToPool(bullet));
        }
    }

    IEnumerator ReturnBulletToPool(GameObject bullet)
    {
        yield return new WaitForSeconds(runtimeData.bulletLifetime);
        if (bullet != null)
        {
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(runtimeData.reloadTime);
        CalculatePoolSize();
        AdjustPoolSize();
        currentAmmo = runtimeData.magazineSize;
        isReloading = false;
        Debug.Log("Reload complete!");
    }

    public void UpgradeBulletDamage()
    {
        runtimeData.bulletDamage += 2;
        Debug.Log($"Upgraded Bullet Damage to {runtimeData.bulletDamage}");
    }

    public void UpgradeBulletSpeed()
    {
        runtimeData.bulletSpeed *= bulletSpeedUpgrade;
        Debug.Log($"Upgraded Bullet Speed to {runtimeData.bulletSpeed:F2}");
    }

    public void UpgradeFiresPerMinute()
    {
        runtimeData.firesPerMinute *= firesPerMinuteUpgrade;
        fireRate = 60f / runtimeData.firesPerMinute;
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Upgraded Fires Per Minute to {runtimeData.firesPerMinute:F2}");
    }

    public void UpgradeBulletLifetime()
    {
        runtimeData.bulletLifetime *= bulletLifetimeUpgrade;
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Upgraded Bullet Lifetime to {runtimeData.bulletLifetime:F2}");
    }

    public void UpgradeMagazineSize()
    {
        runtimeData.magazineSize = Mathf.RoundToInt(runtimeData.magazineSize * magazineSizeUpgrade);
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Upgraded Magazine Size to {runtimeData.magazineSize}");
    }

    public void UpgradeReloadTime()
    {
        runtimeData.reloadTime *= reloadTimeUpgrade;
        Debug.Log($"Upgraded Reload Time to {runtimeData.reloadTime:F2}");
    }

    // Existing getters
    public int GetCurrentAmmo() { return currentAmmo; }
    public bool IsReloading() { return isReloading; }

    // New getters for WeaponData fields
    public int GetBulletDamage() { return runtimeData.bulletDamage; }
    public float GetBulletSpeed() { return runtimeData.bulletSpeed; }
    public float GetFiresPerMinute() { return runtimeData.firesPerMinute; }
    public float GetBulletLifetime() { return runtimeData.bulletLifetime; }
    public int GetMagazineSize() { return runtimeData.magazineSize; }
    public float GetReloadTime() { return runtimeData.reloadTime; }
}