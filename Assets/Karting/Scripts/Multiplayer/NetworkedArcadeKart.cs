using Unity.Netcode;
using UnityEngine;
using KartGame.KartSystems;
using Cinemachine;

public class NetworkedArcadeKart : NetworkBehaviour
{
    private ArcadeKart arcadeKart;
    private Rigidbody rb;
    //private KartAnimation kartAnimation;
    //private KartPlayerAnimator playerAnimator;

    private void Awake()
    {
        arcadeKart = GetComponent<ArcadeKart>();
        rb = GetComponent<Rigidbody>();
        //kartAnimation = GetComponent<KartAnimation>();
        //playerAnimator = GetComponentInChildren<KartPlayerAnimator>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log($"[NetworkedArcadeKart] I OWN this kart! ClientId: {OwnerClientId}");
            rb.isKinematic = false;
            StartCoroutine(InitializePlayerKart());
        }
        else
        {
            Debug.Log($"[NetworkedArcadeKart] NOT my kart. Owner: {OwnerClientId}");
            rb.isKinematic = true;
        }
        
        // ALWAYS enable animation-related components on ALL clients
        if (arcadeKart != null) arcadeKart.enabled = true;
        //if (playerAnimator != null) playerAnimator.enabled = true;
    }

    private System.Collections.IEnumerator InitializePlayerKart()
    {
        // 1. ATTACH CAMERA
        CinemachineVirtualCamera vcam = null;
        while (vcam == null)
        {
            var cameras = FindObjectsOfType<CinemachineVirtualCamera>(true);
            foreach (var cam in cameras)
            {
                if (!cam.gameObject.name.Contains("RearView"))
                {
                    vcam = cam;
                    break;
                }
            }
            if (vcam == null) yield return new WaitForSeconds(0.1f);
        }
        
        Transform capsule = transform.Find("KartBouncingCapsule");
        if (capsule != null)
        {
            vcam.Follow = capsule;
            vcam.LookAt = capsule;
        }
        else
        {
            vcam.Follow = this.transform;
            vcam.LookAt = this.transform;
        }
        
        vcam.gameObject.SetActive(true);
        Debug.Log($"[NetworkedArcadeKart] Camera attached successfully!");

        // 2. FIND START LINE (Using Tag as suggested!)
        GameObject startLine = null;
        while (startLine == null)
        {
            startLine = GameObject.FindGameObjectWithTag("StartFinishLine");
            if (startLine == null) yield return new WaitForSeconds(0.1f);
        }

        // 3. POSITION KART
        Vector3 basePos = startLine.transform.position;
        Quaternion baseRot = startLine.transform.rotation;
        Vector3 forward = startLine.transform.forward;
        Vector3 right = startLine.transform.right;

        int index = (int)OwnerClientId;
        int row = index / 2;
        // Flip this so Host (index 0) spawns on the LEFT side!
        bool isLeft = (index % 2 == 0);

        Vector3 targetPos = basePos - forward * (row * 6f + 3f) + right * (isLeft ? -3f : 3f);
        
        targetPos.y += 15f;
        if (Physics.Raycast(targetPos, Vector3.down, out RaycastHit hit, 50f))
        {
            // Place exactly on the track surface (just 0.1m above to prevent clipping), no falling!
            targetPos = hit.point + Vector3.up * 0.1f;
        }
        
        transform.position = targetPos;
        transform.rotation = baseRot;
        rb.position = targetPos;
        rb.rotation = baseRot;
        rb.velocity = Vector3.zero;
        
        Debug.Log("[NetworkedArcadeKart] Kart spawned at StartFinishLine via Tag!");
    }
}
