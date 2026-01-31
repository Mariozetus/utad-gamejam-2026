using UnityEngine;

public class meleeEnemy : enemy
{
    

    private void Update()
    {
        RaycastHit hit;
        if (shotCoolDown <= 0)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackrange, playerLayer))
            {


                attack();
                shotCoolDown = startShotCoolDown;
                 

            }
        }
        
            shotCoolDown -= Time.deltaTime;
        
    }
    void attack()
    {
        Collider[] player = Physics.OverlapSphere(attackpoint.position, attackrange, playerLayer);
        foreach (Collider collider in player)
        {
            Logger.Log("he atacado a: "+ collider.gameObject.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(attackpoint.position, attackrange);
    }
}
