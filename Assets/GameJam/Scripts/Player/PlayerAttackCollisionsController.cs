using UnityEngine;

using System.Collections.Generic;
public class PlayerAttackCollisionsController : MonoBehaviour
{
    private List<GameObject> enemiesIn = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        Logger.Log("Trigger entered by: " + other.gameObject.name + " Layer: " + other.gameObject.layer, LogType.Player, this);
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Logger.Log("Enemy entered attack trigger: " + other.gameObject.name, LogType.Player, this);
            if (!enemiesIn.Contains(other.gameObject)){
                enemiesIn.Add(other.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Logger.Log("Trigger entered by: " + other.gameObject.name + " Layer: " + other.gameObject.layer, LogType.Player, this);

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Logger.Log("Enemy exited attack trigger: " + other.gameObject.name, LogType.Player, this);
            enemiesIn.Remove(other.gameObject);
        }
    }
    public List<GameObject> GetEnemiesInTrigger()
    {
        return enemiesIn;
    }
}
