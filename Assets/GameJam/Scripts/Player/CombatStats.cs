using UnityEngine;

public class CombatStats : MonoBehaviour
{
    [Header("Base Values")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseAttack = 10f;

    [Header("Stats (Base + Modifiers)")]
    public ModifiableStat MoveSpeed = new ModifiableStat();
    public ModifiableStat Attack = new ModifiableStat();

    private void Awake()
    {
        MoveSpeed.BaseValue = baseMoveSpeed;
        Attack.BaseValue = baseAttack;
    }

    private void Update()
    {
        float now = Time.unscaledTime;
        MoveSpeed.Tick(now);
        Attack.Tick(now);
    }

    public void IncreaseBaseMoveSpeedFlat(float add)
    {
        if (add <= 0f) return;
        MoveSpeed.BaseValue += add;
    }

    public void IncreaseBaseMoveSpeedPercent(float pct01)
    {
        if (pct01 <= 0f) return;
        MoveSpeed.BaseValue += MoveSpeed.BaseValue * pct01;
    }

    public void IncreaseBaseAttackFlat(float add)
    {
        if (add <= 0f) return;
        Attack.BaseValue += add;
    }

    public void IncreaseBaseAttackPercent(float pct01)
    {
        if (pct01 <= 0f) return;
        Attack.BaseValue += Attack.BaseValue * pct01;
    }

    public void SetBaseMoveSpeed(float v) => MoveSpeed.BaseValue = v;
    public void SetBaseAttack(float v) => Attack.BaseValue = v;

    public void AddMoveSpeedMul(string sourceId, float mult, float durationSeconds)
    {
        MoveSpeed.AddOrReplace(new StatModifier
        {
            sourceId = sourceId,
            op = ModOp.Mul,
            value = Mathf.Max(0f, mult),
            endUnscaled = durationSeconds > 0f ? Time.unscaledTime + durationSeconds : -1f
        });
    }

    public void AddMoveSpeedAdd(string sourceId, float add, float durationSeconds)
    {
        MoveSpeed.AddOrReplace(new StatModifier
        {
            sourceId = sourceId,
            op = ModOp.Add,
            value = add,
            endUnscaled = durationSeconds > 0f ? Time.unscaledTime + durationSeconds : -1f
        });
    }

    public void AddAttackMul(string sourceId, float mult, float durationSeconds)
    {
        Attack.AddOrReplace(new StatModifier
        {
            sourceId = sourceId,
            op = ModOp.Mul,
            value = Mathf.Max(0f, mult),
            endUnscaled = durationSeconds > 0f ? Time.unscaledTime + durationSeconds : -1f
        });
    }

    public void AddAttackAdd(string sourceId, float add, float durationSeconds)
    {
        Attack.AddOrReplace(new StatModifier
        {
            sourceId = sourceId,
            op = ModOp.Add,
            value = add,
            endUnscaled = durationSeconds > 0f ? Time.unscaledTime + durationSeconds : -1f
        });
    }

    public void RemoveAttackSource(string sourceId) => Attack.RemoveBySource(sourceId);
    public void RemoveMoveSpeedSource(string sourceId) => MoveSpeed.RemoveBySource(sourceId);
}
