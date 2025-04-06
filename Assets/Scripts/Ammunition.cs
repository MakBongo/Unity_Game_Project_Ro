using UnityEngine;

public abstract class Ammunition : MonoBehaviour
{
    public int damage; // Damage set by Shooting
    public PlayerController player; // Reference to player, set by Shooting
}