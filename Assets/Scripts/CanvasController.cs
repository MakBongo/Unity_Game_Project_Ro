using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CanvasController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public Text displayText;
    public Slider healthSlider;
    public Slider expSlider;
    public GameObject upgradePanel; // Player level-up panel

    [Header("Level Complete UI")]
    public GameObject upgradeDataPanel; // Player upgrades
    public Text upgradeOption1Text;
    public Text upgradeOption2Text;
    public Text upgradeOption3Text;
    public Button upgradeOption1Button;
    public Button upgradeOption2Button;
    public Button upgradeOption3Button;
    public GameObject levelCompletePanel;
    public Text option1Text;
    public Text option2Text;
    public Button option1Button;
    public Button option2Button;

    private enum PlayerUpgradeOption { BulletSpeed, FiresPerMinute, BulletLifetime, MagazineSize, ReloadTime, HealRate, ExpAmount } // Added ExpAmount
    private PlayerUpgradeOption[] playerUpgradeOptions = {
        PlayerUpgradeOption.BulletSpeed, PlayerUpgradeOption.FiresPerMinute, PlayerUpgradeOption.BulletLifetime,
        PlayerUpgradeOption.MagazineSize, PlayerUpgradeOption.ReloadTime, PlayerUpgradeOption.HealRate, PlayerUpgradeOption.ExpAmount
    };
    private PlayerUpgradeOption[] currentPlayerOptions = new PlayerUpgradeOption[3];

    private enum LevelUpgradeOption { Speed, Health, Damage } // Removed ExpAmount
    private LevelUpgradeOption[] levelUpgradeOptions = { LevelUpgradeOption.Speed, LevelUpgradeOption.Health, LevelUpgradeOption.Damage };
    private LevelUpgradeOption[] currentLevelOptions = new LevelUpgradeOption[2];

    private SceneManager sceneManager;
    private Queue<string> panelQueue = new Queue<string>();
    private bool isShowingPanel = false;

    void Start()
    {
        if (playerController != null && healthSlider != null)
        {
            healthSlider.maxValue = playerController.GetMaxHealth();
            healthSlider.value = playerController.GetCurrentHealth();
        }

        if (playerController != null && expSlider != null)
        {
            expSlider.maxValue = playerController.GetMaxExp();
            expSlider.value = playerController.GetCurrentExp();
        }

        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (upgradeDataPanel != null) upgradeDataPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    void Update()
    {
        if (playerController != null && displayText != null)
        {
            int currentAmmo = playerController.GetCurrentAmmo();
            string ammoString = $"Ammo: {currentAmmo}/{playerController.magazineSize}";
            displayText.text = playerController.IsReloading() ? $"{ammoString} - Reloading..." : ammoString;
        }

        if (playerController != null && healthSlider != null)
        {
            healthSlider.maxValue = playerController.GetMaxHealth();
            healthSlider.value = playerController.GetCurrentHealth();
        }

        if (playerController != null && expSlider != null)
        {
            expSlider.maxValue = playerController.GetMaxExp();
            expSlider.value = playerController.GetCurrentExp();
        }

        if (!isShowingPanel && panelQueue.Count > 0)
        {
            ShowNextPanel();
        }
    }

    public void QueuePanel(string panelName)
    {
        panelQueue.Enqueue(panelName);
    }

    void ShowNextPanel()
    {
        if (panelQueue.Count == 0) return;

        string panelName = panelQueue.Dequeue();
        isShowingPanel = true;

        switch (panelName)
        {
            case "PlayerLevelUp":
                ShowUpgradePanel();
                break;
            case "LevelFinished":
                sceneManager = FindObjectOfType<SceneManager>();
                if (sceneManager != null)
                {
                    ShowUpgradeDataPanel();
                }
                break;
        }
    }

    // Player level-up panel (existing)
    public void ShowUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void OnHealthButtonClicked()
    {
        if (playerController != null)
        {
            playerController.UpgradeMaxHealth();
            CloseUpgradePanel();
        }
    }

    public void OnDamageButtonClicked()
    {
        if (playerController != null)
        {
            playerController.UpgradeBulletDamage();
            CloseUpgradePanel();
        }
    }

    public void OnSpeedButtonClicked()
    {
        if (playerController != null)
        {
            playerController.UpgradeMoveSpeed();
            CloseUpgradePanel();
        }
    }

    void CloseUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            Time.timeScale = 1f;
            isShowingPanel = false;
        }
    }

    // Upgrade data panel (player upgrades)
    public void ShowUpgradeDataPanel()
    {
        if (upgradeDataPanel != null)
        {
            upgradeDataPanel.SetActive(true);
            Time.timeScale = 0f;

            List<PlayerUpgradeOption> availableOptions = new List<PlayerUpgradeOption>(playerUpgradeOptions);
            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, availableOptions.Count);
                currentPlayerOptions[i] = availableOptions[randomIndex];
                availableOptions.RemoveAt(randomIndex);
            }

            upgradeOption1Text.text = GetPlayerUpgradeText(currentPlayerOptions[0]);
            upgradeOption2Text.text = GetPlayerUpgradeText(currentPlayerOptions[1]);
            upgradeOption3Text.text = GetPlayerUpgradeText(currentPlayerOptions[2]);

            upgradeOption1Button.onClick.RemoveAllListeners();
            upgradeOption2Button.onClick.RemoveAllListeners();
            upgradeOption3Button.onClick.RemoveAllListeners();
            upgradeOption1Button.onClick.AddListener(() => ApplyPlayerUpgrade(currentPlayerOptions[0]));
            upgradeOption2Button.onClick.AddListener(() => ApplyPlayerUpgrade(currentPlayerOptions[1]));
            upgradeOption3Button.onClick.AddListener(() => ApplyPlayerUpgrade(currentPlayerOptions[2]));
        }
    }

    string GetPlayerUpgradeText(PlayerUpgradeOption option)
    {
        switch (option)
        {
            case PlayerUpgradeOption.BulletSpeed: return $"Bullet Speed +10% (Current: {playerController.bulletSpeed:F1})";
            case PlayerUpgradeOption.FiresPerMinute: return $"Fire Rate +10% (Current: {playerController.firesPerMinute:F1})";
            case PlayerUpgradeOption.BulletLifetime: return $"Bullet Lifetime +10% (Current: {playerController.bulletLifetime:F1})";
            case PlayerUpgradeOption.MagazineSize: return $"Magazine Size +10% (Current: {playerController.magazineSize})";
            case PlayerUpgradeOption.ReloadTime: return $"Reload Time -10% (Current: {playerController.reloadTime:F1})";
            case PlayerUpgradeOption.HealRate: return $"Heal Rate +10% (Current: {playerController.healRate * 100:F2}%)";
            case PlayerUpgradeOption.ExpAmount: return $"EXP Gain +10% (Current: {playerController.expMultiplier * 100:F0}%)"; // New option
            default: return "";
        }
    }

    void ApplyPlayerUpgrade(PlayerUpgradeOption option)
    {
        switch (option)
        {
            case PlayerUpgradeOption.BulletSpeed: playerController.UpgradeBulletSpeed(); break;
            case PlayerUpgradeOption.FiresPerMinute: playerController.UpgradeFiresPerMinute(); break;
            case PlayerUpgradeOption.BulletLifetime: playerController.UpgradeBulletLifetime(); break;
            case PlayerUpgradeOption.MagazineSize: playerController.UpgradeMagazineSize(); break;
            case PlayerUpgradeOption.ReloadTime: playerController.UpgradeReloadTime(); break;
            case PlayerUpgradeOption.HealRate: playerController.UpgradeHealRate(); break;
            case PlayerUpgradeOption.ExpAmount: playerController.UpgradeExpAmount(); break; // New case
        }
        upgradeDataPanel.SetActive(false);
        ShowLevelCompletePanel(); // Proceed to enemy upgrades
    }

    // Level complete panel (enemy upgrades)
    public void ShowLevelCompletePanel()
    {
        if (levelCompletePanel != null && sceneManager != null)
        {
            levelCompletePanel.SetActive(true);

            currentLevelOptions[0] = levelUpgradeOptions[Random.Range(0, levelUpgradeOptions.Length)];
            do
            {
                currentLevelOptions[1] = levelUpgradeOptions[Random.Range(0, levelUpgradeOptions.Length)];
            } while (currentLevelOptions[1] == currentLevelOptions[0]);

            option1Text.text = GetLevelUpgradeText(currentLevelOptions[0]);
            option2Text.text = GetLevelUpgradeText(currentLevelOptions[1]);

            option1Button.onClick.RemoveAllListeners();
            option2Button.onClick.RemoveAllListeners();
            option1Button.onClick.AddListener(() => ApplyLevelUpgrade(currentLevelOptions[0]));
            option2Button.onClick.AddListener(() => ApplyLevelUpgrade(currentLevelOptions[1]));
        }
    }

    string GetLevelUpgradeText(LevelUpgradeOption option)
    {
        switch (option)
        {
            case LevelUpgradeOption.Speed: return "Enemy Speed +20%";
            case LevelUpgradeOption.Health: return "Enemy Health +20%";
            case LevelUpgradeOption.Damage: return "Enemy Damage +20%";
            default: return "";
        }
    }

    void ApplyLevelUpgrade(LevelUpgradeOption option)
    {
        if (sceneManager != null)
        {
            sceneManager.ApplyUpgrade(option.ToString());
            levelCompletePanel.SetActive(false);
            Time.timeScale = 1f;
            isShowingPanel = false;
        }
    }
}