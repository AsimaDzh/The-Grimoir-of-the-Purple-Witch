using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float _moveSpeed = 5f;
    private float _rotationSpeed = 10f;

    [SerializeField] private Transform movePoint;
    [SerializeField] private LayerMask whatStopsMovement;
    [SerializeField] private Animator anim;

    void Start()
    {
        movePoint.parent = null;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, _moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movePoint.position) <= .05f)
        {
            float _horizontal = Input.GetAxisRaw("Horizontal");
            float _vertical = Input.GetAxisRaw("Vertical");

            if (Mathf.Abs(_horizontal) == 1f)
            {
                Vector3 targetPos = movePoint.position + new Vector3(_horizontal, 0f, 0f);
                if (!Physics.CheckSphere(targetPos, .2f, whatStopsMovement.value))
                    movePoint.position = targetPos;
            }
            else if (Mathf.Abs(_vertical) == 1f)
            {
                Vector3 targetPos = movePoint.position + new Vector3(0f, 0f, _vertical);
                if (!Physics.CheckSphere(targetPos, .2f, whatStopsMovement.value))
                    movePoint.position = targetPos;
            }

            anim.SetBool("isMoving", false);
        }
        else
        {
            anim.SetBool("isMoving", true);
        }
    }
}
