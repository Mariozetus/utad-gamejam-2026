using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    private NavMeshAgent _agent;
    
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = GetComponent<Enemy>().EnemyStats.MoveSpeed;
    }
    
    void Update()
    {
        if (player != null)
        {
            _agent.SetDestination(player.position);

        }
        
    }
}
