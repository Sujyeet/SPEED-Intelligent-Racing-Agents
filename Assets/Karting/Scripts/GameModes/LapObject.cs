using UnityEngine;
using KartGame.KartSystems;

/// <summary>
/// This class inherits from TargetObject and represents a LapObject.
/// </summary>
public class LapObject : TargetObject
{
    [Header("LapObject")]
    [Tooltip("Is this the first/last lap object?")]
    public bool finishLap;

    [HideInInspector]
    public bool lapOverNextPass;

    void Start() {
        Register();
    }
    
    void OnEnable()
    {
        lapOverNextPass = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[StartFinishLine TOUCHED] Collider: {other.name} | Parent: {(other.transform.parent != null ? other.transform.parent.name : "None")} | Time: {Time.timeSinceLevelLoad:F1}s");

        // Ignore initial spawn collisions during countdown/scene load
        if (Time.timeSinceLevelLoad < 1.5f)
            return;

        ArcadeKart kart = other.GetComponentInParent<ArcadeKart>();
        if (kart == null) return;

        // Verify this is a kart we own (locally controlled) in multiplayer
        var netObj = kart.GetComponent<Unity.Netcode.NetworkObject>();
        if (netObj != null && netObj.IsSpawned && !netObj.IsOwner)
            return; // Ignore other players' karts trigger entry

        Debug.Log($"[StartFinishLine] Lap trigger entered by kart '{kart.name}'!");
        Objective.OnUnregisterPickup?.Invoke(this);
    }
}
