using Unity.VisualScripting;
using UnityEngine;

public class EnemyLifecycle : MonoBehaviour
{
    private Enemy _enemyComponent;
    private bool _isDead = false;

    private void Awake()
    {
        _enemyComponent = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        _isDead = false;
        
        if (_enemyComponent != null && _enemyComponent.EnemyStats != null)
        {
            Logger.Log($"Enemy {name} activated from pool", LogType.SpawnSystem, this);
        }
    }

    public void Die()
    {
        if (_isDead) return;
        
        _isDead = true;
        Logger.Log($"Enemy {name} died, returning to pool", LogType.SpawnSystem, this);
        
        if(_enemyComponent.EnemyStats.IsBoss)
        {
            Logger.Log($"Boss {name} defeated!", LogType.Enemy, this);
            GameManager.Instance.OnBossDefeated();
        }

        Invoke(nameof(ReturnToPool), 0.1f);
    }

    private void ReturnToPool()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.ReturnEnemyToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TakeFatalDamage()
    {
        Die();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Logger.Log($"Enemy {name} fell into death zone, returning to pool", LogType.SpawnSystem, this);
            Die();
        }
    }
}