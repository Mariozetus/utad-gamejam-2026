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
        
        RaycastHit hit;
        if (shotCoolDown <= 0)
        {
            
            
                if (distance >= 5)
                {
                if (Physics.Raycast(transform.position, transform.forward, out hit, rangedRange, playerLayer))
                {

                
                    RangedAttack();
                }
                else
                {
                    if (Physics.Raycast(transform.position, transform.forward, out hit, meleeRange, playerLayer))
                        Attack();
                }

                shotCoolDown = startShotCoolDown;
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
        }
    }


    protected void RangedAttack()
    {
        var bullet = Instantiate(bulletprefab, attackpoint.position, attackpoint.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = attackpoint.forward * speed;
        Logger.Log("he disparado");
    }
}
