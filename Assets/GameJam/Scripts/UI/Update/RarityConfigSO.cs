using UnityEngine;

public enum RarityType { Common, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "RarityConfig", menuName = "Upgrade/Rarity Config", order = 0)]
public class RarityConfigSO : ScriptableObject
{
    [Header("ID")]
    public RarityType rarity;

    [Header("UI")]
    public string rarityText;
    public Color titleColor = Color.white;
    public Color borderColor = Color.white;

    public Sprite rarityFxSprite;
    public Sprite rarityFrameSprite;
    public Sprite rarityBackgroundSprite;

    [Header("Stat Scaling")]
    public float statMultiplier = 1f;

}