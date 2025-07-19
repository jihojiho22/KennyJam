using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float updateTargetInterval = 0.5f;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 1f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runAnimationName = "isRunning";
    [SerializeField] private string attackAnimationName = "isAttacking";
    
    [Header("Health System")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    
    private NavMeshAgent agent;
    private GameObject currentTarget;
    private float lastTargetUpdate;
    private float lastAttackTime;
    private float attackStartTime;
    private bool isAttacking = false;
    private bool isDead = false;
    
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
        
        // Find HP bar slider if not assigned
        if (healthBar == null)
        {
            FindHealthBar();
        }
        
        // Initialize health
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        FindNewTarget();
    }
    
    void Update()
    {
        if (isDead) return;
        
        if (currentTarget == null)
        {
            FindNewTarget();
        }
        
        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            if (distanceToTarget <= attackRange)
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Attack();
                }
                else if (isAttacking && Time.time - attackStartTime >= attackDuration)
                {
                    isAttacking = false;
                    agent.isStopped = true;
                }
                else if (!isAttacking)
                {
                    agent.isStopped = true;
                }
            }
            else
            {
                MoveToTarget();
            }
            
            UpdateAnimations();
        }
    }
    
    void FindNewTarget()
    {
        GameObject blackEnergy = GameObject.FindGameObjectWithTag("BlackEnergy");
        
        if (blackEnergy != null)
        {
            currentTarget = blackEnergy;
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
            Debug.LogWarning("No slider found in enemy children! Make sure the HP bar slider is a child of this enemy.");
        }
    }
    
    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        attackStartTime = Time.time;
        agent.isStopped = true;
        
        if (animator != null)
        {
            animator.SetBool(attackAnimationName, true);
        }
    }
    
    void MoveToTarget()
    {
        isAttacking = false;
        agent.isStopped = false;
        agent.SetDestination(currentTarget.transform.position);
    }
    
    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool(runAnimationName, !isAttacking && agent.velocity.magnitude > 0.1f);
            animator.SetBool(attackAnimationName, isAttacking);
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        Debug.Log($"Enemy TakeDamage called with {damage} damage. Current health before: {currentHealth}");
        
        currentHealth -= damage;
        UpdateHealthBar();
        
        Debug.Log($"Enemy took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public bool IsDead()
    {
        return isDead;
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
            Debug.Log($"Health bar updated: {currentHealth}/{maxHealth}");
        }
        else
        {
            Debug.LogWarning("Health bar is still null! Make sure the enemy has a UI Slider as a child.");
        }
    }
    
    void Die()
    {
        isDead = true;
        Debug.Log("Enemy died!");
        
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
        
        // Destroy the enemy after a delay (or play death animation)
        Destroy(gameObject, 2f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
} 