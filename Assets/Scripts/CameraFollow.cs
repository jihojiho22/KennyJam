using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -10);
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool lookAtTarget = true;
    
    void Start()
    {
        // If no target is assigned, try to find the player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("No target assigned and no GameObject with 'Player' tag found!");
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move camera to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        
        // Optionally look at target
        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }
    
    // Optional: Set target from another script
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
} 