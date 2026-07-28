using Unity.Netcode;
using UnityEngine;

public class OwnerAuthoritativeKart : NetworkBehaviour
{
    private Rigidbody rb;
    private Transform tf;
    
    // Sync rate (updates per second)
    private float syncRate = 15f;
    private float syncTimer = 0f;
    
    // For interpolation on non-owners
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        tf = transform;
        targetPosition = tf.position;
        targetRotation = tf.rotation;
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            // Owner: Send position to server periodically
            syncTimer += Time.fixedDeltaTime;
            if (syncTimer >= 1f / syncRate)
            {
                syncTimer = 0f;
                SyncKartServerRpc(tf.position, tf.rotation, rb.velocity, rb.angularVelocity);
            }
        }
        else
        {
            // Non-owner: Smooth to received position
            // Only move if Rigidbody is kinematic (otherwise physics handles it)
            if (rb.isKinematic)
            {
                tf.position = Vector3.Lerp(tf.position, targetPosition, Time.fixedDeltaTime * 10f);
                tf.rotation = Quaternion.Slerp(tf.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            }
        }
    }

    [ServerRpc]
    private void SyncKartServerRpc(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel)
    {
        // Server receives from owner, broadcasts to all
        SyncKartClientRpc(pos, rot, vel, angVel);
    }

    [ClientRpc]
    private void SyncKartClientRpc(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel)
    {
        if (IsOwner) return; // Don't apply to owner
        
        // Store target values for interpolation
        targetPosition = pos;
        targetRotation = rot;
        
        // Only set velocities if NOT kinematic (prevents warnings)
        if (!rb.isKinematic)
        {
            rb.velocity = vel;
            rb.angularVelocity = angVel;
        }
    }
}

