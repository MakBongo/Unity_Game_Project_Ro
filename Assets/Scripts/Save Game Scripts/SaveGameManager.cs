using UnityEngine;
using System.IO;

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance { get; private set; }
    private SaveData saveData;
    private string savePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        LoadSaveData();
    }

    void LoadSaveData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"SaveGameManager: Loaded save data. Money: {saveData.money}");
        }
        else
        {
            saveData = new SaveData();
            InitializeDefaultMultipliers();
            Debug.Log("SaveGameManager: No save file found, initialized new SaveData");
        }
    }

    void InitializeDefaultMultipliers()
    {
        if (saveData.ammunitionSpeedMultiplier == 0f) saveData.ammunitionSpeedMultiplier = 1f;
        if (saveData.firesPerMinuteMultiplier == 0f) saveData.firesPerMinuteMultiplier = 1f;
        if (saveData.ammunitionLifetimeMultiplier == 0f) saveData.ammunitionLifetimeMultiplier = 1f;
        if (saveData.magazineSizeMultiplier == 0f) saveData.magazineSizeMultiplier = 1f;
        if (saveData.reloadTimeMultiplier == 0f) saveData.reloadTimeMultiplier = 1f;
        if (saveData.healRateMultiplier == 0f) saveData.healRateMultiplier = 1f;
        if (saveData.expMultiplier == 0f) saveData.expMultiplier = 1f;
        if (saveData.moneyMultiplier == 0f) saveData.moneyMultiplier = 1f;
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("SaveGameManager: Game saved to: " + savePath);
    }

    public SaveData GetSaveData()
    {
        return saveData;
    }

    public void SetMoney(int money)
    {
        saveData.money = money;
        SaveGame();
    }

    public int GetMoney()
    {
        return saveData.money;
    }
}