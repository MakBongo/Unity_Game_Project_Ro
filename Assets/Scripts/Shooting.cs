using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shooting : MonoBehaviour
{
    [Header("Set Shooting")]
    public Transform firePoint;
    public float firePointRadius = 0.5f;

    [Header("Weapon Data")]
    public WeaponData weaponData; // Inspector or GameData
    private WeaponData runtimeData;
    private WeaponData baseData; // Stores base stats

    [Header("Rotation Transition")]
    public float rotationTransitionTime = 0.1f;
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
    public float ammunitionSpeedUpgrade = 0.1f;
    public float firesPerMinuteUpgrade = 0.1f;
    public float ammunitionLifetimeUpgrade = 0.1f;
    public float magazineSizeUpgrade = 0.1f;
    public float reloadTimeUpgrade = 0.1f;
    private float ammunitionSpeedMultiplier = 0f;
    private float firesPerMinuteMultiplier = 0f;
    private float ammunitionLifetimeMultiplier = 0f;
    private float magazineSizeMultiplier = 0f;
    private float reloadTimeMultiplier = 1f;

    [Header("Player Reference")]
    public PlayerController player;

    [Header("Visual and Audio Components")]
    private SpriteRenderer weaponSpriteRenderer;
    private AudioSource fireAudioSource;

    void Start()
    {
        if (weaponData == null)
        {
            weaponData = GameData.GetSelectedWeapon();
            if (weaponData == null)
            {
                Debug.LogError("Shooting: No WeaponData assigned or found in GameData!");
                return;
            }
            Debug.Log($"Shooting: Using weapon {weaponData.name} from GameData");
        }

        runtimeData = Instantiate(weaponData);
        baseData = Instantiate(weaponData);
        ammunitionPool = new Queue<GameObject>();
        ApplyShopUpgrades();
        fireRate = 60f / runtimeData.firesPerMinute;
        CalculatePoolSize();
        InitializeAmmunitionPool();
        currentAmmo = runtimeData.magazineSize;

        if (player == null)
        {
            player = GetComponentInParent<PlayerController>();
            if (player == null)
            {
                Debug.LogError("Shooting: PlayerController not assigned or found in parent!");
            }
        }

        if (firePoint == null)
        {
            Debug.LogError("Shooting: FirePoint not assigned!");
        }

        // Initialize SpriteRenderer
        weaponSpriteRenderer = GetComponent<SpriteRenderer>();
        if (weaponSpriteRenderer == null)
        {
            Debug.LogError("Shooting: SpriteRenderer component not found on this GameObject!");
        }
        else if (runtimeData.weaponSprite != null)
        {
            weaponSpriteRenderer.sprite = runtimeData.weaponSprite;
        }
        else
        {
            Debug.LogWarning("Shooting: No weapon sprite assigned in WeaponData!");
        }

        // Initialize AudioSource
        fireAudioSource = GetComponent<AudioSource>();
        if (fireAudioSource == null)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
        }
        if (runtimeData.fireSound != null)
        {
            fireAudioSource.clip = runtimeData.fireSound;
            fireAudioSource.playOnAwake = false;
        }
        else
        {
            Debug.LogWarning("Shooting: No fire sound assigned in WeaponData!");
        }

        lastRotation = transform.rotation;
    }

    void ApplyShopUpgrades()
    {
        // Load SaveData from SaveGameManager
        SaveData saveData = SaveGameManager.Instance.GetSaveData();

        // Initialize default multipliers if not set
        if (saveData.ammunitionSpeedMultiplier == 0f) saveData.ammunitionSpeedMultiplier = 1f;
        if (saveData.firesPerMinuteMultiplier == 0f) saveData.firesPerMinuteMultiplier = 1f;
        if (saveData.ammunitionLifetimeMultiplier == 0f) saveData.ammunitionLifetimeMultiplier = 1f;
        if (saveData.magazineSizeMultiplier == 0f) saveData.magazineSizeMultiplier = 1f;
        if (saveData.reloadTimeMultiplier == 0f) saveData.reloadTimeMultiplier = 1f;

        // Total Value = WeaponData * ShopSystem Upgrade * Random Upgrade Options
        runtimeData.ammunitionDamage = Mathf.RoundToInt(runtimeData.ammunitionDamage * saveData.ammunitionSpeedMultiplier * (1f + ammunitionSpeedMultiplier));
        runtimeData.ammunitionSpeed = runtimeData.ammunitionSpeed * saveData.ammunitionSpeedMultiplier * (1f + ammunitionSpeedMultiplier);
        runtimeData.firesPerMinute = runtimeData.firesPerMinute * saveData.firesPerMinuteMultiplier * (1f + firesPerMinuteMultiplier);
        runtimeData.ammunitionLifetime = runtimeData.ammunitionLifetime * saveData.ammunitionLifetimeMultiplier * (1f + ammunitionLifetimeMultiplier);
        runtimeData.magazineSize = Mathf.RoundToInt(runtimeData.magazineSize * saveData.magazineSizeMultiplier * (1f + magazineSizeMultiplier));
        runtimeData.reloadTime = runtimeData.reloadTime * saveData.reloadTimeMultiplier * reloadTimeMultiplier;
        runtimeData.reloadTime = Mathf.Max(0.1f, runtimeData.reloadTime);

        Debug.Log($"Shooting: Applied shop upgrades. Damage: {runtimeData.ammunitionDamage}, Speed: {runtimeData.ammunitionSpeed:F2}, FireRate: {runtimeData.firesPerMinute:F2}");
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
            return;
        }

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
        if (ammunitionPool.Count == 0)
        {
            InitializeAmmunitionPool();
        }

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
            Debug.LogWarning("Shooting: Ammunition prefab missing Ammunition component!");
        }

        Rigidbody2D ammunitionRB = ammunition.GetComponent<Rigidbody2D>();
        if (ammunitionRB != null)
        {
            Vector2 direction = transform.right;
            ammunitionRB.velocity = direction * runtimeData.ammunitionSpeed;
        }
        else
        {
            Debug.LogWarning("Shooting: Ammunition prefab missing Rigidbody2D!");
        }

        // Play firing sound
        if (fireAudioSource != null && runtimeData.fireSound != null)
        {
            fireAudioSource.PlayOneShot(runtimeData.fireSound);
        }

        currentAmmo--;
        StartCoroutine(ReturnAmmunitionToPool(ammunition));
    }

    IEnumerator ReturnAmmunitionToPool(GameObject ammunition)
    {
        float elapsed = 0f;
        while (elapsed < runtimeData.ammunitionLifetime)
        {
            if (Time.timeScale > 0f)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
        if (ammunition != null && ammunition.activeSelf)
        {
            ammunition.SetActive(false);
            ammunitionPool.Enqueue(ammunition);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Shooting: Reloading...");
        float elapsed = 0f;
        while (elapsed < runtimeData.reloadTime)
        {
            if (Time.timeScale > 0f)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
        CalculatePoolSize();
        AdjustPoolSize();
        currentAmmo = runtimeData.magazineSize;
        isReloading = false;
        Debug.Log("Shooting: Reload complete!");
    }

    public void UpgradeAmmunitionDamage()
    {
        runtimeData.ammunitionDamage += 2;
        Debug.Log($"Shooting: Upgraded Ammunition Damage to {runtimeData.ammunitionDamage}");
    }

    public void UpgradeAmmunitionSpeed()
    {
        ammunitionSpeedMultiplier += ammunitionSpeedUpgrade;
        runtimeData.ammunitionSpeed *= 1.1f;
        Debug.Log($"Shooting: Upgraded Ammunition Speed to {runtimeData.ammunitionSpeed:F2} (Multiplier: {ammunitionSpeedMultiplier:F2})");
    }

    public void UpgradeFiresPerMinute()
    {
        firesPerMinuteMultiplier += firesPerMinuteUpgrade;
        runtimeData.firesPerMinute *= 1.1f;
        fireRate = 60f / runtimeData.firesPerMinute;
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Shooting: Upgraded Fires Per Minute to {runtimeData.firesPerMinute:F2} (Multiplier: {firesPerMinuteMultiplier:F2})");
    }

    public void UpgradeAmmunitionLifetime()
    {
        ammunitionLifetimeMultiplier += ammunitionLifetimeUpgrade;
        runtimeData.ammunitionLifetime *= 1.1f;
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Shooting: Upgraded Ammunition Lifetime to {runtimeData.ammunitionLifetime:F2} (Multiplier: {ammunitionLifetimeMultiplier:F2})");
    }

    public void UpgradeMagazineSize()
    {
        magazineSizeMultiplier += magazineSizeUpgrade;
        runtimeData.magazineSize = Mathf.RoundToInt(runtimeData.magazineSize * 1.1f);
        CalculatePoolSize();
        AdjustPoolSize();
        Debug.Log($"Shooting: Upgraded Magazine Size to {runtimeData.magazineSize} (Multiplier: {magazineSizeMultiplier:F2})");
    }

    public void UpgradeReloadTime()
    {
        reloadTimeMultiplier -= reloadTimeUpgrade;
        runtimeData.reloadTime *= 0.9f;
        runtimeData.reloadTime = Mathf.Max(0.1f, runtimeData.reloadTime);
        Debug.Log($"Shooting: Upgraded Reload Time to {runtimeData.reloadTime:F2} (Multiplier: {reloadTimeMultiplier:F2})");
    }

    public int GetCurrentAmmo() { return currentAmmo; }
    public bool IsReloading() { return isReloading; }

    public int GetAmmunitionDamage() { return runtimeData.ammunitionDamage; }
    public float GetAmmunitionSpeed() { return runtimeData.ammunitionSpeed; }
    public float GetFiresPerMinute() { return runtimeData.firesPerMinute; }
    public float GetAmmunitionLifetime() { return runtimeData.ammunitionLifetime; }
    public int GetMagazineSize() { return runtimeData.magazineSize; }
    public float GetReloadTime() { return runtimeData.reloadTime; }

    // Getters for base stats
    public float GetBaseAmmunitionSpeed() { return baseData.ammunitionSpeed; }
    public float GetBaseFiresPerMinute() { return baseData.firesPerMinute; }
    public float GetBaseAmmunitionLifetime() { return baseData.ammunitionLifetime; }
    public int GetBaseMagazineSize() { return baseData.magazineSize; }
    public float GetBaseReloadTime() { return baseData.reloadTime; }

    // Getters for multipliers
    public float GetAmmunitionSpeedMultiplier() { return ammunitionSpeedMultiplier; }
    public float GetFiresPerMinuteMultiplier() { return firesPerMinuteMultiplier; }
    public float GetAmmunitionLifetimeMultiplier() { return ammunitionLifetimeMultiplier; }
    public float GetMagazineSizeMultiplier() { return magazineSizeMultiplier; }
    public float GetReloadTimeMultiplier() { return reloadTimeMultiplier; }
}