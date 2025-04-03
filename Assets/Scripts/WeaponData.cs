using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Stats")]
    public int bulletDamage = 10;
    public float bulletSpeed = 20f;
    public float firesPerMinute = 300f;
    public float bulletLifetime = 2f;
    public int magazineSize = 30;
    public float reloadTime = 2f;
}