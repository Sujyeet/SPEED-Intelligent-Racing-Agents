using System;
using KartGame.KartSystems;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(120)]
public sealed class KartAnimationNetworked : MonoBehaviour
{
    [Serializable]
    public class Wheel
    {
        public Transform wheelTransform;
        public WheelCollider wheelCollider;
    }

    [Header("Sources")]
    [SerializeField] private ArcadeKart kartController;
    [SerializeField] private NetworkedKartAnimState animState;
    [SerializeField] private NetworkObject netObj;
    [SerializeField] private Rigidbody rb;

    [Header("Tuning")]
    [SerializeField] private float steeringAnimationDamping = 10f;
    [SerializeField] private float maxSteeringAngle = 30f;

    [Header("Wheels")]
    public Wheel frontLeftWheel;
    public Wheel frontRightWheel;
    public Wheel rearLeftWheel;
    public Wheel rearRightWheel;

    private float smoothedSteer;

    private void Awake()
    {
        if (kartController == null) kartController = GetComponent<ArcadeKart>();
        if (animState == null) animState = GetComponent<NetworkedKartAnimState>();
        if (netObj == null) netObj = GetComponent<NetworkObject>();
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        float steerTarget =
            (animState != null && netObj != null && netObj.IsSpawned)
                ? animState.Steering.Value
                : (kartController != null ? kartController.Input.TurnInput : 0f);

        smoothedSteer = Mathf.MoveTowards(
            smoothedSteer,
            steerTarget,
            steeringAnimationDamping * Time.deltaTime
        );

        float steerAngle = smoothedSteer * maxSteeringAngle;

        // Steering angle
        if (frontLeftWheel?.wheelCollider != null) frontLeftWheel.wheelCollider.steerAngle = steerAngle;
        if (frontRightWheel?.wheelCollider != null) frontRightWheel.wheelCollider.steerAngle = steerAngle;

        // Pose from wheel colliders (suspension + steering orientation)
        UpdateWheelPose(frontLeftWheel);
        UpdateWheelPose(frontRightWheel);
        UpdateWheelPose(rearLeftWheel);
        UpdateWheelPose(rearRightWheel);

        // Manual roll for non-owners / kinematic bodies (remote presentation)
        bool manualRoll =
            (netObj != null && netObj.IsSpawned && !netObj.IsOwner) ||
            (rb != null && rb.isKinematic);

        if (manualRoll)
        {
            float fwdSpeed = (animState != null) ? animState.ForwardSpeed.Value : 0f;
            RollWheel(frontLeftWheel, fwdSpeed);
            RollWheel(frontRightWheel, fwdSpeed);
            RollWheel(rearLeftWheel, fwdSpeed);
            RollWheel(rearRightWheel, fwdSpeed);
        }
    }

    private static void UpdateWheelPose(Wheel w)
    {
        if (w == null || w.wheelTransform == null || w.wheelCollider == null) return;

        w.wheelCollider.GetWorldPose(out var pos, out var rot);
        w.wheelTransform.position = pos;
        w.wheelTransform.rotation = rot;
    }

    private static void RollWheel(Wheel w, float forwardSpeedMetersPerSec)
    {
        if (w == null || w.wheelTransform == null || w.wheelCollider == null) return;

        float radius = Mathf.Max(0.0001f, w.wheelCollider.radius);
        float circumference = 2f * Mathf.PI * radius;

        float metersThisFrame = forwardSpeedMetersPerSec * Time.deltaTime;
        float degrees = (metersThisFrame / circumference) * 360f;

        // If your wheel mesh rotates on a different axis, change Vector3.right accordingly.
        w.wheelTransform.Rotate(Vector3.right, degrees, Space.Self);
    }
}
