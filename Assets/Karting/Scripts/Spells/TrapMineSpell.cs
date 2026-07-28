using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class TrapMineSpell : BaseSpell
    {
        [Header("Trap Mine Settings")]
        public GameObject minePrefab;
        public float spawnBehindDistance = 2f;
        public float invisibleDelay = 2f;

        protected override void OnCast(ArcadeKart kart)
        {
            if (minePrefab == null)
            {
                Debug.LogWarning("TrapMineSpell: No mine prefab assigned!");
                return;
            }

            // Calculate spawn position behind the kart
            Vector3 spawnPos = kart.transform.position - kart.transform.forward * spawnBehindDistance;
            Quaternion flatRot = Quaternion.Euler(-90f, kart.transform.eulerAngles.y, 0f);

            // If we are in a multiplayer game, we must NOT spawn it locally.
            // The ServerRpc will spawn it on the server and replicate it to all clients automatically.
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                TrapMineNetworkHelper helper = kart.GetComponent<TrapMineNetworkHelper>();
                if (helper != null)
                {
                    helper.SpawnMineServerRpc(spawnPos, flatRot);
                }
            }
            else
            {
                // Single-player offline fallback
                Instantiate(minePrefab, spawnPos, flatRot);
            }
        }
    }
}