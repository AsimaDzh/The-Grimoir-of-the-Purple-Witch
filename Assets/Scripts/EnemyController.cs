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

    [Header("========== AI States ==========")]
    [SerializeField] private AIState currentState;

    [Header("========== Patrol ==========")]
    [SerializeField] private Transform wayPoints;
    [SerializeField] private float waitAtPoint = 1.5f;
    private int _currentPatrolIndex = 0;
    private float _waitCounter;


    [Header("========== Movement (grid) ==========")]
    [SerializeField] private LayerMask whatStopsMovement;
    private Vector3 _currentTargetCell;
    private bool _isMoving;
    private float _stepSize = 1f; // размер шага по сетке (обычно 1)
    private float _moveSpeed = 3f;

    private float _rotationVelocity;
    private float _smoothTime = 0.05f;
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
                    // Попытаться начать движение к текущему waypoint; если не получается — пройти дальше по списку
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

                    if (moved) currentState = AIState.Patrolling;
                    else _waitCounter = waitAtPoint;
                    // не получилось сдвинуться ни к одному waypoint — подождём и попробуем снова

                }
                break;


            case AIState.Patrolling:

                if (_isMoving) break; // если уже движемся, не менять цель

                if (wayPoints == null || wayPoints.childCount == 0) break;
                Vector3 waypointPos = wayPoints.GetChild(_currentPatrolIndex).position;
                    
                if (Vector3.Distance(RoundToGrid(transform.position), RoundToGrid(waypointPos)) <= 0.1f)
                {
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % wayPoints.childCount;
                    _waitCounter = waitAtPoint;
                    currentState = AIState.Idle;
                }
                else
                {
                    // пробуем сделать шаг к текущей точке; если не получилось (преграда или уже там) — переключаемся на следующий waypoint
                    bool moved = TryStepTowards(waypointPos);
                    if (!moved)
                        _currentPatrolIndex = (_currentPatrolIndex + 1) % wayPoints.childCount;
                }
                break;


            case AIState.Chasing:

                // если уже движемся к цели, не менять направление
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
        }
    }


    // Пытается сделать шаг по сетке в сторону цели (без диагоналей)
    private bool TryStepTowards(Vector3 worldTarget)
    {
        Vector3 delta = worldTarget - transform.position;
        Vector3 step = GetCardinalStep(delta);
        if (step == Vector3.zero) return false;

        Vector3 targetCell = RoundToGrid(transform.position) + step * _stepSize;

        if (Physics.CheckSphere(
            targetCell, 
            0.3f, 
            whatStopsMovement)) return false;

        _currentTargetCell = targetCell;
        _isMoving = true;

        // Плавное вращение: устанавливаем целевой угол, а фактическое вращение делается в Update
        _targetAngle = Mathf.Atan2(step.x, step.z) * Mathf.Rad2Deg;

        return true;
    }


    // Возвращает шаг по одной оси (+-1 по X или +-1 по Z), выбирая более далёкую ось
    private Vector3 GetCardinalStep(Vector3 delta)
    {
        float absX = Mathf.Abs(delta.x);
        float absZ = Mathf.Abs(delta.z);

        // если очень близко — не делать шага
        if (absX < 0.5f && absZ < 0.5f) return Vector3.zero;

        if (absX > absZ) 
            return new Vector3(Mathf.Sign(delta.x), 0f, 0f);
        else 
            return new Vector3(0f, 0f, Mathf.Sign(delta.z));
    }


    // Округление позиции к сетке (чтобы не было дрейфа)
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

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
