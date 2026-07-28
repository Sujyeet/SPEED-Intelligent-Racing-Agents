using Unity.Netcode;
using UnityEngine;
using KartGame.KartSystems;
using Cinemachine;

public class NetworkedArcadeKart : NetworkBehaviour
{
    private ArcadeKart arcadeKart;
    private Rigidbody rb;
    //private KartAnimation kartAnimation;
    private KartPlayerAnimator playerAnimator;

    private void Awake()
    {
        arcadeKart = GetComponent<ArcadeKart>();
        rb = GetComponent<Rigidbody>();
        //kartAnimation = GetComponent<KartAnimation>();
        playerAnimator = GetComponentInChildren<KartPlayerAnimator>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log($"[NetworkedArcadeKart] I OWN this kart! ClientId: {OwnerClientId}");
            AttachCamera();
            rb.isKinematic = false;
        }
        else
        {
            Debug.Log($"[NetworkedArcadeKart] NOT my kart. Owner: {OwnerClientId}");
            rb.isKinematic = true;
        }
        
        // ALWAYS enable animation-related components on ALL clients
        if (arcadeKart != null) arcadeKart.enabled = true;
        //if (kartAnimation != null) kartAnimation.enabled = true;
        if (playerAnimator != null) playerAnimator.enabled = true;
    }

    private void AttachCamera()
    {
        var vCam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vCam != null)
        {
            vCam.Follow = this.transform;
            vCam.LookAt = this.transform;
            Debug.Log($"[NetworkedArcadeKart] Camera attached!");
        }
        else
        {
            Debug.LogWarning("[NetworkedArcadeKart] No CinemachineVirtualCamera found!");
        }
    }
}
