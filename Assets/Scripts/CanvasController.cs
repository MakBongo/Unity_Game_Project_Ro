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
    public Text coinText;
    public Text roundText; // Text field for displaying round number
    public Text levelText; // Text field for displaying player level

    [Header("Level Up UI")]
    public GameObject upgradePanel;
    public UpgradeSystem upgradeSystem;

    [Header("Round Complete Panel")]
    public GameObject upgradeDataPanel;
    public GameObject enemiesUpgradePanel;

    [Header("Random Upgrades Button")]
    public Text upgradeOption1Text;
    public Text upgradeOption2Text;
    public Text upgradeOption3Text;
    public Button upgradeOption1Button;
    public Button upgradeOption2Button;
    public Button upgradeOption3Button;
    public Button reselectButton; // Button for reselecting upgrades

    [Header("Enemies Upgrades Button")]
    public Text enemiesUpgradesOption1Text;
    public Text enemiesUpgradesOption2Text;
    public Button enemiesUpgradesOption1Button;
    public Button enemiesUpgradesOption2Button;

    [Header("Pause UI")]
    public GameObject pausePanel;

    private enum RoundUpgradeOption { Speed, Health, Damage }
    private RoundUpgradeOption[] roundUpgradeOptions = { RoundUpgradeOption.Speed, RoundUpgradeOption.Health, RoundUpgradeOption.Damage };
    private RoundUpgradeOption[] currentRoundOptions = new RoundUpgradeOption[2];

    private UpgradeSystem.PlayerUpgradeOption[] currentPlayerOptions = new UpgradeSystem.PlayerUpgradeOption[3];

    private RoundManager roundManager;
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

        if (roundManager == null)
        {
            roundManager = FindObjectOfType<RoundManager>();
            if (roundManager == null)
            {
                Debug.LogError("CanvasController: RoundManager not found!");
            }
        }

        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (upgradeDataPanel != null) upgradeDataPanel.SetActive(false);
        if (enemiesUpgradePanel != null) enemiesUpgradePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (playerController != null && coinText != null)
        {
            coinText.text = $"Coins: {playerController.GetMoney()}";
        }

        // Initialize round text
        if (roundManager != null && roundText != null)
        {
            roundText.text = $"Round: {roundManager.GetCurrentRound()}";
        }

        // Initialize level text
        if (playerController != null && levelText != null)
        {
            levelText.text = $"Level: {playerController.GetLevel()}";
        }

        // Set up reselect button listener
        if (reselectButton != null)
        {
            reselectButton.onClick.AddListener(ReselectUpgrades);
            UpdateReselectButtonState();
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
        if (roundManager != null && roundText != null)
        {
            roundText.text = $"Round: {roundManager.GetCurrentRound()}";
        }

        // Update level text
        if (playerController != null && levelText != null)
        {
            levelText.text = $"Level: {playerController.GetLevel()}";
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !isShowingPanel)
        {
            if (pausePanel != null && pausePanel.activeSelf)
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

        // Update reselect button state in case coins change
        UpdateReselectButtonState();
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
                roundManager = FindObjectOfType<RoundManager>();
                if (roundManager != null)
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
            shooting.UpgradeAmmunitionDamage();
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
            enemiesUpgradePanel.SetActive(false); // Ensure enemy upgrade panel is hidden
            Time.timeScale = 0f;

            // Reset reselect state
            upgradeSystem.ResetReselectState();
            RefreshUpgradeOptions();

            upgradeOption1Button.onClick.RemoveAllListeners();
            upgradeOption2Button.onClick.RemoveAllListeners();
            upgradeOption3Button.onClick.RemoveAllListeners();
            upgradeOption1Button.onClick.AddListener(() => ApplyPlayerUpgrade(currentPlayerOptions[0]));
            upgradeOption2Button.onClick.AddListener(() => ApplyPlayerUpgrade(currentPlayerOptions[1]));
            upgradeOption3Button.onClick.AddListener(() => ApplyPlayerUpgrade(currentPlayerOptions[2]));
        }
    }

    void RefreshUpgradeOptions()
    {
        currentPlayerOptions = upgradeSystem.GetRandomUpgradeOptions(3);

        upgradeOption1Text.text = upgradeSystem.GetPlayerUpgradeText(currentPlayerOptions[0]);
        upgradeOption2Text.text = upgradeSystem.GetPlayerUpgradeText(currentPlayerOptions[1]);
        upgradeOption3Text.text = upgradeSystem.GetPlayerUpgradeText(currentPlayerOptions[2]);

        UpdateReselectButtonState();
    }

    void ReselectUpgrades()
    {
        if (upgradeSystem != null && upgradeSystem.TryReselectUpgrades())
        {
            RefreshUpgradeOptions(); // Get new upgrade options
        }
    }

    void UpdateReselectButtonState()
    {
        if (reselectButton != null && upgradeSystem != null)
        {
            bool canAfford = upgradeSystem.CanAffordReselect();
            reselectButton.interactable = canAfford;
            Text buttonText = reselectButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = $"Reselect Upgrades (Cost: {upgradeSystem.GetCurrentReselectCost()} Coins)";
            }
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
        if (enemiesUpgradePanel != null && roundManager != null)
        {
            enemiesUpgradePanel.SetActive(true);
            Time.timeScale = 0f;

            currentRoundOptions[0] = roundUpgradeOptions[Random.Range(0, roundUpgradeOptions.Length)];
            do
            {
                currentRoundOptions[1] = roundUpgradeOptions[Random.Range(0, roundUpgradeOptions.Length)];
            } while (currentRoundOptions[1] == currentRoundOptions[0]);

            enemiesUpgradesOption1Text.text = GetRoundUpgradeText(currentRoundOptions[0]);
            enemiesUpgradesOption2Text.text = GetRoundUpgradeText(currentRoundOptions[1]);

            enemiesUpgradesOption1Button.onClick.RemoveAllListeners();
            enemiesUpgradesOption2Button.onClick.RemoveAllListeners();
            enemiesUpgradesOption1Button.onClick.AddListener(() => ApplyRoundUpgrade(currentRoundOptions[0]));
            enemiesUpgradesOption2Button.onClick.AddListener(() => ApplyRoundUpgrade(currentRoundOptions[1]));
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
        if (roundManager != null)
        {
            roundManager.ApplyUpgrade(option.ToString());
            enemiesUpgradePanel.SetActive(false);
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
            Debug.Log("Game Paused");
        }
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
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