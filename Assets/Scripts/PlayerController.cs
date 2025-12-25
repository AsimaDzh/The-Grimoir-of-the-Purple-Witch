using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float _moveSpeed = 5f;
    private float _smoothTime = 0.05f;
    private float _currentVelocity;
    private Vector3 _lastMoveDir;

    [SerializeField] private Transform movePoint;
    [SerializeField] private LayerMask whatStopsMovement;
    [SerializeField] private Animator anim;

    void Start()
    {
        movePoint.parent = null;
    }

    void Update() // Movement and rotating
    {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            movePoint.position, 
            _moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movePoint.position) <= .05f)
        {
            float _horizontal = Input.GetAxisRaw("Horizontal");
            float _vertical = Input.GetAxisRaw("Vertical");

            Vector3 moveDir = Vector3.zero;

            if (Mathf.Abs(_horizontal) == 1f)
                moveDir = new Vector3(_horizontal, 0f, 0f);
            else if (Mathf.Abs(_vertical) == 1f)
                moveDir = new Vector3(0f, 0f, _vertical);

            if (moveDir != Vector3.zero)
            {
                Vector3 targetPos = movePoint.position + moveDir;

                if (!Physics.CheckSphere(targetPos, .2f, whatStopsMovement))
                {
                    movePoint.position = targetPos;
                    _lastMoveDir = moveDir;

                    var targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;

                    var angle = Mathf.SmoothDampAngle(
                        transform.eulerAngles.y,
                        targetAngle,
                        ref _currentVelocity,
                        _smoothTime);

                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }

                anim.SetBool("isMoving", true);
            }
            else
                anim.SetBool("isMoving", false);
        }
    }

    void LateUpdate() // Adjusting the movePoint position
    {
        movePoint.position = new Vector3(
            Mathf.Round(movePoint.position.x),
            movePoint.position.y,
            Mathf.Round(movePoint.position.z));
    }
}
