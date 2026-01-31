using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModifiableStat
{
    [SerializeField] private float baseValue = 1f;

    private readonly List<StatModifier> _mods = new();
    private bool _dirty = true;
    private float _cached;

    public float BaseValue
    {
        get => baseValue;
        set { baseValue = value; _dirty = true; }
    }

    public float Value
    {
        get
        {
            if (_dirty) Recalculate(Time.unscaledTime);
            return _cached;
        }
    }

    public void AddOrReplace(StatModifier mod)
    {
        for (int i = 0; i < _mods.Count; i++)
        {
            if (_mods[i].sourceId == mod.sourceId && _mods[i].op == mod.op)
            {
                _mods[i] = mod;
                _dirty = true;
                return;
            }
        }

        _mods.Add(mod);
        _dirty = true;
    }

    public void RemoveBySource(string sourceId)
    {
        for (int i = _mods.Count - 1; i >= 0; i--)
        {
            if (_mods[i].sourceId == sourceId)
            {
                _mods.RemoveAt(i);
                _dirty = true;
            }
        }
    }

    public void Tick(float nowUnscaled)
    {
        bool changed = false;
        for (int i = _mods.Count - 1; i >= 0; i--)
        {
            if (_mods[i].IsExpired(nowUnscaled))
            {
                _mods.RemoveAt(i);
                changed = true;
            }
        }
        if (changed) _dirty = true;
    }

    private void Recalculate(float nowUnscaled)
    {
        Tick(nowUnscaled);

        float add = 0f;
        float mul = 1f;

        for (int i = 0; i < _mods.Count; i++)
        {
            var m = _mods[i];
            if (m.op == ModOp.Add) add += m.value;
            else if (m.op == ModOp.Mul) mul *= m.value;
        }

        _cached = (baseValue + add) * mul;
        _dirty = false;
    }
}
