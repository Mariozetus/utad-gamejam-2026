using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] protected GameObject bulletprefab;
    [SerializeField] protected float speed; 

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
        var bullet = Instantiate(bulletprefab, attackpoint.position, attackpoint.rotation);
        bullet.GetComponent<enemyBullet>().SetDamage(enemyStats.Damage);
        bullet.GetComponent<Rigidbody>().linearVelocity = attackpoint.forward * speed;
        Logger.Log("he disparado");
    }

   
}
