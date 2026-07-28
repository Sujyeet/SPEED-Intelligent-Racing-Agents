using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class ProjectileSpell : BaseSpell
    {
        protected override void OnCast(ArcadeKart kart)
        {
            ProjectileNetworkHelper helper = kart.GetComponent<ProjectileNetworkHelper>();
            if (helper == null || helper.projectilePrefab == null)
            {
                Debug.LogWarning("ProjectileSpell: Missing helper or projectile prefab.");
                return;
            }

            Vector3 spawnPos = kart.transform.position + kart.transform.forward * 2f + Vector3.up * 0.5f;
            Quaternion spawnRot = kart.transform.rotation;

            NetworkObject kartNetObj = kart.GetComponent<NetworkObject>();
            if (kartNetObj == null)
            {
                Debug.LogWarning("ProjectileSpell: Kart has no NetworkObject.");
                return;
            }

            helper.SpawnProjectileServerRpc(spawnPos, spawnRot, kartNetObj.OwnerClientId);
        }
    }
}