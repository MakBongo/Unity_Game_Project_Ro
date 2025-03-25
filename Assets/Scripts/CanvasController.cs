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
    public GameObject upgradePanel; // Player upgrade panel

    [Header("Level Complete UI")]
    public GameObject levelFinishedPanel;
    public Button nextButton;
    public GameObject levelCompletePanel;
    public Text option1Text;
    public Text option2Text;
    public Button option1Button;
    public Button option2Button;

    private enum UpgradeOption { Speed, Health, Damage }
    private UpgradeOption[] upgradeOptions = { UpgradeOption.Speed, UpgradeOption.Health, UpgradeOption.Damage };
    private UpgradeOption[] currentOptions = new UpgradeOption[2];
    private SceneManager sceneManager;
    private Queue<string> panelQueue = new Queue<string>(); // Queue for panel order
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
        if (levelFinishedPanel != null) levelFinishedPanel.SetActive(false);
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

        // Show next panel in queue if not currently showing one
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
                    ShowLevelFinishedPanel();
                }
                break;
        }
    }

    // Player upgrade panel
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
            isShowingPanel = false; // Allow next panel
        }
    }

    // Level finished panel
    public void ShowLevelFinishedPanel()
    {
        if (levelFinishedPanel != null)
        {
            levelFinishedPanel.SetActive(true);
            Time.timeScale = 0f;
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(ShowLevelCompletePanel);
        }
    }

    // Level complete panel
    public void ShowLevelCompletePanel()
    {
        if (levelCompletePanel != null && sceneManager != null)
        {
            levelFinishedPanel.SetActive(false);
            levelCompletePanel.SetActive(true);

            currentOptions[0] = upgradeOptions[Random.Range(0, upgradeOptions.Length)];
            do
            {
                currentOptions[1] = upgradeOptions[Random.Range(0, upgradeOptions.Length)];
            } while (currentOptions[1] == currentOptions[0]);

            option1Text.text = GetUpgradeText(currentOptions[0], sceneManager);
            option2Text.text = GetUpgradeText(currentOptions[1], sceneManager);

            option1Button.onClick.RemoveAllListeners();
            option2Button.onClick.RemoveAllListeners();
            option1Button.onClick.AddListener(() => ApplyLevelUpgrade(currentOptions[0]));
            option2Button.onClick.AddListener(() => ApplyLevelUpgrade(currentOptions[1]));
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

    void ApplyLevelUpgrade(UpgradeOption option)
    {
        if (sceneManager != null)
        {
            sceneManager.ApplyUpgrade(option.ToString());
            levelCompletePanel.SetActive(false);
            Time.timeScale = 1f;
            isShowingPanel = false; // Allow next panel
        }
    }
}