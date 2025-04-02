using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [Header("Set Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float firePointRadius = 0.5f;

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
    private bool faceRight = true;

    [Header("Upgrade Multipliers")]
    public float bulletSpeedUpgrade = 1.1f;
    public float firesPerMinuteUpgrade = 1.1f;
    public float bulletLifetimeUpgrade = 1.1f;
    public float magazineSizeUpgrade = 1.1f;
    public float reloadTimeUpgrade = 0.9f;

    [Header("Player Reference")]
    public PlayerController player; // Reference to PlayerController on the parent

    void Start()
    {
        bulletPool = new Queue<GameObject>();
        fireRate = 60f / firesPerMinute;
        CalculatePoolSize();
        InitializeBulletPool();
        currentAmmo = magazineSize;

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
                fireRate = 60f / firesPerMinute;
                nextFireTime = Time.time + fireRate;
            }
            else if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }
        }

        UpdateFirePoint();
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
            Vector2 direction = transform.right; // Use this transform's right direction
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
                bulletScript.damage = bulletDamage;
                bulletScript.player = player; // Use the player reference
            }

            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            Vector2 bulletDirection = transform.right; // Use this transform's right direction
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

    public void UpgradeBulletDamage()
    {
        bulletDamage += 2;
        Debug.Log($"Upgraded Bullet Damage to {bulletDamage}");
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

    public int GetCurrentAmmo() { return currentAmmo; }
    public bool IsReloading() { return isReloading; }
}