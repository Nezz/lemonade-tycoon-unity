using UnityEngine;

/// <summary>
/// Orbits the camera back and forth around a target object.
/// The user can drag (touch or mouse) to rotate the camera horizontally.
/// After releasing, the automatic orbit resumes following a short delay.
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

    [Header("Drag Settings")]
    [Tooltip("How sensitive the drag rotation is (degrees per pixel of drag).")]
    public float dragSensitivity = 0.25f;

    [Tooltip("Seconds to wait after momentum stops before auto-orbit resumes.")]
    public float resumeDelay = 2.0f;

    [Header("Momentum")]
    [Tooltip("How quickly the fling momentum decays. Higher = stops faster.")]
    public float momentumDecay = 5f;

    [Tooltip("Momentum below this threshold (deg/s) is zeroed out.")]
    public float momentumCutoff = 0.5f;

    // Computed at startup from the initial camera/target relationship.
    private float _distance;
    private float _elevationAngle;   // degrees above (+) or below (−) the horizontal plane
    private float _startAngle;       // initial heading around the Y axis (degrees)

    // Sweep state — driven by a sine wave for smooth easing at each end.
    private float _phase;

    // Drag / touch state.
    private bool _isDragging;
    private Vector2 _lastPointerPos;
    private float _dragAngleOffset;    // cumulative heading offset from user drag (degrees)
    private float _resumeTimer;        // counts down to zero after release
    private float _angularVelocity;    // current momentum in degrees/sec

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

        _distance = offset.magnitude;

        float horizontalDistance = new Vector2(offset.x, offset.z).magnitude;
        _elevationAngle = Mathf.Atan2(offset.y, horizontalDistance) * Mathf.Rad2Deg;
        _startAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;

        _phase = 0f;
        _dragAngleOffset = 0f;
        _resumeTimer = 0f;
        _angularVelocity = 0f;
    }

    private void Update()
    {
        HandleInput();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // --- Momentum: apply decaying angular velocity after drag ends ---
        if (!_isDragging && Mathf.Abs(_angularVelocity) > momentumCutoff)
        {
            _dragAngleOffset += _angularVelocity * Time.deltaTime;
            _angularVelocity *= Mathf.Exp(-momentumDecay * Time.deltaTime);

            // Keep the resume timer full while momentum is still going.
            _resumeTimer = resumeDelay;
        }
        else if (!_isDragging)
        {
            _angularVelocity = 0f;
        }

        // --- Auto-orbit (only when not dragging and resume timer has elapsed) ---
        bool autoOrbit = !_isDragging && _resumeTimer <= 0f;

        if (!_isDragging && _resumeTimer > 0f)
        {
            _resumeTimer -= Time.deltaTime;
        }

        if (autoOrbit)
        {
            float omega = (sweepAngle > 0f) ? (orbitSpeed / sweepAngle) : 0f;
            _phase += omega * Time.deltaTime;
        }

        // Sweep offset follows a negated sine so the first half-swing goes left.
        float sweepOffset = -Mathf.Sin(_phase) * sweepAngle;

        // Total heading = start + auto-sweep + user drag offset.
        float headingDeg   = _startAngle + sweepOffset + _dragAngleOffset;
        float headingRad   = headingDeg * Mathf.Deg2Rad;
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

    /// <summary>
    /// Reads touch (mobile) or mouse (editor/desktop) input and converts
    /// horizontal drag distance into a heading offset around the target.
    /// </summary>
    private void HandleInput()
    {
        // --- Touch input (preferred on mobile) ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    BeginDrag(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    ContinueDrag(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndDrag();
                    break;
            }
            return; // skip mouse when touches are active
        }

        // --- Mouse fallback (editor / desktop) ---
        if (Input.GetMouseButtonDown(0))
        {
            BeginDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && _isDragging)
        {
            ContinueDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            EndDrag();
        }
    }

    private void BeginDrag(Vector2 pointerPos)
    {
        _isDragging = true;
        _lastPointerPos = pointerPos;
        _angularVelocity = 0f; // kill any leftover momentum
    }

    private void ContinueDrag(Vector2 pointerPos)
    {
        if (!_isDragging) return;

        float deltaX = pointerPos.x - _lastPointerPos.x;
        float angleDelta = deltaX * dragSensitivity;
        _dragAngleOffset += angleDelta;

        // Track instantaneous angular velocity (degrees per second).
        if (Time.deltaTime > 0f)
        {
            float instantVelocity = angleDelta / Time.deltaTime;
            // Smooth it so a single spike doesn't launch the camera.
            _angularVelocity = Mathf.Lerp(_angularVelocity, instantVelocity, 0.5f);
        }

        _lastPointerPos = pointerPos;
    }

    private void EndDrag()
    {
        _isDragging = false;
        _resumeTimer = resumeDelay;
        // _angularVelocity is already set from ContinueDrag — momentum carries over.
    }
}
