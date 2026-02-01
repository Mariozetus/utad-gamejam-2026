using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradeSelectionManager : MonoBehaviour
{
    [System.Serializable]
    private struct RarityWeight
    {
        public RarityType rarity;
        [Min(0f)] public float weight;
    }

    [Header("Canvas Root")]
    [SerializeField] private GameObject canvasRoot;

    [Header("Cards (3)")]
    [SerializeField] private UpgradeCardUI[] cards;

    [Header("Fixed Stat Configs")]
    [SerializeField] private StatConfigSO healthStat;
    [SerializeField] private StatConfigSO strengthStat;
    [SerializeField] private StatConfigSO speedStat;

    [Header("Rarity Pool (4)")]
    [SerializeField] private RarityConfigSO[] rarityPool;
    [SerializeField] private RarityWeight[] rarityWeights = new[]
    {
        new RarityWeight { rarity = RarityType.Common, weight = 40f },
        new RarityWeight { rarity = RarityType.Rare, weight = 30f },
        new RarityWeight { rarity = RarityType.Epic, weight = 20f },
        new RarityWeight { rarity = RarityType.Legendary, weight = 10f },
    };

    [Header("Apply Target (Player)")]
    [SerializeField] private UpgradeApplier applier;

    private bool _isOpen;

    /* testing cursor visibility
    private void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }*/

    private void Awake()
    {
        if (applier == null) applier = FindFirstObjectByType<UpgradeApplier>();
        if (canvasRoot != null) canvasRoot.SetActive(false);
        //Testing
        //Show();
    }

    public void Show()
    {
        if (_isOpen) return;
        _isOpen = true;

        if (canvasRoot != null) canvasRoot.SetActive(true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        InputManager.Instance?.EnableUiInputActions();

        SetupCard(0, healthStat);
        SetupCard(1, strengthStat);
        SetupCard(2, speedStat);
    }

    private void SetupCard(int index, StatConfigSO stat)
    {
        if (cards == null || index < 0 || index >= cards.Length) return;
        if (cards[index] == null || stat == null) return;

        var rarity = RollRarity();
        cards[index].Setup(this, applier, stat, rarity);
    }

    private RarityConfigSO RollRarity()
    {
        if (rarityWeights == null || rarityWeights.Length == 0)
            return GetRarity(RarityType.Common);

        float total = 0f;
        for (int i = 0; i < rarityWeights.Length; i++)
            total += Mathf.Max(0f, rarityWeights[i].weight);

        if (total <= 0f)
            return GetRarity(RarityType.Common);

        float roll = Random.Range(0f, total);
        for (int i = 0; i < rarityWeights.Length; i++)
        {
            float w = Mathf.Max(0f, rarityWeights[i].weight);
            if (roll < w)
                return GetRarity(rarityWeights[i].rarity);
            roll -= w;
        }

        return GetRarity(rarityWeights[rarityWeights.Length - 1].rarity);
    }

    private RarityConfigSO GetRarity(RarityType type)
    {
        if (rarityPool == null) return null;
        for (int i = 0; i < rarityPool.Length; i++)
        {
            if (rarityPool[i] != null && rarityPool[i].rarity == type)
                return rarityPool[i];
        }
        return rarityPool.Length > 0 ? rarityPool[0] : null;
    }


    public void Select(StatConfigSO stat, RarityConfigSO rarity)
    {
        if (!_isOpen) return;
        if (stat == null || rarity == null || applier == null) { Close(); return; }

        float percent01 = Mathf.Max(0f, rarity.statMultiplier);
        applier.ApplyPercentUpgrade(stat.statType, percent01);

        Close();
    }

    private void Close()
    {
        Time.timeScale = 1f;
        /* TODO: conferme if cursor lock needed
         Cursor.visible = false;
         Cursor.lockState = CursorLockMode.Locked;*/
        InputManager.Instance?.EnablePlayerInputActions();

        if (canvasRoot != null) canvasRoot.SetActive(false);
        _isOpen = false;
    }
}
