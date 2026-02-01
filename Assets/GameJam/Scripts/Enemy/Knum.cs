using Unity.VisualScripting;
using UnityEngine;

public class Knum : Enemy
{
    [SerializeField] private float SpecialAttackRange = 10f;
    [SerializeField] private float specialCoolDownDuration = 30f;
    private float _specialTimer = 0f;

    void Start()
    {
        _specialTimer = specialCoolDownDuration;
    }   


    private void Update()
    {
        RaycastHit hit;
        if (shotCoolDown <= 0)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackrange, playerLayer))
            {
                Attack();
                shotCoolDown = startShotCoolDown;
            }
        }
        shotCoolDown -= Time.deltaTime;
        _specialTimer -= Time.deltaTime;
        if(_specialTimer <= 0f)
        {
            SpecialAttack();
            _specialTimer = specialCoolDownDuration;
        }
    }

    protected override void Attack()
    {
        Collider[] player = Physics.OverlapSphere(attackpoint.position, attackrange, playerLayer);
        foreach (Collider collider in player)
        {
            Logger.Log("he atacado a: "+ collider.gameObject.name, LogType.Enemy, this);
            var playerHealth = collider.GetComponent<Health>();
            playerHealth?.TakeDamage(this.gameObject, enemyStats.Damage);
        }
    }

    protected void SpecialAttack()
    {
        Collider[] players = Physics.OverlapSphere(attackpoint.position, SpecialAttackRange, playerLayer);
        foreach (Collider collider in players)
        {
            Logger.Log("he atacado con especial a: "+ collider.gameObject.name, LogType.Enemy, this);
            var playerHealth = collider.GetComponent<Health>();
            playerHealth?.TakeDamage(this.gameObject, enemyStats.Damage * 2);
        }
    }
}
