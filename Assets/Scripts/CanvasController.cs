using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public Text displayText;
    public Slider healthSlider;
    public Slider expSlider;
    public GameObject upgradePanel; // UI panel for upgrades

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

        // Ensure upgrade panel is hidden at start
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
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
            healthSlider.maxValue = playerController.GetMaxHealth(); // Update in case maxHealth changes
            healthSlider.value = playerController.GetCurrentHealth();
        }

        // Update EXP slider
        if (playerController != null && expSlider != null)
        {
            expSlider.maxValue = playerController.GetMaxExp();
            expSlider.value = playerController.GetCurrentExp();
        }
    }

    public void ShowUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            Time.timeScale = 0f; // Pause game
        }
    }

    // Button methods
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
            Time.timeScale = 1f; // Resume game
        }
    }
}