using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Custom/Enemy/EnemyStats", order = 0)]
public class EnemyStats : ScriptableObject
{
    [SerializeField] private string enemyName;
    [SerializeField] private int _moveSpeed;
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _damage;
    [SerializeField] private float _healthBuffScale;
    [SerializeField] private float _speedBuffScale;
    [SerializeField] private float _damageBuffScale;

    private int _initialSpeed;
    private int _initialHealth;
    private int _initialDamage;

    public EnemyStats()
    {
        _initialSpeed = _moveSpeed;
        _initialHealth = _maxHealth;
        _initialDamage = _damage;
    }


    public void BuffSpeed()
    {
        _moveSpeed = (int)(_initialSpeed * Mathf.Exp(_speedBuffScale * _moveSpeed));
    }

    public void BuffHealth()
    {
        _maxHealth = (int)(_initialHealth * Mathf.Exp(_healthBuffScale * _maxHealth));
    }

    public void BuffDamage()
    {
        _damage = (int)(_initialDamage * Mathf.Exp(_damageBuffScale * _damage));
    }

    public void BuffStats()
    {
        BuffSpeed();
        BuffHealth();
        BuffDamage();   
    }

    public int MoveSpeed { get => _moveSpeed; private set{} }
    public int MaxHealth { get => _maxHealth; private set{} }
    public int Damage { get => _damage; private set{} }
}
