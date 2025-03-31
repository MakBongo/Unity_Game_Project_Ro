using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    [Header("Level Prefabs")]
    public GameObject[] tileMapPrefabs; // Each prefab contains pre-placed enemies with their own stats

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

        // Instantiate the level prefab
        int randomIndex = Random.Range(0, tileMapPrefabs.Length);
        currentTileMap = Instantiate(tileMapPrefabs[randomIndex], Vector3.zero, Quaternion.identity);

        // Find and initialize all enemies within the level prefab
        activeEnemies.Clear();
        Enemy[] enemiesInLevel = currentTileMap.GetComponentsInChildren<Enemy>();
        foreach (Enemy enemy in enemiesInLevel)
        {
            enemy.Initialize(); // Use enemy's built-in stats
            activeEnemies.Add(enemy);
        }

        // Position the player at the PlayerSpawn point
        Transform[] spawnPoints = currentTileMap.GetComponentsInChildren<Transform>();
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
            canvas.QueuePanel("LevelFinished");
        }
    }

    public void ApplyUpgrade(string option)
    {
        // Apply upgrades to all enemies in the next level
        foreach (GameObject prefab in tileMapPrefabs)
        {
            Enemy[] enemies = prefab.GetComponentsInChildren<Enemy>();
            foreach (Enemy enemy in enemies)
            {
                switch (option)
                {
                    case "Speed":
                        enemy.moveSpeed *= 1.2f;
                        break;
                    case "Health":
                        enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * 1.2f);
                        break;
                    case "Damage":
                        enemy.damage = Mathf.RoundToInt(enemy.damage * 1.2f);
                        break;
                }
            }
        }

        currentLevel++;
        GenerateLevel();
    }
}