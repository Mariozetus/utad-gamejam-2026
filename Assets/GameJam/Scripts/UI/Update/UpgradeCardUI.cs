using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text previewBaseText;
    [SerializeField] private TMP_Text previewNewText;

    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fxImage;

    [SerializeField] private Button button;

    private StatConfigSO _stat;
    private RarityConfigSO _rarity;
    private UpgradeSelectionManager _manager;

    public void Setup(UpgradeSelectionManager manager, UpgradeApplier applier, StatConfigSO stat, RarityConfigSO rarity)
    {
        _manager = manager;
        _stat = stat;
        _rarity = rarity;

        if (_stat == null || _rarity == null) return;

        if (titleText) titleText.text = _stat.upgradeTitle;
        if (descText) descText.text = _stat.description;
        if (rarityText) rarityText.text = _rarity.rarityText;

        if (titleText) titleText.color = _rarity.titleColor;
        if (frameImage) frameImage.color = _rarity.borderColor;

        if (iconImage) iconImage.sprite = _stat.statIcon;
        if (frameImage && _rarity.rarityFrameSprite) frameImage.sprite = _rarity.rarityFrameSprite;
        if (backgroundImage && _rarity.rarityBackgroundSprite) backgroundImage.sprite = _rarity.rarityBackgroundSprite;

        if (fxImage)
        {
            fxImage.sprite = _rarity.rarityFxSprite;
            fxImage.enabled = _rarity.rarityFxSprite != null;
        }

        if (applier != null)
        {
            float baseVal = applier.GetBaseValue(_stat.statType);
            float pct = Mathf.Max(0f, _rarity.statMultiplier);
            float newVal = baseVal * (1f + pct);
            if (previewBaseText) previewBaseText.text = $"{baseVal:0.##}";
            if (previewNewText) previewNewText.text = $"{newVal:0.##}";
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (_manager == null || _stat == null || _rarity == null) return;
        _manager.Select(_stat, _rarity);
    }
}

