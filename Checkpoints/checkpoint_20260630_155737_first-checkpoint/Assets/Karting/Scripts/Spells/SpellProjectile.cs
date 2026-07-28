using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class SpellProjectile : NetworkBehaviour
    {
        [Header("Movement")]
        public float speed = 24f;
        public float lifetime = 4f;

        [Header("Launch Effect")]
        public float upwardForce = 8f;
        public float forwardForce = 10f;
        public float sideForce = 0f;

        [Header("VFX")]
        public GameObject hitVFX;

        private ulong ownerClientId;
        private bool hasHit = false;

        public void SetOwner(ulong ownerId)
        {
            ownerClientId = ownerId;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                StartCoroutine(DestroyAfterLifetime());
        }

        void Update()
        {
            if (!IsServer)
                return;

            transform.position += transform.forward * speed * Time.deltaTime;
        }

        IEnumerator DestroyAfterLifetime()
        {
            yield return new WaitForSeconds(lifetime);

            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.Despawn();
        }

        void OnTriggerEnter(Collider other)
{
    if (!IsServer || hasHit)
        return;

    ArcadeKart kart = other.GetComponent<ArcadeKart>();
    if (kart == null)
        kart = other.GetComponentInParent<ArcadeKart>();

    if (kart != null)
    {
        NetworkObject kartNetObj = kart.GetComponent<NetworkObject>();
        if (kartNetObj == null)
            kartNetObj = kart.GetComponentInParent<NetworkObject>();

        if (kartNetObj != null && kartNetObj.OwnerClientId == ownerClientId)
            return;

        hasHit = true;

        Vector3 launchForce =
            transform.forward * forwardForce +
            transform.right * sideForce +
            Vector3.up * upwardForce;

        if (kartNetObj != null && kartNetObj.IsSpawned)
            ApplyLaunchClientRpc(kartNetObj.OwnerClientId, launchForce);
        else
            kart.ApplyLaunch(launchForce);

        if (hitVFX != null)
            SpawnHitVFXClientRpc(transform.position);

        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();

        return;
    }

    // Hit environment / wall / obstacle
    hasHit = true;

    if (hitVFX != null)
        SpawnHitVFXClientRpc(transform.position);

    if (NetworkObject != null && NetworkObject.IsSpawned)
        NetworkObject.Despawn();
}

        [ClientRpc]
        void ApplyLaunchClientRpc(ulong targetClientId, Vector3 force)
        {
            if (NetworkManager.Singleton.LocalClientId != targetClientId)
                return;

            foreach (var netObj in FindObjectsOfType<NetworkObject>())
            {
                if (netObj.OwnerClientId != targetClientId)
                    continue;

                ArcadeKart kart = netObj.GetComponent<ArcadeKart>();
                if (kart == null)
                    kart = netObj.GetComponentInChildren<ArcadeKart>();

                if (kart != null)
                {
                    kart.ApplyLaunch(force);
                    return;
                }
            }
        }

        [ClientRpc]
        void SpawnHitVFXClientRpc(Vector3 position)
        {
            if (hitVFX != null)
                Instantiate(hitVFX, position, Quaternion.identity);
        }
    }
}