
using UnityEngine;


    public enum RarityType { Common, Rare, Epic, Legendary }

    [CreateAssetMenu(fileName = "RarityConfig", menuName = "Upgrade/Rarity Config", order = 0)]
    public class RarityConfigSO : ScriptableObject
    {
        [Header("ID")]
        public RarityType rarity;

        [Header("UI")]
        public string rarityText;          
        public Sprite rarityFxSprite;      
        public Sprite rarityFrameSprite;   
        public Sprite rarityRareBackgroundSprite;

        [Header("Upgrade Rule")]
        [Range(0f, 1f)] public float upgradePercent = 0.05f;
        [Range(1, 10)] public int levelIncrease = 1;      
    }


