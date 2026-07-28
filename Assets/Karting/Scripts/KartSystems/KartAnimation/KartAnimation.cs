using System;
using UnityEngine;

namespace KartGame.KartSystems
{
    [DefaultExecutionOrder(100)]
    public class KartAnimation : MonoBehaviour
    {
        [Serializable] public class Wheel
        {
            [Tooltip("A reference to the transform of the wheel.")]
            public Transform wheelTransform;
            [Tooltip("A reference to the WheelCollider of the wheel.")]
            public WheelCollider wheelCollider;
            [Tooltip("Mirror this wheel on the X axis (use for right-side wheels that show black).")]
            public bool flipX = false;
            
            Quaternion m_SteerlessLocalRotation;

            public void Setup() { if (wheelTransform != null) m_SteerlessLocalRotation = wheelTransform.localRotation; }
            public void StoreDefaultRotation() { if (wheelTransform != null) m_SteerlessLocalRotation = wheelTransform.localRotation; }
            public void SetToDefaultRotation() { if (wheelTransform != null) wheelTransform.localRotation = m_SteerlessLocalRotation; }
        }

        [Tooltip("What kart do we want to listen to?")]
        public ArcadeKart kartController;

        [Space]
        [Tooltip("The damping for the appearance of steering compared to the input.  The higher the number the less damping.")]
        public float steeringAnimationDamping = 10f;

        [Space]
        [Tooltip("The maximum angle in degrees that the front wheels can be turned away from their default positions, when the Steering input is either 1 or -1.")]
        public float maxSteeringAngle;
        [Tooltip("Information referring to the front left wheel of the kart.")]
        public Wheel frontLeftWheel;
        [Tooltip("Information referring to the front right wheel of the kart.")]
        public Wheel frontRightWheel;
        [Tooltip("Information referring to the rear left wheel of the kart.")]
        public Wheel rearLeftWheel;
        [Tooltip("Information referring to the rear right wheel of the kart.")]
        public Wheel rearRightWheel;

        float m_SmoothedSteeringInput;

        void Start()
        {
            frontLeftWheel?.Setup();
            frontRightWheel?.Setup();
            rearLeftWheel?.Setup();
            rearRightWheel?.Setup();
        }

        void FixedUpdate() 
        {
            if (kartController == null) return;

            m_SmoothedSteeringInput = Mathf.MoveTowards(m_SmoothedSteeringInput, kartController.Input.TurnInput, 
                steeringAnimationDamping * Time.deltaTime);

            // Steer front wheels
            float rotationAngle = m_SmoothedSteeringInput * maxSteeringAngle;

            if (frontLeftWheel?.wheelCollider != null) frontLeftWheel.wheelCollider.steerAngle = rotationAngle;
            if (frontRightWheel?.wheelCollider != null) frontRightWheel.wheelCollider.steerAngle = rotationAngle;

            // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(frontLeftWheel);
            UpdateWheelFromCollider(frontRightWheel);
            UpdateWheelFromCollider(rearLeftWheel);
            UpdateWheelFromCollider(rearRightWheel);
        }

        void LateUpdate()
        {
            // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(frontLeftWheel);
            UpdateWheelFromCollider(frontRightWheel);
            UpdateWheelFromCollider(rearLeftWheel);
            UpdateWheelFromCollider(rearRightWheel);
        }

        void UpdateWheelFromCollider(Wheel wheel)
        {
            if (wheel == null || wheel.wheelTransform == null || wheel.wheelCollider == null) return;

            wheel.wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheel.wheelTransform.position = position;
            wheel.wheelTransform.rotation = rotation;

            if (wheel.flipX)
            {
                Vector3 scale = wheel.wheelTransform.localScale;
                if (scale.x > 0)
                {
                    scale.x = -Mathf.Abs(scale.x);
                    wheel.wheelTransform.localScale = scale;
                }
            }
        }
    }
}
