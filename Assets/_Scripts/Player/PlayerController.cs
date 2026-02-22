using UnityEngine;


public class PlayerController : MonoBehaviour
{
    private float _moveSpeed = 5f;
    private float _smoothTime = 0.05f;
    private float _currentVelocity;
    private float _targetAngle;
    private Vector3 _lastMoveDir;

    [SerializeField] private Transform movePoint;
    [SerializeField] private LayerMask whatStopsMovement;
    [SerializeField] private Animator anim;


    void Start()
    {
        movePoint.parent = null;
    }

    private void Update()
    {
        OnMove();
    }


    private void OnMove()
    {
        SmoothRotation();

        transform.position = Vector3.MoveTowards(
            transform.position, 
            movePoint.position, 
            _moveSpeed * Time.deltaTime);


        if (Vector3.Distance(transform.position, movePoint.position) <= .05f)
        {
            // Input handling when at movePoint
            float _horizontal = Input.GetAxisRaw("Horizontal");
            float _vertical = Input.GetAxisRaw("Vertical");

            Vector3 moveDir = Vector3.zero;
            if (Mathf.Abs(_horizontal) == 1f)
                moveDir = new Vector3(_horizontal, 0f, 0f);
            else if (Mathf.Abs(_vertical) == 1f)
                moveDir = new Vector3(0f, 0f, _vertical);

            // Check for collisions and move
            if (moveDir != Vector3.zero)
            {
                Vector3 targetPos = movePoint.position + moveDir;

                if (!Physics.CheckBox(
                    targetPos,
                    new Vector3(0.45f, 0.5f, 0.45f), 
                    Quaternion.identity, 
                    whatStopsMovement))
                {
                    movePoint.position = targetPos;
                    _lastMoveDir = moveDir;
                    _targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                    anim.SetBool("isMoving", true);
                }
            }
            else anim.SetBool("isMoving", false);
        }
    }


    private void SmoothRotation()
    {
        var angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            _targetAngle,
            ref _currentVelocity,
            _smoothTime);

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }


    void LateUpdate() // Snap movePoint to grid
    {
        movePoint.position = new Vector3(
            Mathf.Round(movePoint.position.x),
            movePoint.position.y,
            Mathf.Round(movePoint.position.z));
    }
}
