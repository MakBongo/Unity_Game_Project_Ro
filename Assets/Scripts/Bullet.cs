using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public PlayerController player; // Reference to player to award EXP

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore collision with the player who fired this bullet
        if (collision.gameObject.GetComponent<PlayerController>() == player)
        {
            return; // Do nothing if it's the player
        }

        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            if (enemy.IsDead() && player != null) // Check if enemy died
            {
                player.AddExp(enemy.expValue); // Award EXP
            }
        }
        gameObject.SetActive(false);
    }
}