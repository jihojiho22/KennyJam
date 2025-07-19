using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private bool lockYAxis = true;
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    void Update()
    {
        if (mainCamera != null)
        {
            Vector3 direction = mainCamera.transform.position - transform.position;
            
            if (lockYAxis)
            {
                direction.y = 0;
            }
            
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
} 