using UnityEngine;

/// <summary>
/// Defines a path made of child Transform waypoints.
/// Place this on an empty GameObject and add child empties as waypoints.
/// </summary>
public class WaypointPath : MonoBehaviour
{
    [Tooltip("After reaching the last waypoint, continue from the first.")]
    public bool loop = true;

    [Tooltip("Reverse direction at each end instead of jumping back to start. Ignored when loop is false.")]
    public bool pingPong = false;

    [Tooltip("Color used to draw the path gizmo in the Scene view.")]
    public Color gizmoColor = Color.cyan;

    /// <summary>Number of waypoints (child transforms) in this path.</summary>
    public int WaypointCount => transform.childCount;

    /// <summary>Returns the world position of the waypoint at the given index.</summary>
    public Vector3 GetWaypointPosition(int index)
    {
        return transform.GetChild(index).position;
    }

    /// <summary>Returns the Transform of the waypoint at the given index.</summary>
    public Transform GetWaypoint(int index)
    {
        return transform.GetChild(index);
    }

    /// <summary>
    /// Advances to the next waypoint index, handling loop and ping-pong logic.
    /// </summary>
    /// <param name="currentIndex">The current waypoint index.</param>
    /// <param name="direction">1 for forward, -1 for backward. Updated if ping-pong reverses.</param>
    /// <returns>The next waypoint index, or -1 if the path has ended (non-looping).</returns>
    public int GetNextIndex(int currentIndex, ref int direction)
    {
        int count = WaypointCount;
        if (count < 2) return -1;

        int next = currentIndex + direction;

        if (next >= count)
        {
            if (loop)
            {
                if (pingPong)
                {
                    direction = -1;
                    next = currentIndex + direction;
                }
                else
                {
                    next = 0;
                }
            }
            else
            {
                return -1; // path ended
            }
        }
        else if (next < 0)
        {
            if (loop && pingPong)
            {
                direction = 1;
                next = currentIndex + direction;
            }
            else
            {
                return -1;
            }
        }

        return next;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        int count = WaypointCount;
        if (count < 2) return;

        Gizmos.color = gizmoColor;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetWaypointPosition(i);

            // Draw sphere at each waypoint
            Gizmos.DrawSphere(pos, 0.2f);

            // Draw line to the next waypoint
            if (i < count - 1)
            {
                Gizmos.DrawLine(pos, GetWaypointPosition(i + 1));
            }
        }

        // Close the loop visually if looping and not ping-pong
        if (loop && !pingPong && count > 2)
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.4f);
            Gizmos.DrawLine(GetWaypointPosition(count - 1), GetWaypointPosition(0));
        }
    }
#endif
}
