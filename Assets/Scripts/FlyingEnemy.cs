using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class FlyingEnemy : MonoBehaviour
{
    [SerializeField] private Transform player;
    [Space]
    [SerializeField] private float speed;
    [Tooltip("Distance to start moving to next waypoint")]
    [SerializeField] private float nextWaypointDistance = 2f;
    [Space]
    [SerializeField]bool isPlayerInRange = false;

    private int currentWaypoint = 0;
    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0, 1f);
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
        if(path == null) { return; }

        if(currentWaypoint >= path.vectorPath.Count) { return; }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if(distance < nextWaypointDistance) { currentWaypoint++; }
    }
}
