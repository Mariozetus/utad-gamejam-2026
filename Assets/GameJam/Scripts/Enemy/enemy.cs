using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected EnemyStats enemyStats;
    [SerializeField] protected Transform attackpoint;
    [SerializeField] protected float attackrange = 0.5f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected float startShotCoolDown;
    
    public float shotCoolDown;
    private int _health;

    private void Start()
    {
        _health = enemyStats.MaxHealth;
        shotCoolDown = startShotCoolDown;
    }

    protected virtual void Attack(){}
    
    public int Health => _health;
    public EnemyStats EnemyStats => enemyStats;
}
