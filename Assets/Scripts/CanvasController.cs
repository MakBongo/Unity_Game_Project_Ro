using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public Text displayText;
    public Slider healthSlider;
    public Slider expSlider;
    public GameObject upgradePanel; // Player upgrade panel

    [Header("Level Complete UI")]
    public GameObject levelCompletePanel; // Panel for level completion
    public Text option1Text;              // Text for first upgrade option
    public Text option2Text;              // Text for second upgrade option
    public Button option1Button;          // Button for first option
    public Button option2Button;          // Button for second option

    private enum UpgradeOption { Speed, Health, Damage }
    private UpgradeOption[] upgradeOptions = { UpgradeOption.Speed, UpgradeOption.Health, UpgradeOption.Damage };
    private UpgradeOption[] currentOptions = new UpgradeOption[2];

    void Start()
    {
        // Initialize health slider
        if (playerController != null && healthSlider != null)
        {
            healthSlider.maxValue = playerController.GetMaxHealth();
            healthSlider.value = playerController.GetCurrentHealth();
        }

        // Initialize EXP slider
        if (playerController != null && expSlider != null)
        {
            expSlider.maxValue = playerController.GetMaxExp();
            expSlider.value = playerController.GetCurrentExp();
        }

        // Ensure panels are hidden at start
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    void Update()
    {
        // Update ammo and reload display
        if (playerController != null && displayText != null)
        {
            int currentAmmo = playerController.GetCurrentAmmo();
            string ammoString = $"Ammo: {currentAmmo}/{playerController.magazineSize}";
            displayText.text = playerController.IsReloading() ? $"{ammoString} - Reloading..." : ammoString;
        }

        // Update health slider
        if (playerController != null && healthSlider != null)
        {
            healthSlider.maxValue = playerController.GetMaxHealth();
            healthSlider.value = playerController.GetCurrentHealth();
        }

        // Update EXP slider
        if (playerController != null && expSlider != null)
        {
            expSlider.maxValue = playerController.GetMaxExp();
            expSlider.value = playerController.GetCurrentExp();
        }
    }

    // Player upgrade panel (existing)
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
        }
    }

    // Level complete panel (new)
    public void ShowLevelCompletePanel(SceneManager sceneManager)
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

            // Update button text with current SceneManager values
            option1Text.text = GetUpgradeText(currentOptions[0], sceneManager);
            option2Text.text = GetUpgradeText(currentOptions[1], sceneManager);

            // Assign button actions
            option1Button.onClick.RemoveAllListeners();
            option2Button.onClick.RemoveAllListeners();
            option1Button.onClick.AddListener(() => ApplyLevelUpgrade(currentOptions[0], sceneManager));
            option2Button.onClick.AddListener(() => ApplyLevelUpgrade(currentOptions[1], sceneManager));
        }
    }

    string GetUpgradeText(UpgradeOption option, SceneManager sceneManager)
    {
        switch (option)
        {
            case UpgradeOption.Speed: return $"Enemy Speed +20% (Current: {sceneManager.baseEnemyMoveSpeed:F1})";
            case UpgradeOption.Health: return $"Enemy Health +20% (Current: {sceneManager.baseEnemyHealth})";
            case UpgradeOption.Damage: return $"Enemy Damage +20% (Current: {sceneManager.baseEnemyDamage})";
            default: return "";
        }
    }

    void ApplyLevelUpgrade(UpgradeOption option, SceneManager sceneManager)
    {
        sceneManager.ApplyUpgrade(option.ToString());
        levelCompletePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}