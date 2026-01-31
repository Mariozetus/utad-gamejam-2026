using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] protected GameObject bullet;

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
       Instantiate(bullet, attackpoint.position, attackpoint.rotation);
    }
}
