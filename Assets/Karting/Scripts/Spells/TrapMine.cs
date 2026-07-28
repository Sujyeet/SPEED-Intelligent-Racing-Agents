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

        private bool isArmed = false;
        private bool triggered = false;

        void Start()
        {
            StartCoroutine(ArmMine());
        }

        IEnumerator ArmMine()
        {
            // Wait 1.5 seconds before the mine can be triggered
            // This prevents the person who dropped it from instantly hitting it!
            yield return new WaitForSeconds(1.5f);
            isArmed = true;
        }

        void OnTriggerEnter(Collider other)
        {
            // Only trigger if it has had time to arm itself
            if (!isArmed) return;
            
            Debug.Log("[TrapMine] Trigger entered by: " + other.name + " | IsServer=" + IsServer);

            if (triggered) return;
            if (!IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) return;

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
                Debug.Log("[TrapMine] Networked kart found. NetworkObjectId = " + kartNetObj.NetworkObjectId);
                ApplySlowClientRpc(kartNetObj.NetworkObjectId);
            }
            else
            {
                ApplySlowToKart(kart);
                Debug.Log("[TrapMine] No NetworkObject found, slow applied directly to hit kart.");
            }

            if (explosionVFX != null)
            {
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                {
                    SpawnExplosionClientRpc(transform.position);
                    Debug.Log("[TrapMine] Explosion VFX requested on all clients.");
                }
                else
                {
                    GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
                    Destroy(vfx, 2f);
                }
            }
            else
            {
                Debug.LogWarning("[TrapMine] explosionVFX is null.");
            }

            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.Despawn();
            else
                Destroy(gameObject);
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
        void ApplySlowClientRpc(ulong targetNetworkObjectId)
        {
            NetworkObject targetNetObj = null;
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject spawnedObj))
                {
                    targetNetObj = spawnedObj;
                }
            }

            if (targetNetObj != null)
            {
                // Verify if we own this specific NetworkObject (player owns their kart, server owns AI karts)
                if (targetNetObj.IsOwner)
                {
                    ArcadeKart kart = targetNetObj.GetComponent<ArcadeKart>();
                    if (kart == null) kart = targetNetObj.GetComponentInChildren<ArcadeKart>();
                    if (kart != null)
                    {
                        ApplySlowToKart(kart);
                        Debug.Log("[TrapMine] Slow applied to target networked kart!");
                    }
                }
            }
        }

        [ClientRpc]
        void SpawnExplosionClientRpc(Vector3 spawnPosition)
        {
            if (explosionVFX != null)
            {
                GameObject vfx = Instantiate(explosionVFX, spawnPosition, Quaternion.identity);
                Destroy(vfx, 2f);
            }
        }
    }
}