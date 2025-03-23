using UnityEngine;
using UnityEngine.UI; // For regular UI Text

public class CanvasController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController; // Reference to PlayerController
    public Text displayText;                  // Single UI Text for both ammo and reload status

    void Update()
    {
        if (playerController != null && displayText != null)
        {
            int currentAmmo = playerController.GetCurrentAmmo();
            string ammoString = $"Ammo: {currentAmmo}/{playerController.magazineSize}";

            // Show reload status if reloading, otherwise just show ammo
            displayText.text = playerController.IsReloading() ? $"{ammoString} - Reloading..." : ammoString;
        }
    }
}