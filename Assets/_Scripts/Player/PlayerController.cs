using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [Header("========== Movement (grid) ==========")]
    [SerializeField] private Transform movePoint;
    [SerializeField] private LayerMask whatStopsMovement;

    [Header("========== Stats & Animation ==========")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Animator anim;

    private float _speed = 5f;
    private float _smoothTime = 0.05f;
    private float _currentVelocity;
    private float _targetAngle;
    private Vector3 _lastMoveDir;


    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
        
        if (InputManager.Instance == null)
        {
            Debug.LogError("PlayerController: InputManager instance not found in the scene!", this);
            return;
        }     
    }


    void Start()
    {
        movePoint.parent = null;
    }


    private void Update()
    {
        OnMove();
        InputManager.Instance.ResetButtonFlags();
    }


    private void OnMove()
    {
        SmoothRotation();

        if (playerStats != null && playerStats.playerData != null)
            _speed = playerStats.playerData.moveSpeed;

        transform.position = Vector3.MoveTowards(
            transform.position, 
            movePoint.position,
            _speed * Time.deltaTime);


        if (Vector3.Distance(transform.position, movePoint.position) <= .05f)
        {
            Vector3 _moveDir = Vector3.zero;
            Vector2 _moveInput = InputManager.Instance.MoveInput;
            float _horizontal = Mathf.Round(_moveInput.x);
            float _vertical = Mathf.Round(_moveInput.y);

            if (Mathf.Abs(_horizontal) == 1f)
                _moveDir = new Vector3(_horizontal, 0f, 0f);
            else if (Mathf.Abs(_vertical) == 1f)
                _moveDir = new Vector3(0f, 0f, _vertical);

            // Check for collisions and move
            if (_moveDir != Vector3.zero)
            {
                Vector3 _targetPos = movePoint.position + _moveDir;

                if (!Physics.CheckBox(
                    _targetPos,
                    new Vector3(0.45f, 0.5f, 0.45f), 
                    Quaternion.identity, 
                    whatStopsMovement))
                {
                    movePoint.position = _targetPos;
                    _lastMoveDir = _moveDir;
                    _targetAngle = Mathf.Atan2(_moveDir.x, _moveDir.z) * Mathf.Rad2Deg;
                    anim.SetBool("isMoving", true);
                }
            }
            else anim.SetBool("isMoving", false);
        }
    }


    private void SmoothRotation()
    {
        var _angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            _targetAngle,
            ref _currentVelocity,
            _smoothTime);

        transform.rotation = Quaternion.Euler(0f, _angle, 0f);
    }


    void LateUpdate() // Snap movePoint to grid
    {
        movePoint.position = new Vector3(
            Mathf.Round(movePoint.position.x),
            movePoint.position.y,
            Mathf.Round(movePoint.position.z));
    }
}
