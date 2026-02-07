using UnityEngine;

/// <summary>
/// Orbits the camera back and forth around a target object.
/// Starting from its initial position, the camera sweeps 90° left,
/// then 180° right (to 90° past the start), then 180° left, and repeats.
/// Distance, elevation, and initial heading are derived from the
/// camera's position relative to the target at startup.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object to orbit around. If left empty, the script is disabled at runtime.")]
    public Transform target;

    [Header("Orbit Settings")]
    [Tooltip("Orbit speed in degrees per second.")]
    public float orbitSpeed = 10f;

    [Tooltip("How far (in degrees) the camera sweeps to each side of its starting heading.")]
    public float sweepAngle = 90f;

    [Tooltip("If true the camera always faces the target.")]
    public bool lookAtTarget = true;

    // Computed at startup from the initial camera/target relationship.
    private float _distance;
    private float _elevationAngle;   // degrees above (+) or below (−) the horizontal plane
    private float _startAngle;       // initial heading around the Y axis (degrees)

    // Sweep state — driven by a sine wave for smooth easing at each end.
    // A full sine cycle covers: start → left limit → start → right limit → start.
    // We begin at phase −π/2 so that sin starts at −1 (leftward) and rises,
    // meaning the first motion is: centre → left limit (smooth ease-out),
    // then left → right (smooth ease-in / ease-out), and so on.
    private float _phase;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning($"[OrbitCamera] No target assigned on {gameObject.name}. Disabling.");
            enabled = false;
            return;
        }

        // Vector from target to camera in world space.
        Vector3 offset = transform.position - target.position;

        // Distance is the full 3D magnitude.
        _distance = offset.magnitude;

        // Horizontal distance (XZ plane) for angle calculations.
        float horizontalDistance = new Vector2(offset.x, offset.z).magnitude;

        // Elevation angle: the angle between the horizontal plane and the offset vector.
        _elevationAngle = Mathf.Atan2(offset.y, horizontalDistance) * Mathf.Rad2Deg;

        // Starting heading: angle around the Y axis (0 = +Z, 90 = +X).
        _startAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;

        // Phase so that sin(_phase) = 0 and the first movement heads left.
        // sin rises from 0 → 1 here, and we negate it below, so offset goes 0 → −sweep (left).
        _phase = 0f;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Convert orbitSpeed (degrees/sec of the peak linear speed) into a
        // phase rate.  Peak speed of A·sin is A·ω, so ω = orbitSpeed / sweepAngle.
        float omega = (sweepAngle > 0f) ? (orbitSpeed / sweepAngle) : 0f;
        _phase += omega * Time.deltaTime;

        // Sweep offset follows a negated sine so the first half-swing goes left.
        float sweepOffset = -Mathf.Sin(_phase) * sweepAngle;

        // Current heading = starting heading + sweep offset.
        float headingRad   = (_startAngle + sweepOffset) * Mathf.Deg2Rad;
        float elevationRad = _elevationAngle * Mathf.Deg2Rad;

        float horizontalDistance = _distance * Mathf.Cos(elevationRad);

        Vector3 offset = new Vector3(
            horizontalDistance * Mathf.Sin(headingRad),
            _distance * Mathf.Sin(elevationRad),
            horizontalDistance * Mathf.Cos(headingRad)
        );

        transform.position = target.position + offset;

        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }
}
