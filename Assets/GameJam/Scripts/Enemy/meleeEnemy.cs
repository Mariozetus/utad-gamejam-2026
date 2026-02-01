using UnityEngine;

public class MeleeEnemy : Enemy
{
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(attackpoint.position, attackrange);
    }
}
