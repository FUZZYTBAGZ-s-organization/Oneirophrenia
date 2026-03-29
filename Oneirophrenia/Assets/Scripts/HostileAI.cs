using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Transform player;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;

    [Header("Patrol")]
    [SerializeField] private float waitTimeAtPoint = 1f; // pause at patrol point
    private Vector3 walkPoint;
    private bool walkPointSet;
    private bool waitingAtPoint;

    [Header("Combat")]
    [SerializeField] private float timeBetweenAttacks = 1f;
    private bool alreadyAttacked;
    [SerializeField] private float shootForce = 10f;
    [SerializeField] private float upwardForce = 5f;

    [Header("Ranges")]
    [SerializeField] private float sightRange = 20f;
    [SerializeField] private float attackRange = 10f;

    private bool playerInSightRange;
    private bool playerInAttackRange;

    // Stuck detection variables
    private float stuckTimer = 0f; // Timer to track how long the agent is stuck
    private float stuckThreshold = 2f; // Time threshold to consider the agent as stuck

    private void Awake()
    {
        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject obj = GameObject.Find("Player");
            if (obj != null)
                player = obj.transform;
        }
    }

    private void Update()
    {
        // Check ranges
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        // State machine
        if (!playerInSightRange && !playerInAttackRange)
            Patrol();
        else if (playerInSightRange && !playerInAttackRange)
            Chase();
        else if (playerInAttackRange && playerInSightRange)
            Attack();

        // Check if the agent is stuck and handle un-stucking
        CheckIfStuck();
    }

    // ---------------- PATROL ----------------
    private void Patrol()
    {
        if (waitingAtPoint) return;

        navAgent.isStopped = false;

        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
            navAgent.SetDestination(walkPoint);

        // Check if reached destination
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            walkPointSet = false;
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private void SearchWalkPoint()
    {
        // Pick a random point anywhere on the NavMesh (across the entire map)
        for (int i = 0; i < 10; i++)
        {
            // Search for random points across the whole NavMesh area
            Vector3 randomPoint = new Vector3(
                Random.Range(-100f, 100f),  // Adjust these ranges based on your map size
                0f,                         // Keep height constant or change based on the ground
                Random.Range(-100f, 100f)
            );

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                walkPoint = hit.position;
                walkPointSet = true;
                return;
            }
        }

        // If no valid point found, stay at the current position
        walkPointSet = false;
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        waitingAtPoint = true;
        navAgent.isStopped = true;
        yield return new WaitForSeconds(Random.Range(waitTimeAtPoint, waitTimeAtPoint + 2f));
        waitingAtPoint = false;
        navAgent.isStopped = false;
    }

    // ---------------- CHASE ----------------
    private void Chase()
    {
        navAgent.isStopped = false;

        if (player != null)
            navAgent.SetDestination(player.position);
    }

    // ---------------- ATTACK ----------------
    private void Attack()
    {
        navAgent.isStopped = true;

        if (player == null) return;

        // Rotate the AI body only on the Y-axis
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 5f
            );
        }

        // Rotate firePoint to aim at player
        Vector3 targetDirection = player.position - firePoint.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        firePoint.rotation = Quaternion.Slerp(
            firePoint.rotation,
            targetRotation,
            Time.deltaTime * 10f
        );

        if (!alreadyAttacked)
        {
            Shoot();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * shootForce, ForceMode.Impulse);
            rb.AddForce(firePoint.up * upwardForce, ForceMode.Impulse);
        }

        Destroy(bullet, 3f);
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // ---------------- UNSTUCK MECHANISM ----------------
    private void CheckIfStuck()
    {
        // If the agent's velocity is too low and it hasn't reached the destination for a while, it might be stuck
        if (navAgent.velocity.magnitude < 0.1f && navAgent.remainingDistance > 0.1f)
        {
            stuckTimer += Time.deltaTime;

            // If the stuck timer exceeds the threshold, unstick the AI
            if (stuckTimer >= stuckThreshold)
            {
                Debug.Log("AI is stuck, un-sticking...");
                UnstuckAI();
            }
        }
        else
        {
            stuckTimer = 0f; // Reset the timer if the agent is moving
        }
    }

    private void UnstuckAI()
    {
        // Reset the path and stop the agent
        navAgent.ResetPath();

        // Optionally, move the AI a little bit to avoid being stuck in the same place
        Vector3 randomDirection = Random.insideUnitSphere * 2f; // Move it by 2 units in a random direction
        randomDirection.y = 0; // Keep it on the same ground level
        Vector3 newPosition = transform.position + randomDirection;

        // Ensure that the new position is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newPosition, out hit, 2f, NavMesh.AllAreas))
        {
            navAgent.Warp(hit.position); // Instantly teleport the agent to the new position
            Debug.Log("AI unstuck by teleporting.");
        }
        else
        {
            // If we can't find a valid point, just make the agent wander a bit
            SearchWalkPoint();
            navAgent.SetDestination(walkPoint);
            Debug.Log("AI unstuck by wandering.");
        }

        stuckTimer = 0f; // Reset stuck timer after un-sticking
    }

    // ---------------- DEBUG ----------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}