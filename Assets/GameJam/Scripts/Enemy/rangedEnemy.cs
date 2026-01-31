using UnityEngine;

public class rangedEnemy : enemy
{
    public GameObject bullet;
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
       Instantiate(bullet, attackpoint.position, attackpoint.rotation);
    }
}
