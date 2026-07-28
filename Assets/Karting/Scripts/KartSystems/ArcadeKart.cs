using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class ArcadeKart : MonoBehaviour
    {
        [System.Serializable]
        public class StatPowerups
        {
            public ArcadeKart.Stats modifiers;
            public string PowerUpID;
            public float ElapsedTime;
            public float MaxTime;
        }

        [System.Serializable]
        public struct Stats
        {
            [Header("Movement Settings")]
            [Min(0.001f), Tooltip("Top speed attainable when moving forward.")]
            public float TopSpeed;

            [Tooltip("How quickly the kart reaches top speed.")]
            public float Acceleration;

            [Min(0.001f), Tooltip("Top speed attainable when moving backward.")]
            public float ReverseSpeed;

            [Tooltip("How quickly the kart reaches top speed, when moving backward.")]
            public float ReverseAcceleration;

            [Tooltip("How quickly the kart starts accelerating from 0. A higher number means it accelerates faster sooner.")]
            [Range(0.2f, 1)]
            public float AccelerationCurve;

            [Tooltip("How quickly the kart slows down when the brake is applied.")]
            public float Braking;

            [Tooltip("How quickly the kart will reach a full stop when no inputs are made.")]
            public float CoastingDrag;

            [Range(0.0f, 1.0f)]
            [Tooltip("The amount of side-to-side friction.")]
            public float Grip;

            [Tooltip("How tightly the kart can turn left or right.")]
            public float Steer;

            [Tooltip("Additional gravity for when the kart is in the air.")]
            public float AddedGravity;

            
            public static Stats operator +(Stats a, Stats b)
            {
                return new Stats
                {
                    Acceleration        = a.Acceleration + b.Acceleration,
                    AccelerationCurve   = a.AccelerationCurve + b.AccelerationCurve,
                    Braking             = a.Braking + b.Braking,
                    CoastingDrag        = a.CoastingDrag + b.CoastingDrag,
                    AddedGravity        = a.AddedGravity + b.AddedGravity,
                    Grip                = a.Grip + b.Grip,
                    ReverseAcceleration = a.ReverseAcceleration + b.ReverseAcceleration,
                    ReverseSpeed        = a.ReverseSpeed + b.ReverseSpeed,
                    TopSpeed            = a.TopSpeed + b.TopSpeed,
                    Steer               = a.Steer + b.Steer,
                };
            }
        }

        public Rigidbody Rigidbody { get; private set; }
        public InputData Input     { get; private set; }
        public float AirPercent    { get; private set; }
        public float GroundPercent { get; private set; }

//Base stats for Local Kart (non-networked) and Remote Karts (networked). 
        public ArcadeKart.Stats baseStats = new ArcadeKart.Stats
        {
            TopSpeed = 16f,              // Fast top speed for exciting racing
            Acceleration = 5f,           // Strong but not instant - feels powerful
            AccelerationCurve = 0.7f,    // Gradual power curve (valid range 0.2-1.0)
            Braking = 10f,               // Reduced braking so speed stays above threshold during drift initiation
            ReverseAcceleration = 4f,    // Slower reverse - realistic
            ReverseSpeed = 8f,           // Reasonable reverse speed
            Steer = 5.5f,                  // Slightly reduced - heavy cars turn slower
            CoastingDrag = 1.5f,         // Low drag - maintains momentum like a heavy car
            Grip = 0.80f,                // Reduced grip - allows sliding, feels weighty
            AddedGravity = 12f,           // More gravity - kart feels planted and heavy
        };

        [Header("Vehicle Visual")] 
        public List<GameObject> m_VisualWheels;

        [Header("Vehicle Physics")]
        [Tooltip("The transform that determines the position of the kart's mass.")]
        public Transform CenterOfMass;

        [Range(0.0f, 20.0f), Tooltip("Coefficient used to reorient the kart in the air. The higher the number, the faster the kart will readjust itself along the horizontal plane.")]
        public float AirborneReorientationCoefficient = 3.0f;

        [Header("Drifting")]
        [Range(0.01f, 1.0f), Tooltip("The grip value when drifting.")]
        public float DriftGrip = 0.50f; // Tighter, more responsive drifts
        [Range(0.0f, 10.0f), Tooltip("Additional steer when the kart is drifting.")]
        public float DriftAdditionalSteer = 3.5f; // Reduced for a controlled slide entry
        [Range(1.0f, 30.0f), Tooltip("The higher the angle, the easier it is to regain full grip.")]
        public float MinAngleToFinishDrift = 10.0f;
        [Range(0.01f, 0.99f), Tooltip("Mininum speed percentage to switch back to full grip.")]
        public float MinSpeedPercentToFinishDrift = 0.3f;
        [Range(1.0f, 20.0f), Tooltip("The higher the value, the easier it is to control the drift steering.")]
        public float DriftControl = 14.0f;
        [Range(0.0f, 20.0f), Tooltip("The lower the value, the longer the drift will last without trying to control it by steering.")]
        public float DriftDampening = 10.0f;
        [Range(0.5f, 0.99f), Tooltip("Speed cap as fraction of max speed while drifting (e.g. 0.85 = 85%).")]
        public float DriftSpeedFraction = 0.85f;

        [Header("VFX")]
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public ParticleSystem DriftSparkVFX;
        [Range(0.0f, 0.2f), Tooltip("Offset to displace the VFX to the side.")]
        public float DriftSparkHorizontalOffset = 0.1f;
        [Range(0.0f, 90.0f), Tooltip("Angle to rotate the VFX.")]
        public float DriftSparkRotation = 17.0f;
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public GameObject DriftTrailPrefab;
        [Range(-0.1f, 0.1f), Tooltip("Vertical to move the trails up or down and ensure they are above the ground.")]
        public float DriftTrailVerticalOffset;
        [Tooltip("VFX that will spawn upon landing, after a jump.")]
        public GameObject JumpVFX;
        [Tooltip("VFX that is spawn on the nozzles of the kart.")]
        public GameObject NozzleVFX;
        [Tooltip("List of the kart's nozzles.")]
        public List<Transform> Nozzles;

        [Header("Suspensions")]
        [Tooltip("The maximum extension possible between the kart's body and the wheels.")]
        [Range(0.0f, 1.0f)]
        public float SuspensionHeight = 0.2f;
        [Range(10.0f, 100000.0f), Tooltip("The higher the value, the stiffer the suspension will be.")]
        public float SuspensionSpring = 30000.0f;
        [Range(0.0f, 5000.0f), Tooltip("The higher the value, the faster the kart will stabilize itself.")]
        public float SuspensionDamp = 2000.0f;
        [Tooltip("Vertical offset to adjust the position of the wheels relative to the kart's body.")]
        [Range(-1.0f, 1.0f)]
        public float WheelsPositionVerticalOffset = 0.0f;

        [Header("Physical Wheels")]
        [Tooltip("The physical representations of the Kart's wheels.")]
        public WheelCollider FrontLeftWheel;
        public WheelCollider FrontRightWheel;
        public WheelCollider RearLeftWheel;
        public WheelCollider RearRightWheel;

        [Tooltip("Which layers the wheels will detect.")]
        public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

        [Header("Drift Visual")]
        [Tooltip("The kart body transform to tilt during drift. Assign the visual body child object.")]
        public Transform kartBodyTransform;
        [Range(0f, 15f), Tooltip("Maximum body lean angle in degrees during drift.")]
        public float driftLeanAngle = 8f;
        [Range(1f, 20f), Tooltip("How quickly the body lean interpolates.")]
        public float driftLeanSpeed = 8f;
        float m_CurrentLean = 0f;

        IInput[] m_Inputs;

        const float k_NullInput = 0.01f;
        const float k_NullSpeed = 0.01f;
        Vector3 m_VerticalReference = Vector3.up;

        
        public bool WantsToDrift { get; private set; } = false;
        public bool IsDrifting { get; private set; } = false;
        float m_CurrentGrip = 1.0f;
        float m_DriftTurningPower = 0.0f;
        float m_PreviousGroundPercent = 1.0f;
        float m_DriftDuration = 0f;
        readonly List<(GameObject trailRoot, WheelCollider wheel, TrailRenderer trail)> m_DriftTrailInstances = new List<(GameObject, WheelCollider, TrailRenderer)>();
        readonly List<(WheelCollider wheel, float horizontalOffset, float rotation, ParticleSystem sparks)> m_DriftSparkInstances = new List<(WheelCollider, float, float, ParticleSystem)>();

       
    bool m_CanMove = true;
    List<StatPowerups> m_ActivePowerupList = new List<StatPowerups>();
    ArcadeKart.Stats m_FinalStats;

        Quaternion m_LastValidRotation;
        Vector3 m_LastValidPosition;
        Vector3 m_LastCollisionNormal;
        bool m_HasCollision;
        bool m_InAir = false;

        public void AddPowerup(StatPowerups statPowerup) => m_ActivePowerupList.Add(statPowerup);
        public void SetCanMove(bool move) => m_CanMove = move;
        public bool CanMove => m_CanMove;
        public List<StatPowerups> ActivePowerups => m_ActivePowerupList;
        public bool IsHexActive => m_HexActive;
        public float HexTimeRemaining => Mathf.Max(0f, m_HexEndTime - Time.time);
        public float GetMaxSpeed() => Mathf.Max(m_FinalStats.TopSpeed, m_FinalStats.ReverseSpeed);

        bool m_HexActive = false;
        float m_HexEndTime = 0f;

        public void ApplyHex(float duration)
{
        m_HexActive = true;
        m_HexEndTime = Time.time + duration;
}
        private void ActivateDriftVFX(bool active)
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                bool isGrounded = vfx.wheel.isGrounded || vfx.wheel.GetGroundHit(out WheelHit hit);
                if (active && isGrounded)
                {
                    if (!vfx.sparks.isPlaying)
                        vfx.sparks.Play();
                }
                else
                {
                    if (vfx.sparks.isPlaying)
                        vfx.sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
                    
            }

            foreach (var trail in m_DriftTrailInstances)
            {
                bool isGrounded = trail.wheel.isGrounded || trail.wheel.GetGroundHit(out WheelHit hit);
                trail.Item3.emitting = active && isGrounded;
            }
        }

        private void UpdateDriftVFXOrientation()
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                vfx.sparks.transform.position = vfx.wheel.transform.position - (vfx.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up) + (transform.right * vfx.horizontalOffset);
                vfx.sparks.transform.rotation = transform.rotation * Quaternion.Euler(0.0f, 0.0f, vfx.rotation);
            }

            foreach (var trail in m_DriftTrailInstances)
            {
                trail.trailRoot.transform.position = trail.wheel.transform.position - (trail.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up);
                trail.trailRoot.transform.rotation = transform.rotation;
            }
        }

        void UpdateSuspensionParams(WheelCollider wheel)
        {
            wheel.suspensionDistance = SuspensionHeight;
            wheel.center = new Vector3(0.0f, WheelsPositionVerticalOffset, 0.0f);
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = SuspensionSpring;
            spring.damper = SuspensionDamp;
            wheel.suspensionSpring = spring;
        }

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            m_Inputs = GetComponents<IInput>();

            // Sanitize drift parameters at runtime to prevent deadlock
            DriftGrip = Mathf.Clamp(DriftGrip, 0.1f, 1.0f);
            MinSpeedPercentToFinishDrift = Mathf.Clamp(MinSpeedPercentToFinishDrift, 0.05f, 0.8f);
            DriftSpeedFraction = Mathf.Clamp(DriftSpeedFraction, 0.5f, 0.99f);
            if (MinSpeedPercentToFinishDrift >= DriftSpeedFraction)
            {
                MinSpeedPercentToFinishDrift = DriftSpeedFraction - 0.05f;
            }

            // Auto-assign the kart body transform if it is null, the root transform, or referencing an external prefab asset
            bool isInvalid = kartBodyTransform == null || 
                             kartBodyTransform == transform || 
                             !kartBodyTransform.IsChildOf(transform);

            if (isInvalid)
            {
                Transform visualRoot = transform.Find("KartVisual/Kart");
                if (visualRoot == null)
                    visualRoot = transform.Find("KartVisual");
                if (visualRoot == null)
                    visualRoot = transform.Find("Kart");
                
                if (visualRoot != null)
                {
                    kartBodyTransform = visualRoot;
                    Debug.Log($"[ArcadeKart] Automatically resolved kartBodyTransform to child: {visualRoot.name}");
                }
                else
                {
                    kartBodyTransform = null;
                }
            }

            UpdateSuspensionParams(FrontLeftWheel);
            UpdateSuspensionParams(FrontRightWheel);
            UpdateSuspensionParams(RearLeftWheel);
            UpdateSuspensionParams(RearRightWheel);

            m_CurrentGrip = baseStats.Grip;

            if (DriftSparkVFX != null)
            {
                AddSparkToWheel(RearLeftWheel, -DriftSparkHorizontalOffset, -DriftSparkRotation);
                AddSparkToWheel(RearRightWheel, DriftSparkHorizontalOffset, DriftSparkRotation);
            }

            if (DriftTrailPrefab != null)
            {
                AddTrailToWheel(RearLeftWheel);
                AddTrailToWheel(RearRightWheel);
            }

            if (NozzleVFX != null)
            {
                foreach (var nozzle in Nozzles)
                {
                    Instantiate(NozzleVFX, nozzle, false);
                }
            }
        }

void Start()
{
    
    if (gameObject.CompareTag("Player") || gameObject.CompareTag("Human"))
    {
        // Reduce speed for human players
        baseStats.TopSpeed *= 0.85f;
        baseStats.Acceleration *= 0.9f;
    }
}
        void AddTrailToWheel(WheelCollider wheel)
        {
            GameObject trailRoot = Instantiate(DriftTrailPrefab, gameObject.transform, false);
            TrailRenderer trail = trailRoot.GetComponentInChildren<TrailRenderer>();
            trail.emitting = false;
            m_DriftTrailInstances.Add((trailRoot, wheel, trail));
        }

        void AddSparkToWheel(WheelCollider wheel, float horizontalOffset, float rotation)
        {
            GameObject vfx = Instantiate(DriftSparkVFX.gameObject, wheel.transform, false);
            ParticleSystem spark = vfx.GetComponent<ParticleSystem>();
            spark.Stop();
            m_DriftSparkInstances.Add((wheel, horizontalOffset, -rotation, spark));
        }

        private void Update()
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned && !netObj.IsOwner)
            {
                // Sync drift VFX for remote karts based on the Networked anim state
                var animState = GetComponent<NetworkedKartAnimState>();
                if (animState != null)
                {
                    ActivateDriftVFX(animState.Drifting.Value);
                }
                UpdateDriftVFXOrientation();
            }
        }

        void FixedUpdate()
        {
            // Check if we're a remote client (not owner)
    NetworkObject netObj = GetComponent<NetworkObject>();
    if (netObj != null && netObj.IsSpawned && !netObj.IsOwner)
    {
        // Remote client - don't apply physics, just let Network Transform sync position
        return;
    }
            UpdateSuspensionParams(FrontLeftWheel);
            UpdateSuspensionParams(FrontRightWheel);
            UpdateSuspensionParams(RearLeftWheel);
            UpdateSuspensionParams(RearRightWheel);

            GatherInputs();

          
            TickPowerups();

            
            Rigidbody.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);

            int groundedCount = 0;
            if (FrontLeftWheel.isGrounded && FrontLeftWheel.GetGroundHit(out WheelHit hit))
                groundedCount++;
            if (FrontRightWheel.isGrounded && FrontRightWheel.GetGroundHit(out hit))
                groundedCount++;
            if (RearLeftWheel.isGrounded && RearLeftWheel.GetGroundHit(out hit))
                groundedCount++;
            if (RearRightWheel.isGrounded && RearRightWheel.GetGroundHit(out hit))
                groundedCount++;

            
            GroundPercent = (float) groundedCount / 4.0f;
            AirPercent = 1 - GroundPercent;

            
            if (m_CanMove)
            {
                MoveVehicle(Input.Accelerate, Input.Brake, Input.TurnInput);
            }
            GroundAirbourne();

            m_PreviousGroundPercent = GroundPercent;

            UpdateDriftVFXOrientation();
        }

        void GatherInputs()
{
    InputData currentInput = new InputData();
    WantsToDrift = false;

    if (m_Inputs == null) return;
    
    for (int i = 0; i < m_Inputs.Length; i++)
    {
        if (m_Inputs[i] != null)
        {
            currentInput = m_Inputs[i].GenerateInput();
        }
    }

    if (m_HexActive)
    {
        if (Time.time >= m_HexEndTime)
        {
            m_HexActive = false;
        }
        else
        {
            bool oldAccelerate = currentInput.Accelerate;
            currentInput.Accelerate = currentInput.Brake;
            currentInput.Brake = oldAccelerate;
            currentInput.TurnInput *= -1f;
        }
    }

    Input = currentInput;
    WantsToDrift = Input.Brake && Vector3.Dot(Rigidbody.velocity, transform.forward) > 0.0f;
}

        void TickPowerups()
        {
            
            m_ActivePowerupList.RemoveAll((p) => { return p.ElapsedTime > p.MaxTime; });

           
            var powerups = new Stats();

            
            for (int i = 0; i < m_ActivePowerupList.Count; i++)
            {
                var p = m_ActivePowerupList[i];

          
                p.ElapsedTime += Time.fixedDeltaTime;

                
                powerups += p.modifiers;
            }

          
            m_FinalStats = baseStats + powerups;

           
            m_FinalStats.Grip = Mathf.Clamp(m_FinalStats.Grip, 0, 1);
        }

        void GroundAirbourne()
        {
           
            if (AirPercent >= 0.25f)
            {
                Rigidbody.velocity += Physics.gravity * Time.fixedDeltaTime * m_FinalStats.AddedGravity;
            }
        }

        public void ApplyLaunch(Vector3 force)
{
    Rigidbody.AddForce(force, ForceMode.VelocityChange);
}


        public void Reset()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = euler.z = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }

        public float LocalSpeed()
        {
            if (m_CanMove)
            {
                float dot = Vector3.Dot(transform.forward, Rigidbody.velocity);
                if (Mathf.Abs(dot) > 0.1f)
                {
                    float speed = Rigidbody.velocity.magnitude;
                    return dot < 0 ? -(speed / m_FinalStats.ReverseSpeed) : (speed / m_FinalStats.TopSpeed);
                }
                return 0f;
            }
            else
            {
              
                return Input.Accelerate ? 1.0f : 0.0f;
            }
        }

        void OnCollisionEnter(Collision collision) => m_HasCollision = true;
        void OnCollisionExit(Collision collision) => m_HasCollision = false;

        void OnCollisionStay(Collision collision)
        {
            m_HasCollision = true;
            m_LastCollisionNormal = Vector3.zero;
            float dot = -1.0f;

            foreach (var contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > dot)
                    m_LastCollisionNormal = contact.normal;
            }
        }

        void MoveVehicle(bool accelerate, bool brake, float turnInput)
        {
            float accelInput = (accelerate ? 1.0f : 0.0f) - (brake ? 1.0f : 0.0f);

            
            float accelerationCurveCoeff = 5;
            Vector3 localVel = transform.InverseTransformVector(Rigidbody.velocity);

            bool accelDirectionIsFwd = accelInput >= 0;
            bool localVelDirectionIsFwd = localVel.z >= 0;

           
            float maxSpeed = localVelDirectionIsFwd ? m_FinalStats.TopSpeed : m_FinalStats.ReverseSpeed;
            float accelPower = accelDirectionIsFwd ? m_FinalStats.Acceleration : m_FinalStats.ReverseAcceleration;

            float currentSpeed = Rigidbody.velocity.magnitude;
            float accelRampT = currentSpeed / maxSpeed;
            float multipliedAccelerationCurve = m_FinalStats.AccelerationCurve * accelerationCurveCoeff;
            float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

            bool isBraking = (localVelDirectionIsFwd && brake) || (!localVelDirectionIsFwd && accelerate);

     
            float finalAccelPower = isBraking ? m_FinalStats.Braking : accelPower;

            float finalAcceleration = finalAccelPower * accelRamp;

          
            float turningPower = IsDrifting ? m_DriftTurningPower : turnInput * m_FinalStats.Steer;

            Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
            Vector3 fwd = turnAngle * transform.forward;
            Vector3 movement = fwd * accelInput * finalAcceleration * ((m_HasCollision || GroundPercent > 0.0f) ? 1.0f : 0.0f);

          
            bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

           
            if (wasOverMaxSpeed && !isBraking) 
                movement *= 0.0f;

            Vector3 newVelocity = Rigidbody.velocity + movement * Time.fixedDeltaTime;
            newVelocity.y = Rigidbody.velocity.y;

           
            if (GroundPercent > 0.0f && !wasOverMaxSpeed)
            {
                newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
            }

           
            if (Mathf.Abs(accelInput) < k_NullInput && GroundPercent > 0.0f)
            {
                newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, Rigidbody.velocity.y, 0), Time.fixedDeltaTime * m_FinalStats.CoastingDrag);
            }

            if (!Rigidbody.isKinematic)
            {
                Rigidbody.velocity = newVelocity;
            }

          
            if (GroundPercent > 0.0f)
            {
                if (m_InAir)
                {
                    m_InAir = false;
                    Instantiate(JumpVFX, transform.position, Quaternion.identity);
                }

                
                float angularVelocitySteering = 0.4f;
                float angularVelocitySmoothSpeed = 20f;

                if (!localVelDirectionIsFwd && !accelDirectionIsFwd) 
                    angularVelocitySteering *= -1.0f;

                // FIX: Prevent turning on the spot when idle. Require at least 1 m/s for full steering.
                float speedFactor = Mathf.Clamp01(currentSpeed / 1f);
                float effectiveTurningPower = turningPower * speedFactor;

                var angularVel = Rigidbody.angularVelocity;

                angularVel.y = Mathf.MoveTowards(angularVel.y, effectiveTurningPower * angularVelocitySteering, Time.fixedDeltaTime * angularVelocitySmoothSpeed);

                if (!Rigidbody.isKinematic)
                {
                    Rigidbody.angularVelocity = angularVel;
                }

                float velocitySteering = 25f;

               
                if (GroundPercent >= 0.0f && m_PreviousGroundPercent < 0.1f)
                {
                    Vector3 flattenVelocity = Vector3.ProjectOnPlane(Rigidbody.velocity, m_VerticalReference).normalized;
                    if (Vector3.Dot(flattenVelocity, transform.forward * Mathf.Sign(accelInput)) < Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad))
                    {
                        IsDrifting = true;
                        m_CurrentGrip = DriftGrip;
                        m_DriftTurningPower = 0.0f;
                    }
                }

           
                if (!IsDrifting)
                {
                    if ((WantsToDrift || isBraking) && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                    {
                        IsDrifting = true;
                        m_DriftTurningPower = turningPower + (Mathf.Sign(turningPower) * DriftAdditionalSteer);
                        m_CurrentGrip = DriftGrip;

                        ActivateDriftVFX(true);
                    }
                }

                if (IsDrifting)
                {
                    m_DriftDuration += Time.fixedDeltaTime;

                    // Cap speed during drift to prevent the kart feeling too fast while sliding
                    float driftMaxSpeed = maxSpeed * DriftSpeedFraction;
                    if (currentSpeed > driftMaxSpeed)
                    {
                        Rigidbody.velocity = Rigidbody.velocity.normalized * driftMaxSpeed;
                    }

                    // Dynamic color feedback for Mini-Turbo stages
                    Color feedbackColor = new Color(0f, 0.8f, 1f); // Cyan (Stage 0)
                    if (m_DriftDuration >= 2.0f)
                    {
                        feedbackColor = Color.red; // Super Mini-Turbo (Stage 2)
                    }
                    else if (m_DriftDuration >= 1.0f)
                    {
                        feedbackColor = new Color(1f, 0.5f, 0f); // Orange (Stage 1)
                    }

                    foreach (var vfx in m_DriftSparkInstances)
                    {
                        var main = vfx.sparks.main;
                        main.startColor = feedbackColor;

                        // Propagate the color stage to all sub-particle systems (Sparks, Beam, Smoke)
                        var children = vfx.sparks.GetComponentsInChildren<ParticleSystem>();
                        foreach (var child in children)
                        {
                            var childMain = child.main;
                            childMain.startColor = feedbackColor;
                        }
                    }

                    foreach (var trail in m_DriftTrailInstances)
                    {
                        trail.trail.startColor = feedbackColor;
                        trail.trail.endColor = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, 0f);
                    }

                    float turnInputAbs = Mathf.Abs(turnInput);
                    if (turnInputAbs < k_NullInput)
                        m_DriftTurningPower = Mathf.MoveTowards(m_DriftTurningPower, 0.0f, Mathf.Clamp01(DriftDampening * Time.fixedDeltaTime));

                    float driftMaxSteerValue = m_FinalStats.Steer + DriftAdditionalSteer;
                    m_DriftTurningPower = Mathf.Clamp(m_DriftTurningPower + (turnInput * Mathf.Clamp01(DriftControl * Time.fixedDeltaTime)), -driftMaxSteerValue, driftMaxSteerValue);

                    bool facingVelocity = Vector3.Dot(Rigidbody.velocity.normalized, transform.forward * Mathf.Sign(accelInput)) > Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad);

                    bool canEndDrift = true;
                    if (isBraking)
                        canEndDrift = false;
                    else if (!facingVelocity)
                        canEndDrift = false;
                    else if (turnInputAbs >= k_NullInput)
                        canEndDrift = false;

                    if (canEndDrift || currentSpeed < k_NullSpeed)
                    {
                        // Apply Mini-Turbo boost upon successful drift completion (5-10% speed increase)
                        if (m_DriftDuration >= 1.0f)
                        {
                            float boostDuration = m_DriftDuration >= 2.0f ? 1.2f : 0.6f;
                            // Stage 1: ~5% of TopSpeed, Stage 2: ~10% of TopSpeed
                            float speedBoost = m_DriftDuration >= 2.0f ? (maxSpeed * 0.10f) : (maxSpeed * 0.05f);

                            var miniTurbo = new ArcadeKart.StatPowerups
                            {
                                PowerUpID = "DriftMiniTurbo",
                                MaxTime = boostDuration,
                                ElapsedTime = 0f,
                                modifiers = new ArcadeKart.Stats
                                {
                                    TopSpeed = speedBoost,
                                    Acceleration = 1.5f
                                }
                            };
                            AddPowerup(miniTurbo);
                        }

                        IsDrifting = false;
                        m_CurrentGrip = m_FinalStats.Grip;
                        m_DriftDuration = 0f;
                    }

                }

                
                if (!Rigidbody.isKinematic)
                {
                    Rigidbody.velocity = Quaternion.AngleAxis(turningPower * Mathf.Sign(localVel.z) * velocitySteering * m_CurrentGrip * Time.fixedDeltaTime, transform.up) * Rigidbody.velocity;
                }
            }
            else
            {
                m_InAir = true;
                if (IsDrifting)
                {
                    IsDrifting = false;
                    m_CurrentGrip = m_FinalStats.Grip;
                    m_DriftDuration = 0f;
                }
            }

            bool validPosition = false;
            if (Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out RaycastHit hit, 3.0f, 1 << 9 | 1 << 10 | 1 << 11)) 
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > hit.normal.y) ? m_LastCollisionNormal : hit.normal;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime * (GroundPercent > 0.0f ? 10.0f : 1.0f)));  
            }
            else
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > 0.0f) ? m_LastCollisionNormal : Vector3.up;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime));
            }

            validPosition = GroundPercent > 0.7f && !m_HasCollision && Vector3.Dot(m_VerticalReference, Vector3.up) > 0.9f;

           
            if (GroundPercent < 0.7f)
            {
                Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * 0.98f, 0.0f);
                Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, m_VerticalReference);
                finalOrientationDirection.Normalize();
                if (finalOrientationDirection.sqrMagnitude > 0.0f)
                {
                    Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, m_VerticalReference), Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime)));
                }
            }
            else if (validPosition)
            {
                m_LastValidPosition = transform.position;
                m_LastValidRotation.eulerAngles = new Vector3(0.0f, transform.rotation.y, 0.0f);
            }

            ActivateDriftVFX(IsDrifting && GroundPercent > 0.0f);

            // Visual body lean during drift
            if (kartBodyTransform != null)
            {
                float targetLean = 0f;
                if (IsDrifting && GroundPercent > 0.0f)
                {
                    // Lean into the drift direction (negative of drift turning = lean inward)
                    targetLean = -Mathf.Sign(m_DriftTurningPower) * driftLeanAngle;
                }
                m_CurrentLean = Mathf.MoveTowards(m_CurrentLean, targetLean, driftLeanSpeed * driftLeanAngle * Time.fixedDeltaTime);
                kartBodyTransform.localRotation = Quaternion.Euler(0f, 0f, m_CurrentLean);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // Sanitize drift parameters in the editor to prevent deadlock
            DriftGrip = Mathf.Clamp(DriftGrip, 0.1f, 1.0f);
            MinSpeedPercentToFinishDrift = Mathf.Clamp(MinSpeedPercentToFinishDrift, 0.05f, 0.8f);
            DriftSpeedFraction = Mathf.Clamp(DriftSpeedFraction, 0.5f, 0.99f);
            if (MinSpeedPercentToFinishDrift >= DriftSpeedFraction)
            {
                MinSpeedPercentToFinishDrift = DriftSpeedFraction - 0.05f;
            }
        }
#endif
    }
}
