using UnityEngine;
using System.Linq;

public class TrackWidthMeasurer : MonoBehaviour
{
    [Header("Measurement Configuration")]
    [Tooltip("Set this to the layer your track object is on. This ensures raycasts only hit the track.")]
    public LayerMask trackLayerMask;

    [Tooltip("The height above the lane divider to start the raycasts from.")]
    public float raycastHeightOffset = 0.5f;

    private const string LANE_DIVIDER_NAME = "LaneDivider";

    [ContextMenu("Measure Track Width From Centerline")]
    public void MeasureWidth()
    {
        // Find the LaneDivider child object
        Transform laneDivider = transform.Find(LANE_DIVIDER_NAME);

        if (laneDivider == null)
        {
            Debug.LogError($"Could not find child object named '{LANE_DIVIDER_NAME}'. Please ensure it exists and is named correctly.", this);
            return;
        }

        if (trackLayerMask == 0)
        {
             Debug.LogError("Track Layer Mask is not set. Please assign the track's layer in the Inspector.", this);
             return;
        }

        Vector3 centerPoint = laneDivider.position + new Vector3(0, raycastHeightOffset, 0);

        // Determine the "right" direction relative to the track's orientation
        Vector3 rightDirection = transform.right;

        float rightDistance = 0f;
        float leftDistance = 0f;

        // Raycast to the right
        if (Physics.Raycast(centerPoint, rightDirection, out RaycastHit hitRight, 20f, trackLayerMask))
        {
            rightDistance = hitRight.distance;
            Debug.DrawLine(centerPoint, hitRight.point, Color.green, 10f);
        }
        else
        {
            Debug.LogWarning("Raycast to the right did not hit a wall. Check raycast direction and layer mask.", this);
        }

        // Raycast to the left
        if (Physics.Raycast(centerPoint, -rightDirection, out RaycastHit hitLeft, 20f, trackLayerMask))
        {
            leftDistance = hitLeft.distance;
            Debug.DrawLine(centerPoint, hitLeft.point, Color.green, 10f);
        }
        else
        {
            Debug.LogWarning("Raycast to the left did not hit a wall. Check raycast direction and layer mask.", this);
        }

        if (rightDistance > 0 && leftDistance > 0)
        {
            float totalWidth = rightDistance + leftDistance;
            Debug.Log($"<color=cyan><b>Measured Track Width: {totalWidth:F2} Unity units.</b></color>", this);
            Debug.Log($"Update the 'Track Width' field in your KartAgent component to <color=yellow><b>{totalWidth:F1}f</b></color>.", this);
        }
        else
        {
            Debug.LogError("Failed to measure track width. Could not detect both walls.", this);
        }
    }
}
