using UnityEngine;

// This script acts as the bridge between Unity's physics events and your performance logger.
[RequireComponent(typeof(ManualPlayerPerformanceLogger))]
public class ManualKartEventHandler : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("The main capsule collider for the kart's body. Used to filter out wheel collisions.")]
    public CapsuleCollider kartBodyCollider;

    private ManualPlayerPerformanceLogger performanceLogger;

    void Awake()
    {
        // Automatically find the required components on this GameObject
        performanceLogger = GetComponent<ManualPlayerPerformanceLogger>();
        if (kartBodyCollider == null)
        {
            kartBodyCollider = GetComponent<CapsuleCollider>();
        }

        // Validate setup
        if (kartBodyCollider == null)
            Debug.LogError("ManualKartEventHandler: A CapsuleCollider is required for collision detection but was not found.", this);
        if (performanceLogger == null)
            Debug.LogError("ManualKartEventHandler: The ManualPlayerPerformanceLogger was not found.", this);
    }

    void OnCollisionEnter(Collision collision)
    {
        // This method is called by Unity's physics engine on any collision.
        // We must validate that it's a wall collision with the kart's main body.

        if (performanceLogger == null || kartBodyCollider == null) return;

        bool isWallCollision = false;
        foreach (ContactPoint contact in collision.contacts)
        {
            // IMPORTANT: This check ensures we only count collisions with the kart's body, not its wheels.
            if (contact.thisCollider == kartBodyCollider)
            {
                // Now, validate if the surface we hit is actually a wall.
                if (ValidateIsWall(contact))
                {
                    isWallCollision = true;
                    break; // A valid wall collision was found, no need to check other contact points.
                }
            }
        }

        if (isWallCollision)
        {
            // Tell the logger to record the event.
            performanceLogger.RecordCollision();
        }
    }

    private bool ValidateIsWall(ContactPoint contact)
    {
        // A wall's "normal" (the direction its surface faces) should be mostly horizontal.
        // We check the angle between the surface normal and the world's "up" direction.
        const float wallAngleThreshold = 75f; // Angles greater than this are considered walls/slopes.
        
        float angle = Vector3.Angle(Vector3.up, contact.normal);
        return angle > wallAngleThreshold;
    }
}
