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
    [SerializeField] private float walkPointRange = 10f;
    private Vector3 walkPoint;
    private bool walkPointSet;

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
    }

    // ---------------- PATROL ----------------
    private void Patrol()
    {
        navAgent.isStopped = false;

        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
            navAgent.SetDestination(walkPoint);

        if (Vector3.Distance(transform.position, walkPoint) < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        Vector3 point = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        // Ground check 
        if (Physics.Raycast(point, Vector3.down, 2f, groundLayer))
        {
            walkPoint = point;
            walkPointSet = true;
        }
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

        // Rotate the AI body only on the Y-axis (capsule stays upright)
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0; // ignore vertical rotation
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 5f
            );
        }

        // Rotate the firePoint (or gun/upper body) to aim directly at the player
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

    // ---------------- DEBUG ----------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
