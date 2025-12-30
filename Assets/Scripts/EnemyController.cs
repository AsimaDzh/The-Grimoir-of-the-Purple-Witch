using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("========== Movement ==========")]
    [SerializeField] private LayerMask whatStopsMovement;
    [SerializeField] private Transform movePoint;
    private float _patrolSpeed = 3f;
    private float _smoothTime = 0.05f;

    [Header("========== AI ==========")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;
    private float detectionRange = 10f;

    [SerializeField] private int _currentPatrolIndex = 0;
    private float _currentVelocity;
    private float _targetAngle;
    private Vector3 _lastMoveDir;
    private Rigidbody _rb;

    private const float arriveThreshold = 0.05f;


    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        movePoint.parent = null;

        // Snap movePoint to integers to align to grid
        movePoint.position = new Vector3(
            Mathf.Round(movePoint.position.x),
            movePoint.position.y,
            Mathf.Round(movePoint.position.z));
    }

    
    void FixedUpdate()
    {
        // Smooth rotation towards target angle
        var angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            _targetAngle,
            ref _currentVelocity,
            _smoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        // Physically move towards movePoint using Rigidbody for consistent physics
        Vector3 newPos = Vector3.MoveTowards(
            transform.position, 
            movePoint.position, 
            _patrolSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(newPos);

        // When reached movePoint, decide next step
        if (Vector3.Distance(transform.position, movePoint.position) <= arriveThreshold)
        {
            if (PlayerInRange()) DecideNextStepTowards(player.position);
            else Patrol();
        }
    }


    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[_currentPatrolIndex];
        
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
            targetPoint = patrolPoints[_currentPatrolIndex];
        }

        DecideNextStepTowards(targetPoint.position);
    }


    bool PlayerInRange()
    {
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }


    private void DecideNextStepTowards(Vector3 targetWorldPos)
    {
        // Work in grid-aligned space based on movePoint
        Vector3 delta = targetWorldPos - movePoint.position;

        // Choose axis with larger absolute distance prevents diagonal movement
        Vector3 moveDir = Vector3.zero;
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.z))
            moveDir = new Vector3(Mathf.Sign(delta.x), 0f, 0f);
        else
            moveDir = new Vector3(0f, 0f, Mathf.Sign(delta.z));

        // Try primary axis; if blocked, try the other axis (fallback)
        if (TrySetMovePoint(moveDir)) return;

        Vector3 altDir = (moveDir.x != 0f) ? new Vector3(0f, 0f, Mathf.Sign(delta.z)) : new Vector3(Mathf.Sign(delta.x), 0f, 0f);
        if (altDir != Vector3.zero)
            TrySetMovePoint(altDir);
    }


    // Returns true if movePoint updated
    private bool TrySetMovePoint(Vector3 moveDir)
    {
        Vector3 targetPos = movePoint.position + moveDir;
        
        if (!Physics.CheckSphere(targetPos, 0.2f, whatStopsMovement)) // Check for obstacles
        {
            movePoint.position = new Vector3(
                Mathf.Round(targetPos.x),
                targetPos.y,
                Mathf.Round(targetPos.z));
            _lastMoveDir = moveDir;
            _targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            return true;
        }

        return false;
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

    void LateUpdate()
    {
        // Keep movePoint snapped to grid to avoid floating drift
        if (movePoint != null)
            movePoint.position = new Vector3(
                Mathf.Round(movePoint.position.x),
                movePoint.position.y,
                Mathf.Round(movePoint.position.z));
    }
}
