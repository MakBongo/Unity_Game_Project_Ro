using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Add your collision logic here if needed
        // For example: if (collision.CompareTag("Enemy")) { Destroy(collision.gameObject); }

        // Note: We don't destroy the bullet here anymore since it's pooled
        gameObject.SetActive(false);  // Deactivate instead of destroying
    }
}