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

    [Header("UI References")]
    public GameObject levelCompletePanel;
    public UnityEngine.UI.Text option1Text;
    public UnityEngine.UI.Text option2Text;
    public UnityEngine.UI.Button option1Button;
    public UnityEngine.UI.Button option2Button;

    private int currentLevel = 1;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private GameObject currentTileMap;
    private PlayerController player;
    private bool levelCompleted = false; // Flag to prevent repeated calls

    private enum UpgradeOption { Speed, Health, Damage }
    private UpgradeOption[] upgradeOptions = { UpgradeOption.Speed, UpgradeOption.Health, UpgradeOption.Damage };
    private UpgradeOption[] currentOptions = new UpgradeOption[2];

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
        GenerateLevel();
    }

    void Update()
    {
        // Check if all enemies are defeated and level hasn't been marked completed yet
        if (!levelCompleted && activeEnemies.Count > 0 && activeEnemies.TrueForAll(e => e == null || e.IsDead()))
        {
            LevelCompleted();
            levelCompleted = true; // Mark level as completed
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

        levelCompleted = false; // Reset flag for new level
        Debug.Log($"Level {currentLevel} generated with {activeEnemies.Count} enemies.");
    }

    void LevelCompleted()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            Time.timeScale = 0f;

            // Select two random upgrade options
            currentOptions[0] = upgradeOptions[Random.Range(0, upgradeOptions.Length)];
            do
            {
                currentOptions[1] = upgradeOptions[Random.Range(0, upgradeOptions.Length)];
            } while (currentOptions[1] == currentOptions[0]);

            option1Text.text = GetUpgradeText(currentOptions[0]);
            option2Text.text = GetUpgradeText(currentOptions[1]);

            option1Button.onClick.RemoveAllListeners();
            option2Button.onClick.RemoveAllListeners();
            option1Button.onClick.AddListener(() => ApplyUpgrade(currentOptions[0]));
            option2Button.onClick.AddListener(() => ApplyUpgrade(currentOptions[1]));
        }
    }

    string GetUpgradeText(UpgradeOption option)
    {
        switch (option)
        {
            case UpgradeOption.Speed: return $"Enemy Speed +20% (Current: {baseEnemyMoveSpeed:F1})";
            case UpgradeOption.Health: return $"Enemy Health +20% (Current: {baseEnemyHealth})";
            case UpgradeOption.Damage: return $"Enemy Damage +20% (Current: {baseEnemyDamage})";
            default: return "";
        }
    }

    void ApplyUpgrade(UpgradeOption option)
    {
        switch (option)
        {
            case UpgradeOption.Speed:
                baseEnemyMoveSpeed *= 1.2f;
                break;
            case UpgradeOption.Health:
                baseEnemyHealth = Mathf.RoundToInt(baseEnemyHealth * 1.2f);
                break;
            case UpgradeOption.Damage:
                baseEnemyDamage = Mathf.RoundToInt(baseEnemyDamage * 1.2f);
                break;
        }

        currentLevel++;
        levelCompletePanel.SetActive(false);
        Time.timeScale = 1f;
        GenerateLevel();
    }
}