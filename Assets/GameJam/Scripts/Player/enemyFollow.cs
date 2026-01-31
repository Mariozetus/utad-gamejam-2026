using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    private NavMeshAgent _agent;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = GetComponent<Enemy>().EnemyStats.MoveSpeed;
        player = GameManager.Instance.Player;

    }
    
    void Update()
    {
        if (player != null)
        {
            _agent.SetDestination(player.transform.position);

        }
        
    }
}
