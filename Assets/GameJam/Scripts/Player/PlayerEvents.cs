using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action<GameObject> EnemyKilled;
    public static event Action<GameObject, float> DamageDealt;
    public static event Action<GameObject, float> DamageTaken;
    public static event Action DashUsed;

    public delegate void OutgoingDamageModifyHandler(GameObject attacker, GameObject target, ref float damage);
    public static event OutgoingDamageModifyHandler OutgoingDamageModify;

    public delegate void IncomingDamageModifyHandler(GameObject attacker, GameObject target, ref float damage);
    public static event IncomingDamageModifyHandler IncomingDamageModify;

    public static float ApplyOutgoingDamage(GameObject attacker, GameObject target, float baseDamage)
    {
        float dmg = Mathf.Max(0f, baseDamage);

        if (attacker != null)
        {
            var stats = attacker.GetComponent<CombatStats>();
            if (stats != null)
            {
                dmg = Mathf.Max(0f, stats.Attack.Value);
            }
        }

        OutgoingDamageModify?.Invoke(attacker, target, ref dmg);
        return Mathf.Max(0f, dmg);
    }

    public static float ApplyOutgoingDamage(GameObject target, float baseDamage)
    {
        GameObject attacker = null;
        if (PlayerController.Instance != null) attacker = PlayerController.Instance.gameObject;
        return ApplyOutgoingDamage(attacker, target, baseDamage);
    }

    public static float ApplyIncomingDamage(GameObject attacker, GameObject target, float baseDamage)
    {
        float dmg = Mathf.Max(0f, baseDamage);

        if (attacker != null)
        {
            var stats = attacker.GetComponent<CombatStats>();
            if (stats != null)
            {
                dmg = Mathf.Max(0f, stats.Attack.Value);
            }
        }

        IncomingDamageModify?.Invoke(attacker, target, ref dmg);
        return Mathf.Max(0f, dmg);
    }

    public static void RaiseEnemyKilled(GameObject enemy) => EnemyKilled?.Invoke(enemy);
    public static void RaiseDamageDealt(GameObject target, float amount) => DamageDealt?.Invoke(target, amount);
    public static void RaiseDamageTaken(GameObject source, float amount) => DamageTaken?.Invoke(source, amount);
    public static void RaiseDashUsed() => DashUsed?.Invoke();
}
