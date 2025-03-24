using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController; // Reference to PlayerController
    public Text displayText;                  // UI Text for ammo and reload status
    public Slider healthSlider;               // UI Slider for health
    public Slider expSlider;                  // UI Slider for experience

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
            healthSlider.value = playerController.GetCurrentHealth();
        }

        // Update EXP slider
        if (playerController != null && expSlider != null)
        {
            expSlider.maxValue = playerController.GetMaxExp(); // Update max EXP in case it changes
            expSlider.value = playerController.GetCurrentExp();
        }
    }
}