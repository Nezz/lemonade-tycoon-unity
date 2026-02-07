using UnityEngine;

/// <summary>
/// Drives a vehicle along a WaypointPath at a configurable speed.
/// Attach to vehicle prefabs (cars, buses, trucks).
/// </summary>
public class CarPathDriver : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("The waypoint path this vehicle follows.")]
    public WaypointPath path;

    [Tooltip("Start at a random position along the path so vehicles are spread out.")]
    public bool randomStartPosition = false;

    [Header("Movement")]
    [Tooltip("Driving speed in units per second.")]
    public float speed = 6f;

    [Tooltip("How quickly the vehicle rotates to face the next waypoint.")]
    public float rotationSpeed = 4f;

    [Tooltip("Distance threshold to consider a waypoint reached.")]
    public float arrivalThreshold = 0.3f;

    private static readonly Quaternion ModelOffset = Quaternion.Euler(0f, 90f, 0f);

    private int _currentWaypointIndex;
    private int _direction = 1;
    private bool _pathFinished;

    private void Start()
    {
        if (path == null || path.WaypointCount < 2)
        {
            enabled = false;
            return;
        }

        // Pick a starting waypoint
        if (randomStartPosition)
        {
            _currentWaypointIndex = Random.Range(0, path.WaypointCount);
        }
        else
        {
            _currentWaypointIndex = 0;
        }

        // Snap to the starting waypoint and face the next one
        transform.position = path.GetWaypointPosition(_currentWaypointIndex);

        int nextIndex = path.GetNextIndex(_currentWaypointIndex, ref _direction, out _);
        if (nextIndex >= 0)
        {
            // Face the next waypoint immediately at start
            Vector3 toNext = path.GetWaypointPosition(nextIndex) - transform.position;
            toNext.y = 0f;
            if (toNext != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(toNext) * ModelOffset;
            }
            _currentWaypointIndex = nextIndex;
        }
        else
        {
            _pathFinished = true;
        }
    }

    private void Update()
    {
        if (_pathFinished) return;

        Vector3 target = path.GetWaypointPosition(_currentWaypointIndex);
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f; // keep vehicle on flat ground

        float distance = toTarget.magnitude;

        if (distance <= arrivalThreshold)
        {
            OnWaypointReached();
            return;
        }

        // Smoothly rotate toward the waypoint
        if (toTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toTarget) * ModelOffset;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move toward the waypoint
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void OnWaypointReached()
    {
        int nextIndex = path.GetNextIndex(_currentWaypointIndex, ref _direction, out bool shouldTeleport);

        if (nextIndex < 0)
        {
            _pathFinished = true;
            return;
        }

        _currentWaypointIndex = nextIndex;

        if (shouldTeleport)
        {
            // Non-ping-pong loop: teleport back to start
            transform.position = path.GetWaypointPosition(_currentWaypointIndex);

            // Face the next waypoint after teleporting
            int peekDir = _direction;
            int peekNext = path.GetNextIndex(_currentWaypointIndex, ref peekDir, out _);
            if (peekNext >= 0)
            {
                Vector3 toNext = path.GetWaypointPosition(peekNext) - transform.position;
                toNext.y = 0f;
                if (toNext != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(toNext) * ModelOffset;
                }
            }
        }
    }
}
