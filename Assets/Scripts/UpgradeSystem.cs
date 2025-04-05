using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    public PlayerController playerController;
    public Shooting shooting;

    public enum PlayerUpgradeOption { BulletSpeed, FiresPerMinute, BulletLifetime, MagazineSize, ReloadTime, HealRate, ExpAmount, MoneyAmount }
    private PlayerUpgradeOption[] playerUpgradeOptions = {
        PlayerUpgradeOption.BulletSpeed, PlayerUpgradeOption.FiresPerMinute, PlayerUpgradeOption.BulletLifetime,
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
            case PlayerUpgradeOption.BulletSpeed: return $"Bullet Speed +10% (Current: {shooting.GetBulletSpeed():F1})";
            case PlayerUpgradeOption.FiresPerMinute: return $"Fire Rate +10% (Current: {shooting.GetFiresPerMinute():F1})";
            case PlayerUpgradeOption.BulletLifetime: return $"Bullet Lifetime +10% (Current: {shooting.GetBulletLifetime():F1})";
            case PlayerUpgradeOption.MagazineSize: return $"Magazine Size +10% (Current: {shooting.GetMagazineSize()})";
            case PlayerUpgradeOption.ReloadTime: return $"Reload Time -10% (Current: {shooting.GetReloadTime():F1})";
            case PlayerUpgradeOption.HealRate: return $"Heal Rate +10% (Current: {playerController.healRate * 100:F2}%)";
            case PlayerUpgradeOption.ExpAmount: return $"EXP Gain +10% (Current: {playerController.expMultiplier * 100:F0}%)";
            case PlayerUpgradeOption.MoneyAmount: return $"Money Gain +10% (Current: {playerController.moneyMultiplier * 100:F0}%)";
            default: return "";
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
            case PlayerUpgradeOption.BulletSpeed: shooting.UpgradeBulletSpeed(); break;
            case PlayerUpgradeOption.FiresPerMinute: shooting.UpgradeFiresPerMinute(); break;
            case PlayerUpgradeOption.BulletLifetime: shooting.UpgradeBulletLifetime(); break;
            case PlayerUpgradeOption.MagazineSize: shooting.UpgradeMagazineSize(); break;
            case PlayerUpgradeOption.ReloadTime: shooting.UpgradeReloadTime(); break;
            case PlayerUpgradeOption.HealRate: playerController.UpgradeHealRate(); break;
            case PlayerUpgradeOption.ExpAmount: playerController.UpgradeExpAmount(); break;
            case PlayerUpgradeOption.MoneyAmount: playerController.UpgradeMoneyAmount(); break;
        }
    }
}