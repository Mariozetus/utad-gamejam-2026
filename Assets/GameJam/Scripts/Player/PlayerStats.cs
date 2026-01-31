using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private float baseMoveSpeed = 5f;

    [Header("Runtime")]
    [SerializeField] public ModifiableStat MoveSpeed = new ModifiableStat();

    private void Awake()
    {
        MoveSpeed.BaseValue = baseMoveSpeed;
    }

    private void Update()
    {
        MoveSpeed.Tick(Time.unscaledTime);
    }

    public void SetBaseMoveSpeed(float baseValue)
    {
        MoveSpeed.BaseValue = baseValue;
    }

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

    public void RemoveMoveSpeedSource(string sourceId)
    {
        MoveSpeed.RemoveBySource(sourceId);
    }
}