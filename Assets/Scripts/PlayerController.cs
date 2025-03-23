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

    [Header("Set GroundCheck")]
    public Transform GroundCheck;
    public float CheckRadius;
    public LayerMask WhatIsGround;
    private bool isGrounded;

    [Header("Set Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireRate = 0.2f;
    public float bulletLifetime = 2f;
    public int magazineSize = 30;          // Maximum rounds in magazine
    public float reloadTime = 2f;          // Time to reload in seconds
    private int poolSize;                  // Now calculated dynamically
    private float nextFireTime = 0f;
    private Queue<GameObject> bulletPool;
    private int currentAmmo;              // Current rounds in magazine
    private bool isReloading = false;     // Reload state

    void Start()
    {
        PlayerRB = GetComponent<Rigidbody2D>();
        CalculatePoolSize();              // Calculate pool size before initializing
        InitializeBulletPool();
        currentAmmo = magazineSize;       // Start with full magazine
    }

    void CalculatePoolSize()
    {
        // Calculate max bullets in flight: shots per second * lifetime, plus a buffer
        float shotsPerSecond = 1f / fireRate;
        int calculatedPoolSize = Mathf.CeilToInt(shotsPerSecond * bulletLifetime) + 5; // Buffer of 5
        poolSize = Mathf.Max(calculatedPoolSize, magazineSize); // Ensure it¡¦s at least magazineSize
    }

    void InitializeBulletPool()
    {
        bulletPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    void Update()
    {
        if (!isReloading)
        {
            if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextFireTime && currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            else if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }
        }
        firePoint.rotation = Quaternion.Euler(0f, 0f, faceRight ? 0f : 180f);
    }

    void FixedUpdate()
    {
        inputX = Input.GetAxis("Horizontal");
        PlayerRB.velocity = new Vector2(inputX * moveSpeed, PlayerRB.velocity.y);
        if (faceRight == false && inputX > 0)
        {
            Flip();
        }
        else if (faceRight == true && inputX < 0)
        {
            Flip();
        }
        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, CheckRadius, WhatIsGround);
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            PlayerRB.velocity = Vector2.up * JumpForce;
        }
    }

    void Flip()
    {
        faceRight = !faceRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    void Shoot()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;
            bullet.SetActive(true);
            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            float direction = faceRight ? 1f : -1f;
            bulletRB.velocity = new Vector2(direction * bulletSpeed, 0f);
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
        currentAmmo = magazineSize;
        isReloading = false;
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}