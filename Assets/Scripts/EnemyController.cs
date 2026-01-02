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

    [Header("========== Components ==========")]
    NavMeshAgent _navMeshAgent;

    [Header("========== Movement (grid) ==========")]
    [SerializeField] private LayerMask whatStopsMovement;
    private float _stepSize = 1f; // размер шага по сетке (обычно 1)
    private float _reachThreshold = 0.2f; // порог достижения точки назначения
    private Vector3 _currentGridTarget;

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

        // Отключаем автоматическое перемещение позиции агента по Transform,
        // но навмеш всё ещё будет управлять позицией объекта.
        if (_navMeshAgent != null)
            _navMeshAgent.updatePosition = true;
    }

    
    void FixedUpdate()
    {
        if (player == null || _navMeshAgent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case AIState.Idle:

                if (_waitCounter > 0)
                    _waitCounter -= Time.fixedDeltaTime;
                else
                {
                    currentState = AIState.Patrolling;
                    // стартуем патруль с попытки сделать шаг к следующей точке
                    TryStepTowards(wayPoints.GetChild(_currentPatrolIndex).position);
                }

                if (distanceToPlayer <= detectionRange)
                {
                    currentState = AIState.Chasing;
                    _timeSinceLastSeen = suspiciousTime;
                }
                break;


            case AIState.Patrolling:

                // когда агент дошёл до последней целевой клетки — делаем следующий шаг
                if (_navMeshAgent.remainingDistance <= _reachThreshold)
                {
                    // Если уже рядом с целевой патрульной точкой — переключаемся в Idle и ждём
                    Vector3 waypointPos = wayPoints.GetChild(_currentPatrolIndex).position;
                    if (Vector3.Distance(RoundToGrid(transform.position), RoundToGrid(waypointPos)) <= 0.1f)
                    {
                        _currentPatrolIndex++;

                        if (_currentPatrolIndex >= wayPoints.childCount)
                            _currentPatrolIndex = 0;

                        currentState = AIState.Idle;
                        _waitCounter = _waitAtPoint;
                    }
                    else
                    {
                        // иначе делаем следующий шаг к той же точке (шаг по оси, без диагоналей)
                        TryStepTowards(waypointPos);
                    }
                }

                if (distanceToPlayer <= detectionRange)
                {
                    currentState = AIState.Chasing;
                    _timeSinceLastSeen = suspiciousTime;
                }
                break;


            case AIState.Chasing:

                // Только когда текущая цель пройдена, ставим следующую цель (шаг)
                if (_navMeshAgent.remainingDistance <= _reachThreshold)
                {
                    Vector3 step = GetCardinalStep(player.transform.position - transform.position);
                    if (step != Vector3.zero)
                    {
                        Vector3 targetPos = RoundToGrid(transform.position) + step * _stepSize;
                        if (!Physics.CheckSphere(targetPos, 0.2f, whatStopsMovement))
                        {
                            _navMeshAgent.isStopped = false;
                            _navMeshAgent.SetDestination(targetPos);
                        }
                        else
                        {
                            // заблокировано — останавливаемся и начинаем уменьшать таймер подозрительности
                            _navMeshAgent.isStopped = true;
                            _timeSinceLastSeen -= Time.fixedDeltaTime;
                        }
                    }
                }

                if (distanceToPlayer > detectionRange)
                {
                    // если игрок ушёл из зоны — уменьшаем таймер
                    _timeSinceLastSeen -= Time.fixedDeltaTime;
                    if (_timeSinceLastSeen <= 0f)
                    {
                        currentState = AIState.Idle;
                        _timeSinceLastSeen = suspiciousTime;
                        _navMeshAgent.isStopped = false;
                    }
                }
                // если видим игрока — обновляем таймер
                else _timeSinceLastSeen = suspiciousTime;
                break;
        }
    }


    // Пытается сделать шаг по сетке в сторону цели (без диагоналей)
    private void TryStepTowards(Vector3 targetWorldPos)
    {
        Vector3 step = GetCardinalStep(targetWorldPos - transform.position);
        if (step == Vector3.zero) return;
        Vector3 targetPos = RoundToGrid(transform.position) + step * _stepSize;
        
        if (!Physics.CheckSphere(targetPos, 0.2f, whatStopsMovement))
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(targetPos);
        }
        else _navMeshAgent.ResetPath();
    }


    // Возвращает шаг по одной оси (+-1 по X или +-1 по Z), выбирая более далёкую ось
    private Vector3 GetCardinalStep(Vector3 delta)
    {
        float absX = Mathf.Abs(delta.x);
        float absZ = Mathf.Abs(delta.z);

        // если очень близко — не делать шага
        if (absX < 0.5f && absZ < 0.5f) return Vector3.zero;

        if (absX >= absZ) 
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
