using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Stats")]
    public int ammunitionDamage = 10;    // Renamed from bulletDamage
    public float ammunitionSpeed = 20f;  // Renamed from bulletSpeed
    public float firesPerMinute = 300f;
    public float ammunitionLifetime = 2f; // Renamed from bulletLifetime
    public int magazineSize = 30;
    public float reloadTime = 2f;
}