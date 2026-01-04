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
    private float _stepSize = 1f; // размер шага по сетке (обычно 1)
    private float _moveSpeed = 4f;
    private bool _isMoving;

    [Header("========== Chasing ==========")]
    [SerializeField] private float detectionRange;

    [Header("========== Suspicious ==========")]
    [SerializeField] private float suspiciousTime;

    private GameObject player;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        _waitCounter = waitAtPoint;
        SnapToGrid();
    }

    
    void Update()
    {
        if (player == null) return;

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
                    currentState = AIState.Patrolling;
                    // стартуем патруль с попытки сделать шаг к следующей точке
                    TryStepTowards(wayPoints.GetChild(_currentPatrolIndex).position);
                }
                break;


            case AIState.Patrolling:

                if (_isMoving) return; // ждём окончания движения к текущей цели
                
                Vector3 waypointPos = wayPoints.GetChild(_currentPatrolIndex).position;
                    
                if (Vector3.Distance(RoundToGrid(transform.position), RoundToGrid(waypointPos)) <= 0.1f)
                {
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % wayPoints.childCount;
                    _waitCounter = waitAtPoint;
                    currentState = AIState.Idle;
                }
                else
                {
                    // иначе делаем следующий шаг к той же точке (шаг по оси, без диагоналей)
                    TryStepTowards(waypointPos);
                }
                break;


            case AIState.Chasing:

                // если уже движемся к цели, не менять направление
                if (_isMoving) return;
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
    private void TryStepTowards(Vector3 worldTarget)
    {
        Vector3 delta = worldTarget - transform.position;
        Vector3 step = GetCardinalStep(delta);
        if (step == Vector3.zero) return;

        Vector3 targetCell = RoundToGrid(transform.position) + step * _stepSize;

        if (Physics.CheckSphere(targetCell, 0.3f, whatStopsMovement))
            return;

        _currentTargetCell = targetCell;
        _isMoving = true;

        RotateTo(step);
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

    void SnapToGrid()
    {
        transform.position = RoundToGrid(transform.position);
    }


    void RotateTo(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        transform.rotation = Quaternion.LookRotation(dir);
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
