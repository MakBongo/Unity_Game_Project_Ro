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
    public Text roundText; // New: Text field for displaying round number
    public UpgradeSystem upgradeSystem;

    [Header("Round Complete UI")]
    public GameObject upgradeDataPanel;
    public Text upgradeOption1Text;
    public Text upgradeOption2Text;
    public Text upgradeOption3Text;
    public Button upgradeOption1Button;
    public Button upgradeOption2Button;
    public Button upgradeOption3Button;
    public GameObject roundCompletePanel;
    public Text option1Text;
    public Text option2Text;
    public Button option1Button;
    public Button option2Button;

    [Header("Pause UI")]
    public GameObject pausePanel;
    private bool isPaused = false;

    private enum RoundUpgradeOption { Speed, Health, Damage }
    private RoundUpgradeOption[] roundUpgradeOptions = { RoundUpgradeOption.Speed, RoundUpgradeOption.Health, RoundUpgradeOption.Damage };
    private RoundUpgradeOption[] currentRoundOptions = new RoundUpgradeOption[2];

    private UpgradeSystem.PlayerUpgradeOption[] currentPlayerOptions = new UpgradeSystem.PlayerUpgradeOption[3];

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

        // Find SceneManager at start
        if (sceneManager == null)
        {
            sceneManager = FindObjectOfType<SceneManager>();
            if (sceneManager == null)
            {
                Debug.LogError("CanvasController: SceneManager not found!");
            }
        }

        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (upgradeDataPanel != null) upgradeDataPanel.SetActive(false);
        if (roundCompletePanel != null) roundCompletePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (playerController != null && coinText != null)
        {
            coinText.text = $"Coins: {playerController.GetMoney()}";
        }

        // Initialize round text
        if (sceneManager != null && roundText != null)
        {
            roundText.text = $"Round: {sceneManager.GetCurrentRound()}";
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

        // Update round text
        if (sceneManager != null && roundText != null)
        {
            roundText.text = $"Round: {sceneManager.GetCurrentRound()}";
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
            case "RoundFinished":
                sceneManager = FindObjectOfType<SceneManager>();
                if (sceneManager != null)
                {
                    ShowUpgradeDataPanel(); // Show player upgrades first
                }
                break;
        }
    }

    // Player level-up panel (for player leveling up, not round completion)
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

    // Player upgrade panel (for round completion)
    public void ShowUpgradeDataPanel()
    {
        if (upgradeDataPanel != null && upgradeSystem != null)
        {
            upgradeDataPanel.SetActive(true);
            roundCompletePanel.SetActive(false); // Ensure enemy upgrade panel is hidden
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

    void ApplyPlayerUpgrade(UpgradeSystem.PlayerUpgradeOption option)
    {
        if (upgradeSystem != null)
        {
            upgradeSystem.ApplyPlayerUpgrade(option);
            upgradeDataPanel.SetActive(false);
            ShowRoundCompletePanel(); // Proceed to enemy upgrades after player upgrade
        }
    }

    // Round complete panel (enemy upgrades)
    public void ShowRoundCompletePanel()
    {
        if (roundCompletePanel != null && sceneManager != null)
        {
            roundCompletePanel.SetActive(true);
            Time.timeScale = 0f;

            currentRoundOptions[0] = roundUpgradeOptions[Random.Range(0, roundUpgradeOptions.Length)];
            do
            {
                currentRoundOptions[1] = roundUpgradeOptions[Random.Range(0, roundUpgradeOptions.Length)];
            } while (currentRoundOptions[1] == currentRoundOptions[0]);

            option1Text.text = GetRoundUpgradeText(currentRoundOptions[0]);
            option2Text.text = GetRoundUpgradeText(currentRoundOptions[1]);

            option1Button.onClick.RemoveAllListeners();
            option2Button.onClick.RemoveAllListeners();
            option1Button.onClick.AddListener(() => ApplyRoundUpgrade(currentRoundOptions[0]));
            option2Button.onClick.AddListener(() => ApplyRoundUpgrade(currentRoundOptions[1]));
        }
    }

    string GetRoundUpgradeText(RoundUpgradeOption option)
    {
        switch (option)
        {
            case RoundUpgradeOption.Speed: return "Enemy Speed +20%";
            case RoundUpgradeOption.Health: return "Enemy Health +20%";
            case RoundUpgradeOption.Damage: return "Enemy Damage +20%";
            default: return "";
        }
    }

    void ApplyRoundUpgrade(RoundUpgradeOption option)
    {
        if (sceneManager != null)
        {
            sceneManager.ApplyUpgrade(option.ToString());
            roundCompletePanel.SetActive(false);
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