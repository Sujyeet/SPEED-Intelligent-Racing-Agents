using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class ProjectileSpell : BaseSpell
    {
        [Header("Launch Trajectory")]
        [Tooltip("How much to tilt the projectile upwards (negative means pitch up in Unity)")]
        public float upwardPitch = -2f;
        public float forwardOffset = 2f;
        public float upwardOffset = 0.6f;

        protected override void OnCast(ArcadeKart kart)
        {
            ProjectileNetworkHelper helper = kart.GetComponent<ProjectileNetworkHelper>();
            if (helper == null || helper.projectilePrefab == null)
            {
                Debug.LogWarning("ProjectileSpell: Missing helper or projectile prefab.");
                return;
            }

            Vector3 spawnPos = kart.transform.position + kart.transform.forward * forwardOffset + Vector3.up * upwardOffset;
            
            // Pitch the rotation upwards by the specified amount
            Quaternion spawnRot = kart.transform.rotation * Quaternion.Euler(upwardPitch, 0f, 0f);

            NetworkObject kartNetObj = kart.GetComponent<NetworkObject>();
            if (kartNetObj == null)
            {
                Debug.LogWarning("ProjectileSpell: Kart has no NetworkObject.");
                return;
            }

            helper.SpawnProjectileServerRpc(spawnPos, spawnRot, kartNetObj.NetworkObjectId);
        }
    }
}