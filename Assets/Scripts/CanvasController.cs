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
    public Text scoreText; // Text field for displaying score
    public Text bossTimerText; // Text field for displaying Boss Round timer

    [Header("Level Up UI")]
    public GameObject upgradePanel;
    public UpgradeSystem upgradeSystem;

    [Header("Round Complete Panel")]
    public GameObject upgradeDataPanel;
    public GameObject enemiesUpgradePanel;

    [Header("Game Over UI")]
    public GameObject gameOverPanel; // Reference to Game Over panel
    public Text gameOverScoreText; // Text field for displaying current score in Game Over panel
    public Text gameOverHighestScoreText; // Text field for displaying highest score in Game Over panel

    [Header("Scene Complete UI")]
    public GameObject sceneCompletePanel; // Reference to Scene Complete panel
    public Text sceneCompleteScoreText; // Text field for displaying current score in Scene Complete panel
    public Text sceneCompleteHighestScoreText; // Text field for displaying highest score in Scene Complete panel

    [Header("Random Upgrades Button")]
    public Transform playerUpgradeButtonParent; // Parent for dynamic player upgrade buttons
    public GameObject playerUpgradeButtonPrefab; // Prefab with Button and Text
    private List<Button> playerUpgradeButtons = new List<Button>(); // Track instantiated buttons
    private List<Text> playerUpgradeTexts = new List<Text>(); // Track text components

    [Header("Enemies Upgrades Button")]
    public Transform enemyUpgradeButtonParent; // Parent for dynamic enemy upgrade buttons
    public GameObject enemyUpgradeButtonPrefab; // Prefab with Button and Text
    private List<Button> enemyUpgradeButtons = new List<Button>(); // Track instantiated buttons
    private List<Text> enemyUpgradeTexts = new List<Text>(); // Track text components

    [Header("Reselect Button")]
    public Button reselectButton; // Button for reselecting upgrades

    [Header("Audio")]
    public AudioClip buttonClickSound; // Sound to play when a button is clicked
    private AudioSource audioSource; // AudioSource for playing button sounds

    [Header("Pause UI")]
    public GameObject pausePanel;

    private enum RoundUpgradeOption { Speed, Health, Damage }
    private RoundUpgradeOption[] roundUpgradeOptions = { RoundUpgradeOption.Speed, RoundUpgradeOption.Health, RoundUpgradeOption.Damage };
    private RoundUpgradeOption[] currentRoundOptions = new RoundUpgradeOption[2];

    private UpgradeSystem.PlayerUpgradeOption[] currentPlayerOptions = new UpgradeSystem.PlayerUpgradeOption[3];

    private RoundManager roundManager;
    private Queue<string> panelQueue = new Queue<string>();
    private bool isShowingPanel = false;
    private float timerElapsed = 0f; // Track elapsed time for Boss Round timer

    void Start()
    {
        // Initialize AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        if (buttonClickSound != null)
        {
            audioSource.clip = buttonClickSound;
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
        }

        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (upgradeDataPanel != null) upgradeDataPanel.SetActive(false);
        if (enemiesUpgradePanel != null) enemiesUpgradePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (sceneCompletePanel != null) sceneCompletePanel.SetActive(false); // Initialize Scene Complete panel

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

        // Initialize score text
        if (roundManager != null && scoreText != null)
        {
            scoreText.text = $"Score: {roundManager.GetScore()}";
        }
        else if (scoreText == null)
        {
            Debug.LogWarning("CanvasController: ScoreText reference not set in Inspector!");
        }

        // Initialize boss timer text
        if (bossTimerText == null)
        {
            Debug.LogWarning("CanvasController: BossTimerText reference not set in Inspector!");
        }

        // Initialize Game Over score texts
        if (gameOverScoreText == null)
        {
            Debug.LogWarning("CanvasController: GameOverScoreText reference not set in Inspector!");
        }
        if (gameOverHighestScoreText == null)
        {
            Debug.LogWarning("CanvasController: GameOverHighestScoreText reference not set in Inspector!");
        }

        // Initialize Scene Complete score texts
        if (sceneCompleteScoreText == null)
        {
            Debug.LogWarning("CanvasController: SceneCompleteScoreText reference not set in Inspector!");
        }
        if (sceneCompleteHighestScoreText == null)
        {
            Debug.LogWarning("CanvasController: SceneCompleteHighestScoreText reference not set in Inspector!");
        }

        // Set up reselect button listener
        if (reselectButton != null)
        {
            reselectButton.onClick.AddListener(() =>
            {
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.Play();
                }
                ReselectUpgrades();
            });
            UpdateReselectButtonState();
        }

        // Initialize upgrade buttons
        SetupPlayerUpgradeButtons();
        SetupEnemyUpgradeButtons();
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

        // Update score text
        if (roundManager != null && scoreText != null)
        {
            scoreText.text = $"Score: {roundManager.GetScore()}";
        }

        // Update boss timer text
        if (roundManager != null && bossTimerText != null)
        {
            if (roundManager.isBossRoundTriggered)
            {
                bossTimerText.text = "Next round is Boss Round";
            }
            else
            {
                timerElapsed += Time.deltaTime;
                float timeRemaining = Mathf.Max(0f, roundManager.bossRoundTimer - timerElapsed);
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                bossTimerText.text = $"Boss Round In: {minutes:D2}:{seconds:D2}";
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !isShowingPanel)
        {
            if (pausePanel != null && pausePanel.activeSelf)
            {
                ResumeGame();
            }
            else if ((gameOverPanel == null || !gameOverPanel.activeSelf) && (sceneCompletePanel == null || !sceneCompletePanel.activeSelf)) // Prevent pause during Game Over or Scene Complete
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
            case "GameOver":
                ShowGameOverPanel();
                break;
            case "SceneComplete":
                ShowSceneCompletePanel();
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
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        if (playerController != null)
        {
            playerController.UpgradeMaxHealth();
            CloseUpgradePanel();
        }
    }

    public void OnDamageButtonClicked()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        if (shooting != null)
        {
            shooting.UpgradeAmmunitionDamage();
            CloseUpgradePanel();
        }
    }

    public void OnSpeedButtonClicked()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
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

    // Game Over panel
    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;

            // Update score texts
            if (roundManager != null && gameOverScoreText != null)
            {
                gameOverScoreText.text = $"Score: {roundManager.GetScore()}";
            }
            if (gameOverHighestScoreText != null)
            {
                int highestScore = SaveGameManager.Instance != null ? SaveGameManager.Instance.GetHighestScore() : 0;
                gameOverHighestScoreText.text = $"Highest Score: {highestScore}";
            }

            Debug.Log("CanvasController: Game Over panel shown.");
        }
    }

    // Scene Complete panel
    public void ShowSceneCompletePanel()
    {
        if (sceneCompletePanel != null)
        {
            sceneCompletePanel.SetActive(true);
            Time.timeScale = 0f;

            // Update score texts
            if (roundManager != null && sceneCompleteScoreText != null)
            {
                sceneCompleteScoreText.text = $"Score: {roundManager.GetScore()}";
            }
            if (sceneCompleteHighestScoreText != null)
            {
                int highestScore = SaveGameManager.Instance != null ? SaveGameManager.Instance.GetHighestScore() : 0;
                sceneCompleteHighestScoreText.text = $"Highest Score: {highestScore}";
            }

            Debug.Log("CanvasController: Scene Complete panel shown.");
        }
    }

    public void ContinueGame()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        if (sceneCompletePanel != null)
        {
            sceneCompletePanel.SetActive(false);
            Time.timeScale = 1f;
            isShowingPanel = false;
            if (roundManager != null)
            {
                roundManager.ContinueToNextRound();
            }
            Debug.Log("CanvasController: Continuing to next round.");
        }
    }

    public void ReturnToMainMenu()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        Debug.Log("CanvasController: Returning to Main Menu.");
    }

    public void RestartGame()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            // Delay scene reload to allow sound to play
            StartCoroutine(DelayedRestart(audioSource.clip.length));
        }
        else
        {
            // If no sound, restart immediately
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Debug.Log("CanvasController: Game Restarted");
        }
    }

    private System.Collections.IEnumerator DelayedRestart(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use real-time to ignore time scale
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("CanvasController: Game Restarted");
    }

    // Reset Boss Round timer display
    public void ResetBossTimer()
    {
        timerElapsed = 0f;
        Debug.Log("CanvasController: Boss Round timer reset.");
    }

    // Setup dynamic player upgrade buttons
    void SetupPlayerUpgradeButtons()
    {
        if (playerUpgradeButtonParent == null || playerUpgradeButtonPrefab == null)
        {
            Debug.LogError("CanvasController: PlayerUpgradeButtonParent or PlayerUpgradeButtonPrefab not assigned!");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in playerUpgradeButtonParent)
        {
            Destroy(child.gameObject);
        }
        playerUpgradeButtons.Clear();
        playerUpgradeTexts.Clear();

        // Create three buttons for player upgrades
        for (int i = 0; i < 3; i++)
        {
            GameObject buttonObj = Instantiate(playerUpgradeButtonPrefab, playerUpgradeButtonParent);
            Button button = buttonObj.GetComponent<Button>();
            Text text = buttonObj.GetComponentInChildren<Text>();

            if (button == null)
            {
                Debug.LogWarning($"CanvasController: Player upgrade button prefab {buttonObj.name} missing Button component!");
                continue;
            }
            if (text == null)
            {
                Debug.LogWarning($"CanvasController: Player upgrade button {buttonObj.name} missing Text component!");
                continue;
            }

            playerUpgradeButtons.Add(button);
            playerUpgradeTexts.Add(text);
        }

        Debug.Log("CanvasController: Created 3 player upgrade buttons");
    }

    // Setup dynamic enemy upgrade buttons
    void SetupEnemyUpgradeButtons()
    {
        if (enemyUpgradeButtonParent == null || enemyUpgradeButtonPrefab == null)
        {
            Debug.LogError("CanvasController: EnemyUpgradeButtonParent or EnemyUpgradeButtonPrefab not assigned!");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in enemyUpgradeButtonParent)
        {
            Destroy(child.gameObject);
        }
        enemyUpgradeButtons.Clear();
        enemyUpgradeTexts.Clear();

        // Create two buttons for enemy upgrades
        for (int i = 0; i < 2; i++)
        {
            GameObject buttonObj = Instantiate(enemyUpgradeButtonPrefab, enemyUpgradeButtonParent);
            Button button = buttonObj.GetComponent<Button>();
            Text text = buttonObj.GetComponentInChildren<Text>();

            if (button == null)
            {
                Debug.LogWarning($"CanvasController: Enemy upgrade button prefab {buttonObj.name} missing Button component!");
                continue;
            }
            if (text == null)
            {
                Debug.LogWarning($"CanvasController: Enemy upgrade button {buttonObj.name} missing Text component!");
                continue;
            }

            enemyUpgradeButtons.Add(button);
            enemyUpgradeTexts.Add(text);
        }

        Debug.Log("CanvasController: Created 2 enemy upgrade buttons");
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
        }
    }

    void RefreshUpgradeOptions()
    {
        if (upgradeSystem == null) return;

        currentPlayerOptions = upgradeSystem.GetRandomUpgradeOptions(3);

        for (int i = 0; i < playerUpgradeButtons.Count; i++)
        {
            if (i < currentPlayerOptions.Length)
            {
                playerUpgradeTexts[i].text = upgradeSystem.GetPlayerUpgradeText(currentPlayerOptions[i]);
                int index = i; // Capture index for listener
                playerUpgradeButtons[i].onClick.RemoveAllListeners();
                playerUpgradeButtons[i].onClick.AddListener(() =>
                {
                    if (audioSource != null && audioSource.clip != null)
                    {
                        audioSource.Play();
                    }
                    ApplyPlayerUpgrade(currentPlayerOptions[index]);
                });
            }
        }

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

            for (int i = 0; i < enemyUpgradeButtons.Count; i++)
            {
                if (i < currentRoundOptions.Length)
                {
                    enemyUpgradeTexts[i].text = GetRoundUpgradeText(currentRoundOptions[i]);
                    int index = i; // Capture index for listener
                    enemyUpgradeButtons[i].onClick.RemoveAllListeners();
                    enemyUpgradeButtons[i].onClick.AddListener(() =>
                    {
                        if (audioSource != null && audioSource.clip != null)
                        {
                            audioSource.Play();
                        }
                        ApplyRoundUpgrade(currentRoundOptions[index]);
                    });
                }
            }
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
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            Debug.Log("Game Resumed");
        }
    }

    public void QuitGame()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Game Quit");
    }
}