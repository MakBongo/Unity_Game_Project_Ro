using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public Shooting shooting;
    public Text displayText;
    public Slider healthSlider;
    public Slider expSlider;
    public GameObject upgradePanel;
    public Text coinText;
    public UpgradeSystem upgradeSystem; // New: Reference to UpgradeSystem

    [Header("Level Complete UI")]
    public GameObject upgradeDataPanel;
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

    [Header("Pause UI")]
    public GameObject pausePanel;
    private bool isPaused = false;

    private enum LevelUpgradeOption { Speed, Health, Damage }
    private LevelUpgradeOption[] levelUpgradeOptions = { LevelUpgradeOption.Speed, LevelUpgradeOption.Health, LevelUpgradeOption.Damage };
    private LevelUpgradeOption[] currentLevelOptions = new LevelUpgradeOption[2];

    private UpgradeSystem.PlayerUpgradeOption[] currentPlayerOptions = new UpgradeSystem.PlayerUpgradeOption[3]; // Updated to use UpgradeSystem enum

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

        if (playerController != null && shooting == null)
        {
            shooting = playerController.shooting;
            if (shooting == null)
            {
                Debug.LogError("Shooting reference not set in CanvasController! Ensure PlayerController has assigned Shooting.");
            }
            else
            {
                Debug.Log("Shooting reference successfully set in CanvasController!");
            }
        }

        if (upgradeSystem == null)
        {
            upgradeSystem = FindObjectOfType<UpgradeSystem>();
            if (upgradeSystem == null)
            {
                Debug.LogError("CanvasController: UpgradeSystem not found!");
            }
        }

        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (upgradeDataPanel != null) upgradeDataPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (playerController != null && coinText != null)
        {
            coinText.text = $"Coins: {playerController.GetMoney()}";
        }
    }

    void Update()
    {
        if (shooting != null && displayText != null)
        {
            if (shooting.IsReloading())
            {
                displayText.text = "Reloading...";
            }
            else
            {
                int currentAmmo = shooting.GetCurrentAmmo();
                string ammoString = $"{currentAmmo}/{shooting.GetMagazineSize()}";
                displayText.text = ammoString;
            }
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

        if (playerController != null && coinText != null)
        {
            coinText.text = $"Coins: {playerController.GetMoney()}";
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !isShowingPanel)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
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

    // Player level-up panel
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
        if (shooting != null)
        {
            shooting.UpgradeBulletDamage();
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
        if (upgradeDataPanel != null && upgradeSystem != null)
        {
            upgradeDataPanel.SetActive(true);
            Time.timeScale = 0f;

            currentPlayerOptions = upgradeSystem.GetRandomUpgradeOptions(3);

            upgradeOption1Text.text = upgradeSystem.GetPlayerUpgradeText(currentPlayerOptions[0]);
            upgradeOption2Text.text = upgradeSystem.GetPlayerUpgradeText(currentPlayerOptions[1]);
            upgradeOption3Text.text = upgradeSystem.GetPlayerUpgradeText(currentPlayerOptions[2]);

            upgradeOption1Button.onClick.RemoveAllListeners();
            upgradeOption2Button.onClick.RemoveAllListeners();
            upgradeOption3Button.onClick.RemoveAllListeners();
            upgradeOption1Button.onClick.AddListener(() => ApplyPlayerUpgrade(currentPlayerOptions[0]));
            upgradeOption2Button.onClick.AddListener(() => ApplyPlayerUpgrade(currentPlayerOptions[1]));
            upgradeOption3Button.onClick.AddListener(() => ApplyPlayerUpgrade(currentPlayerOptions[2]));
        }
    }

    void ApplyPlayerUpgrade(UpgradeSystem.PlayerUpgradeOption option) // Updated to use UpgradeSystem enum
    {
        if (upgradeSystem != null)
        {
            upgradeSystem.ApplyPlayerUpgrade(option);
            upgradeDataPanel.SetActive(false);
            ShowLevelCompletePanel();
        }
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

    // Pause panel methods
    void PauseGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
            Debug.Log("Game Paused");
        }
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
            Debug.Log("Game Resumed");
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Game Quit");
    }
}