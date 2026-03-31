using UnityEngine;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;   // NavMeshAgent component used to move the AI on the NavMesh
    public Transform player;     // Reference to the player's Transform

    [Header("Detection")]
    public float sightRange = 10f;      // How far the AI can "see" the player
    public LayerMask Ground, Player;    // LayerMasks for ground and player detection

    [Header("Patrol Settings")]
    public float walkPointRange = 10f;  // Maximum random distance for patrol points
    public float waitTimeAtPoint = 2f;  // Time AI waits at a patrol point before moving

    [Header("Investigation Settings")]
    public float investigationTime = 3f;  // Time AI spends investigating a point

    [Header("Melee Attack Settings")]
    public float attackRange = 2f;       // Distance at which AI can attack the player
    public float attackCooldown = 1f;    // Time between melee attacks
    private float lastAttackTime;        // Tracks last attack time for cooldown

    private Vector3 walkPoint;           // Current patrol target point
    private bool walkPointSet;           // Flag to check if a patrol point has been set
    private float waitTimer;             // Timer for waiting at patrol point
    private bool investigating = false;  // Is AI currently investigating?
    private float investigationTimer;    // Tracks time spent at investigation point

    private Vector3 investigationPoint;  // The position AI moves to when investigating

    // ------------------------------
    private void Start()
    {
        // Get the NavMeshAgent component attached to this AI
        agent = GetComponent<NavMeshAgent>();

        // Automatically find the player if not assigned
        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null)
                player = obj.transform;
        }
    }

    // ------------------------------
    private void Update()
    {
        // Safety check: AI must be on a NavMesh
        if (!agent.isOnNavMesh)
        {
            Debug.LogError("NOT ON NAVMESH");
            return;
        }

        // Measure distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Decide behavior based on distance and investigation state
        if (distanceToPlayer <= sightRange)
        {
            ChasePlayer();  // Player detected, chase them
        }
        else if (investigating)
        {
            Investigate();  // Currently investigating a point
        }
        else
        {
            Patroling();    // Default patrol behavior
        }

        // Check melee attack conditions: player in range and cooldown passed
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            MeleeAttack();
        }
    }

    #region Patroling
    void Patroling()
    {
        // If no patrol point is set, pick one
        if (!walkPointSet)
        {
            SearchWalkPoint();
            return;
        }

        // Move towards the current patrol point
        agent.SetDestination(walkPoint);

        // Check if AI has reached the patrol point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;  // Increment waiting timer
            if (waitTimer >= waitTimeAtPoint)  // If waited enough, pick a new patrol point
            {
                walkPointSet = false;
                waitTimer = 0f;
            }
        }
    }

    void SearchWalkPoint()
    {
        int maxAttempts = 10;  // Limit attempts to find a valid patrol point

        for (int i = 0; i < maxAttempts; i++)
        {
            // Pick a random point within range
            float randomX = Random.Range(-walkPointRange, walkPointRange);
            float randomZ = Random.Range(-walkPointRange, walkPointRange);

            Vector3 randomPoint = new Vector3(
                transform.position.x + randomX,
                transform.position.y + 20f,  // Start raycast from above
                transform.position.z + randomZ
            );

            // Raycast down to find the ground position
            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 40f, Ground))
            {
                Vector3 candidate = hit.point;

                // Make sure the point is on the NavMesh
                if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    walkPoint = navHit.position;  // Valid patrol point found
                    walkPointSet = true;
                    return;
                }
            }
        }

        // Failed to find a point
        walkPointSet = false;
    }
    #endregion

    #region Chasing
    void ChasePlayer()
    {
        // Set destination to the player position
        agent.SetDestination(player.position);
        investigating = false; // Stop investigating if chasing
    }
    #endregion

    #region Investigation
    public void TriggerInvestigation(Vector3 point)
    {
        // Start investigating a location
        investigationPoint = point;
        investigating = true;
        agent.SetDestination(investigationPoint);
        investigationTimer = 0f;
        Debug.Log($"Investigation triggered at {investigationPoint}");
    }

    void Investigate()
    {
        // Move towards investigation point
        agent.SetDestination(investigationPoint);

        // Check if AI has reached the point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            investigationTimer += Time.deltaTime;  // Count time spent investigating

            if (investigationTimer >= investigationTime)  // Done investigating
            {
                investigating = false;   // Reset investigation state
                investigationTimer = 0f;
                walkPointSet = false;    // Resume patrol
                Debug.Log("Investigation complete, resuming patrol");
            }
        }
    }
    #endregion

    #region Melee Attack
    void MeleeAttack()
    {
        // Damage the player if they have a Health component
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(10f);
        }

        lastAttackTime = Time.time;  // Update cooldown
        Debug.Log("AI attacked the player!");
    }
    #endregion

    #region Debug
    void OnDrawGizmos()
    {
        // Draw patrol point in green
        if (walkPointSet)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(walkPoint, 0.3f);
        }

        // Draw investigation point in yellow
        if (investigating)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(investigationPoint, 0.3f);
        }

        // Draw line to player in red
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
    #endregion
}