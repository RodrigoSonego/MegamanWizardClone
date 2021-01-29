using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class FlyingEnemy : Enemy
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject projectilePrefab;
    [Space]
    [SerializeField] float detectionRadius;
    [SerializeField] float attackRadius;
    [Space]
    [SerializeField] private float shootCooldown;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float speed;
    [Space]
    [Tooltip("Distance to start moving to next waypoint")]
    [SerializeField] private float nextWaypointDistance = 2f;

    private int currentWaypoint = 0;
    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;

    private bool hasDetectedPlayer = false;
    private bool canAttack = true;

    Coroutine updatePathCoroutine = null;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
    }

    void UpdatePath()
    {
        if (seeker.IsDone()) { seeker.StartPath(rb.position, player.position, OnPathComplete); }
    }

    void OnPathComplete(Path path)
    {
        if (!path.error)
        {
            this.path = path;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        CheckDistanceToPlayer();

        CheckPath();
    }

    void CheckDistanceToPlayer()
    {
        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        if (distanceToPlayer > detectionRadius && !hasDetectedPlayer) { return; }

        hasDetectedPlayer = true;

        if(distanceToPlayer < attackRadius && canAttack)
        {
            StartCoroutine(ShootAnimation());
        }

    }

    void CheckPath()
    {
        if (updatePathCoroutine == null)
        {
            updatePathCoroutine = StartCoroutine(UpdatePathTime());
        }

        if (path == null) { return; }

        if (currentWaypoint >= path.vectorPath.Count) { return; }

        MoveTowardsPlayer();
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance) { currentWaypoint++; }
    }

    void Attack()
    {
        Vector2 direction = (Vector2)player.position - rb.position;
        Vector2 force = direction.normalized * projectileSpeed;
        float fireballRotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject fireball = Instantiate(projectilePrefab, rb.position, Quaternion.Euler(0, 0, fireballRotation));
        Destroy(fireball, 5f);
        fireball.GetComponent<Rigidbody2D>().velocity = force;
    }

    IEnumerator ShootAnimation()
    {
        canAttack = false;
        animator.SetBool("attacking", true);
        yield return new WaitForSeconds(0.8f);
        Attack();

        yield return new WaitForSeconds(.05f);
        animator.SetBool("attacking", false);

        yield return new WaitForSeconds(shootCooldown);
        canAttack = true;
    }



    IEnumerator UpdatePathTime()
    {
        while (hasDetectedPlayer)
        {
            yield return new WaitForSeconds(0.7f);
            UpdatePath();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
