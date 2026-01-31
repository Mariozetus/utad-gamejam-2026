using UnityEngine;

public enum StatType { Health, Strength, Speed }

public enum IncreaseMode
{
    Flat,            
    PercentCurrent   
}

[CreateAssetMenu(fileName = "StatConfig", menuName = "Upgrade/Stat Config", order = 1)]
public class StatConfigSO : ScriptableObject
{
    [Header("ID")]
    public StatType statType;

    [Header("UI Texts")]
    public string upgradeTitle;
    [TextArea(2, 5)] public string description;

    [Header("UI Icon")]
    public Sprite statIcon;

    [Header("Increase Rule")]
    public IncreaseMode increaseMode = IncreaseMode.Flat;

    [Tooltip("Flat: +baseFlat\nPercentCurrent: +current * basePercent")]
    public float baseFlat = 10f;

    [Range(0f, 2f)]
    public float basePercent = 0.10f;

}