using KartGame.KartSystems;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(200)] // Run after typical controller updates
public sealed class NetworkedKartAnimState : NetworkBehaviour
{
    [Header("Optional references (auto-filled in Awake if left empty)")]
    [SerializeField] private ArcadeKart kart;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private NetworkedInputSync inputSync;

    // Owner-written, everyone-read animation state
    public readonly NetworkVariable<float> Steering = new(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public readonly NetworkVariable<float> ForwardSpeed = new(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public readonly NetworkVariable<bool> Grounded = new(
        true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Reset()
    {
        kart = GetComponent<ArcadeKart>();
        rb = GetComponent<Rigidbody>();
        inputSync = GetComponent<NetworkedInputSync>();
    }

    private void Awake()
    {
        if (kart == null) kart = GetComponent<ArcadeKart>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (inputSync == null) inputSync = GetComponent<NetworkedInputSync>();
    }

    private void FixedUpdate()
    {
        // Only the owning player writes these values, and only after NGO has spawned the object.
        if (!IsSpawned || !IsOwner)
            return;

        // Steering: prefer the already-networked input source (owner writes it).
        float steer =
            inputSync != null ? inputSync.TurnInput :
            kart != null ? kart.Input.TurnInput :
            0f;

        Steering.Value = Mathf.Clamp(steer, -1f, 1f);

        // Speed for wheel roll (signed, along forward)
        if (rb != null)
        {
            Vector3 v = rb.velocity;
            ForwardSpeed.Value = Vector3.Dot(v, transform.forward);
        }
        else
        {
            ForwardSpeed.Value = 0f;
        }

        // Grounded: compute from the owner's simulation
        Grounded.Value = (kart != null) && (kart.GroundPercent >= 0.5f);
    }
}
