using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class TrapMine : NetworkBehaviour
    {
        [Header("Mine Settings")]
        public float slowTopSpeedAmount = 8f;
        public float slowAccelerationAmount = 3f;
        public float slowDuration = 2f;
        public float invisibleDelay = 2f;
        public GameObject explosionVFX;

        private bool triggered = false;

        void Start()
        {
            StartCoroutine(GoInvisible());

            if (IsServer)
                StartCoroutine(AutoDestroy());
        }

        IEnumerator GoInvisible()
        {
            yield return new WaitForSeconds(invisibleDelay);

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = false;
        }

        IEnumerator AutoDestroy()
        {
            yield return new WaitForSeconds(15f);

            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.Despawn();
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log("[TrapMine] Trigger entered by: " + other.name + " | IsServer=" + IsServer);

            if (triggered) return;
            if (!IsServer) return;

            ArcadeKart kart = other.GetComponent<ArcadeKart>();
            if (kart == null)
                kart = other.GetComponentInParent<ArcadeKart>();

            if (kart == null)
            {
                Debug.Log("[TrapMine] No ArcadeKart found on collider or parent.");
                return;
            }

            triggered = true;

            NetworkObject kartNetObj = kart.GetComponent<NetworkObject>();
            if (kartNetObj == null)
                kartNetObj = kart.GetComponentInParent<NetworkObject>();

            if (kartNetObj != null)
            {
                ulong kartOwnerId = kartNetObj.OwnerClientId;
                Debug.Log("[TrapMine] Networked kart found. OwnerClientId = " + kartOwnerId);

                ApplySlowClientRpc(kartOwnerId);
            }
            else
            {
                ApplySlowToKart(kart);
                Debug.Log("[TrapMine] No NetworkObject found, slow applied directly to hit kart.");
            }

            if (explosionVFX != null)
            {
                SpawnExplosionClientRpc(transform.position);
                Debug.Log("[TrapMine] Explosion VFX requested on all clients.");
            }
            else
            {
                Debug.LogWarning("[TrapMine] explosionVFX is null.");
            }

            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.Despawn();
        }

        void ApplySlowToKart(ArcadeKart kart)
        {
            var slow = new ArcadeKart.StatPowerups
            {
                PowerUpID = "TrapMineSlow",
                ElapsedTime = 0f,
                MaxTime = slowDuration,
                modifiers = new ArcadeKart.Stats
                {
                    TopSpeed = -slowTopSpeedAmount,
                    Acceleration = -slowAccelerationAmount
                }
            };

            kart.AddPowerup(slow);
        }

        [ClientRpc]
        void ApplySlowClientRpc(ulong targetClientId)
        {
            Debug.Log("[TrapMine] ApplySlowClientRpc received on client " +
                      NetworkManager.Singleton.LocalClientId +
                      " target = " + targetClientId);

            if (NetworkManager.Singleton.LocalClientId != targetClientId)
                return;

            foreach (var netObj in FindObjectsOfType<NetworkObject>())
            {
                if (netObj.OwnerClientId == targetClientId)
                {
                    ArcadeKart kart = netObj.GetComponent<ArcadeKart>();
                    if (kart == null)
                        kart = netObj.GetComponentInChildren<ArcadeKart>();

                    if (kart != null)
                    {
                        ApplySlowToKart(kart);
                        Debug.Log("[TrapMine] Slow applied to networked kart owner!");
                        return;
                    }
                }
            }

            Debug.LogWarning("[TrapMine] Could not find local owned ArcadeKart to slow.");
        }

        [ClientRpc]
        void SpawnExplosionClientRpc(Vector3 spawnPosition)
        {
            if (explosionVFX != null)
                Instantiate(explosionVFX, spawnPosition, Quaternion.identity);
        }
    }
}