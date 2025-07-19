using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float attackDuration = 1f;
    
    [Header("Combat Settings")]
    [SerializeField] private float sliceAttackRange = 2f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sliceDamage = 25f;
    [SerializeField] private float shootDamage = 15f;
    
    [Header("References")]
    [SerializeField] private BuildingSystem buildingSystem;
    [SerializeField] private PlayerMovement playerMovement;
    
    private PlayerInput playerInput;
    private float lastAttackTime;
    private bool isAttacking = false;
    private GameObject targetEnemy;
    private bool isMovingToEnemy = false;
    
    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (buildingSystem == null)
        {
            buildingSystem = FindFirstObjectByType<BuildingSystem>();
        }
        
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
    }
    
    void Update()
    {
        // Move to enemy if needed
        if (isMovingToEnemy && targetEnemy != null)
        {
            MoveToEnemy();
        }
        
        // Check if attack duration is over
        if (isAttacking && Time.time - lastAttackTime >= attackDuration)
        {
            ResetAttackAnimations();
        }
    }
    
    void OnEnable()
    {
        // Don't use input system to avoid conflicts
    }
    
    void OnDisable()
    {
        // Don't use input system to avoid conflicts
    }
    

    
    public void HandleLeftClick()
    {
        if (isAttacking) return;
        
        Debug.Log("LEFT CLICK DETECTED");
        GameObject enemy = GetEnemyUnderMouse();
        
        if (enemy != null)
        {
            Debug.Log($"LEFT CLICK: Found enemy {enemy.name} - STARTING SLICE ATTACK");
            
            targetEnemy = enemy;
            FaceEnemy();
            
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy > sliceAttackRange)
            {
                // Need to move to enemy first
                isMovingToEnemy = true;
                if (playerMovement != null)
                {
                    playerMovement.SetCombatMovement(true);
                }
                Debug.Log($"Moving to enemy first (distance: {distanceToEnemy}, range: {sliceAttackRange})");
            }
            else
            {
                // Already in range, attack immediately
                StartAttack("Slice");
            }
        }
        else
        {
            // No enemy clicked - just cancel combat movement, don't interfere with PlayerMovement
            Debug.Log("LEFT CLICK: No enemy found - canceling combat movement");
            CancelCombatMovement();
        }
    }
    
    public void HandleRightClick()
    {
        if (isAttacking) return;
        
        Debug.Log("RIGHT CLICK DETECTED");
        GameObject enemy = GetEnemyUnderMouse();
        
        if (enemy != null)
        {
            Debug.Log($"RIGHT CLICK: Found enemy {enemy.name} - STARTING SHOOT ATTACK");
            
            targetEnemy = enemy;
            FaceEnemy();
            StartAttack("Shoot");
        }
        else
        {
            // No enemy clicked - cancel any combat movement and let BuildingSystem handle it
            Debug.Log("RIGHT CLICK: No enemy found - canceling combat movement");
            CancelCombatMovement();
        }
    }
    
    void FaceEnemy()
    {
        if (targetEnemy != null)
        {
            Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;
            direction.y = 0; // Keep rotation on horizontal plane
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
    
    void MoveToEnemy()
    {
        if (targetEnemy == null) return;
        
        float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.transform.position);
        
        if (distanceToEnemy <= sliceAttackRange)
        {
            // Reached attack range, stop moving and attack
            isMovingToEnemy = false;
            if (playerMovement != null)
            {
                playerMovement.SetCombatMovement(false);
            }
            FaceEnemy();
            StartAttack("Slice");
            return;
        }
        
        // Move towards enemy
        Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;
        direction.y = 0; // Keep movement on horizontal plane
        
        Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        transform.position = newPosition;
        
        // Face enemy while moving
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    void StartAttack(string attackType)
    {
        if (animator != null)
        {
            animator.SetBool(attackType, true);
            isAttacking = true;
            lastAttackTime = Time.time;
            Debug.Log($"Started {attackType} attack for {attackDuration} seconds");
        }
        
        // Deal damage to enemy with timing
        if (targetEnemy != null)
        {
            Debug.Log($"Target enemy found: {targetEnemy.name}");
            EnemyBehavior enemyBehavior = targetEnemy.GetComponent<EnemyBehavior>();
            if (enemyBehavior != null)
            {
                float damage = (attackType == "Slice") ? sliceDamage : shootDamage;
                Debug.Log($"Scheduling {damage} damage to enemy with {attackType} attack");
                
                // Deal damage in the middle of the animation
                float damageDelay = attackDuration * 0.5f; // Middle of animation
                StartCoroutine(DealDamageWithDelay(enemyBehavior, damage, damageDelay));
            }
            else
            {
                Debug.LogWarning($"Enemy {targetEnemy.name} doesn't have EnemyBehavior component!");
            }
        }
        else
        {
            Debug.LogWarning("No target enemy found for damage!");
        }
    }
    
    void ResetAttackAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("Slice", false);
            animator.SetBool("Shoot", false);
        }
        
        isAttacking = false;
        targetEnemy = null;
        isMovingToEnemy = false;
        
        // Let PlayerMovement resume animation control
        if (playerMovement != null)
        {
            playerMovement.SetCombatMovement(false);
        }
        
        Debug.Log("Attack animations reset");
    }
    
    public void CancelCombatMovement()
    {
        // Cancel any combat movement and let other systems handle the click
        isMovingToEnemy = false;
        targetEnemy = null;
        
        // Let PlayerMovement resume animation control
        if (playerMovement != null)
        {
            playerMovement.SetCombatMovement(false);
        }
        
        Debug.Log("Combat movement canceled - allowing other systems to handle click");
    }
    
    System.Collections.IEnumerator DealDamageWithDelay(EnemyBehavior enemy, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (enemy != null && !enemy.IsDead())
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Dealt {damage} damage to enemy after {delay} seconds delay");
        }
        else
        {
            Debug.Log("Enemy is null or dead, skipping damage");
        }
    }
    
    GameObject GetEnemyUnderMouse()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Hit object: {hit.collider.name} with tag: {hit.collider.tag}");
            if (hit.collider.CompareTag("Enemy"))
            {
                return hit.collider.gameObject;
            }
        }
        else
        {
            Debug.Log("No object hit with raycast");
        }
        
        return null;
    }
} 