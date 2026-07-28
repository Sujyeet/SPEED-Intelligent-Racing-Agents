using Unity.Netcode;
using UnityEngine;

public sealed class NetworkedInputSync : NetworkBehaviour
{
    private readonly NetworkVariable<float> turnInput = new(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private readonly NetworkVariable<float> accelInput = new(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public float TurnInput => turnInput.Value;
    public float AccelInput => accelInput.Value;

    private void Update()
    {
        if (!IsOwner) return;

        turnInput.Value = Input.GetAxis("Horizontal");
        accelInput.Value = Input.GetAxis("Vertical");
    }
}
