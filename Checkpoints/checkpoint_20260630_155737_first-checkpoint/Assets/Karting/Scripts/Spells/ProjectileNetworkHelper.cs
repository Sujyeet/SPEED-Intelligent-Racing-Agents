using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class ProjectileNetworkHelper : NetworkBehaviour
    {
        public GameObject projectilePrefab;

        [ServerRpc(RequireOwnership = false)]
        public void SpawnProjectileServerRpc(Vector3 position, Quaternion rotation, ulong shooterClientId)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("ProjectileNetworkHelper: projectilePrefab is null.");
                return;
            }

            GameObject projectile = Instantiate(projectilePrefab, position, rotation);

            NetworkObject netObj = projectile.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogWarning("ProjectileNetworkHelper: projectile prefab needs a NetworkObject.");
                Destroy(projectile);
                return;
            }

            SpellProjectile projectileScript = projectile.GetComponent<SpellProjectile>();
            if (projectileScript != null)
            {
                projectileScript.SetOwner(shooterClientId);
            }

            netObj.Spawn();
        }
    }
}