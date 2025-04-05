using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SceneManager : MonoBehaviour
{
    [Header("Round Prefabs")]
    public GameObject[] tileMapPrefabs;

    private int currentRound = 1;
    private int highestRound = 0;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private GameObject currentTileMap;
    private PlayerController player;
    private bool roundCompleted = false;

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
        LoadGame(); // Load money, highest round, and highest level
        GenerateRound();
    }

    void Update()
    {
        if (!roundCompleted && activeEnemies.Count > 0 && activeEnemies.TrueForAll(e => e == null || e.IsDead()))
        {
            RoundCompleted();
            roundCompleted = true;
        }
    }

    void GenerateRound()
    {
        if (currentTileMap != null)
        {
            Destroy(currentTileMap);
        }

        int randomIndex = Random.Range(0, tileMapPrefabs.Length);
        currentTileMap = Instantiate(tileMapPrefabs[randomIndex], Vector3.zero, Quaternion.identity);

        activeEnemies.Clear();
        Enemy[] enemiesInRound = currentTileMap.GetComponentsInChildren<Enemy>();
        foreach (Enemy enemy in enemiesInRound)
        {
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
        Debug.Log($"Round {currentRound} generated with {activeEnemies.Count} enemies.");
    }

    void RoundCompleted()
    {
        if (player != null)
        {
            player.AddMoney(10);
            if (currentRound > highestRound)
            {
                highestRound = currentRound;
                Debug.Log($"New record set! Highest Round: {highestRound}");
            }
            SaveGame(); // Save money, highest round, and highest level
            Debug.Log($"Round {currentRound} completed! Money increased by 10. Total money: {player.GetMoney()}");
        }

        CanvasController canvas = FindObjectOfType<CanvasController>();
        if (canvas != null)
        {
            canvas.QueuePanel("RoundFinished");
        }
    }

    public void ApplyUpgrade(string option)
    {
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

        currentRound++;
        GenerateRound();
    }

    void SaveGame()
    {
        if (player == null)
        {
            Debug.LogError("PlayerController not found in SceneManager!");
            return;
        }

        SaveData data = new SaveData
        {
            money = player.GetMoney(),
            highestRound = highestRound,
            highestLevel = player.GetHighestLevel() // New: Save highest level
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
                player.AddMoney(data.money - player.GetMoney());
                highestRound = data.highestRound;
                player.SetHighestLevel(data.highestLevel); // New: Load highest level (requires setter)
                Debug.Log($"Game loaded. Money set to: {player.GetMoney()}, Highest Round: {highestRound}, Highest Level: {player.GetHighestLevel()}");
            }
        }
        else
        {
            Debug.Log("No save file found at: " + savePath);
        }
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }

    public int GetHighestRound()
    {
        return highestRound;
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }
}