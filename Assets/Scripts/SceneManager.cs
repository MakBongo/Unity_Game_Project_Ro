using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    [Header("Level Prefabs")]
    public GameObject[] tileMapPrefabs;

    [Header("Enemy Settings")]
    public float baseEnemyMoveSpeed = 2f;
    public int baseEnemyHealth = 50;
    public int baseEnemyDamage = 10;
    public int baseExpValue = 20;
    public GameObject enemyPrefab;
    public int enemiesPerLevel = 5;

    private int currentLevel = 1;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private GameObject currentTileMap;
    private PlayerController player;
    private bool levelCompleted = false;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        GenerateLevel();
    }

    void Update()
    {
        if (!levelCompleted && activeEnemies.Count > 0 && activeEnemies.TrueForAll(e => e == null || e.IsDead()))
        {
            LevelCompleted();
            levelCompleted = true;
        }
    }

    void GenerateLevel()
    {
        if (currentTileMap != null)
        {
            Destroy(currentTileMap);
        }

        int randomIndex = Random.Range(0, tileMapPrefabs.Length);
        currentTileMap = Instantiate(tileMapPrefabs[randomIndex], Vector3.zero, Quaternion.identity);

        Transform[] spawnPoints = currentTileMap.GetComponentsInChildren<Transform>();
        List<Transform> validSpawnPoints = new List<Transform>();
        foreach (Transform t in spawnPoints)
        {
            if (t.CompareTag("SpawnPoint"))
            {
                validSpawnPoints.Add(t);
            }
        }

        activeEnemies.Clear();
        for (int i = 0; i < enemiesPerLevel && i < validSpawnPoints.Count; i++)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, validSpawnPoints[i].position, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.moveSpeed = baseEnemyMoveSpeed;
                enemy.maxHealth = baseEnemyHealth;
                enemy.damage = baseEnemyDamage;
                enemy.expValue = baseExpValue;
                enemy.Initialize();
                activeEnemies.Add(enemy);
            }
        }

        foreach (Transform t in spawnPoints)
        {
            if (t.CompareTag("PlayerSpawn"))
            {
                player.transform.position = t.position;
                break;
            }
        }

        levelCompleted = false;
        Debug.Log($"Level {currentLevel} generated with {activeEnemies.Count} enemies.");
    }

    void LevelCompleted()
    {
        CanvasController canvas = FindObjectOfType<CanvasController>();
        if (canvas != null)
        {
            canvas.ShowLevelCompletePanel(this); // Pass SceneManager reference
        }
    }

    // Called by CanvasController to apply the selected upgrade
    public void ApplyUpgrade(string option)
    {
        switch (option)
        {
            case "Speed":
                baseEnemyMoveSpeed *= 1.2f;
                break;
            case "Health":
                baseEnemyHealth = Mathf.RoundToInt(baseEnemyHealth * 1.2f);
                break;
            case "Damage":
                baseEnemyDamage = Mathf.RoundToInt(baseEnemyDamage * 1.2f);
                break;
        }

        currentLevel++;
        GenerateLevel();
    }
}