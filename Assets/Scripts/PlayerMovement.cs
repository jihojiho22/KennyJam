using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.1f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private float maxRaycastDistance = 100f;
    
    [Header("Collision Detection")]
    [SerializeField] private LayerMask obstacleLayer = 1;
    [SerializeField] private float playerRadius = 0.5f;
    
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Camera playerCamera;
    private PlayerInput playerInput;
    private InputAction leftClickAction;
    
    void Awake()
    {
        playerInput = new PlayerInput();
    }
    
    void OnEnable()
    {
        playerInput.Enable();
        
        leftClickAction = playerInput.Player.LeftClick;
        leftClickAction.performed += OnLeftClick;
    }
    
    void OnDisable()
    {
        if (leftClickAction != null)
            leftClickAction.performed -= OnLeftClick;
        if (playerInput != null)
            playerInput.Disable();
    }
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("No main camera found! Please tag your camera as 'MainCamera'");
        }
        
        targetPosition = transform.position;
    }

    void Update()
    {
        MoveToTarget();
        animator.SetBool("IsRunning", isMoving);
    }
    
    void OnLeftClick(InputAction.CallbackContext context)
    {
        HandleMouseInput();
    }
    
    void HandleMouseInput()
    {
        if (IsMouseOverUI())
            return;
            
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayer))
        {
            targetPosition = hit.point;
            isMoving = true;
            
            Debug.Log($"Moving to: {targetPosition}");
        }
    }
    
    void MoveToTarget()
    {
        if (!isMoving) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (distanceToTarget <= stoppingDistance)
        {
            isMoving = false;
            return;
        }
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        float moveDistance = moveSpeed * Time.deltaTime;
        
        Vector3 newPosition = transform.position + direction * moveDistance;
        
        RaycastHit hit;
        
        if (Physics.SphereCast(transform.position, playerRadius, direction, out hit, moveDistance, obstacleLayer))
        {
            Debug.Log($"Obstacle detected: {hit.collider.name}");
            isMoving = false;
            return;
        }
        
        transform.position = newPosition;
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    bool IsMouseOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null && 
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
    
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