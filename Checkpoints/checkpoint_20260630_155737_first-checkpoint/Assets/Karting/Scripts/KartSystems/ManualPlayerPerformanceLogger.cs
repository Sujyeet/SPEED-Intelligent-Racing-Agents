using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

public class ManualPlayerPerformanceLogger : MonoBehaviour
{
    [Header("Configuration")]
    public string agentType = "Manual_Player";
    [Tooltip("Leave blank to use default path")]
    public string logFilePath = "";
    public bool enableLogging = true;
    public bool saveDetailedTimeSeries = true;

    [Header("Agent References")]
    // This now correctly looks for the original ArcadeKart script
    public KartGame.KartSystems.ArcadeKart kart;
    
    [Header("Path Deviation Configuration")]
    [Tooltip("Track width for normalized deviation calculation")]
    public float trackWidth = 10f;
    [Tooltip("Enable debug visualization for path deviation")]
    public bool showPathDeviationDebug = false;
    [Tooltip("Maximum acceptable deviation before penalty")]
    public float maxAcceptableDeviation = 2f;
    [Tooltip("Maximum distance to search for lane dividers")]
    public float maxSearchDistance = 15f;

    [Header("Metrics")]
    // CRITICAL FIX: These are now INSTANCE variables. Each logger gets its own timer.
    private float startTime;
    private float completionTime;
    private int collisionCount = 0;
    private float distanceTraveled = 0f;
    private List<float> speedSamples = new List<float>();
    private List<float> pathDeviationSamples = new List<float>();
    private List<float> steeringAngleSamples = new List<float>();
    private List<float> accelerationSamples = new List<float>();
    private Vector3 lastPosition;
    private Vector3 lastVelocity;
    private List<TimeSeriesDataPoint> timeSeriesData = new List<TimeSeriesDataPoint>();
    private List<CheckpointData> checkpointData = new List<CheckpointData>();
    private List<LapData> lapData = new List<LapData>();
    private Rigidbody rb;
    private bool hasFinished = false;
    private float sampleInterval = 0.1f;
    private float timeSinceLastSample = 0f;
    private string actualLogPath;

    // Per-lap accumulators
    private List<float> lapSpeedSamples = new List<float>();
    private List<float> lapSteeringSamples = new List<float>();
    private List<float> lapAccelerationSamples = new List<float>();
    private List<float> lapPathDeviationSamples = new List<float>();
    private int lapCollisionCount = 0;
    private float lapStartTimestamp = 0f;

    // Lap management
    public int currentLap = 0;
    public float lapStartTime = 0f;

    [System.Serializable]
    private class TimeSeriesDataPoint
    {
        public float time;
        public Vector3 position;
        public Vector3 velocity;
        public float speed;
        public float steeringAngle;
        public float acceleration;
        public float pathDeviation;
        public string phase;
    }

    [System.Serializable]
    private class CheckpointData
    {
        public int checkpointID;
        public string segmentName;
        public bool isSegmentStart;
        public bool isSegmentEnd;
        public float segmentTime;
        public float timestamp;
        public Vector3 position;
        public float speed;
    }

    [System.Serializable]
    private class LapData
    {
        public int lapNumber;
        public float lapTime;
        public float startTimestamp;
        public float endTimestamp;
        public float averageSpeed;
        public float maxSpeed;
        public float minSpeed;
        public float averageSteering;
        public float maxSteering;
        public float averageAcceleration;
        public float maxAcceleration;
        public float totalPathDeviation;
        public int collisionCount;
        public float lapEfficiency;
    }

    void Awake()
    {
        // Find components on this GameObject automatically
        if (kart == null) kart = GetComponent<KartGame.KartSystems.ArcadeKart>();
        rb = GetComponent<Rigidbody>();
        
        ValidateSetup();
    }

    void Start()
    {
        SetupLoggingDirectory();
        Debug.Log("Manual Player Performance Logger initialized successfully");
    }

    void OnEnable()
    {
        // Use OnEnable to start the timer, as it's called right before the first frame.
        // This ensures the timer is fresh for each run and prevents shared timing issues.
        ResetMetrics();
    }

    void ValidateSetup()
    {
        if (kart == null)
        {
            Debug.LogError("Manual Player: ArcadeKart component not found! Performance logging will not work properly.");
        }
        else
        {
            Debug.Log($"Manual Player performance logger connected to ArcadeKart: {kart.name}");
        }

        // Validate lane divider setup for path deviation
        GameObject[] laneDividers = GameObject.FindGameObjectsWithTag("LaneDivider");
        
        if (laneDividers.Length == 0)
        {
            Debug.LogWarning("Manual Player: No GameObjects found with 'LaneDivider' tag! Path deviation will always be zero.");
        }
        else
        {
            Debug.Log($"Manual Player: Found {laneDividers.Length} lane divider objects for path deviation calculation");
            
            // Test calculation from current position
            float testDeviation = GetCurrentPathDeviation();
            Debug.Log($"Manual Player: Current path deviation from kart position: {testDeviation:F3} meters");
        }
    }

    void SetupLoggingDirectory()
    {
        actualLogPath = string.IsNullOrEmpty(logFilePath) ? Path.Combine(Application.dataPath, "Logs") : logFilePath;
        if (!Directory.Exists(actualLogPath))
            Directory.CreateDirectory(actualLogPath);
        Debug.Log($"Performance logging initialized. Data will be saved to: {actualLogPath}");
    }
    
    void FixedUpdate()
    {
        if (hasFinished || !enableLogging || rb == null) return;
        
        float segmentDistance = Vector3.Distance(transform.position, lastPosition);
        distanceTraveled += segmentDistance;
        Vector3 acceleration = (rb.velocity - lastVelocity) / Time.fixedDeltaTime;
        float accelerationMagnitude = acceleration.magnitude;
        timeSinceLastSample += Time.fixedDeltaTime;
        
        if (timeSinceLastSample >= sampleInterval)
        {
            float speed = rb.velocity.magnitude;
            speedSamples.Add(speed);
            lapSpeedSamples.Add(speed);

            // Get steering angle from ArcadeKart input
            float steeringAngle = kart != null ? Mathf.Abs(kart.Input.TurnInput * 45f) : 0f;
            steeringAngleSamples.Add(steeringAngle);
            lapSteeringSamples.Add(steeringAngle);

            accelerationSamples.Add(accelerationMagnitude);
            lapAccelerationSamples.Add(accelerationMagnitude);

            // Calculate actual path deviation using identical DRL logic
            float deviation = GetCurrentPathDeviation();
            pathDeviationSamples.Add(deviation);
            lapPathDeviationSamples.Add(deviation);

            if (saveDetailedTimeSeries)
            {
                TimeSeriesDataPoint dataPoint = new TimeSeriesDataPoint
                {
                    time = Time.time - startTime,
                    position = transform.position,
                    velocity = rb.velocity,
                    speed = speed,
                    steeringAngle = steeringAngle,
                    acceleration = accelerationMagnitude,
                    pathDeviation = deviation,
                    phase = "driving"
                };
                timeSeriesData.Add(dataPoint);
            }
            timeSinceLastSample = 0f;
        }
        lastPosition = transform.position;
        lastVelocity = rb.velocity;
    }
    
    /// <summary>
    /// Calculates current path deviation using identical logic to DRL agent
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
        }
        
        // Return 0 if no valid dividers found, otherwise return minimum distance
        return minDistance == float.MaxValue ? 0f : minDistance;
    }

    public void RecordCollision()
    {
        if (!enableLogging) return;
        
        collisionCount++;
        lapCollisionCount++;
        Debug.Log($"<color=green>Manual Player collision detected: Total={collisionCount}, This lap={lapCollisionCount}</color>");
    }

    public void RecordCheckpoint(int checkpointID, string segmentName, bool isSegmentStart, bool isSegmentEnd, float segmentTime)
    {
        if (!enableLogging || rb == null) return;

        CheckpointData data = new CheckpointData
        {
            checkpointID = checkpointID,
            segmentName = segmentName,
            isSegmentStart = isSegmentStart,
            isSegmentEnd = isSegmentEnd,
            segmentTime = segmentTime,
            timestamp = Time.time - startTime,
            position = transform.position,
            speed = rb.velocity.magnitude
        };

        checkpointData.Add(data);
        
        if (isSegmentEnd && segmentTime > 0)
            Debug.Log($"Manual Player - Segment '{segmentName}' completed in {segmentTime:F2} seconds at speed {rb.velocity.magnitude:F2} m/s");
    }

    public void RecordLapCompletion(int lapNumber, float lapTime /* ignored */)
{
    if (!enableLogging) return;

    // Recompute from this logger's own timer
    lapTime = Time.time - lapStartTime;

    float averageSpeed = lapSpeedSamples.Count > 0 ? lapSpeedSamples.Average() : 0f;
    float maxSpeed = lapSpeedSamples.Count > 0 ? lapSpeedSamples.Max() : 0f;
    float minSpeed = lapSpeedSamples.Count > 0 ? lapSpeedSamples.Min() : 0f;
    float averageSteering = lapSteeringSamples.Count > 0 ? lapSteeringSamples.Average() : 0f;
    float maxSteering = lapSteeringSamples.Count > 0 ? lapSteeringSamples.Max() : 0f;
    float averageAcceleration = lapAccelerationSamples.Count > 0 ? lapAccelerationSamples.Average() : 0f;
    float maxAcceleration = lapAccelerationSamples.Count > 0 ? lapAccelerationSamples.Max() : 0f;
    float totalPathDeviation = lapPathDeviationSamples.Sum();
    float lapEfficiency = (distanceTraveled > 0f)
        ? (distanceTraveled - lapPathDeviationSamples.Sum()) / distanceTraveled
        : 0f;

    LapData lap = new LapData
    {
        lapNumber = lapNumber,
        lapTime = lapTime,
        startTimestamp = lapStartTimestamp,
        endTimestamp = Time.time - startTime,
        averageSpeed = averageSpeed,
        maxSpeed = maxSpeed,
        minSpeed = minSpeed,
        averageSteering = averageSteering,
        maxSteering = maxSteering,
        averageAcceleration = averageAcceleration,
        maxAcceleration = maxAcceleration,
        totalPathDeviation = totalPathDeviation,
        collisionCount = lapCollisionCount,
        lapEfficiency = lapEfficiency
    };

    lapData.Add(lap);
    Debug.Log($"Manual Player Performance Logger: Lap {lapNumber} completed in {lapTime:F2} seconds (logger-timed)");

    // Reset per-lap accumulators
    lapSpeedSamples.Clear();
    lapSteeringSamples.Clear();
    lapAccelerationSamples.Clear();
    lapPathDeviationSamples.Clear();
    lapCollisionCount = 0;

    // Advance lap timers/counters
    lapStartTimestamp = Time.time - startTime;
    currentLap++;
    lapStartTime = Time.time;
}


    public void SetFinished()
    {
        if (hasFinished || !enableLogging) return;
        hasFinished = true;
        completionTime = Time.time - startTime; // Calculate final time for THIS instance
        SaveResults();
    }

    void SaveResults()
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = Path.Combine(actualLogPath, $"AI_{agentType}_Run_{timestamp}.csv");
        string timeSeriesFilename = Path.Combine(actualLogPath, $"AI_{agentType}_TimeSeries_{timestamp}.csv");

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Metric,Value");
        sb.AppendLine($"Agent Type,{agentType}");
        sb.AppendLine($"Completion Time (s),{completionTime:F3}");
        sb.AppendLine($"Collision Count,{collisionCount}");
        sb.AppendLine($"Distance Traveled (m),{distanceTraveled:F3}");

        float avgSpeed = speedSamples.Count > 0 ? speedSamples.Average() : 0f;
        if (speedSamples.Count > 0)
        {
            float maxSpeed = speedSamples.Max();
            float minSpeed = speedSamples.Min();
            float speedStdDev = CalculateStandardDeviation(speedSamples);
            sb.AppendLine($"Average Speed (m/s),{avgSpeed:F5}");
            sb.AppendLine($"Maximum Speed (m/s),{maxSpeed:F5}");
            sb.AppendLine($"Minimum Speed (m/s),{minSpeed:F5}");
            sb.AppendLine($"Speed Standard Deviation,{speedStdDev:F6}");
            sb.AppendLine($"Speed Consistency,{(avgSpeed != 0f ? 1.0f - (speedStdDev / avgSpeed) : 0f):F7}");
        }

        if (steeringAngleSamples.Count > 0)
        {
            float avgSteering = steeringAngleSamples.Average();
            float maxSteering = steeringAngleSamples.Max();
            sb.AppendLine($"Average Steering Angle (deg),{avgSteering:F7}");
            sb.AppendLine($"Maximum Steering Angle (deg),{maxSteering:F5}");
            sb.AppendLine($"Steering Smoothness,{(1.0f - (avgSteering / 180.0f)):F7}");
        }

        if (accelerationSamples.Count > 0)
        {
            float avgAccel = accelerationSamples.Average();
            float maxAccel = accelerationSamples.Max();
            sb.AppendLine($"Average Acceleration (m/s²),{avgAccel:F4}");
            sb.AppendLine($"Maximum Acceleration (m/s²),{maxAccel:F3}");
        }

        // Path deviation (now using actual calculation)
        if (pathDeviationSamples.Count > 0)
        {
            sb.AppendLine($"Average Path Deviation (m),{pathDeviationSamples.Average():F3}");
            sb.AppendLine($"Maximum Path Deviation (m),{pathDeviationSamples.Max():F3}");
        }
        else
        {
            sb.AppendLine($"Average Path Deviation (m),0");
            sb.AppendLine($"Maximum Path Deviation (m),0");
        }

        // Add lap data matching DRL format
        if (lapData.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Lap Data");
            sb.AppendLine("Lap,StartTime,EndTime,LapTime,AvgSpeed,MaxSpeed,MinSpeed,AvgSteering,MaxSteering,AvgAccel,MaxAccel,PathDeviation,Collisions,Efficiency");
            foreach (var lap in lapData)
            {
                sb.AppendLine($"{lap.lapNumber},{lap.startTimestamp:F3},{lap.endTimestamp:F3},{lap.lapTime:F3},{lap.averageSpeed:F3},{lap.maxSpeed:F3},{lap.minSpeed:F3},{lap.averageSteering:F3},{lap.maxSteering:F3},{lap.averageAcceleration:F3},{lap.maxAcceleration:F3},{lap.totalPathDeviation:F3},{lap.collisionCount},{lap.lapEfficiency:F3}");
            }
        }

        if (lapData.Count > 1)
        {
            sb.AppendLine();
            sb.AppendLine("Lap Summary");
            sb.AppendLine("Metric,Value");
            sb.AppendLine($"Total Laps,{lapData.Count}");
            sb.AppendLine($"Best Lap Time,{lapData.Min(l => l.lapTime):F3}");
            sb.AppendLine($"Worst Lap Time,{lapData.Max(l => l.lapTime):F3}");
            sb.AppendLine($"Average Lap Time,{lapData.Average(l => l.lapTime):F3}");
            sb.AppendLine($"Lap Time Consistency,{(1.0f - (CalculateStandardDeviation(lapData.Select(l => l.lapTime).ToList()) / lapData.Average(l => l.lapTime))):F3}");

            float avgSpeedSummary = speedSamples.Count > 0 ? speedSamples.Average() : 0f;
            if (avgSpeedSummary > 0)
            {
                float idealTime = distanceTraveled / avgSpeedSummary;
                float efficiency = idealTime / completionTime;
                sb.AppendLine($"Time Efficiency,{efficiency:F7}");
            }
        }

        // Checkpoint data section
        if (checkpointData.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Checkpoint Data");
            sb.AppendLine("CheckpointID,SegmentName,IsStart,IsEnd,SegmentTime,Timestamp,Speed");
            foreach (var checkpoint in checkpointData)
            {
                sb.AppendLine($"{checkpoint.checkpointID},{checkpoint.segmentName},{checkpoint.isSegmentStart},{checkpoint.isSegmentEnd},{checkpoint.segmentTime:F3},{checkpoint.timestamp:F3},{checkpoint.speed:F3}");
            }

            var segments = checkpointData.Where(c => c.isSegmentEnd && c.segmentTime > 0)
                .GroupBy(c => c.segmentName)
                .Select(g => new
                {
                    SegmentName = g.Key,
                    AverageTime = g.Average(c => c.segmentTime),
                    MinTime = g.Min(c => c.segmentTime),
                    MaxTime = g.Max(c => c.segmentTime),
                    Count = g.Count()
                });

            if (segments.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Segment Summary");
                sb.AppendLine("SegmentName,AverageTime,MinTime,MaxTime,Count");
                foreach (var segment in segments)
                {
                    sb.AppendLine($"{segment.SegmentName},{segment.AverageTime:F3},{segment.MinTime:F3},{segment.MaxTime:F3},{segment.Count}");
                }
            }
        }

        File.WriteAllText(filename, sb.ToString());
        Debug.Log($"Saved Manual Player performance summary to: {filename}");
        Debug.Log($"Full path: {Path.GetFullPath(filename)}");

        // Save time-series data when enabled and data exists
        if (saveDetailedTimeSeries && timeSeriesData.Count > 0)
        {
            SaveTimeSeriesData(timeSeriesFilename);
        }
        else if (saveDetailedTimeSeries)
        {
            Debug.LogWarning("Time-series logging enabled but no data collected. Check FixedUpdate execution.");
        }
    }

    void SaveTimeSeriesData(string filename)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Time,PosX,PosY,PosZ,VelX,VelY,VelZ,Speed,SteeringAngle,Acceleration,PathDeviation,Phase,CheckpointID");

        foreach (var point in timeSeriesData)
        {
            int checkpointID = -1;
            // Find nearest checkpoint match based on timestamp
            foreach (var checkpoint in checkpointData)
            {
                if (Mathf.Abs(point.time - checkpoint.timestamp) < 0.1f)
                {
                    checkpointID = checkpoint.checkpointID;
                    break;
                }
            }

            sb.AppendLine($"{point.time:F3}," +
                $"{point.position.x:F3},{point.position.y:F3},{point.position.z:F3}," +
                $"{point.velocity.x:F3},{point.velocity.y:F3},{point.velocity.z:F3}," +
                $"{point.speed:F3},{point.steeringAngle:F3},{point.acceleration:F3},{point.pathDeviation:F3},{point.phase},{checkpointID}");
        }

        File.WriteAllText(filename, sb.ToString());
        Debug.Log($"Saved Manual Player detailed time series data to: {filename}");
        Debug.Log($"Full path: {Path.GetFullPath(filename)}");
    }

    float CalculateStandardDeviation(List<float> values)
    {
        if (values.Count <= 1) return 0;
        float avg = values.Average();
        float sum = values.Sum(v => (v - avg) * (v - avg));
        return Mathf.Sqrt(sum / (values.Count - 1));
    }

    public void ResetMetrics()
    {
        // Reset all data for a new run - CRITICAL for fixing shared timing issue
        if (!hasFinished && enableLogging)
        {
            completionTime = Time.time - startTime;
            SaveResults();
        }

        hasFinished = false;
        startTime = Time.time; // Start a fresh timer for this instance
        lapStartTime = startTime;
        completionTime = 0f;
        collisionCount = 0;
        distanceTraveled = 0f;
        speedSamples.Clear();
        pathDeviationSamples.Clear();
        steeringAngleSamples.Clear();
        accelerationSamples.Clear();
        timeSeriesData.Clear();
        checkpointData.Clear();
        lapData.Clear();
        timeSinceLastSample = 0f;
        currentLap = 1;

        lastPosition = transform.position;
        lastVelocity = rb != null ? rb.velocity : Vector3.zero;

        // Reset per-lap accumulators
        lapSpeedSamples.Clear();
        lapSteeringSamples.Clear();
        lapAccelerationSamples.Clear();
        lapPathDeviationSamples.Clear();
        lapCollisionCount = 0;
        lapStartTimestamp = 0f;

        Debug.Log($"{agentType} Performance metrics reset for new trial - Fresh timer started");
    }

    void OnDisable()
    {
        // Ensure logs are saved if the object is disabled or the game stops
        SetFinished();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SetFinished();
    }

    void OnApplicationQuit()
    {
        SetFinished();
    }

    [ContextMenu("Save Performance Log")]
    public void ManualSaveLog()
    {
        SetFinished();
    }

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
