using UnityEngine;
using Unity.Netcode;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkedKartController : NetworkBehaviour
{
    [Header("Kart Settings")]
    public float acceleration = 30f;
    public float maxSpeed = 20f;
    public float turnSpeed = 100f;
    public float drag = 3f;
    public float angularDrag = 5f;

    [Header("Camera Settings")]
    public string virtualCameraTag = "CinemachineVirtualCamera";

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = drag;
        rb.angularDrag = angularDrag;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            AttachCamera();
        }
        else
        {
            rb.isKinematic = true;
        }
    }

    private void AttachCamera()
    {
        GameObject camObj = GameObject.FindGameObjectWithTag(virtualCameraTag);
        
        if (camObj == null)
        {
            var vCam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vCam != null) camObj = vCam.gameObject;
        }

        if (camObj != null)
        {
            var vCam = camObj.GetComponent<CinemachineVirtualCamera>();
            if (vCam != null)
            {
                vCam.Follow = this.transform;
                vCam.LookAt = this.transform;
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        MoveKart();
    }

    private void MoveKart()
    {
        // Get current speed
        float currentSpeed = Vector3.Dot(rb.velocity, transform.forward);

        // Apply acceleration only if under max speed
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            if (currentSpeed < maxSpeed && moveInput > 0) // Forward
            {
                rb.AddForce(transform.forward * moveInput * acceleration, ForceMode.Acceleration);
            }
            else if (moveInput < 0) // Backward (allow reverse)
            {
                rb.AddForce(transform.forward * moveInput * acceleration * 0.5f, ForceMode.Acceleration);
            }
        }

        // Apply turning only if moving
        if (Mathf.Abs(turnInput) > 0.1f && Mathf.Abs(currentSpeed) > 1f)
        {
            float turn = turnInput * turnSpeed * (currentSpeed / maxSpeed) * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }
}
