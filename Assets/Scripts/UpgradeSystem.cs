using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    public PlayerController playerController;
    public Shooting shooting;

    [Header("Reselect Settings")]
    public int initialReselectCost = 5; // Initial cost for reselecting upgrades
    private int currentReselectCost; // Tracks the current cost
    private int reselectCount; // Tracks number of reselections in the current panel
    private const int maxReselectCost = 100; // Maximum reselect cost

    public enum PlayerUpgradeOption { AmmunitionSpeed, FiresPerMinute, AmmunitionLifetime, MagazineSize, ReloadTime, HealRate, ExpAmount, MoneyAmount }
    private PlayerUpgradeOption[] playerUpgradeOptions = {
        PlayerUpgradeOption.AmmunitionSpeed, PlayerUpgradeOption.FiresPerMinute, PlayerUpgradeOption.AmmunitionLifetime,
        PlayerUpgradeOption.MagazineSize, PlayerUpgradeOption.ReloadTime, PlayerUpgradeOption.HealRate,
        PlayerUpgradeOption.ExpAmount, PlayerUpgradeOption.MoneyAmount
    };

    void Start()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("UpgradeSystem: PlayerController not found!");
            }
        }

        if (shooting == null && playerController != null)
        {
            shooting = playerController.shooting;
            if (shooting == null)
            {
                Debug.LogError("UpgradeSystem: Shooting script not found!");
            }
        }

        // Initialize reselect cost
        currentReselectCost = initialReselectCost;
        reselectCount = 0;
    }

    public PlayerUpgradeOption[] GetRandomUpgradeOptions(int count = 3)
    {
        PlayerUpgradeOption[] options = new PlayerUpgradeOption[count];
        System.Collections.Generic.List<PlayerUpgradeOption> availableOptions = new System.Collections.Generic.List<PlayerUpgradeOption>(playerUpgradeOptions);

        for (int i = 0; i < count; i++)
        {
            if (availableOptions.Count == 0) break;
            int randomIndex = Random.Range(0, availableOptions.Count);
            options[i] = availableOptions[randomIndex];
            availableOptions.RemoveAt(randomIndex);
        }

        return options;
    }

    public string GetPlayerUpgradeText(PlayerUpgradeOption option)
    {
        if (playerController == null || shooting == null) return "Error: References missing";

        switch (option)
        {
            case PlayerUpgradeOption.AmmunitionSpeed:
                float speedAdd = shooting.GetBaseAmmunitionSpeed() * 0.1f;
                return $"Ammunition Speed +{speedAdd:F1} (Current: {shooting.GetAmmunitionSpeed():F1})";
            case PlayerUpgradeOption.FiresPerMinute:
                float fpmAdd = shooting.GetBaseFiresPerMinute() * 0.1f;
                return $"Fire Rate +{fpmAdd:F1} (Current: {shooting.GetFiresPerMinute():F1})";
            case PlayerUpgradeOption.AmmunitionLifetime:
                float lifetimeAdd = shooting.GetBaseAmmunitionLifetime() * 0.1f;
                return $"Ammunition Lifetime +{lifetimeAdd:F1} (Current: {shooting.GetAmmunitionLifetime():F1})";
            case PlayerUpgradeOption.MagazineSize:
                int sizeAdd = Mathf.RoundToInt(shooting.GetBaseMagazineSize() * 0.1f);
                return $"Magazine Size +{sizeAdd} (Current: {shooting.GetMagazineSize()})";
            case PlayerUpgradeOption.ReloadTime:
                float reloadReduce = shooting.GetBaseReloadTime() * 0.1f;
                return $"Reload Time -{reloadReduce:F1} (Current: {shooting.GetReloadTime():F1})";
            case PlayerUpgradeOption.HealRate:
                float healAdd = playerController.healRate * 0.1f; // Use current healRate
                return $"Heal Rate +{healAdd:F4} (Current: {playerController.healRate:F4})";
            case PlayerUpgradeOption.ExpAmount:
                float expAdd = playerController.expMultiplier * 0.1f; // Use current expMultiplier
                return $"EXP Gain +{expAdd:F2} (Current: {playerController.expMultiplier:F2})";
            case PlayerUpgradeOption.MoneyAmount:
                float moneyAdd = playerController.moneyMultiplier * 0.1f; // Use current moneyMultiplier
                return $"Money Gain +{moneyAdd:F2} (Current: {playerController.moneyMultiplier:F2})";
            default:
                return "";
        }
    }

    public void ApplyPlayerUpgrade(PlayerUpgradeOption option)
    {
        if (playerController == null || shooting == null)
        {
            Debug.LogError("UpgradeSystem: Cannot apply upgrade, references missing!");
            return;
        }

        switch (option)
        {
            case PlayerUpgradeOption.AmmunitionSpeed: shooting.UpgradeAmmunitionSpeed(); break;
            case PlayerUpgradeOption.FiresPerMinute: shooting.UpgradeFiresPerMinute(); break;
            case PlayerUpgradeOption.AmmunitionLifetime: shooting.UpgradeAmmunitionLifetime(); break;
            case PlayerUpgradeOption.MagazineSize: shooting.UpgradeMagazineSize(); break;
            case PlayerUpgradeOption.ReloadTime: shooting.UpgradeReloadTime(); break;
            case PlayerUpgradeOption.HealRate: playerController.UpgradeHealRate(); break;
            case PlayerUpgradeOption.ExpAmount: playerController.UpgradeExpAmount(); break;
            case PlayerUpgradeOption.MoneyAmount: playerController.UpgradeMoneyAmount(); break;
        }
    }

    public void ResetReselectState()
    {
        currentReselectCost = initialReselectCost;
        reselectCount = 0;
        Debug.Log($"Reselect state reset. Cost: {currentReselectCost}, Count: {reselectCount}");
    }

    public bool TryReselectUpgrades()
    {
        if (playerController == null)
        {
            Debug.LogError("Cannot reselect upgrades: PlayerController is missing!");
            return false;
        }

        if (playerController.GetMoney() >= currentReselectCost)
        {
            playerController.AddMoney(-currentReselectCost); // Deduct coins
            reselectCount++;

            // Update reselect cost with maximum cap
            if (reselectCount == 1)
            {
                currentReselectCost = Mathf.Min(currentReselectCost + 5, maxReselectCost); // First reselect: add 5
            }
            else
            {
                currentReselectCost = Mathf.Min(Mathf.RoundToInt(currentReselectCost * 1.5f), maxReselectCost); // Subsequent reselections: increase by 50%
            }

            Debug.Log($"Upgrades reselected for {currentReselectCost} coins. Reselection #{reselectCount}. Next cost: {currentReselectCost}");
            return true;
        }
        else
        {
            Debug.Log("Not enough coins to reselect upgrades!");
            return false;
        }
    }

    public int GetCurrentReselectCost()
    {
        return currentReselectCost;
    }

    public bool CanAffordReselect()
    {
        return playerController != null && playerController.GetMoney() >= currentReselectCost;
    }
}