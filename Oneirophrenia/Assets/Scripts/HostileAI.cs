using UnityEngine;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Detection")]
    public float sightRange = 10f;
    public LayerMask Ground, Player;

    [Header("Patrol Settings")]
    public float walkPointRange = 10f;
    public float waitTimeAtPoint = 2f;

    [Header("Investigation Settings")]
    public float investigationTime = 3f;

    [Header("Melee Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    private Vector3 walkPoint;
    private bool walkPointSet;
    private float waitTimer;
    private bool investigating = false;
    private float investigationTimer;

    // Variable for the investigation point - This should be declared at the class level
    private Vector3 investigationPoint;     // The position that the AI will move to for investigation

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null)
                player = obj.transform;
        }
    }

    private void Update()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogError("NOT ON NAVMESH");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= sightRange)
        {
            ChasePlayer();
        }
        else if (investigating)
        {
            Investigate();
        }
        else
        {
            Patroling();
        }

        // Melee attack when player is in range and attack cooldown has passed
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            MeleeAttack();
        }
    }

    #region Patroling
    void Patroling()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
            return;
        }

        agent.SetDestination(walkPoint);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                walkPointSet = false;
                waitTimer = 0f;
            }
        }
    }

    void SearchWalkPoint()
    {
        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(-walkPointRange, walkPointRange);
            float randomZ = Random.Range(-walkPointRange, walkPointRange);

            Vector3 randomPoint = new Vector3(
                transform.position.x + randomX,
                transform.position.y + 20f,
                transform.position.z + randomZ
            );

            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 40f, Ground))
            {
                Vector3 candidate = hit.point;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    walkPoint = navHit.position;
                    walkPointSet = true;
                    return;
                }
            }
        }

        walkPointSet = false;
    }
    #endregion

    #region Chasing
    void ChasePlayer()
    {
        agent.SetDestination(player.position);
        investigating = false; // Stop investigating if chasing the player
    }
    #endregion

    #region Investigation
    public void TriggerInvestigation(Vector3 point)
    {
        investigationPoint = point;  // Set the location to investigate
        investigating = true;        // Set investigating flag to true
        agent.SetDestination(investigationPoint);  // Move the AI to the investigation point
        investigationTimer = 0f;     // Reset the investigation timer
        Debug.Log($"Investigation triggered at {investigationPoint}");
    }

    void Investigate()
    {
        agent.SetDestination(investigationPoint);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            investigationTimer += Time.deltaTime; // Increment timer

            if (investigationTimer >= investigationTime) // If investigation time is up
            {
                investigating = false; // Stop investigating
                investigationTimer = 0f; // Reset timer
                walkPointSet = false; // Ready to pick a new patrol point
                Debug.Log("Investigation complete, resuming patrol");
            }
        }
    }
    #endregion

    #region Melee Attack
    void MeleeAttack()
    {
        Health playerHealth = player.GetComponent<Health>(); // Get the player's health component
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(10f);  // Inflict damage (can adjust damage value)
        }

        lastAttackTime = Time.time; // Record the time of the attack
        Debug.Log("AI attacked the player!");
    }
    #endregion

    #region Debug
    void OnDrawGizmos()
    {
        if (walkPointSet)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(walkPoint, 0.3f);
        }

        if (investigating)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(investigationPoint, 0.3f);  // Visualize the investigation point
        }

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
    #endregion
}