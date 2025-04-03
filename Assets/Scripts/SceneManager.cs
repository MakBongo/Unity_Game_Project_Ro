using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SceneManager : MonoBehaviour
{
    [Header("Level Prefabs")]
    public GameObject[] tileMapPrefabs; // Each prefab contains pre-placed enemies with their own stats

    private int currentLevel = 1;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private GameObject currentTileMap;
    private PlayerController player;
    private bool levelCompleted = false;

    // Upgrade multipliers tracked in memory
    private float speedMultiplier = 1f;
    private float healthMultiplier = 1f;
    private float damageMultiplier = 1f;

    // Save file path
    private string savePath;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        LoadGame(); // Load money at start
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

        activeEnemies.Clear();
        Enemy[] enemiesInLevel = currentTileMap.GetComponentsInChildren<Enemy>();
        foreach (Enemy enemy in enemiesInLevel)
        {
            // Apply upgrades to instantiated enemies only
            enemy.moveSpeed *= speedMultiplier;
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * healthMultiplier);
            enemy.damage = Mathf.RoundToInt(enemy.damage * damageMultiplier);
            enemy.Initialize();
            activeEnemies.Add(enemy);
        }

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
        // Increase money by 10 and save
        if (player != null)
        {
            player.AddMoney(10);
            SaveGame();
            Debug.Log($"Level {currentLevel} completed! Money increased by 10. Total money: {player.GetMoney()}");
        }

        CanvasController canvas = FindObjectOfType<CanvasController>();
        if (canvas != null)
        {
            canvas.QueuePanel("LevelFinished");
        }
    }

    public void ApplyUpgrade(string option)
    {
        // Update multipliers instead of modifying prefabs
        switch (option)
        {
            case "Speed":
                speedMultiplier *= 1.2f;
                Debug.Log($"Enemy Speed Multiplier increased to {speedMultiplier:F2}");
                break;
            case "Health":
                healthMultiplier *= 1.2f;
                Debug.Log($"Enemy Health Multiplier increased to {healthMultiplier:F2}");
                break;
            case "Damage":
                damageMultiplier *= 1.2f;
                Debug.Log($"Enemy Damage Multiplier increased to {damageMultiplier:F2}");
                break;
        }

        currentLevel++;
        GenerateLevel();
    }

    // Save system using independent SaveData
    void SaveGame()
    {
        if (player == null)
        {
            Debug.LogError("PlayerController not found in SceneManager!");
            return;
        }

        SaveData data = new SaveData
        {
            money = player.GetMoney()
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved to: " + savePath);
    }

    void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (player != null)
            {
                player.AddMoney(data.money - player.GetMoney()); // Adjust to match saved value
                Debug.Log("Game loaded. Money set to: " + player.GetMoney());
            }
        }
        else
        {
            Debug.Log("No save file found at: " + savePath);
        }
    }

    void OnApplicationQuit()
    {
        SaveGame(); // Save when quitting
    }
}