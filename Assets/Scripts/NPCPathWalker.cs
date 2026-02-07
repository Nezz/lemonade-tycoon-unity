using UnityEngine;
using System.Collections;

/// <summary>
/// Moves an NPC along a WaypointPath and plays a walk animation.
/// Attach to NPC prefabs.
/// </summary>
public class NPCPathWalker : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("The waypoint path this NPC follows.")]
    public WaypointPath path;

    [Tooltip("Start at a random waypoint instead of the first one.")]
    public bool randomStartPosition = false;

    [Header("Movement")]
    [Tooltip("Walking speed in units per second.")]
    public float walkSpeed = 1.2f;

    [Tooltip("How quickly the NPC rotates to face the next waypoint.")]
    public float rotationSpeed = 5f;

    [Tooltip("Distance threshold to consider a waypoint reached.")]
    public float arrivalThreshold = 0.15f;

    [Tooltip("Seconds to pause at each end of the path before turning around.")]
    public float pauseDuration = 2f;

    [Tooltip("Crossfade duration when switching animations.")]
    public float crossfadeDuration = 0.4f;

    private int _currentWaypointIndex;
    private int _direction = 1;
    private bool _isWalking;
    private bool _isPaused;
    private Animator _animator;

    private void OnEnable()
    {
        if (path == null || path.WaypointCount < 2)
        {
            enabled = false;
            return;
        }

        _animator = GetComponent<Animator>();

        // Pick a starting waypoint
        if (randomStartPosition)
        {
            _currentWaypointIndex = Random.Range(0, path.WaypointCount);
        }
        else
        {
            _currentWaypointIndex = 0;
        }

        // Snap to the starting waypoint
        transform.position = path.GetWaypointPosition(_currentWaypointIndex);

        // Advance to the next waypoint so we have somewhere to walk to
        int nextIndex = path.GetNextIndex(_currentWaypointIndex, ref _direction, out _);
        if (nextIndex >= 0)
        {
            _currentWaypointIndex = nextIndex;
        }

        // Start walking
        StartWalkAnimation();
    }

    private void Update()
    {
        if (_isPaused) return;

        Vector3 target = path.GetWaypointPosition(_currentWaypointIndex);
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f; // keep NPC upright on flat ground

        float distance = toTarget.magnitude;

        if (distance <= arrivalThreshold)
        {
            OnWaypointReached();
            return;
        }

        // Rotate toward the waypoint
        if (toTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move toward the waypoint
        transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
    }

    private void OnWaypointReached()
    {
        int prevDirection = _direction;
        int nextIndex = path.GetNextIndex(_currentWaypointIndex, ref _direction, out bool shouldTeleport);

        if (nextIndex < 0)
        {
            // Path ended (non-looping) -- stop
            StopWalkAnimation();
            enabled = false;
            return;
        }

        _currentWaypointIndex = nextIndex;

        if (shouldTeleport)
        {
            // Non-ping-pong loop: teleport back to start and pause
            StartCoroutine(TeleportAndPause());
        }
        else if (_direction != prevDirection)
        {
            // Ping-pong: reversed direction, pause before turning around
            StartCoroutine(PauseAtEndpoint());
        }
    }

    private IEnumerator TeleportAndPause()
    {
        _isPaused = true;
        StopWalkAnimation();

        yield return new WaitForSeconds(pauseDuration);

        // Teleport to the new waypoint position
        transform.position = path.GetWaypointPosition(_currentWaypointIndex);

        StartWalkAnimation();
        _isPaused = false;
    }

    private IEnumerator PauseAtEndpoint()
    {
        _isPaused = true;
        StopWalkAnimation();

        yield return new WaitForSeconds(pauseDuration);

        StartWalkAnimation();
        _isPaused = false;
    }

    private void StartWalkAnimation()
    {
        _animator.CrossFadeInFixedTime("walk", crossfadeDuration);
    }

    private void StopWalkAnimation()
    {
        if (!_isWalking) return;
        _isWalking = false;
        
        _animator.CrossFadeInFixedTime("idle", crossfadeDuration);
    }
}
