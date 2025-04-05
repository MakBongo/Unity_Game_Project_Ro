using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SceneManager : MonoBehaviour
{
    [Header("Round Prefabs")] // Updated header
    public GameObject[] tileMapPrefabs; // Each prefab contains pre-placed enemies with their own stats

    private int currentRound = 1; // Renamed from currentLevel
    private List<Enemy> activeEnemies = new List<Enemy>();
    private GameObject currentTileMap;
    private PlayerController player;
    private bool roundCompleted = false; // Renamed from levelCompleted

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
        GenerateRound(); // Renamed from GenerateLevel
    }

    void Update()
    {
        if (!roundCompleted && activeEnemies.Count > 0 && activeEnemies.TrueForAll(e => e == null || e.IsDead()))
        {
            RoundCompleted(); // Renamed from LevelCompleted
            roundCompleted = true;
        }
    }

    void GenerateRound() // Renamed from GenerateLevel
    {
        if (currentTileMap != null)
        {
            Destroy(currentTileMap);
        }

        int randomIndex = Random.Range(0, tileMapPrefabs.Length);
        currentTileMap = Instantiate(tileMapPrefabs[randomIndex], Vector3.zero, Quaternion.identity);

        activeEnemies.Clear();
        Enemy[] enemiesInRound = currentTileMap.GetComponentsInChildren<Enemy>(); // Renamed variable
        foreach (Enemy enemy in enemiesInRound)
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

        roundCompleted = false;
        Debug.Log($"Round {currentRound} generated with {activeEnemies.Count} enemies."); // Updated log
    }

    void RoundCompleted() // Renamed from LevelCompleted
    {
        // Increase money by 10 and save
        if (player != null)
        {
            player.AddMoney(10);
            SaveGame();
            Debug.Log($"Round {currentRound} completed! Money increased by 10. Total money: {player.GetMoney()}"); // Updated log
        }

        CanvasController canvas = FindObjectOfType<CanvasController>();
        if (canvas != null)
        {
            canvas.QueuePanel("RoundFinished"); // Renamed from "LevelFinished"
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

        currentRound++; // Updated variable name
        GenerateRound(); // Updated method call
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