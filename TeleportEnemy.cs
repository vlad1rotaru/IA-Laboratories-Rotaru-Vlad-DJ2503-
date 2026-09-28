using UnityEngine;

public class TeleportEnemy : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Search,
        Teleport
    }

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Detection")]
    public float detectionRange = 6f;
    [Range(0f, 360f)]
    public float fieldOfView = 180f;
    public LayerMask obstacleLayer;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public int attackDamage = 20;
    public float attackCooldown = 1f;

    [Header("Search")]
    public float searchTime = 4f;

    [Header("Teleport")]
    public float teleportDistance = 4f;
    public float teleportCooldown = 5f;
    public float minTeleportTriggerDistance = 3f;

    [Header("Start")]
    public float idleTime = 2f;

    private State currentState = State.Idle;
    private Rigidbody2D rb;

    private int currentPatrolPoint;
    private float idleTimer;
    private float searchTimer;
    private float attackTimer;
    private float teleportTimer;

    private Vector2 lastKnownPlayerPosition;
    private Vector2 facingDirection = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        idleTimer = idleTime;
        teleportTimer = 0f;
    }

    private void Update()
    {
        if (player == null)
            return;

        switch (currentState)
        {
            case State.Idle:
                IdleState();
                break;

            case State.Patrol:
                PatrolState();
                break;

            case State.Chase:
                ChaseState();
                break;

            case State.Attack:
                AttackState();
                break;

            case State.Search:
                SearchState();
                break;

            case State.Teleport:
                TeleportState();
                break;
        }

        attackTimer -= Time.deltaTime;
        teleportTimer -= Time.deltaTime;
    }

    private void IdleState()
    {
        rb.linearVelocity = Vector2.zero;

        if (CanSeePlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0)
        {
            ChangeState(State.Patrol);
        }
    }

    private void PatrolState()
    {
        if (CanSeePlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Transform target = patrolPoints[currentPatrolPoint];

        MoveTowards(target.position);

        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            currentPatrolPoint++;

            if (currentPatrolPoint >= patrolPoints.Length)
                currentPatrolPoint = 0;
        }
    }

    private void ChaseState()
    {
        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= attackRange)
            {
                ChangeState(State.Attack);
                return;
            }

            if (teleportTimer <= 0 && distance >= minTeleportTriggerDistance)
            {
                ChangeState(State.Teleport);
                return;
            }

            MoveTowards(player.position);
        }
        else
        {
            ChangeState(State.Search);
        }
    }

    private void AttackState()
    {
        rb.linearVelocity = Vector2.zero;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            ChangeState(State.Chase);
            return;
        }

        if (CanSeePlayer() && attackTimer <= 0)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    private void SearchState()
    {
        MoveTowards(lastKnownPlayerPosition);

        if (CanSeePlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        if (Vector2.Distance(transform.position, lastKnownPlayerPosition) < 0.2f)
        {
            rb.linearVelocity = Vector2.zero;

            searchTimer -= Time.deltaTime;

            if (searchTimer <= 0)
            {
                ChangeState(State.Patrol);
            }
        }
    }

    private void TeleportState()
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        Vector2 direction = toPlayer.normalized;

        float targetDist = Mathf.Min(teleportDistance, Mathf.Max(0.5f, toPlayer.magnitude - 1f));
        Vector2 destination = (Vector2)transform.position + direction * targetDist;

        if (IsTeleportPositionValid(destination))
        {
            transform.position = destination;
            rb.position = destination;

            Debug.Log("Teleport Enemy successfully teleported!");
            teleportTimer = teleportCooldown;
        }
        else
        {
            Debug.Log("Teleport position was invalid, retrying later.");
            teleportTimer = 1f;
        }

        ChangeState(State.Chase);
    }

    private bool CanSeePlayer()
    {
        Vector2 direction = (Vector2)player.position - (Vector2)transform.position;
        float distance = direction.magnitude;

        if (distance > detectionRange)
            return false;

        direction.Normalize();

        float angle = Vector2.Angle(facingDirection, direction);

        if (angle > fieldOfView / 2f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            detectionRange,
            obstacleLayer | LayerMask.GetMask("Player")
        );

        if (hit.collider != null)
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    private bool IsTeleportPositionValid(Vector2 position)
    {
        Collider2D obstacle = Physics2D.OverlapCircle(
            position,
            0.45f,
            obstacleLayer
        );

        return obstacle == null;
    }

    private void MoveTowards(Vector2 target)
    {
        Vector2 direction = target - (Vector2)transform.position;

        if (direction.magnitude > 0.05f)
        {
            direction.Normalize();
            facingDirection = direction;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Attack()
    {
        Debug.Log("Teleport Enemy attacks!");

        Health health = player.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(attackDamage);
        }
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        if (newState == State.Search)
        {
            searchTimer = searchTime;
        }

        Debug.Log("Teleport Enemy State: " + currentState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.DrawWireSphere(transform.position, teleportDistance);

        Vector3 left = Quaternion.Euler(0, 0, fieldOfView / 2f) * facingDirection;
        Vector3 right = Quaternion.Euler(0, 0, -fieldOfView / 2f) * facingDirection;

        Gizmos.DrawLine(transform.position, transform.position + left * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + right * detectionRange);
    }
}