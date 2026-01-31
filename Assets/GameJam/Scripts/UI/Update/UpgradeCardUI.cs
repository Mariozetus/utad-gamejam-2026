using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text valueText;

    [Header("Images")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image backgroundImage;

    private UpgradeOption _option;
    private UpgradeApplier _applier;

    public void Setup(UpgradeOption option, UpgradeApplier applier)
    {
        _option = option;
        _applier = applier;

        if (_option?.stat == null || _option?.rarity == null) return;

        if (titleText) titleText.text = _option.stat.upgradeTitle;
        if (rarityText) rarityText.text = _option.rarity.rarityText;

        if (titleText) titleText.color = _option.rarity.titleColor;
        if (frameImage) frameImage.color = _option.rarity.borderColor;

        if (iconImage) iconImage.sprite = _option.stat.statIcon;
        if (frameImage && _option.rarity.rarityFrameSprite) frameImage.sprite = _option.rarity.rarityFrameSprite;
        if (backgroundImage && _option.rarity.rarityBackgroundSprite) backgroundImage.sprite = _option.rarity.rarityBackgroundSprite;

        float mult = Mathf.Max(0f, _option.rarity.statMultiplier);
        string preview = BuildPreview(_option.stat, mult);
        if (valueText) valueText.text = preview;
    }

    private string BuildPreview(StatConfigSO stat, float mult)
    {
        if (stat.increaseMode == IncreaseMode.Flat)
            return $"+{(stat.baseFlat * mult):0.##}";
        return $"+{(stat.basePercent * mult * 100f):0.#}%";
    }
    public void Choose()
    {
        if (_applier == null || _option == null) return;
        _applier.Apply(_option);
    }
}