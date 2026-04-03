using UnityEngine;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Sight")]
    public float sightRange = 10f;
    public float sightAngle = 120f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;
    public float eyeHeight = 1.5f;

    [Header("Hearing")]
    public float hearingRange = 8f;

    [Header("Combat")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Patrol")]
    public float walkPointRange = 10f;
    public float waitTimeAtPoint = 2f;
    private Vector3 walkPoint;
    private bool walkPointSet;
    private float waitTimer;

    [Header("Investigation")]
    public float investigationTime = 3f;
    private bool investigating;
    private Vector3 investigationPoint;
    private float investigationTimer;

    private Vector3 lastPlayerSighting;
    private bool playerLost;
    private float normalSpeed;
    private float chaseSpeed = 3.5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        normalSpeed = agent.speed;

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (CanSeePlayer())
        {
            ChasePlayer();
            lastPlayerSighting = player.position;
            playerLost = false;
        }
        else
        {
            if (!playerLost && dist <= sightRange)
            {
                TriggerInvestigation(lastPlayerSighting);
                playerLost = true;
            }

            if (investigating) Investigate();
            else Patrol();
        }

        if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            MeleeAttack();
        }
    }

    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 dir = (player.position - origin).normalized;

        if (Vector3.Distance(origin, player.position) > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > sightAngle / 2f) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, sightRange, obstacleMask | playerMask))
            return ((1 << hit.collider.gameObject.layer) & playerMask) != 0;

        return false;
    }

    public void HearSound(Vector3 soundPosition, float loudness)
    {
        float dist = Vector3.Distance(transform.position, soundPosition);

        if (dist <= hearingRange * loudness)
        {
            investigationPoint = soundPosition;

            // Make AI move faster if the noise is loud or the player is panicked
            float sanityFactor = 1f;
            SanityManager sm = FindObjectOfType<SanityManager>();
            if (sm != null)
                sanityFactor = 1f + (1f - sm.currentSanity / sm.maxSanity);

            agent.speed = Mathf.Max(normalSpeed, chaseSpeed * loudness * sanityFactor);

            TriggerInvestigation(soundPosition);
        }
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
        investigating = false;
    }

    void MeleeAttack()
    {
        lastAttackTime = Time.time;
        Debug.Log("AI attacked player!");
    }

    void Patrol()
    {
        if (!walkPointSet) { SearchWalkPoint(); return; }
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
        Vector3 random = new Vector3(Random.Range(-walkPointRange, walkPointRange), 0, Random.Range(-walkPointRange, walkPointRange));
        walkPoint = transform.position + random;
        walkPointSet = true;
    }

    void TriggerInvestigation(Vector3 point)
    {
        investigationPoint = point;
        investigating = true;
        agent.SetDestination(point);
        investigationTimer = 0f;
    }

    void Investigate()
    {
        agent.SetDestination(investigationPoint);
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            investigationTimer += Time.deltaTime;
            if (investigationTimer >= investigationTime)
            {
                investigating = false;
                walkPointSet = false;
                playerLost = false;
            }
        }
    }
}