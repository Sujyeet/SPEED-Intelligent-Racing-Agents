using KartGame.KartSystems;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KartGame.AI
{
    [System.Serializable]
    public struct Sensor
    {
        public Transform Transform;
        public float RayDistance;
        public float HitValidationDistance;
    }

    public enum AgentMode
    {
        Training,
        Inferencing
    }

    public class KartAgent : Agent, IInput
    {
        #region Training Modes
        [Tooltip("Are we training the agent or is the agent production ready?")]
        public AgentMode Mode = AgentMode.Training;
        [Tooltip("What is the initial checkpoint the agent will go to? This value is only for inferencing.")]
        public ushort InitCheckpointIndex;
        #endregion

        #region Senses
        [Header("Observation Params")]
        [Tooltip("What objects should the raycasts hit and detect?")]
        public LayerMask Mask;
        [Tooltip("Sensors contain ray information to sense out the world, you can have as many sensors as you need.")]
        public Sensor[] Sensors;
        [Header("Checkpoints"), Tooltip("What are the series of checkpoints for the agent to seek and pass through?")]
        public Collider[] Colliders;
        [Tooltip("What layer are the checkpoints on? This should be an exclusive layer for the agent to use.")]
        public LayerMask CheckpointMask;
        [Space]
        [Tooltip("Would the agent need a custom transform to be able to raycast and hit the track? " +
                 "If not assigned, then the root transform will be used.")]
        public Transform AgentSensorTransform;
        #endregion

        #region Basic Rewards
        [Header("Basic Rewards"), Tooltip("What penalty is given when the agent crashes?")]
        public float HitPenalty = -1f;
        [Tooltip("How much reward is given when the agent successfully passes the checkpoints?")]
        public float PassCheckpointReward = 1f;
        [Tooltip("Should typically be a small value, but we reward the agent for moving in the right direction.")]
        public float TowardsCheckpointReward = 0.01f;
        [Tooltip("Typically if the agent moves faster, we want to reward it for finishing the track quickly.")]
        public float SpeedReward = 0.02f;
        [Tooltip("Reward the agent when it keeps accelerating")]
        public float AccelerationReward = 0.01f;
        #endregion

        #region Speed Thresholds
        [Header("Speed Thresholds")]
        [Tooltip("Minimum speed for speed rewards")]
        public float MinSpeedThreshold = 1.5f;
        [Tooltip("Maximum speed for speed rewards")]
        public float MaxSpeedThreshold = 6f;
        [Tooltip("Penalty for driving too slowly")]
        public float SlowSpeedPenalty = -0.01f;
        [Tooltip("Penalty for driving too fast")]
        public float FastSpeedPenalty = -0.005f;
        #endregion

        #region Natural Driving Rewards
        [Header("Natural Driving Rewards")]
        [Tooltip("Penalty for jerky steering movements")]
        public float SmoothSteeringPenalty = -0.001f;
        [Tooltip("Reward for maintaining appropriate speed on straights")]
        public float StraightSpeedReward = 0.005f;
        public float OptimalStraightSpeed = 8f;
        [Tooltip("Minimum speed for straight segments")]
        public float MinStraightSpeed = 4f;
        [Tooltip("Angle threshold to consider a segment as 'straight'")]
        public float StraightAngleThreshold = 15f;
        #endregion

        #region Path Deviation Configuration
        [Header("Path Deviation Configuration - Simplified")]
        [Tooltip("Track width for normalized deviation calculation")]
        public float trackWidth = 8f;
        [Tooltip("Enable debug visualization for path deviation")]
        public bool showPathDeviationDebug = false;
        [Tooltip("Penalty for path deviation")]
        public float pathDeviationPenalty = -0.005f;
        [Tooltip("Maximum acceptable deviation before penalty")]
        public float maxAcceptableDeviation = 2f;
        [Tooltip("Maximum distance to search for lane dividers")]
        public float maxSearchDistance = 15f;
        #endregion

        #region Time Scale Adjustment
        [Header("Time Scale Adjustment")]
        [Tooltip("Multiplier to adjust actions for different time scales (0.05 for 20x→1x)")]
        public float timeScaleMultiplier = 0.05f;
        [Tooltip("Enable/disable time scale adjustment")]
        public bool enableTimeScaleAdjustment = true;
        [Tooltip("Separate multiplier for steering sensitivity")]
        public float steeringMultiplier = 0.05f;
        [Tooltip("Separate multiplier for throttle/brake sensitivity")]
        public float throttleMultiplier = 0.05f;
        #endregion

        #region ResetParams
        [Header("Inference Reset Params")]
        [Tooltip("What is the unique mask that the agent should detect when it falls out of the track?")]
        public LayerMask OutOfBoundsMask;
        [Tooltip("What are the layers we want to detect for the track and the ground?")]
        public LayerMask TrackMask;
        [Tooltip("How far should the ray be when casted? For larger karts - this value should be larger too.")]
        public float GroundCastDistance;
        #endregion

        #region Collision Detection
        [Header("Collision Detection")]
        [Tooltip("Main body capsule collider for wall collision detection")]
        public CapsuleCollider kartBodyCollider;
        [Tooltip("Enable debug visualization for collision detection")]
        public bool showCollisionDebug = false;
        #endregion

        #region Performance Logging
        [Header("Performance Logging")]
        [Tooltip("Performance logger for capturing detailed agent metrics")]
        public DRLPerformanceLogger performanceLogger;
        #endregion

        #region Debugging
        [Header("Debug Option")] [Tooltip("Should we visualize the rays that the agent draws?")]
        public bool ShowRayCasts;
        #endregion

        

        // Core variables
        ArcadeKart m_Kart;
        bool m_Acceleration;
        bool m_Brake;
        public float m_Steering;
        int m_CheckpointIndex;
        int agentCompletedLaps = 0;
        bool m_EndEpisode;
        float m_LastAccumulatedReward;
        // Natural driving variables
        private float m_LastSteering = 0f;

        // Path deviation tracking
        //private float currentPathDeviation = 0f; // for simple API if needed

        void Awake()
        {
            m_Kart = GetComponent<ArcadeKart>();
            if (AgentSensorTransform == null) AgentSensorTransform = transform;
            if (kartBodyCollider == null) kartBodyCollider = GetComponent<CapsuleCollider>();
            if (performanceLogger == null) performanceLogger = GetComponent<DRLPerformanceLogger>();
        }

        void Start()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.drag = 0.3f;
            rb.angularDrag = 3f;
            rb.mass = 1200f;
            rb.centerOfMass = new Vector3(0, -0.4f, 0.1f);
            ValidateCollisionSetup();
            
             // TIME SCALE ADJUSTMENT 
    if (Mathf.Approximately(Time.timeScale, 1.0f))
    {
        Time.timeScale = 1.3f;
        Debug.Log("Game speed set to 1.5x by " + gameObject.name);
    }
    
        }

        void ValidateCollisionSetup()
        {
            if (kartBodyCollider == null)
                Debug.LogError("Kart body capsule collider not assigned! Wall collision detection will not work.");
            else
                Debug.Log($"Collision detection initialized with capsule collider: {kartBodyCollider.name}");
        }

        void Update()
        {
            if (m_EndEpisode)
            {
                m_EndEpisode = false;
                AddReward(m_LastAccumulatedReward);
                EndEpisode();
                OnEpisodeBegin();
            }
        }

        void LateUpdate()
        {
            switch (Mode)
            {
                case AgentMode.Inferencing:
                    if (ShowRayCasts)
                        Debug.DrawRay(transform.position, Vector3.down * GroundCastDistance, Color.cyan);
                    if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out var hit, GroundCastDistance, TrackMask)
                        && ((1 << hit.collider.gameObject.layer) & OutOfBoundsMask) > 0)
                    {
                        var checkpoint = Colliders[m_CheckpointIndex].transform;
                        transform.localRotation = checkpoint.rotation;
                        transform.position = checkpoint.position;
                        m_Kart.Rigidbody.velocity = default;
                        m_Steering = 0f;
                        m_Acceleration = m_Brake = false;
                    }
                    break;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            var maskedValue = 1 << other.gameObject.layer;
            var triggered = maskedValue & CheckpointMask;
            FindCheckpointIndex(other, out var index);
            if (triggered > 0 && (index > m_CheckpointIndex || (index == 0 && m_CheckpointIndex == Colliders.Length - 1)))
            {
                AddReward(PassCheckpointReward);
                m_CheckpointIndex = index;
                if (performanceLogger != null)
                    performanceLogger.RecordCheckpoint(index, $"Checkpoint_{index}",
                        index == 0, index == 0, 0f);
                if (index == 0 && (m_CheckpointIndex == Colliders.Length - 1 || performanceLogger?.currentLap > 0))
                {
                    if (performanceLogger != null)
                        performanceLogger.RecordLapCompletion(performanceLogger.currentLap, 0f);

                    // Single player lap finish logic (only in Single Player, never in Multiplayer)
                    if (KartGame.GameFlow.GameModeManager.IsSinglePlayer)
                    {
                        agentCompletedLaps++;
                        int targetLaps = 3;

                        if (agentCompletedLaps >= targetLaps)
                        {
                            if (KartGame.GameFlow.GameModeManager.OnAgentFinishedRace != null)
                            {
                                KartGame.GameFlow.GameModeManager.OnAgentFinishedRace.Invoke(this);
                            }
                            else if (m_Kart != null)
                            {
                                m_Kart.SetCanMove(false);
                            }
                        }
                    }
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            bool isWallCollision = false;
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.thisCollider == kartBodyCollider)
                {
                    if (ValidateWallCollision(contact))
                    {
                        isWallCollision = true;
                        if (showCollisionDebug)
                        {
                            Debug.DrawRay(contact.point, contact.normal * 2f, Color.red, 2f);
                            Debug.Log($"Wall collision detected at: {contact.point} with normal: {contact.normal}");
                        }
                        break;
                    }
                }
            }
            if (isWallCollision && performanceLogger != null)
                performanceLogger.RecordCollision();
        }

        bool ValidateWallCollision(ContactPoint contact)
        {
            Vector3 normal = contact.normal;
            Vector3 contactPoint = contact.point;
            Vector3 kartCenter = transform.position;
            float collisionHeight = contactPoint.y - kartCenter.y;
            bool appropriateHeight = (collisionHeight > -0.3f && collisionHeight < 1.5f);
            float normalAngle = Vector3.Angle(normal, Vector3.up);
            bool wallLikeNormal = (normalAngle > 30f && normalAngle < 150f);
            Vector3 velocity = GetComponent<Rigidbody>().velocity.normalized;
            float velocityOpposition = Vector3.Dot(normal, -velocity);
            bool opposesMovement = (velocityOpposition > 0.2f);
            return appropriateHeight && wallLikeNormal && opposesMovement;
        }

        void FindCheckpointIndex(Collider checkPoint, out int index)
        {
            for (int i = 0; i < Colliders.Length; i++)
            {
                if (Colliders[i].GetInstanceID() == checkPoint.GetInstanceID())
                {
                    index = i;
                    return;
                }
            }
            index = -1;
        }

        bool IsOnStraightSegment()
        {
            if (Colliders.Length < 3) return true;
            var current = m_CheckpointIndex;
            var next = (current + 1) % Colliders.Length;
            var nextNext = (current + 2) % Colliders.Length;
            var dir1 = (Colliders[next].transform.position - Colliders[current].transform.position).normalized;
            var dir2 = (Colliders[nextNext].transform.position - Colliders[next].transform.position).normalized;
            float angle = Vector3.Angle(dir1, dir2);
            return angle < StraightAngleThreshold;
        }

        void CalculateSpeedRewards()
        {
            float currentSpeed = m_Kart.LocalSpeed();
            if (currentSpeed >= MinSpeedThreshold && currentSpeed <= MaxSpeedThreshold)
            {
                float speedRatio = (currentSpeed - MinSpeedThreshold) / (MaxSpeedThreshold - MinSpeedThreshold);
                AddReward(speedRatio * SpeedReward);
            }
            else if (currentSpeed < MinSpeedThreshold)
            {
                AddReward(SlowSpeedPenalty);
            }
            else if (currentSpeed > MaxSpeedThreshold)
            {
                AddReward(FastSpeedPenalty);
            }
        }

        void AddSmoothSteeringReward()
        {
            float steeringChange = Mathf.Abs(m_Steering - m_LastSteering);
            if (steeringChange > 0.3f)
                AddReward(SmoothSteeringPenalty * steeringChange);
            m_LastSteering = m_Steering;
        }

        void AddStraightSegmentRewards()
        {
            if (IsOnStraightSegment())
            {
                float currentSpeed = m_Kart.LocalSpeed();
                if (currentSpeed >= MinStraightSpeed && currentSpeed <= OptimalStraightSpeed)
                {
                    float speedRatio = currentSpeed / OptimalStraightSpeed;
                    AddReward(StraightSpeedReward * speedRatio);
                }
            }
        }

        // ================================
        // === PATH DEVIATION FUNCTION ====
        // ================================
        public float GetCurrentPathDeviation()
        {
            Vector3 pos = transform.position;
            float minDistance = float.MaxValue;
            GameObject[] dividers = GameObject.FindGameObjectsWithTag("LaneDivider");
            foreach (var divider in dividers)
            {
                Vector3 dividerPos = divider.transform.position;
                float d = Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(dividerPos.x, 0, dividerPos.z));
                if (d < minDistance)
                    minDistance = d;
            }
            if (minDistance == float.MaxValue) return 0f;
            if (showPathDeviationDebug)
                Debug.DrawLine(pos, new Vector3(pos.x, 0, pos.z + minDistance), Color.yellow, 0.2f); // For visual debug
            return minDistance;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(m_Kart.LocalSpeed());
            var next = (m_CheckpointIndex + 1) % Colliders.Length;
            var nextCollider = Colliders[next];
            if (nextCollider == null)
                return;
            var direction = (nextCollider.transform.position - m_Kart.transform.position).normalized;
            sensor.AddObservation(Vector3.Dot(m_Kart.Rigidbody.velocity.normalized, direction));
            sensor.AddObservation(IsOnStraightSegment() ? 1f : 0f);
            if (ShowRayCasts)
                Debug.DrawLine(AgentSensorTransform.position, nextCollider.transform.position, Color.magenta);
            m_LastAccumulatedReward = 0.0f;
            m_EndEpisode = false;
            for (var i = 0; i < Sensors.Length; i++)
            {
                var current = Sensors[i];
                var xform = current.Transform;
                var hit = Physics.Raycast(AgentSensorTransform.position, xform.forward, out var hitInfo,
                    current.RayDistance, Mask, QueryTriggerInteraction.Ignore);
                if (ShowRayCasts)
                {
                    Debug.DrawRay(AgentSensorTransform.position, xform.forward * current.RayDistance, Color.green);
                    Debug.DrawRay(AgentSensorTransform.position, xform.forward * current.HitValidationDistance, Color.red);
                }
                if (hit && hitInfo.distance < current.HitValidationDistance)
                    Debug.DrawRay(hitInfo.point, Vector3.up * 3.0f, Color.blue);
                if (hit)
                {
                    if (hitInfo.distance < current.HitValidationDistance * 0.5f)
                    {
                        m_LastAccumulatedReward += HitPenalty;
                        m_EndEpisode = true;
                    }
                }
                sensor.AddObservation(hit ? hitInfo.distance : current.RayDistance);
            }
            sensor.AddObservation(m_Acceleration);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            InterpretDiscreteActions(actions);
            var next = (m_CheckpointIndex + 1) % Colliders.Length;
            var nextCollider = Colliders[next];
            var direction = (nextCollider.transform.position - m_Kart.transform.position).normalized;
            var reward = Vector3.Dot(m_Kart.Rigidbody.velocity.normalized, direction);
            if (ShowRayCasts) Debug.DrawRay(AgentSensorTransform.position, m_Kart.Rigidbody.velocity, Color.blue);
            AddReward(reward * TowardsCheckpointReward);
            AddReward((m_Acceleration && !m_Brake ? 1.0f : 0.0f) * AccelerationReward);
            CalculateSpeedRewards();
            AddSmoothSteeringReward();
            AddStraightSegmentRewards();
            // You could log/penalize path deviation here if you want to add it to the reward
        }

        public override void OnEpisodeBegin()
        {
            agentCompletedLaps = 0;
            switch (Mode)
            {
                case AgentMode.Training:
                    if (performanceLogger != null)
                    {
                        performanceLogger.SetFinished();
                        performanceLogger.ResetMetrics();
                    }
                    m_CheckpointIndex = Random.Range(0, Colliders.Length - 1);
                    var collider = Colliders[m_CheckpointIndex];
                    transform.localRotation = collider.transform.rotation;
                    transform.position = collider.transform.position;
                    m_Kart.Rigidbody.velocity = default;
                    m_Acceleration = false;
                    m_Brake = false;
                    m_Steering = 0f;
                    m_LastSteering = 0f;
                    break;
                default:
                    break;
            }
        }

        void InterpretDiscreteActions(ActionBuffers actions)
        {
            if (enableTimeScaleAdjustment)
            {
                m_Steering = (actions.DiscreteActions[0] - 1f) * steeringMultiplier;
                m_Acceleration = actions.DiscreteActions[1] >= 1.0f;
                m_Brake = actions.DiscreteActions[1] < 1.0f;
            }
            else
            {
                m_Steering = actions.DiscreteActions[0] - 1f;
                m_Acceleration = actions.DiscreteActions[1] >= 1.0f;
                m_Brake = actions.DiscreteActions[1] < 1.0f;
            }
        }

        public InputData GenerateInput()
        {
            return new InputData
            {
                Accelerate = m_Acceleration,
                Brake = m_Brake,
                TurnInput = m_Steering
            };
        }
    }
}
