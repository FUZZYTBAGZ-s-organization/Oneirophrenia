using UnityEngine;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;  // The AI's NavMesh Agent, which controls its movement
    public Transform player;    // Reference to the player character (for sight and chasing)

    [Header("Sight Settings")]
    public float sightRange = 10f;  // The maximum distance the AI can see the player
    public float sightAngle = 120f; // The angle (FOV) in which the AI can see the player
    public LayerMask obstacleMask;  // Layer mask for obstacles that block the AI's line of sight
    public LayerMask playerMask;    // Layer mask for the player
    public float eyeHeight = 1.5f;  // The height of the AI's eyes for raycasting

    [Header("Melee Attack Settings")]
    public float attackRange = 2f;  // Range at which the AI can attack the player
    public float attackCooldown = 1f; // Time before the AI can attack again
    private float lastAttackTime;   // Keeps track of the last time the AI attacked

    [Header("Patrol Settings")]
    public float walkPointRange = 10f;  // Range for random patrol points
    public float waitTimeAtPoint = 2f;  // Time the AI waits at a patrol point
    private Vector3 walkPoint;         // The patrol point the AI is heading to
    private bool walkPointSet;         // Whether the AI has set a patrol point
    private float waitTimer;           // Timer for waiting at patrol points

    [Header("Investigation Settings")]
    public float investigationTime = 3f;  // Time the AI spends investigating a point
    private bool investigating = false;   // Whether the AI is investigating
    private Vector3 investigationPoint;   // The point the AI is investigating
    private float investigationTimer;     // Timer for investigating

    // Memory of last player sight / alert
    private Vector3 lastPlayerSighting;  // The last position where the player was seen
    private bool playerLost = false;     // Whether the AI has lost the player

    private bool isDead = false; // Flag to track if the AI is dead

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;  // Find the player if it's not assigned
        }
    }

    private void Update()
    {
        if (isDead) return;  // Skip update if the AI is dead (avoids crashes)

        if (!agent.isOnNavMesh) return;  // Ensure the AI is on a valid NavMesh

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if the AI can see the player
        if (CanSeePlayer())
        {
            ChasePlayer();  // Chase the player
            lastPlayerSighting = player.position;  // Update the last seen position of the player
            playerLost = false;  // Reset the player lost state
        }
        else
        {
            // If the player is lost, investigate the last known position
            if (!playerLost && distanceToPlayer <= sightRange)
            {
                TriggerInvestigation(lastPlayerSighting); // Investigate the last sighted position
                playerLost = true;  // Mark that the player has been lost
            }

            // Handle investigation and patrolling behavior
            if (investigating)
            {
                Investigate();  // Perform the investigation
            }
            else
            {
                Patroling();  // Continue patrolling
            }
        }

        // Handle melee attack if player is within attack range and cooldown is passed
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            MeleeAttack();  // Attack the player
        }
    }

    #region Sight
    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;  // Set the origin point for the raycast
        Vector3 dir = (player.position - origin).normalized;  // Direction from AI to player

        Debug.DrawRay(origin, dir * sightRange, Color.red);  // Debug: draw a ray to visualize sight

        // Check if the player is within sight range and angle
        if (Vector3.Distance(origin, player.position) > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > sightAngle / 2f) return false;  // If player is outside of the AI's vision cone, return false

        // Perform raycast to check for obstacles between the AI and player
        if (Physics.Raycast(origin, dir, out RaycastHit hit, sightRange, obstacleMask | playerMask))
        {
            return ((1 << hit.collider.gameObject.layer) & playerMask) != 0;  // Check if the hit object is the player
        }

        return false;
    }
    #endregion

    #region Chase
    void ChasePlayer()
    {
        agent.SetDestination(player.position);  // Move toward the player's position
        investigating = false;  // Stop investigating if we can see the player
    }
    #endregion

    #region Melee
    void MeleeAttack()
    {
        Health playerHealth = player.GetComponent<Health>();  // Get the player's health component
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(10f);  // Deal damage to the player
        }
        lastAttackTime = Time.time;  // Update the time for the last attack
        Debug.Log("AI attacked the player!");
    }
    #endregion

    #region Patrol
    void Patroling()
    {
        if (!walkPointSet)  // If no patrol point is set, search for a new one
        {
            SearchWalkPoint();
            return;
        }

        agent.SetDestination(walkPoint);  // Move towards the patrol point

        // If the AI reaches the patrol point, start waiting
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                walkPointSet = false;  // Reset patrol point after waiting
                waitTimer = 0f;
            }
        }
    }

    void SearchWalkPoint()
    {
        // Search for a random patrol point within the defined range
        for (int i = 0; i < 10; i++)
        {
            float randomX = Random.Range(-walkPointRange, walkPointRange);
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            Vector3 randomPoint = new Vector3(transform.position.x + randomX, transform.position.y + 20f, transform.position.z + randomZ);

            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 40f, obstacleMask))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    walkPoint = navHit.position;  // Set the patrol point
                    walkPointSet = true;  // Mark patrol point as set
                    return;
                }
            }
        }

        walkPointSet = false;  // If no valid patrol point is found, reset patrol state
    }
    #endregion

    #region Investigation
    public void TriggerInvestigation(Vector3 point)
    {
        if (point == Vector3.zero) // Avoid triggering investigation at invalid position
        {
            Debug.LogWarning("Invalid alert position received at (0, 0, 0).");
            return;
        }

        investigationPoint = point;
        investigating = true;
        agent.SetDestination(investigationPoint);  // Move to the last known player location
        investigationTimer = 0f;
        Debug.Log($"AI is investigating alerted position at {investigationPoint}");
    }

    void Investigate()
    {
        agent.SetDestination(investigationPoint);

        // When the AI reaches the investigation point, start the timer
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            investigationTimer += Time.deltaTime;
            if (investigationTimer >= investigationTime)
            {
                investigating = false;  // Stop investigating after the timer
                investigationTimer = 0f;
                walkPointSet = false;  // Reset patrol state
                playerLost = false;  // Reset player lost state
                lastPlayerSighting = Vector3.zero;  // Forget the last seen position
                Debug.Log("Investigation complete, resuming patrol");
            }
        }
    }
    #endregion

    #region Alert from Shooting
    // Call this when the player fires a shot
    public void AlertToPosition(Vector3 position)
    {
        if (position == Vector3.zero) // Prevent alert at (0, 0, 0)
        {
            Debug.LogWarning("Invalid alert position received at (0, 0, 0).");
            return;
        }

        lastPlayerSighting = position;
        TriggerInvestigation(position);  // Investigate the alerted position
        playerLost = true;  // Mark that the player has been lost
    }
    #endregion

    #region Death
    public void Die()
    {
        isDead = true;  // Mark the AI as dead
        if (agent != null)
        {
            agent.isStopped = true; // Stop the NavMeshAgent's movement
        }
        Debug.Log($"{gameObject.name} has died!");
        Destroy(gameObject, 2f); // Delay the destruction to ensure clean-up
    }
    #endregion

    #region FOV Visualization
    private void OnDrawGizmosSelected()
    {
        if (agent == null) return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, sightRange);

        Vector3 leftDir = Quaternion.Euler(0, -sightAngle / 2f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, sightAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + leftDir * sightRange);
        Gizmos.DrawLine(origin, origin + rightDir * sightRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(lastPlayerSighting, 0.3f);

        if (investigating)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(investigationPoint, 0.3f);
        }
    }
    #endregion
}