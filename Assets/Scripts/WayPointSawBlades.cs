using UnityEngine;
public class WaypointSawTrap : MonoBehaviour
{
    public enum MovementMode { Loop, PingPong, StopAtEnd }
    [Header("Saw")]
    [Tooltip("The moving object containing the blade and collider.")]
    [SerializeField] private Transform sawMover;

    [Header("Path")]
    [Tooltip("Assign points in travel order: A, B, C, D, E.")]
    [SerializeField] private Transform[] waypoints;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField]
    private MovementMode movementMode =
        MovementMode.PingPong;

    [Tooltip("Move the saw to Point A when the scene starts.")]
    [SerializeField] private bool startAtFirstPoint = true;

    [Header("Gizmo Settings")]
    [SerializeField] private bool showPath = true;
    [SerializeField] private float pointSize = 0.12f;

    private int targetIndex;
    private bool movingForward = true;
    private bool stopped;

    private const float ArrivalDistance = 0.001f;

    private void Awake()
    {
        if (sawMover == null)
        {
            Debug.LogError(
                "WaypointSawTrap: Assign Saw Mover in the Inspector.",
                this
            );

            enabled = false;
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError(
                "WaypointSawTrap: Add at least one waypoint.",
                this
            );

            enabled = false;
            return;
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                Debug.LogError(
                    $"WaypointSawTrap: Waypoint {i} is missing.",
                    this
                );

                enabled = false;
                return;
            }
        }

        if (startAtFirstPoint)
        {
            sawMover.position = waypoints[0].position;
        }

        // Begin by travelling toward Point B.
        targetIndex = waypoints.Length > 1 ? 1 : 0;
    }

    private void Update()
    {
        if (stopped || waypoints.Length < 2)
            return;

        Vector3 targetPosition = waypoints[targetIndex].position;

        sawMover.position = Vector3.MoveTowards(
            sawMover.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(sawMover.position, targetPosition)
            <= ArrivalDistance)
        {
            sawMover.position = targetPosition;
            ReachedWaypoint();
        }
    }

    private void ReachedWaypoint()
    {
        switch (movementMode)
        {
            case MovementMode.Loop:
                MoveToNextWaypoint();
                break;

            case MovementMode.PingPong:
                ReverseAtEndpoints();
                break;

            case MovementMode.StopAtEnd:
                StopAtEndpoint();
                break;
        }
    }

    private void MoveToNextWaypoint()
    {
        targetIndex = (targetIndex + 1) % waypoints.Length;
    }

    private void ReverseAtEndpoints()
    {
        if (movingForward)
        {
            if (targetIndex >= waypoints.Length - 1)
            {
                movingForward = false;
                targetIndex = waypoints.Length - 2;
            }
            else
            {
                targetIndex++;
            }
        }
        else
        {
            if (targetIndex <= 0)
            {
                movingForward = true;
                targetIndex = 1;
            }
            else
            {
                targetIndex--;
            }
        }
    }

    private void StopAtEndpoint()
    {
        if (targetIndex >= waypoints.Length - 1)
        {
            stopped = true;
        }
        else
        {
            targetIndex++;
        }
    }

    // Draw the route in Unity's Scene view.
    private void OnDrawGizmos()
    {
        if (!showPath || waypoints == null || waypoints.Length == 0)
            return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform point = waypoints[i];

            if (point == null)
                continue;

            // Draw each waypoint.
            Gizmos.DrawWireSphere(point.position, pointSize);

            // Draw the path to the next waypoint.
            int nextIndex = i + 1;

            if (nextIndex < waypoints.Length &&
                waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(
                    point.position,
                    waypoints[nextIndex].position
                );
            }
        }
        // Show the final connection for Loop mode.
        if (movementMode == MovementMode.Loop &&
            waypoints.Length > 1 &&
            waypoints[0] != null &&
            waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.DrawLine(
                waypoints[waypoints.Length - 1].position,
                waypoints[0].position
            );
        }
    }

    // Optional methods for activating the trap from another script.

    public void ResumeMovement()
    {
        stopped = false;
    }

    public void StopMovement()
    {
        stopped = true;
    }
}
