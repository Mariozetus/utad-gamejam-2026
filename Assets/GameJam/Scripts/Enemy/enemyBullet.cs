using UnityEngine;

public class enemyBullet : MonoBehaviour
{
    public float life = 3;
    private float _damage;
    private float _lifeTimer;

    private void OnEnable()
    {
        // Reset timer when retrieved from pool
        _lifeTimer = life;
    }

    private void Update()
    {
        // Manual lifetime management instead of Destroy()
        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f)
        {
            ReturnToPool();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Health>().TakeDamage(this.gameObject, _damage);
            ReturnToPool();
        }
    }

    public void SetDamage(float damage)
    {
        _damage = damage;
    }
    
    private void ReturnToPool()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.ReturnProjectileToPool(gameObject);
        }
        else
        {
            // Fallback if EnemyManager is not available
            Destroy(gameObject);
        }
    }
}
