using System;
using UnityEngine;

public enum ModOp
{
    Add, 
    Mul  
}

[Serializable]
public struct StatModifier
{
    public string sourceId;
    public ModOp op;
    public float value;        
    public float endUnscaled;  // <0 perma

    public bool IsExpired(float nowUnscaled) => endUnscaled >= 0f && nowUnscaled >= endUnscaled;
}