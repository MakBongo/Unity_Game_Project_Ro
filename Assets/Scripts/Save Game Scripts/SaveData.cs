using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int money;
    public int highestRound;
    public int highestLevel;
    public int highestScore; // Changed from score to highestScore
    // Shop upgrade multipliers
    public float ammunitionSpeedMultiplier;
    public float firesPerMinuteMultiplier;
    public float ammunitionLifetimeMultiplier;
    public float magazineSizeMultiplier;
    public float reloadTimeMultiplier;
    public float healRateMultiplier;
    public float expMultiplier;
    public float moneyMultiplier;
}