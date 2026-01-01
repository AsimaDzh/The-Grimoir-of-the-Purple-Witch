using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    enum AIState
    {
        Idle,
        Patrolling,
        Chasing
    }

    [Header("========== Patrol ==========")]
    [SerializeField] private Transform wayPoints;
    private int _currentPatrolIndex = 0;
    private float _waitAtPoint = 2f;
    private float _waitCounter;
    private float _patrolSpeed = 3f;

    [Header("========== Components ==========")]
    NavMeshAgent _navMeshAgent;

    [Header("========== AI States ==========")]
    [SerializeField] private AIState currentState;

    [Header("========== Chasing ==========")]
    [SerializeField] private float detectionRange;

    [Header("========== Suspicious ==========")]
    [SerializeField] private float suspiciousTime;
    private float _timeSinceLastSeen;

    private GameObject player;


    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");

        _waitCounter = _waitAtPoint;
        _timeSinceLastSeen = suspiciousTime;
    }

    
    void FixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case AIState.Idle:

                if (_waitCounter > 0)
                    _waitCounter -= Time.fixedDeltaTime;
                else
                {
                    currentState = AIState.Patrolling;
                    _navMeshAgent.SetDestination(wayPoints.GetChild(_currentPatrolIndex).position);
                }
                if (distanceToPlayer <= detectionRange)
                    currentState = AIState.Chasing;
                break;


            case AIState.Patrolling:

                if (_navMeshAgent.remainingDistance <= 0.2f)
                {
                    _currentPatrolIndex++;

                    if (_currentPatrolIndex >= wayPoints.childCount)
                        _currentPatrolIndex = 0;

                    currentState = AIState.Idle;
                    _waitCounter = _waitAtPoint;
                }
                if (distanceToPlayer <= detectionRange)
                    currentState = AIState.Chasing;
                break;


            case AIState.Chasing:

                _navMeshAgent.SetDestination(player.transform.position);
                if (distanceToPlayer > detectionRange)
                {
                    _navMeshAgent.isStopped = true;
                    _navMeshAgent.velocity = Vector3.zero;
                    _timeSinceLastSeen -= Time.fixedDeltaTime;

                    if (_timeSinceLastSeen <= 0)
                    {
                        currentState = AIState.Idle;
                        _timeSinceLastSeen = suspiciousTime;
                        _navMeshAgent.isStopped = false;
                    }
                }
                break;
        }
    }
    

    private void OnDrawGizmos()
    {
        if (wayPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform waypoint in wayPoints)
            {
                if (waypoint != null)
                    Gizmos.DrawSphere(waypoint.position, 0.3f);
            }
        }

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
