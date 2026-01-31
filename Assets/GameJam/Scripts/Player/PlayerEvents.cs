using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action<GameObject> EnemyKilled;
    public static event Action<GameObject, float> DamageDealt; 
    public static event Action<float> DamageTaken;            
    public static event Action DashUsed;

    public delegate void OutgoingDamageModifyHandler(GameObject target, ref float damage);
    public static event OutgoingDamageModifyHandler OutgoingDamageModify;

    public delegate void IncomingDamageModifyHandler(GameObject source, ref float damage);
    public static event IncomingDamageModifyHandler IncomingDamageModify;

    public static float ApplyOutgoingDamage(GameObject target, float baseDamage)
    {
        float dmg = Mathf.Max(0f, baseDamage);
        OutgoingDamageModify?.Invoke(target, ref dmg);
        return Mathf.Max(0f, dmg);
    }

    public static float ApplyIncomingDamage(GameObject source, float baseDamage)
    {
        float dmg = Mathf.Max(0f, baseDamage);
        IncomingDamageModify?.Invoke(source, ref dmg);
        return Mathf.Max(0f, dmg);
    }

    public static void RaiseEnemyKilled(GameObject enemy)
    {
        EnemyKilled?.Invoke(enemy);
    }

    public static void RaiseDamageDealt(GameObject target, float amount)
    {
        DamageDealt?.Invoke(target, amount);
    }

    public static void RaiseDamageTaken(float amount)
    {
        DamageTaken?.Invoke(amount);
    }

    public static void RaiseDashUsed()
    {
        DashUsed?.Invoke();
    }
}