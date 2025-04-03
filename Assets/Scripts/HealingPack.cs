using UnityEngine;

public class HealingPack : MonoBehaviour
{
    [Header("Healing Settings")]
    private float healPercentage = 0.25f; // 25% of max health

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            // Calculate heal amount based on player's max health
            int maxHealth = player.GetMaxHealth();
            float healAmount = maxHealth * healPercentage;
            int roundedHealAmount = Mathf.RoundToInt(healAmount);

            // Heal the player
            int currentHealth = player.GetCurrentHealth();
            int newHealth = Mathf.RoundToInt(Mathf.Clamp(currentHealth + healAmount, 0, maxHealth));
            player.SetCurrentHealth(newHealth); // We'll add this method to PlayerController

            Debug.Log($"Healing Pack restored {roundedHealAmount} health. Player health: {newHealth}/{maxHealth}");

            // Destroy the healing pack after use
            Destroy(gameObject);
        }
    }
}