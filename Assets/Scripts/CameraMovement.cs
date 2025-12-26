using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform _target;
    private float _smoothRate = 5f;
  
    void LateUpdate()
    {
        Vector3 desiredPosition = new Vector3(
            _target.position.x,
            transform.position.y,
            _target.position.z);

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            Time.deltaTime * _smoothRate);

        transform.position = smoothedPosition;
    }
}
