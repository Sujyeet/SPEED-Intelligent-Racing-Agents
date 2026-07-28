using UnityEngine;

public class ManualPlayerCollisionHandler : MonoBehaviour
{
    [Header("Collision Detection")]
    [Tooltip("Main body capsule collider for wall collision detection")]
    public CapsuleCollider kartBodyCollider;
    [Tooltip("Enable debug visualization for collision detection")]
    public bool showCollisionDebug = false;
    
    private ManualPlayerPerformanceLogger performanceLogger;
    
    [Header("Path Deviation Configuration")]
    [Tooltip("Track width for normalized deviation calculation")]
    public float trackWidth = 10f; // Updated to match your track width
    [Tooltip("Enable debug visualization for path deviation")]
    public bool showPathDeviationDebug = false;
    [Tooltip("Maximum acceptable deviation before penalty")]
    public float maxAcceptableDeviation = 2f;
    [Tooltip("Maximum distance to search for lane dividers")]
    public float maxSearchDistance = 15f;

    void Start()
    {
        performanceLogger = GetComponent<ManualPlayerPerformanceLogger>();
        
        // Auto-assign capsule collider if not set
        if (kartBodyCollider == null) 
            kartBodyCollider = GetComponent<CapsuleCollider>();
            
        ValidateCollisionSetup();
        ValidateLaneDividerSetup();
    }
    
    void ValidateCollisionSetup()
    {
        if (kartBodyCollider == null)
        {
            Debug.LogError("Manual Player: Kart body capsule collider not assigned! Wall collision detection will not work.");
        }
        else
        {
            Debug.Log($"Manual Player collision detection initialized with capsule collider: {kartBodyCollider.name}");
        }
        
        if (performanceLogger == null)
        {
            Debug.LogError("Manual Player: Performance logger not found! Collisions will not be recorded.");
        }
    }
    
    /// <summary>
    /// Validates lane divider configuration for path deviation calculation
    /// </summary>
    void ValidateLaneDividerSetup()
    {
        GameObject[] laneDividers = GameObject.FindGameObjectsWithTag("LaneDivider");
        
        if (laneDividers.Length == 0)
        {
            Debug.LogWarning("Manual Player: No GameObjects found with 'LaneDivider' tag! Path deviation will always be zero.");
            Debug.LogWarning("Please tag your lane divider objects with 'LaneDivider' in the Inspector.");
        }
        else
        {
            Debug.Log($"Manual Player: Found {laneDividers.Length} lane divider objects for path deviation calculation:");
            for (int i = 0; i < Mathf.Min(laneDividers.Length, 5); i++)
            {
                if (laneDividers[i] != null)
                {
                    Debug.Log($"  - {laneDividers[i].name} at position {laneDividers[i].transform.position}");
                }
            }
            
            // Test calculation from current position
            float testDeviation = GetCurrentPathDeviation();
            Debug.Log($"Manual Player: Current path deviation from kart position: {testDeviation:F3} meters");
            
            // Validate track width configuration
            if (trackWidth > 0)
            {
                float deviationPercentage = (testDeviation / (trackWidth * 0.5f)) * 100f;
                Debug.Log($"Manual Player: Path deviation as percentage of track half-width: {deviationPercentage:F1}%");
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        bool isWallCollision = false;
        
        foreach (ContactPoint contact in collision.contacts)
        {
            // Verify collision involves the kart body capsule collider
            if (contact.thisCollider == kartBodyCollider)
            {
                // Additional validation to ensure this is a wall collision
                if (ValidateWallCollision(contact))
                {
                    isWallCollision = true;
                    
                    if (showCollisionDebug)
                    {
                        Debug.DrawRay(contact.point, contact.normal * 2f, Color.red, 2f);
                        Debug.Log($"Manual Player wall collision detected at: {contact.point} with normal: {contact.normal}");
                    }
                    break;
                }
            }
        }
        
        // Record collision if validated
        if (isWallCollision && performanceLogger != null)
        {
            performanceLogger.RecordCollision();
        }
    }
    
    /// <summary>
    /// Calculates current path deviation using identical logic to DRL agent
    /// This method replicates the exact algorithm used in your KartAgent script
    /// </summary>
    /// <returns>Distance to nearest lane divider in Unity units</returns>
    public float GetCurrentPathDeviation()
    {
        Vector3 pos = transform.position;
        float minDistance = float.MaxValue;
        
        // Find all objects tagged with "LaneDivider" - identical to DRL agent
        GameObject[] dividers = GameObject.FindGameObjectsWithTag("LaneDivider");
        
        if (dividers.Length == 0)
        {
            if (showPathDeviationDebug)
            {
                Debug.LogWarning("Manual Player: No lane dividers found with 'LaneDivider' tag for path deviation calculation.");
            }
            return 0f;
        }
        
        // Find closest lane divider using same logic as DRL agent
        foreach (var divider in dividers)
        {
            if (divider == null) continue;
            
            Vector3 dividerPos = divider.transform.position;
            
            // Skip dividers that are too far away for performance optimization
            float distanceToObject = Vector3.Distance(pos, dividerPos);
            if (distanceToObject > maxSearchDistance) continue;
            
            // Calculate 2D distance (ignore Y-axis) - IDENTICAL to DRL agent logic
            float d = Vector3.Distance(
                new Vector3(pos.x, 0, pos.z), 
                new Vector3(dividerPos.x, 0, dividerPos.z)
            );
            
            if (d < minDistance)
            {
                minDistance = d;
            }
        }
        
        // Debug visualization matching DRL agent exactly
        if (showPathDeviationDebug && minDistance != float.MaxValue)
        {
            // Use the exact same debug visualization as your DRL agent
            Vector3 debugEndPoint = new Vector3(pos.x, 0, pos.z + minDistance);
            Debug.DrawLine(pos, debugEndPoint, Color.yellow, 0.2f);
            
            // Additional debug information for manual player
            Debug.Log($"Manual Player Path Deviation: {minDistance:F3}m from nearest lane divider");
        }
        
        // Return 0 if no valid dividers found, otherwise return minimum distance
        return minDistance == float.MaxValue ? 0f : minDistance;
    }
    
    /// <summary>
    /// Validates if the collision represents a wall impact rather than ground/ceiling contact
    /// </summary>
    /// <param name="contact">Collision contact point information</param>
    /// <returns>True if collision is determined to be with a wall</returns>
    bool ValidateWallCollision(ContactPoint contact)
    {
        Vector3 normal = contact.normal;
        Vector3 contactPoint = contact.point;
        Vector3 kartCenter = transform.position;
        
        // Ensure collision occurs at appropriate height (not ground level)
        float collisionHeight = contactPoint.y - kartCenter.y;
        bool appropriateHeight = (collisionHeight > -0.3f && collisionHeight < 1.5f);
        
        // Verify normal direction suggests wall (not ground or ceiling)
        float normalAngle = Vector3.Angle(normal, Vector3.up);
        bool wallLikeNormal = (normalAngle > 30f && normalAngle < 150f);
        
        // Check if collision opposes movement direction
        Vector3 velocity = GetComponent<Rigidbody>().velocity.normalized;
        float velocityOpposition = Vector3.Dot(normal, -velocity);
        bool opposesMovement = (velocityOpposition > 0.2f);
        
        return appropriateHeight && wallLikeNormal && opposesMovement;
    }
    
    /// <summary>
    /// Context menu method to validate manual player lane dividers setup
    /// </summary>
    [ContextMenu("Validate Manual Player Lane Dividers")]
    public void ValidateManualPlayerLaneDividers()
    {
        ValidateLaneDividerSetup();
    }
    
    /// <summary>
    /// Context menu method to test path deviation calculation at current position
    /// </summary>
    [ContextMenu("Test Path Deviation Calculation")]
    public void TestPathDeviationCalculation()
    {
        float deviation = GetCurrentPathDeviation();
        Debug.Log($"=== MANUAL PLAYER PATH DEVIATION TEST ===");
        Debug.Log($"Current Position: {transform.position}");
        Debug.Log($"Path Deviation: {deviation:F3} meters");
        
        if (trackWidth > 0)
        {
            float normalizedDeviation = (deviation / (trackWidth * 0.5f)) * 100f;
            Debug.Log($"Normalized Deviation: {normalizedDeviation:F1}% of track half-width");
            
            if (deviation > maxAcceptableDeviation)
            {
                Debug.LogWarning($"Deviation exceeds acceptable threshold of {maxAcceptableDeviation}m!");
            }
        }
        
        GameObject[] dividers = GameObject.FindGameObjectsWithTag("LaneDivider");
        Debug.Log($"Total Lane Dividers Found: {dividers.Length}");
    }
}
