using UnityEngine;

public class UpgradeApplier : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Health health;
    [SerializeField] private CombatStats combatStats;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (combatStats == null) combatStats = GetComponent<CombatStats>();
    }

    public void Apply(UpgradeOption option)
    {
        if (option == null || option.stat == null || option.rarity == null) return;

        float mult = Mathf.Max(0f, option.rarity.statMultiplier);

        switch (option.stat.statType)
        {
            case StatType.Health:
                ApplyHealth(option.stat, mult);
                break;

            case StatType.Strength:
                ApplyAttack(option.stat, mult);
                break;

            case StatType.Speed:
                ApplySpeed(option.stat, mult);
                break;
        }
    }

    private void ApplyHealth(StatConfigSO stat, float mult)
    {
        if (health == null) return;

        if (stat.increaseMode == IncreaseMode.Flat)
            health.IncreaseMaxHealthFlat(stat.baseFlat * mult, healToFull: true);
        else
            health.IncreaseMaxHealthPercent(stat.basePercent * mult, healToFull: true);
    }

    private void ApplyAttack(StatConfigSO stat, float mult)
    {
        if (combatStats == null) return;

        if (stat.increaseMode == IncreaseMode.Flat)
            combatStats.IncreaseBaseAttackFlat(stat.baseFlat * mult);
        else
            combatStats.IncreaseBaseAttackPercent(stat.basePercent * mult);
    }

    private void ApplySpeed(StatConfigSO stat, float mult)
    {
        if (combatStats == null) return;

        if (stat.increaseMode == IncreaseMode.Flat)
            combatStats.IncreaseBaseMoveSpeedFlat(stat.baseFlat * mult);
        else
            combatStats.IncreaseBaseMoveSpeedPercent(stat.basePercent * mult);
    }
}