using UnityEngine;

public class guardianEnemy : Enemy
{
    [SerializeField] protected GameObject bulletprefab;
    [SerializeField] protected float speed;
    [SerializeField] protected float distance;
    [SerializeField] protected float meleeRange;
    [SerializeField] protected float rangedRange;

    private void Update()
    {
        distance = Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position);

        if (shotCoolDown <= 0)
        {
        
            if (distance >= 5)
            {
                
                if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rangedRange, playerLayer))
                {
                    RangedAttack();
                    shotCoolDown = startShotCoolDown;
                }
            }
            else 
            {
               
                if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, meleeRange, playerLayer))
                {
                    Attack();
                    shotCoolDown = startShotCoolDown;
                }
            }
        }

        shotCoolDown -= Time.deltaTime;
    }
    protected override void Attack()
    {
        Collider[] player = Physics.OverlapSphere(attackpoint.position, meleeRange, playerLayer);
        foreach (Collider collider in player)
        {
            Logger.Log("he atacado a: " + collider.gameObject.name, LogType.Enemy, this);
            var playerHealth = collider.GetComponent<Health>();
            playerHealth?.TakeDamage(this.gameObject, enemyStats.Damage);
        }
    }


    protected void RangedAttack()
    {
        GameObject bullet = EnemyManager.Instance.GetPooledProjectile(bulletprefab);
        if (bullet != null)
        {
            bullet.transform.position = attackpoint.position;
            bullet.transform.rotation = attackpoint.rotation;
            
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = attackpoint.forward * speed;
            }
            
            // Set damage if the bullet has the component
            enemyBullet bulletScript = bullet.GetComponent<enemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDamage(enemyStats.Damage);
            }
            
            Logger.Log($"Fired pooled projectile from {name}", LogType.Enemy, this);
        }
        else
        {
            Logger.Warning("Failed to get projectile from pool", LogType.Enemy, this);
        }
    }
}
