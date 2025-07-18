using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.1f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private float maxRaycastDistance = 100f;
    
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Camera playerCamera;
    private PlayerInput playerInput;
    private InputAction clickAction;
    
    void Awake()
    {
        // Set up input actions
        playerInput = new PlayerInput();
    }
    
    void OnEnable()
    {
        // Enable the input action map
        playerInput.Enable();
        
        // Subscribe to the click action
        clickAction = playerInput.Player.Click;
        clickAction.performed += OnClick;
    }
    
    void OnDisable()
    {
        // Unsubscribe and disable input
        if (clickAction != null)
            clickAction.performed -= OnClick;
        playerInput.Disable();
    }
    
    void Start()
    {
        // Get the main camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("No main camera found! Please tag your camera as 'MainCamera'");
        }
        
        // Set initial target position to current position
        targetPosition = transform.position;
    }
    
    void Update()
    {
        MoveToTarget();
    }
    
    void OnClick(InputAction.CallbackContext context)
    {
        HandleMouseInput();
    }
    
    void HandleMouseInput()
    {
        // Get mouse position from the input system
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        // Cast ray to detect ground
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayer))
        {
            // Set target position to hit point
            targetPosition = hit.point;
            isMoving = true;
            
            // Optional: Add visual feedback
            Debug.Log($"Moving to: {targetPosition}");
        }
    }
    
    void MoveToTarget()
    {
        if (!isMoving) return;
        
        // Calculate distance to target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        // Check if we've reached the target
        if (distanceToTarget <= stoppingDistance)
        {
            isMoving = false;
            return;
        }
        
        // Calculate direction to target
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // Move towards target
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Optional: Rotate player to face movement direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    // Optional: Visualize the target position in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
        
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
} 