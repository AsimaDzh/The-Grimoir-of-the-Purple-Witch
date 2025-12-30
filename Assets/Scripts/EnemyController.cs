using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    private int _currentPatrolIndex = 0;
    private float detectionRange = 10f;

    private Rigidbody _rb;


    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    
    void FixedUpdate()
    {
        if (PlayerInRange()) ChasePlayer();
        else Patrol();
    }


    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[_currentPatrolIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;

        _rb.MovePosition(transform.position + direction * patrolSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
            _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
    }


    bool PlayerInRange()
    {
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }


    private void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        _rb.MovePosition(transform.position + direction * patrolSpeed * Time.fixedDeltaTime);
    }


    private void OnDrawGizmos()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.red;
            foreach (Transform waypoint in patrolPoints)
            {
                if (waypoint != null)
                    Gizmos.DrawSphere(waypoint.position, 0.3f);
            }

            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                var allPoints = patrolPoints[(i + 1) % patrolPoints.Length];

                if (patrolPoints[i] != null && allPoints != null)
                    Gizmos.DrawLine(patrolPoints[i].position, allPoints.position);
            }
        }

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
