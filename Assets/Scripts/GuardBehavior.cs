using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GuardBehavior : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolRange = 10f;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float waitTimeAtPoint = 2f;
    
    [Header("Combat Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 1f;
    [SerializeField] private float attackDamage = 30f;
    
    [Header("Health System")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runAnimationName = "IsRunning";
    [SerializeField] private string attackAnimationName = "IsAttacking";
    
    private NavMeshAgent agent;
    private Vector3 startPosition;
    private Vector3 currentPatrolTarget;
    private GameObject currentEnemy;
    private float lastAttackTime;
    private float attackStartTime;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isPatrolling = true;
    private float waitTimer = 0f;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("Guard animator not found! Make sure the guard has an Animator component.");
        }
        else
        {
            Debug.Log($"Guard animator found: {animator.name}");
        }
        
        // Find HP bar slider if not assigned
        if (healthBar == null)
        {
            FindHealthBar();
        }
        
        // Initialize health
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        // Set patrol settings
        startPosition = transform.position;
        agent.speed = patrolSpeed;
        
        // Start patrolling
        SetNewPatrolTarget();
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Check for enemies in detection range
        GameObject enemy = FindNearestEnemy();
        
        if (enemy != null)
        {
            // Enemy found, switch to combat mode
            isPatrolling = false;
            currentEnemy = enemy;
            HandleCombat();
        }
        else
        {
            // No enemy, return to patrolling
            if (!isPatrolling)
            {
                isPatrolling = true;
                currentEnemy = null;
                SetNewPatrolTarget();
            }
            HandlePatrol();
        }
        
        UpdateAnimations();
        
        // Check if attack duration is over
        if (isAttacking && Time.time - attackStartTime >= attackDuration)
        {
            isAttacking = false;
        }
    }
    
    void HandlePatrol()
    {
        if (currentPatrolTarget == Vector3.zero)
        {
            SetNewPatrolTarget();
            return;
        }
        
        float distanceToTarget = Vector3.Distance(transform.position, currentPatrolTarget);
        
        if (distanceToTarget <= 1f)
        {
            // Reached patrol point, wait
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                SetNewPatrolTarget();
                waitTimer = 0f;
            }
        }
        else
        {
            // Move to patrol target
            agent.SetDestination(currentPatrolTarget);
        }
    }
    
    void HandleCombat()
    {
        if (currentEnemy == null) return;
        
        float distanceToEnemy = Vector3.Distance(transform.position, currentEnemy.transform.position);
        
        if (distanceToEnemy <= attackRange)
        {
            // In attack range
            if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
            {
                Attack();
            }
            else if (!isAttacking)
            {
                agent.isStopped = true;
            }
        }
        else
        {
            // Move to enemy
            agent.isStopped = false;
            agent.SetDestination(currentEnemy.transform.position);
        }
    }
    
    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        attackStartTime = Time.time;
        agent.isStopped = true;
        
        // Face the enemy
        if (currentEnemy != null)
        {
            Vector3 direction = (currentEnemy.transform.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        if (animator != null)
        {
            animator.SetBool(attackAnimationName, true);
        }
        
        // Deal damage in the middle of the animation
        StartCoroutine(DealDamageWithDelay(attackDuration * 0.5f));
    }
    
    System.Collections.IEnumerator DealDamageWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentEnemy != null)
        {
            EnemyBehavior enemyBehavior = currentEnemy.GetComponent<EnemyBehavior>();
            if (enemyBehavior != null && !enemyBehavior.IsDead())
            {
                enemyBehavior.TakeDamage(attackDamage);
                Debug.Log($"Guard dealt {attackDamage} damage to enemy");
            }
        }
    }
    
    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearestEnemy = null;
        float nearestDistance = detectionRange;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= detectionRange && distance < nearestDistance)
            {
                nearestEnemy = enemy;
                nearestDistance = distance;
            }
        }
        
        return nearestEnemy;
    }
    
    void SetNewPatrolTarget()
    {
        // Generate random point within patrol range
        Vector2 randomCircle = Random.insideUnitCircle * patrolRange;
        Vector3 randomPoint = startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // Make sure point is on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1))
        {
            currentPatrolTarget = hit.position;
            agent.SetDestination(currentPatrolTarget);
        }
        else
        {
            // If random point failed, try again
            SetNewPatrolTarget();
        }
    }
    
    void FindHealthBar()
    {
        // Look for slider in children (including canvas)
        Slider[] sliders = GetComponentsInChildren<UnityEngine.UI.Slider>();
        if (sliders.Length > 0)
        {
            healthBar = sliders[0];
            Debug.Log($"Found HP bar slider: {healthBar.name}");
        }
        else
        {
            Debug.LogWarning("No slider found in guard children! Make sure the HP bar slider is a child of this guard.");
        }
    }
    
    void UpdateHealthBar()
    {
        // Try to find health bar if it's null
        if (healthBar == null)
        {
            FindHealthBar();
        }
        
        if (healthBar != null)
        {
            // Set the slider's max value to match maxHealth
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        else
        {
            Debug.LogWarning("Health bar is still null! Make sure the guard has a UI Slider as a child.");
        }
    }
    
    void UpdateAnimations()
    {
        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            
            // Run when moving (patrolling or chasing enemy) and not attacking
            animator.SetBool(runAnimationName, isMoving && !isAttacking);
            
            // Attack animation when attacking
            animator.SetBool(attackAnimationName, isAttacking);
            
            // Debug logging to see what's happening
            Debug.Log($"Guard moving: {isMoving}, attacking: {isAttacking}, velocity: {agent.velocity.magnitude}");
        }
        else
        {
            Debug.LogWarning("Guard animator is null!");
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        UpdateHealthBar();
        
        Debug.Log($"Guard took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    void Die()
    {
        isDead = true;
        Debug.Log("Guard died!");
        
        // Stop all behavior
        if (agent != null)
        {
            agent.isStopped = true;
        }
        
        // You can add death animation here
        if (animator != null)
        {
            // animator.SetTrigger("Die");
        }
        
        // Destroy the guard after a delay (or play death animation)
        Destroy(gameObject, 2f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw patrol range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, patrolRange);
        
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw patrol target
        if (currentPatrolTarget != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentPatrolTarget, 0.5f);
            Gizmos.DrawLine(transform.position, currentPatrolTarget);
        }
        
        // Draw enemy target
        if (currentEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentEnemy.transform.position);
        }
    }
} 