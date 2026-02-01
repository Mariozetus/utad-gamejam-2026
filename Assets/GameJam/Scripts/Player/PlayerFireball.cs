using UnityEngine;

public class PlayerFireball : MonoBehaviour
{
    public float life = 3;
    private float _damage;
    private float _lifeTimer;

    private void OnEnable()
    {
        _lifeTimer = life;
    }

    private void Update()
    {
        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(this.gameObject, _damage);
            }
            Destroy(gameObject);
        }
    }

    public void SetDamage(float damage)
    {
        _damage = damage;
    }
}
