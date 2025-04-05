using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [Header("Set Shooting")]
    public GameObject ammunitionPrefab; // Renamed from bulletPrefab
    public Transform firePoint;
    public float firePointRadius = 0.5f;

    [Header("Weapon Data")]
    public WeaponData weaponData; // Template ScriptableObject
    private WeaponData runtimeData; // Runtime copy

    private int poolSize;
    private float nextFireTime = 0f;
    private Queue<GameObject> ammunitionPool; // Renamed from bulletPool
    private int currentAmmo;
    private bool isReloading = false;
    private float fireRate;
    private bool faceRight = true;

    [Header("Upgrade Multipliers")]
    public float ammunitionSpeedUpgrade = 1.1f; // Renamed from bulletSpeedUpgrade
    public float firesPerMinuteUpgrade = 1.1f;
    public float ammunitionLifetimeUpgrade = 1.1f; // Renamed from bulletLifetimeUpgrade
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

        ammunitionPool = new Queue<GameObject>(); // Renamed from bulletPool
        fireRate = 60f / runtimeData.firesPerMinute;
        CalculatePoolSize();
        InitializeAmmunitionPool(); // Renamed from InitializeBulletPool
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
        int calculatedPoolSize = Mathf.CeilToInt(shotsPerSecond * runtimeData.ammunitionLifetime) + 5; // Renamed bulletLifetime
        poolSize = Mathf.Max(calculatedPoolSize, runtimeData.magazineSize);
    }

    void InitializeAmmunitionPool() // Renamed from InitializeBulletPool
    {
        while (ammunitionPool.Count < poolSize)
        {
            GameObject ammunition = Instantiate(ammunitionPrefab); // Renamed from bullet
            ammunition.SetActive(false);
            ammunitionPool.Enqueue(ammunition);
        }
    }

    void AdjustPoolSize()
    {
        while (ammunitionPool.Count > poolSize)
        {
            GameObject ammunition = ammunitionPool.Dequeue(); // Renamed from bullet
            if (!ammunition.activeSelf)
            {
                Destroy(ammunition);
            }
            else
            {
                ammunitionPool.Enqueue(ammunition);
                break;
            }
        }
        InitializeAmmunitionPool(); // Renamed from InitializeBulletPool
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
        if (ammunitionPool.Count > 0)
        {
            GameObject ammunition = ammunitionPool.Dequeue(); // Renamed from bullet
            ammunition.transform.position = firePoint.position;
            ammunition.transform.rotation = firePoint.rotation;
            ammunition.SetActive(true);

            Bullet ammunitionScript = ammunition.GetComponent<Bullet>(); // Renamed from bulletScript (assuming Bullet class exists)
            if (ammunitionScript != null)
            {
                ammunitionScript.damage = runtimeData.ammunitionDamage; // Renamed from bulletDamage
                ammunitionScript.player = player;
            }

            Rigidbody2D ammunitionRB = ammunition.GetComponent<Rigidbody2D>(); // Renamed from bulletRB
            Vector2 ammunitionDirection = transform.right; // Renamed from bulletDirection
            ammunitionRB.velocity = ammunitionDirection * runtimeData.ammunitionSpeed; // Renamed from bulletSpeed

            currentAmmo--;
            StartCoroutine(ReturnAmmunitionToPool(ammunition)); // Renamed from ReturnBulletToPool
        }
    }

    IEnumerator ReturnAmmunitionToPool(GameObject ammunition) // Renamed from ReturnBulletToPool
    {
        yield return new WaitForSeconds(runtimeData.ammunitionLifetime); // Renamed from bulletLifetime
        if (ammunition != null)
        {
            ammunition.SetActive(false);
            ammunitionPool.Enqueue(ammunition);
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

    public void UpgradeAmmunitionDamage() // Renamed from UpgradeBulletDamage
    {
        runtimeData.ammunitionDamage += 2; // Renamed from bulletDamage
        Debug.Log($"Upgraded Ammunition Damage to {runtimeData.ammunitionDamage}");
    }

    public void UpgradeAmmunitionSpeed() // Renamed from UpgradeBulletSpeed
    {
        runtimeData.ammunitionSpeed *= ammunitionSpeedUpgrade; // Renamed from bulletSpeed
        Debug.Log($"Upgraded Ammunition Speed to {runtimeData.ammunitionSpeed:F2}");
    }

    public void UpgradeFiresPerMinute()
    {
        runtimeData.firesPerMinute *= firesPerMinuteUpgrade;
        fireRate = 60f / runtimeData.firesPerMinute;
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Upgraded Fires Per Minute to {runtimeData.firesPerMinute:F2}");
    }

    public void UpgradeAmmunitionLifetime() // Renamed from UpgradeBulletLifetime
    {
        runtimeData.ammunitionLifetime *= ammunitionLifetimeUpgrade; // Renamed from bulletLifetime
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Upgraded Ammunition Lifetime to {runtimeData.ammunitionLifetime:F2}");
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

    // New getters for WeaponData fields (renamed)
    public int GetAmmunitionDamage() { return runtimeData.ammunitionDamage; } // Renamed from GetBulletDamage
    public float GetAmmunitionSpeed() { return runtimeData.ammunitionSpeed; } // Renamed from GetBulletSpeed
    public float GetFiresPerMinute() { return runtimeData.firesPerMinute; }
    public float GetAmmunitionLifetime() { return runtimeData.ammunitionLifetime; } // Renamed from GetBulletLifetime
    public int GetMagazineSize() { return runtimeData.magazineSize; }
    public float GetReloadTime() { return runtimeData.reloadTime; }
}