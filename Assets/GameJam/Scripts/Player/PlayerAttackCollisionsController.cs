using UnityEngine;

using System.Collections.Generic;
public class PlayerAttackCollisionsController : MonoBehaviour
{
    private List<GameObject> enemiesIn = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!enemiesIn.Contains(other.gameObject)){
                enemiesIn.Add(other.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            enemiesIn.Remove(other.gameObject);
        }
    }
    public List<GameObject> GetEnemiesInTrigger()
    {
        return enemiesIn;
    }
}
