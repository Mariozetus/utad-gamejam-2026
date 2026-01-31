using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Custom/Enemy/EnemyStats", order = 0)]
public class EnemyStats : ScriptableObject
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _maxHealth;
    [SerializeField] private int _damage;

    public float MoveSpeed { get => _moveSpeed; private set{} }
    public float MaxHealth { get => _maxHealth; private set{} }
    public int Damage { get => _damage; private set{} }
}
