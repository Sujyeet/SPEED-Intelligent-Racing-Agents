using UnityEngine;

namespace KartGame.KartSystems
{
    public class HexSpell : BaseSpell
    {
        [Header("Hex Settings")]
        public float range = 18f;
        public float hexDuration = 2f;

        private HexNetworkHelper networkHelper;

        void Awake()
        {
            networkHelper = GetComponent<HexNetworkHelper>();
        }

        protected override void OnCast(ArcadeKart kart)
        {
            if (networkHelper == null)
            {
                Debug.LogError("HexSpell: Missing HexNetworkHelper on this kart.");
                return;
            }

            networkHelper.RequestHex(range, hexDuration);
        }
    }
}