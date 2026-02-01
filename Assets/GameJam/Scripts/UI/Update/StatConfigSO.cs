using UnityEngine;

public enum StatType { Health, Strength, Speed }

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
}