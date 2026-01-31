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
        // distance = Vector3.Distance(transform.position);
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
        bullet.GetComponent<Rigidbody>().linearVelocity = attackpoint.forward * speed;
        Logger.Log("he disparado");
    }
}
