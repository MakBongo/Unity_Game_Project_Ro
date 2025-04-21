using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RoundManager : MonoBehaviour
{
    [Header("Round Prefabs")]
    public GameObject[] tileMapPrefabs; // Regular tile map prefabs
    public GameObject[] bossTileMapPrefabs; // Independent boss tile map prefabs

    private int currentRound = 1;
    private int highestRound = 0;
    private int score = 0; // Current score for the session
    private List<Enemy> activeEnemies = new List<Enemy>();
    private GameObject currentTileMap;
    private PlayerController player;
    private bool roundCompleted = false;
    private int lastTileMapIndex = -1; // Track the last used tile map index for regular rounds

    // Upgrade multipliers tracked in memory
    private float speedMultiplier = 1f;
    private float healthMultiplier = 1f;
    private float damageMultiplier = 1f;

    // Timer for Boss Round
    public float bossRoundTimer = 900f; // 15 minutes in seconds
    public bool isBossRoundTriggered = false; // Changed to public to allow CanvasController access
    private bool isBossRoundActive = false; // Flag to track if current round is Boss Round

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        LoadGame(); // Load money, highest round, highest level, and highest score
        GenerateRound();
        // Start the Boss Round timer
        StartCoroutine(BossRoundTimerCoroutine());
    }

    void Update()
    {
        if (!roundCompleted && activeEnemies.Count > 0 && activeEnemies.TrueForAll(e => e == null || e.IsDead()))
        {
            RoundCompleted();
            roundCompleted = true;
        }
    }

    // Coroutine to handle the 15-minute timer
    private IEnumerator BossRoundTimerCoroutine()
    {
        yield return new WaitForSeconds(bossRoundTimer);
        isBossRoundTriggered = true;
        Debug.Log("15-minute timer expired! Next round will be a Boss Round.");
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

        int tileMapIndex;
        GameObject selectedPrefab;
        if (isBossRoundTriggered && !isBossRoundActive)
        {
            // Boss Round logic
            isBossRoundActive = true;
            isBossRoundTriggered = false; // Reset trigger to prevent repeated Boss Rounds
            // Select a random boss tile map prefab
            if (bossTileMapPrefabs.Length > 0)
            {
                tileMapIndex = Random.Range(0, bossTileMapPrefabs.Length);
                selectedPrefab = bossTileMapPrefabs[tileMapIndex];
                Debug.Log($"Generating Boss Round {currentRound} with boss tile map index {tileMapIndex}");
            }
            else
            {
                // Fallback to regular tile map if bossTileMapPrefabs is empty
                Debug.LogWarning("No boss tile map prefabs assigned! Falling back to regular tile map.");
                tileMapIndex = Random.Range(0, tileMapPrefabs.Length);
                selectedPrefab = tileMapPrefabs[tileMapIndex];
                lastTileMapIndex = tileMapIndex; // Update lastTileMapIndex for fallback
            }
        }
        else
        {
            // Regular round logic
            isBossRoundActive = false;
            // Select a random tile map index different from the last one
            do
            {
                tileMapIndex = Random.Range(0, tileMapPrefabs.Length);
            } while (tileMapIndex == lastTileMapIndex && tileMapPrefabs.Length > 1); // Ensure different index unless only one prefab exists
            lastTileMapIndex = tileMapIndex; // Update the last used index
            selectedPrefab = tileMapPrefabs[tileMapIndex];
        }

        currentTileMap = Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity);

        activeEnemies.Clear();
        Enemy[] enemiesInRound = currentTileMap.GetComponentsInChildren<Enemy>();
        foreach (Enemy enemy in enemiesInRound)
        {
            // Apply multipliers, with additional scaling for Boss Round
            enemy.moveSpeed *= speedMultiplier * (isBossRoundActive ? 1.5f : 1f); // 50% faster in Boss Round
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * healthMultiplier * (isBossRoundActive ? 2f : 1f)); // 2x health in Boss Round
            enemy.damage = Mathf.RoundToInt(enemy.damage * damageMultiplier * (isBossRoundActive ? 1.5f : 1f)); // 50% more damage in Boss Round
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
        Debug.Log($"Round {currentRound} generated with {activeEnemies.Count} enemies. {(isBossRoundActive ? "Boss Round!" : "Regular Round")}");
    }

    void RoundCompleted()
    {
        if (player != null)
        {
            // Award more money for completing a Boss Round
            player.AddMoney(isBossRoundActive ? 50 : 10);
            score += isBossRoundActive ? 150 : 50; // More points for Boss Round
            if (currentRound > highestRound)
            {
                highestRound = currentRound;
                Debug.Log($"New record set! Highest Round: {highestRound}");
            }
            SaveGame(); // Save money, highest round, highest level, and highest score
            Debug.Log($"Round {currentRound} completed! Money increased by {(isBossRoundActive ? 50 : 10)}. Total money: {player.GetMoney()}, Score: {score}");
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