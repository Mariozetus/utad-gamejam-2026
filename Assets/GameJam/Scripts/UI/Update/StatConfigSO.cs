using UnityEngine;


    public enum StatType { Health, Strength, Speed }

    [CreateAssetMenu(fileName = "StatConfig", menuName = "Upgrade/Stat Config", order = 1)]
    public class StatConfigSO : ScriptableObject
    {
        [Header("ID")]
        public StatType statType;

        [Header("UI Texts")]
        public string upgradeTitle;       
        [TextArea(2,5)] public string description; 
        public string statLabel;           

        [Header("UI Icon")]
        public Sprite statIcon;           

        [Header("Base Value")]
        public float baseValue = 100f;
    }

