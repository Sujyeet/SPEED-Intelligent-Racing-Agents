using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    // This must be NetworkBehaviour so it can send ServerRpc calls
    public class TrapMineNetworkHelper : NetworkBehaviour
    {
        public GameObject minePrefab; // Same prefab as TrapMineSpell — assign in Inspector

        // [ServerRpc] means: "client calls this, but it runs ON THE SERVER"
        // SenderId tells the server which client called it — used for ownership checks later
        [ServerRpc(RequireOwnership = false)]
        public void SpawnMineServerRpc(Vector3 position, Quaternion rotation)
        {
            // This code runs on the server only
            // Instantiate the mine — only server can spawn networked objects
            GameObject mine = Instantiate(minePrefab, position, rotation);

            // Get the NetworkObject component — every networked prefab needs this
            NetworkObject netObj = mine.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                // Spawn() tells the server to create this object on ALL clients
                // Everyone now sees the mine
                netObj.Spawn();

                // Pass the invisible delay to the mine script
                TrapMine mineScript = mine.GetComponent<TrapMine>();
                if (mineScript != null)
                    mineScript.invisibleDelay = 2f;
            }
        }
    }
}