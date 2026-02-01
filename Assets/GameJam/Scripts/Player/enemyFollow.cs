using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    private NavMeshAgent _agent;
    private float _followSpeed = 3.5f;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _followSpeed = GetComponent<Enemy>().EnemyStats.MoveSpeed;
        _agent.speed = _followSpeed;
        player = GameManager.Instance.Player;
        _agent.enabled = true;
    }
    
    void Update()
    {
        if (player != null)
        {
            _agent.speed = _followSpeed;
            Logger.Log("Following Player " + player.transform.position, LogType.Enemy, this);
            _agent.SetDestination(player.transform.position);
            transform.LookAt(player.transform.position);

        }

        if(!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _agent.velocity = Vector3.zero;
        }
        
    }
}
