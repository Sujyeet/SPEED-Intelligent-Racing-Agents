using UnityEngine;
using KartGame.KartSystems;

public class PauseDebugger : MonoBehaviour
{
    private ArcadeKart m_Kart;
    private Rigidbody m_Rb;

    private float m_LastTimeScale = 1f;
    private float m_LastFixedDeltaTime = 0.02f;

    void Start()
    {
        m_Kart = GetComponent<ArcadeKart>();
        m_Rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Detect resume moment (timeScale going from 0 back to 1)
        if (m_LastTimeScale == 0f && Time.timeScale == 1f)
        {
            Debug.Log("=== RESUMED ===");
            Debug.Log("velocity: " + m_Rb.velocity);
            Debug.Log("angularVelocity: " + m_Rb.angularVelocity);
            Debug.Log("isKinematic: " + m_Rb.isKinematic);
            Debug.Log("useGravity: " + m_Rb.useGravity);
            Debug.Log("drag: " + m_Rb.drag);
            Debug.Log("angularDrag: " + m_Rb.angularDrag);
            Debug.Log("fixedDeltaTime: " + Time.fixedDeltaTime);
            Debug.Log("GroundPercent: " + m_Kart.GroundPercent);
            Debug.Log("AirPercent: " + m_Kart.AirPercent);
        }

        // Detect pause moment
        if (m_LastTimeScale == 1f && Time.timeScale == 0f)
        {
            Debug.Log("=== PAUSED ===");
            Debug.Log("velocity: " + m_Rb.velocity);
            Debug.Log("angularVelocity: " + m_Rb.angularVelocity);
            Debug.Log("isKinematic: " + m_Rb.isKinematic);
            Debug.Log("useGravity: " + m_Rb.useGravity);
            Debug.Log("drag: " + m_Rb.drag);
            Debug.Log("angularDrag: " + m_Rb.angularDrag);
            Debug.Log("fixedDeltaTime: " + Time.fixedDeltaTime);
            Debug.Log("GroundPercent: " + m_Kart.GroundPercent);
            Debug.Log("AirPercent: " + m_Kart.AirPercent);
        }

        m_LastTimeScale = Time.timeScale;
        m_LastFixedDeltaTime = Time.fixedDeltaTime;
    }

    // Log every fixed frame for 2 seconds after resume
    private bool m_Logging = false;
    private float m_LogUntil = 0f;
    private int m_FrameCount = 0;

    void FixedUpdate()
    {
        if (m_LastTimeScale == 0f && Time.timeScale == 1f)
        {
            m_Logging = true;
            m_LogUntil = Time.time + 2f;
            m_FrameCount = 0;
        }

        if (m_Logging && Time.time < m_LogUntil)
        {
            m_FrameCount++;
            if (m_FrameCount <= 10) // first 10 physics frames only
            {
                Debug.Log($"[Frame {m_FrameCount}] speed={m_Rb.velocity.magnitude:F2} " +
                          $"ground={m_Kart.GroundPercent:F2} " +
                          $"air={m_Kart.AirPercent:F2} " +
                          $"fixedDT={Time.fixedDeltaTime:F4}");
            }
        }
    }
}