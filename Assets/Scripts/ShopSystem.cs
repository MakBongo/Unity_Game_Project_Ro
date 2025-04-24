using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopSystem : MonoBehaviour
{
    [Header("UI References")]
    public Text coinText; // Displays coin count
    public Transform upgradeButtonParent; // Parent for upgrade buttons
    public GameObject upgradeButtonPrefab; // Button prefab with Text components
    public Button addMoneyButton; // Button to add 100 money

    [Header("Upgrade Settings")]
    public int basePrice = 100; // Initial price for each upgrade

    private SaveData saveData;
    private Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();
    private Dictionary<string, float> upgradeMultipliers = new Dictionary<string, float>();
    private Dictionary<string, int> upgradePrices = new Dictionary<string, int>();

    private string[] upgradeOptions = {
        "AmmunitionSpeed", "FiresPerMinute", "AmmunitionLifetime",
        "MagazineSize", "ReloadTime", "HealRate", "ExpAmount", "MoneyAmount"
    };

    void Start()
    {
        LoadSaveData();
        InitializeUpgrades();
        SetupUpgradeButtons();
        SetupMoneyButton();
        UpdateCoinDisplay();
    }

    void LoadSaveData()
    {
        saveData = SaveGameManager.Instance.GetSaveData();
        InitializeDefaultMultipliers();
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

    void InitializeUpgrades()
    {
        foreach (string option in upgradeOptions)
        {
            upgradeLevels[option] = 0;
            upgradeMultipliers[option] = option == "ReloadTime" ? 1f : 1f;
            upgradePrices[option] = basePrice;
        }

        // Apply saved multipliers
        upgradeMultipliers["AmmunitionSpeed"] = saveData.ammunitionSpeedMultiplier;
        upgradeMultipliers["FiresPerMinute"] = saveData.firesPerMinuteMultiplier;
        upgradeMultipliers["AmmunitionLifetime"] = saveData.ammunitionLifetimeMultiplier;
        upgradeMultipliers["MagazineSize"] = saveData.magazineSizeMultiplier;
        upgradeMultipliers["ReloadTime"] = saveData.reloadTimeMultiplier;
        upgradeMultipliers["HealRate"] = saveData.healRateMultiplier;
        upgradeMultipliers["ExpAmount"] = saveData.expMultiplier;
        upgradeMultipliers["MoneyAmount"] = saveData.moneyMultiplier;

        // Calculate levels and prices based on multipliers
        foreach (string option in upgradeOptions)
        {
            float multiplier = upgradeMultipliers[option];
            int level;
            if (option == "ReloadTime")
            {
                // For reload time, multiplier = 0.8^level (decreasing)
                level = multiplier >= 1f ? 0 : Mathf.FloorToInt(Mathf.Log(multiplier, 0.8f));
            }
            else
            {
                // For others, multiplier = 1.25^level (increasing)
                level = Mathf.FloorToInt(Mathf.Log(multiplier, 1.25f));
            }
            upgradeLevels[option] = level;
            upgradePrices[option] = basePrice;
            for (int i = 0; i < level; i++)
            {
                upgradePrices[option] = Mathf.FloorToInt(upgradePrices[option] * 1.5f);
            }
        }
    }

    void SetupUpgradeButtons()
    {
        if (upgradeButtonParent == null || upgradeButtonPrefab == null)
        {
            Debug.LogError("ShopSystem: UpgradeButtonParent or UpgradeButtonPrefab not assigned!");
            return;
        }

        foreach (Transform child in upgradeButtonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (string option in upgradeOptions)
        {
            GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonParent);
            Button button = buttonObj.GetComponent<Button>();
            Text text = buttonObj.GetComponentInChildren<Text>();

            if (button == null || text == null)
            {
                Debug.LogWarning($"ShopSystem: Upgrade button {buttonObj.name} missing Button or Text!");
                continue;
            }

            string displayText = GetUpgradeDisplayText(option);
            text.text = $"{displayText}\nCost: {upgradePrices[option]} Coins";
            button.onClick.AddListener(() => PurchaseUpgrade(option));
        }
    }

    void SetupMoneyButton()
    {
        if (addMoneyButton == null)
        {
            Debug.LogWarning("ShopSystem: AddMoneyButton not assigned in Inspector!");
            return;
        }

        addMoneyButton.onClick.RemoveAllListeners();
        addMoneyButton.onClick.AddListener(AddMoney);

        Text buttonText = addMoneyButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = "Add 100 Money";
        }
    }

    string GetUpgradeDisplayText(string option)
    {
        switch (option)
        {
            case "AmmunitionSpeed":
                return $"Ammunition Speed +25% (Current: x{upgradeMultipliers[option]:F2})";
            case "FiresPerMinute":
                return $"Fire Rate +25% (Current: x{upgradeMultipliers[option]:F2})";
            case "AmmunitionLifetime":
                return $"Ammunition Lifetime +25% (Current: x{upgradeMultipliers[option]:F2})";
            case "MagazineSize":
                return $"Magazine Size +25% (Current: x{upgradeMultipliers[option]:F2})";
            case "ReloadTime":
                return $"Reload Time -20% (Current: x{upgradeMultipliers[option]:F2})";
            case "HealRate":
                return $"Heal Rate +25% (Current: x{upgradeMultipliers[option]:F2})";
            case "ExpAmount":
                return $"EXP Gain +25% (Current: x{upgradeMultipliers[option]:F2})";
            case "MoneyAmount":
                return $"Money Gain +25% (Current: x{upgradeMultipliers[option]:F2})";
            default:
                return "Unknown Upgrade";
        }
    }

    void PurchaseUpgrade(string option)
    {
        int price = upgradePrices[option];
        int currentMoney = SaveGameManager.Instance.GetMoney();
        if (currentMoney >= price)
        {
            SaveGameManager.Instance.SetMoney(currentMoney - price);
            upgradeLevels[option]++;
            if (option == "ReloadTime")
            {
                upgradeMultipliers[option] *= 0.8f; // Decrease reload time by 20%
            }
            else
            {
                upgradeMultipliers[option] *= 1.25f; // Increase other stats by 25%
            }
            upgradePrices[option] = Mathf.FloorToInt(upgradePrices[option] * 1.5f);

            // Update SaveData
            switch (option)
            {
                case "AmmunitionSpeed": saveData.ammunitionSpeedMultiplier = upgradeMultipliers[option]; break;
                case "FiresPerMinute": saveData.firesPerMinuteMultiplier = upgradeMultipliers[option]; break;
                case "AmmunitionLifetime": saveData.ammunitionLifetimeMultiplier = upgradeMultipliers[option]; break;
                case "MagazineSize": saveData.magazineSizeMultiplier = upgradeMultipliers[option]; break;
                case "ReloadTime": saveData.reloadTimeMultiplier = upgradeMultipliers[option]; break;
                case "HealRate": saveData.healRateMultiplier = upgradeMultipliers[option]; break;
                case "ExpAmount": saveData.expMultiplier = upgradeMultipliers[option]; break;
                case "MoneyAmount": saveData.moneyMultiplier = upgradeMultipliers[option]; break;
            }

            UpdateCoinDisplay();
            SetupUpgradeButtons();
            SaveGameManager.Instance.SaveGame();
            Debug.Log($"ShopSystem: Purchased {option}. New multiplier: {upgradeMultipliers[option]:F2}, Next price: {upgradePrices[option]}");
        }
        else
        {
            Debug.Log("ShopSystem: Not enough coins!");
        }
    }

    void AddMoney()
    {
        int currentMoney = SaveGameManager.Instance.GetMoney();
        SaveGameManager.Instance.SetMoney(currentMoney + 100);
        UpdateCoinDisplay();
        Debug.Log($"ShopSystem: Added 100 money. New total: {SaveGameManager.Instance.GetMoney()}");
    }

    void UpdateCoinDisplay()
    {
        if (coinText != null)
        {
            coinText.text = $"Coins: {SaveGameManager.Instance.GetMoney()}";
        }
        else
        {
            Debug.LogError("ShopSystem: CoinText not assigned!");
        }
    }

    public SaveData GetSaveData()
    {
        return saveData;
    }
}