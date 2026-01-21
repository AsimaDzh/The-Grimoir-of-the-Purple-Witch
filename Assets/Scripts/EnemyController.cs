using UnityEngine;

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
    [SerializeField] private float waitAtPoint = 3f;
    private int _currentPatrolIndex = 0;
    private float _waitCounter;
    private bool _isWaitingAfterStep = false;


    [Header("========== Movement (grid) ==========")]
    [SerializeField] private LayerMask whatStopsMovement;
    [SerializeField] private float _stepSize = 1f; // размер шага по сетке (обычно 1)
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
            return; // не делать ничего, пока ждём после шага
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
                    // Если есть waypoints — используем существующую логику.
                    // Иначе пытаемся сделать случайный шаг.
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
                    else moved = TryRandomStep(); // Нет waypoint-ов — пробуем случайный шаг

                    if (moved) currentState = AIState.Patrolling;
                    else _waitCounter = waitAtPoint;
                    // не получилось сдвинуться — подождём и попробуем снова
                }
                break;


            case AIState.Patrolling:

                if (_isMoving) break; // если уже движемся, не менять цель

                if (wayPoints == null || wayPoints.childCount == 0)
                {
                    // Случайное патрулирование: пробуем сделать новый случайный шаг
                    bool moved = TryRandomStep();
                    if (!moved)
                    {
                        // если не получилось — переходим в ожидание
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
                        // пробуем сделать шаг к текущей точке; если не получилось (преграда или уже там) — переключаемся на следующий waypoint
                        bool moved = TryStepTowards(waypointPos);
                        if (!moved)
                            _currentPatrolIndex = (_currentPatrolIndex + 1) % wayPoints.childCount;
                    }
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

            if (currentState == AIState.Patrolling && (wayPoints == null || wayPoints.childCount == 0))
            {
                _isWaitingAfterStep = true;
                _waitCounter = waitAtPoint;
            }      
        }
    }


    // Пытается сделать шаг по сетке в сторону цели (без диагоналей)
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

        // Плавное вращение: устанавливаем целевой угол, а фактическое вращение делается в Update
        _targetAngle = Mathf.Atan2(step.x, step.z) * Mathf.Rad2Deg;

        return true;
    }


    //Координаты четырёх направлений (вперёд, назад, влево, вправо)
    private static readonly Vector3[] CardinalDirections =
    {
        new Vector3(3f, 0f, 0f),
        new Vector3(-3f, 0f, 0f),
        new Vector3(0f, 0f, 3f),
        new Vector3(0f, 0f, -3f)
    };


    // Перемешивает массив направлений случайным образом
    private void ShuffleDirections(Vector3[] dirs)
    {
        for (int i = 0; i < dirs.Length; i++)
        {
            int rnd = Random.Range(i, dirs.Length);
            (dirs[i], dirs[rnd]) = (dirs[rnd], dirs[i]);
        }
    }


    // Пытается сделать случайный шаг в одном из четырёх направлений
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
                new Vector3(0.45f, 0.5f, 0.45f),// half-extents - половина размеров коробки
                Quaternion.identity,
                whatStopsMovement)) continue;

            _currentTargetCell = targetCell;
            _isMoving = true;
            _targetAngle = Mathf.Atan2(step.x, step.z) * Mathf.Rad2Deg;
            return true;
        }
        return false;
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
