using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public Slider healthSlider;         // The UI Slider component
    public Vector3 offset = new Vector3(0f, 1f, 0f); // Offset above the enemy

    private Enemy enemy;                // Reference to the Enemy script
    private Canvas canvas;              // Reference to the Canvas component

    void Start()
    {
        // Get the Enemy component from the parent GameObject
        enemy = GetComponentInParent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("EnemyHealthBar must be a child of an Enemy GameObject with an Enemy script!");
            Destroy(gameObject);
            return;
        }

        // Get the Canvas component
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("EnemyHealthBar requires a Canvas component!");
            Destroy(gameObject);
            return;
        }

        // Set the Canvas to World Space
        canvas.renderMode = RenderMode.WorldSpace;

        // Configure the slider
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = enemy.maxHealth;
            healthSlider.value = enemy.maxHealth; // Initial health
        }
        else
        {
            Debug.LogError("Health Slider not assigned in EnemyHealthBar!");
        }
    }

    void Update()
    {
        // Update the slider value based on the enemy's current health
        if (enemy != null && healthSlider != null)
        {
            healthSlider.value = GetCurrentHealth();
        }

        // Position the health bar above the enemy
        if (enemy != null)
        {
            transform.position = enemy.transform.position + offset;
        }
    }

    // Helper method to get the enemy's current health
    private float GetCurrentHealth()
    {
        // Assuming Enemy script has a public method or property to get current health
        // Since Enemy uses private currentHealth, we'll need to add a getter
        return enemy.GetCurrentHealth();
    }
}