using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform _target;
  
    void Update()
    {
        transform.position = new Vector3(
            _target.position.x,
            transform.position.y,
            _target.position.z);
    }
}
