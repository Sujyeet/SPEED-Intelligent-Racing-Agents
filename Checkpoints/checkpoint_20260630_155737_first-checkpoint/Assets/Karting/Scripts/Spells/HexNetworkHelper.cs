using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class HexNetworkHelper : NetworkBehaviour
    {
        private ArcadeKart arcadeKart;

        void Awake()
        {
            arcadeKart = GetComponent<ArcadeKart>();
        }

        public void RequestHex(float range, float duration)
        {
            if (!IsOwner)
                return;

            RequestHexServerRpc(range, duration);
        }

        [ServerRpc]
        private void RequestHexServerRpc(float range, float duration, ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;

            ArcadeKart bestTarget = null;
            float bestDistance = range;

            ArcadeKart[] allKarts = FindObjectsOfType<ArcadeKart>();

            foreach (ArcadeKart candidate in allKarts)
            {
                if (candidate == null)
                    continue;

                if (candidate == arcadeKart)
                    continue;

                NetworkObject candidateNetObj = candidate.GetComponent<NetworkObject>();

                if (candidateNetObj != null && candidateNetObj.IsSpawned && candidateNetObj.OwnerClientId == senderClientId)
                    continue;

                float distance = Vector3.Distance(transform.position, candidate.transform.position);

                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = candidate;
                }
            }

            if (bestTarget == null)
            {
                Debug.Log("Hex: No target in range.");
                return;
            }

            NetworkObject targetNetObj = bestTarget.GetComponent<NetworkObject>();

            // Human / networked player target
            if (targetNetObj != null && targetNetObj.IsSpawned)
            {
                HexNetworkHelper targetHelper = bestTarget.GetComponent<HexNetworkHelper>();

                if (targetHelper == null)
                {
                    Debug.LogWarning("Hex: Target player is missing HexNetworkHelper.");
                    return;
                }

                targetHelper.ApplyHexClientRpc(duration, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { targetNetObj.OwnerClientId }
                    }
                });

                Debug.Log($"Hex: sent to player target {bestTarget.name}, owner {targetNetObj.OwnerClientId}");
                return;
            }

            // AI / ML-Agent / local non-networked target
            bestTarget.ApplyHex(duration);
            Debug.Log($"Hex: applied locally to AI target {bestTarget.name}");
        }

        [ClientRpc]
        private void ApplyHexClientRpc(float duration, ClientRpcParams clientRpcParams = default)
        {
            if (arcadeKart == null)
                arcadeKart = GetComponent<ArcadeKart>();

            if (arcadeKart == null)
            {
                Debug.LogWarning("Hex: ArcadeKart missing on target.");
                return;
            }

            arcadeKart.ApplyHex(duration);
            Debug.Log($"Hex: effect applied on {gameObject.name}");
        }
    }
}