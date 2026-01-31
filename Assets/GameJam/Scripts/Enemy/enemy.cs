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
    private int _maxHealth;
    private EnemyLifecycle _lifecycle;

    private void Awake()
    {
        _lifecycle = GetComponent<EnemyLifecycle>();
        if (_lifecycle == null)
        {
            _lifecycle = gameObject.AddComponent<EnemyLifecycle>();
        }
    }

    private void Start()
    {
        ResetHealth();
        shotCoolDown = startShotCoolDown;
    }

    private void OnEnable()
    {
        // Reset when retrieved from pool
        ResetHealth();
        shotCoolDown = startShotCoolDown;
    }

    private void ResetHealth()
    {
        _maxHealth = enemyStats.MaxHealth;
        _health = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        Logger.Log($"{name} took {damage} damage. Health: {_health}/{_maxHealth}", LogType.Enemy, this);

        if (_health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Logger.Log($"{name} died", LogType.Enemy, this);
        
        if (_lifecycle != null)
        {
            _lifecycle.Die();
        }
    }

    protected virtual void Attack(){}
    
    public int Health => _health;
    public int MaxHealth => _maxHealth;
    public EnemyStats EnemyStats => enemyStats;
    public bool IsAlive => _health > 0;
}
