using UnityEngine;

public static class GameData
{
    private static WeaponData selectedWeapon;

    public static void SetSelectedWeapon(WeaponData weapon)
    {
        selectedWeapon = weapon;
        Debug.Log($"GameData: Selected weapon set to {weapon?.name ?? "null"}");
    }

    public static WeaponData GetSelectedWeapon()
    {
        return selectedWeapon;
    }

    public static void ClearSelectedWeapon()
    {
        selectedWeapon = null;
    }
}