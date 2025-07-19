using UnityEngine;
using UnityEngine.AI;

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
    
    private NavMeshAgent agent;
    private GameObject currentTarget;
    private float lastTargetUpdate;
    private float lastAttackTime;
    private float attackStartTime;
    private bool isAttacking = false;
    
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
        
        FindNewTarget();
    }
    
    void Update()
    {
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