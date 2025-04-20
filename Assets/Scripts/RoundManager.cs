using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RoundManager : MonoBehaviour
{
    [Header("Round Prefabs")]
    public GameObject[] tileMapPrefabs;

    private int currentRound = 1;
    private int highestRound = 0;
    private int score = 0; // Current score for the session
    private List<Enemy> activeEnemies = new List<Enemy>();
    private GameObject currentTileMap;
    private PlayerController player;
    private bool roundCompleted = false;
    private int lastTileMapIndex = -1; // Track the last used tile map index

    // Upgrade multipliers tracked in memory
    private float speedMultiplier = 1f;
    private float healthMultiplier = 1f;
    private float damageMultiplier = 1f;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        LoadGame(); // Load money, highest round, highest level, and highest score
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
        // Clean up items in the "Item" layer
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.layer == LayerMask.NameToLayer("Item"))
            {
                Destroy(obj);
            }
        }

        if (currentTileMap != null)
        {
            Destroy(currentTileMap);
        }

        // Select a random tile map index different from the last one
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, tileMapPrefabs.Length);
        } while (randomIndex == lastTileMapIndex && tileMapPrefabs.Length > 1); // Ensure different index unless only one prefab exists
        lastTileMapIndex = randomIndex; // Update the last used index

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
            score += 50; // Add 50 points for completing the round
            if (currentRound > highestRound)
            {
                highestRound = currentRound;
                Debug.Log($"New record set! Highest Round: {highestRound}");
            }
            SaveGame(); // Save money, highest round, highest level, and highest score
            Debug.Log($"Round {currentRound} completed! Money increased by 10. Total money: {player.GetMoney()}, Score: {score}");
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

        SaveData data = SaveGameManager.Instance.GetSaveData();
        data.money = player.GetMoney();
        data.highestRound = highestRound;
        data.highestLevel = player.GetHighestLevel();
        if (score > data.highestScore) // Only update if current score is higher
        {
            data.highestScore = score;
            Debug.Log($"New highest score saved: {data.highestScore}");
        }
        SaveGameManager.Instance.SaveGame();
    }

    void LoadGame()
    {
        SaveData data = SaveGameManager.Instance.GetSaveData();
        if (player != null)
        {
            player.AddMoney(data.money - player.GetMoney());
            highestRound = data.highestRound;
            player.SetHighestLevel(data.highestLevel);
            Debug.Log($"Game loaded. Money set to: {player.GetMoney()}, Highest Round: {highestRound}, Highest Score: {data.highestScore}, Highest Level: {player.GetHighestLevel()}");
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

    public int GetScore()
    {
        return score;
    }
}