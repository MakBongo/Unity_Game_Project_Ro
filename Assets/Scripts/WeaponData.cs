using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Stats")]
    public GameObject ammunitionPrefab;  // Moved from Shooting class
    public int ammunitionDamage = 10;
    public float ammunitionSpeed = 20f;
    public float firesPerMinute = 300f;
    public float ammunitionLifetime = 2f;
    public int magazineSize = 30;
    public float reloadTime = 2f;
}