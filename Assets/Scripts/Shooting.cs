using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [Header("Set Shooting")]
    public Transform firePoint;
    public float firePointRadius = 0.5f;

    [Header("Weapon Data")]
    public WeaponData weaponData; // Can be assigned in Inspector or set via GameData
    private WeaponData runtimeData;

    [Header("Rotation Transition")]
    public float rotationTransitionTime = 0.5f;
    private Quaternion lastRotation;
    private float transitionProgress = 1f;
    private bool wasPausedLastFrame = false;

    private int poolSize;
    private float nextFireTime = 0f;
    private Queue<GameObject> ammunitionPool;
    private int currentAmmo;
    private bool isReloading = false;
    private float fireRate;
    private bool faceRight = true;

    [Header("Upgrade Multipliers")]
    public float ammunitionSpeedUpgrade = 1.1f;
    public float firesPerMinuteUpgrade = 1.1f;
    public float ammunitionLifetimeUpgrade = 1.1f;
    public float magazineSizeUpgrade = 1.1f;
    public float reloadTimeUpgrade = 0.9f;

    [Header("Player Reference")]
    public PlayerController player;

    void Start()
    {
        // Use GameData if weaponData is not assigned
        if (weaponData == null)
        {
            weaponData = GameData.GetSelectedWeapon();
            if (weaponData == null)
            {
                Debug.LogError("Shooting: No WeaponData assigned and none found in GameData!");
                return;
            }
            Debug.Log($"Shooting: Using weapon {weaponData.name} from GameData");
        }

        runtimeData = Instantiate(weaponData);

        ammunitionPool = new Queue<GameObject>();
        fireRate = 60f / runtimeData.firesPerMinute;
        CalculatePoolSize();
        InitializeAmmunitionPool();
        currentAmmo = runtimeData.magazineSize;

        if (player == null)
        {
            player = GetComponentInParent<PlayerController>();
            if (player == null)
            {
                Debug.LogError("PlayerController not assigned and not found in parent!");
            }
        }

        lastRotation = transform.rotation;
    }

    void Update()
    {
        bool isPaused = Time.timeScale == 0f;

        if (isPaused)
        {
            if (!wasPausedLastFrame)
            {
                lastRotation = transform.rotation;
            }
            wasPausedLastFrame = true;
        }
        else
        {
            if (wasPausedLastFrame)
            {
                transitionProgress = 0f;
                wasPausedLastFrame = false;
            }

            if (transitionProgress < 1f)
            {
                transitionProgress += Time.deltaTime / rotationTransitionTime;
                transitionProgress = Mathf.Clamp01(transitionProgress);

                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;
                Vector2 direction = (mousePos - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

                transform.rotation = Quaternion.Lerp(lastRotation, targetRotation, transitionProgress);

                if (direction.x > 0 && !faceRight)
                {
                    Flip();
                }
                else if (direction.x < 0 && faceRight)
                {
                    Flip();
                }
            }
            else
            {
                UpdateGunRotation();
            }

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
        }

        UpdateFirePoint();
    }

    void CalculatePoolSize()
    {
        fireRate = 60f / runtimeData.firesPerMinute;
        float shotsPerSecond = 1f / fireRate;
        int calculatedPoolSize = Mathf.CeilToInt(shotsPerSecond * runtimeData.ammunitionLifetime) + 5;
        poolSize = Mathf.Max(calculatedPoolSize, runtimeData.magazineSize);
    }

    void InitializeAmmunitionPool()
    {
        while (ammunitionPool.Count < poolSize)
        {
            GameObject ammunition = Instantiate(runtimeData.ammunitionPrefab);
            ammunition.SetActive(false);
            ammunitionPool.Enqueue(ammunition);
        }
    }

    void AdjustPoolSize()
    {
        while (ammunitionPool.Count > poolSize)
        {
            GameObject ammunition = ammunitionPool.Dequeue();
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
        InitializeAmmunitionPool();
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
            GameObject ammunition = ammunitionPool.Dequeue();
            ammunition.transform.position = firePoint.position;
            ammunition.transform.rotation = firePoint.rotation;
            ammunition.SetActive(true);

            Ammunition ammunitionScript = ammunition.GetComponent<Ammunition>();
            if (ammunitionScript != null)
            {
                ammunitionScript.damage = runtimeData.ammunitionDamage;
                ammunitionScript.player = player;

                Grenade grenadeScript = ammunitionScript as Grenade;
                if (grenadeScript != null)
                {
                    grenadeScript.SetExplosionDelay(runtimeData.ammunitionLifetime);
                }
            }
            else
            {
                Debug.LogWarning("Ammunition prefab is missing an Ammunition component!");
            }

            Rigidbody2D ammunitionRB = ammunition.GetComponent<Rigidbody2D>();
            Vector2 ammunitionDirection = transform.right;
            ammunitionRB.velocity = ammunitionDirection * runtimeData.ammunitionSpeed;

            currentAmmo--;
            StartCoroutine(ReturnAmmunitionToPool(ammunition));
        }
    }

    IEnumerator ReturnAmmunitionToPool(GameObject ammunition)
    {
        yield return new WaitForSeconds(runtimeData.ammunitionLifetime);
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

    public void UpgradeAmmunitionDamage()
    {
        runtimeData.ammunitionDamage += 2;
        Debug.Log($"Upgraded Ammunition Damage to {runtimeData.ammunitionDamage}");
    }

    public void UpgradeAmmunitionSpeed()
    {
        runtimeData.ammunitionSpeed *= ammunitionSpeedUpgrade;
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

    public void UpgradeAmmunitionLifetime()
    {
        runtimeData.ammunitionLifetime *= ammunitionLifetimeUpgrade;
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

    public int GetCurrentAmmo() { return currentAmmo; }
    public bool IsReloading() { return isReloading; }

    public int GetAmmunitionDamage() { return runtimeData.ammunitionDamage; }
    public float GetAmmunitionSpeed() { return runtimeData.ammunitionSpeed; }
    public float GetFiresPerMinute() { return runtimeData.firesPerMinute; }
    public float GetAmmunitionLifetime() { return runtimeData.ammunitionLifetime; }
    public int GetMagazineSize() { return runtimeData.magazineSize; }
    public float GetReloadTime() { return runtimeData.reloadTime; }
}