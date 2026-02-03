using UnityEngine;

enum AIState
{
    Idle = 0,
    Patrolling = 1,
    Chasing = 2
}

public class EnemyController : MonoBehaviour
{
    [Header("========== AI States ==========")]
    [SerializeField] private AIState currentState;


    [Header("========== Patrol ==========")]
    [SerializeField] private Transform wayPoints;
    [SerializeField] private float waitAtPoint = 3f;
    private int _currentPatrolIndex = 0;
    private float _waitCounter;
    private bool _isWaitingAfterStep = false;


    [Header("========== Movement (grid) ==========")]
    [SerializeField] private LayerMask whatStopsMovement;
    [SerializeField] private float _stepSize = 1f; // size of one grid cell
    [SerializeField] private float _moveSpeed = 3f;
    private Vector3 _currentTargetCell;
    private bool _isMoving;

    [SerializeField] private float _smoothTime = 0.05f;
    private float _rotationVelocity;
    private float _targetAngle;


    [Header("========== Chasing ==========")]
    [SerializeField] private float detectionRange;

    private GameObject player;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        _waitCounter = waitAtPoint;
        transform.position = RoundToGrid(transform.position);
    }

    
    void Update()
    {
        if (player == null) return;

        if (_isWaitingAfterStep)
        { 
            _waitCounter -= Time.deltaTime;
            if (_waitCounter <= 0f) _isWaitingAfterStep = false;
            return; // not moving while waiting
        }

        float angle = Mathf.SmoothDampAngle( // Smooth rotation
            transform.eulerAngles.y,
            _targetAngle,
            ref _rotationVelocity,
            _smoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= detectionRange)
            currentState = AIState.Chasing;
        else if (currentState == AIState.Chasing)
            currentState = AIState.Idle;

        switch (currentState)
        {
            case AIState.Idle:

                if (_waitCounter > 0)
                    _waitCounter -= Time.deltaTime;
                else
                {
                    // if there are waypoints, try to go to the next one
                    // else try a random step
                    bool moved = false;
                    if (wayPoints != null && wayPoints.childCount > 0)
                    {
                        int tries = 0;
                        while (tries < wayPoints.childCount && !moved)
                        {
                            moved = TryStepTowards(wayPoints.GetChild(_currentPatrolIndex).position);
                            if (!moved)
                                _currentPatrolIndex = (_currentPatrolIndex + 1) % wayPoints.childCount;
                            tries++;
                        }
                    }
                    else moved = TryRandomStep();

                    if (moved) currentState = AIState.Patrolling;
                    else _waitCounter = waitAtPoint;
                    // if couldn't move, stay idle and reset wait counter
                }
                break;


            case AIState.Patrolling:

                if (_isMoving) break; // if already moving, don't change target

                if (wayPoints == null || wayPoints.childCount == 0)
                {
                    bool moved = TryRandomStep();
                    if (!moved)
                    {
                        _waitCounter = waitAtPoint;
                        currentState = AIState.Idle;
                    }
                }
                else
                {
                    Vector3 waypointPos = wayPoints.GetChild(_currentPatrolIndex).position;
                    
                    if (Vector3.Distance(RoundToGrid(transform.position), RoundToGrid(waypointPos)) <= 0.1f)
                    {
                        _currentPatrolIndex = (_currentPatrolIndex + 1) % wayPoints.childCount;
                        _waitCounter = waitAtPoint;
                        currentState = AIState.Idle;
                    }
                    else
                    {
                        // trying to move towards the current waypoint
                        // if can't move, go to the next waypoint
                        bool moved = TryStepTowards(waypointPos);
                        if (!moved)
                            _currentPatrolIndex = (_currentPatrolIndex + 1) % wayPoints.childCount;
                    }
                }
                break;


            case AIState.Chasing:

                // if already moving, don't change target
                if (_isMoving) break;
                TryStepTowards(player.transform.position);
                break;
        }

        Move();
    }


    void Move()
    {
        if (!_isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            _currentTargetCell,
            _moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, _currentTargetCell) < 0.05f)
        {
            transform.position = _currentTargetCell;
            _isMoving = false;

            if (currentState == AIState.Patrolling && (wayPoints == null || wayPoints.childCount == 0))
            {
                _isWaitingAfterStep = true;
                _waitCounter = waitAtPoint;
            }      
        }
    }


    // Trying to step towards a world target position
    private bool TryStepTowards(Vector3 worldTarget)
    {
        Vector3 delta = worldTarget - transform.position;
        Vector3 step = GetCardinalStep(delta);
        if (step == Vector3.zero) return false;

        Vector3 targetCell = RoundToGrid(transform.position) + step * _stepSize;

        if (Physics.CheckBox(
            targetCell,
            new Vector3(0.45f, 0.5f, 0.45f),// half-extents
            Quaternion.identity,
            whatStopsMovement)) return false;

        _currentTargetCell = targetCell;
        _isMoving = true;

        // Smooth rotation towards movement direction
        // Actual rotation happens in Update()
        _targetAngle = Mathf.Atan2(step.x, step.z) * Mathf.Rad2Deg;

        return true;
    }


    private static readonly Vector3[] CardinalDirections =
    {
        new Vector3(3f, 0f, 0f),
        new Vector3(-3f, 0f, 0f),
        new Vector3(0f, 0f, 3f),
        new Vector3(0f, 0f, -3f)
    };


    private void ShuffleDirections(Vector3[] dirs)
    {
        for (int i = 0; i < dirs.Length; i++)
        {
            int rnd = Random.Range(i, dirs.Length);
            (dirs[i], dirs[rnd]) = (dirs[rnd], dirs[i]);
        }
    }


    // Trying to make a random step in any cardinal direction
    private bool TryRandomStep()
    {
        Vector3 origin = RoundToGrid(transform.position);

        ShuffleDirections(CardinalDirections);

        for (int i = 0; i < CardinalDirections.Length; i++)
        {
            Vector3 step = CardinalDirections[i];
            Vector3 targetCell = origin + step * _stepSize;

            if (Physics.CheckBox(
                targetCell, 
                new Vector3(0.45f, 0.5f, 0.45f),
                Quaternion.identity,
                whatStopsMovement)) continue;

            _currentTargetCell = targetCell;
            _isMoving = true;
            _targetAngle = Mathf.Atan2(step.x, step.z) * Mathf.Rad2Deg;
            return true;
        }
        return false;
    }


    // Returning a cardinal step direction (x or z) towards the target
    private Vector3 GetCardinalStep(Vector3 delta)
    {
        float absX = Mathf.Abs(delta.x);
        float absZ = Mathf.Abs(delta.z);

        // if already close enough, don't move
        if (absX < 0.5f && absZ < 0.5f) return Vector3.zero;

        if (absX > absZ) 
            return new Vector3(Mathf.Sign(delta.x), 0f, 0f);
        else 
            return new Vector3(0f, 0f, Mathf.Sign(delta.z));
    }


    // Rounding position to nearest grid cell (x, z)
    private Vector3 RoundToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x),
            pos.y,
            Mathf.Round(pos.z)
        );
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
