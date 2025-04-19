using UnityEngine;

public class Coin : MonoBehaviour
{
    public int moneyValue = 5;

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.AddMoney(moneyValue);
            Destroy(gameObject);
            Debug.Log($"Player picked up {moneyValue} money from coin!");
        }
    }
}