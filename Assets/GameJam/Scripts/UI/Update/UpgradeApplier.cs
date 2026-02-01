using UnityEngine;

public class UpgradeApplier : MonoBehaviour
{
    [Header("Targets (Player)")]
    [SerializeField] private Health health;
    [SerializeField] private CombatStats combatStats;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (combatStats == null) combatStats = GetComponent<CombatStats>();
    }

    public float GetBaseValue(StatType type)
    {
        switch (type)
        {
            case StatType.Health:
                return health != null ? health.Max : 0f;

            case StatType.Strength:
                return combatStats != null ? combatStats.GetBaseAttack() : 0f;

            case StatType.Speed:
                return combatStats != null ? combatStats.GetBaseMoveSpeed() : 0f;
        }
        return 0f;
    }

    public void ApplyPercentUpgrade(StatType type, float percent01)
    {
        percent01 = Mathf.Max(0f, percent01);

        switch (type)
        {
            case StatType.Health:
                if (health != null) health.IncreaseMaxHealthPercent(percent01, healToFull: true);
                break;

            case StatType.Strength:
                if (combatStats != null) combatStats.IncreaseBaseAttackPercent(percent01);
                break;

            case StatType.Speed:
                if (combatStats != null) combatStats.IncreaseBaseMoveSpeedPercent(percent01);
                break;
        }
    }
}