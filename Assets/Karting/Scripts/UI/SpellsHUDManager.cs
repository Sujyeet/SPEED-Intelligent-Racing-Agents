using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KartGame.KartSystems;
using Unity.Netcode;

namespace KartGame.UI
{
    /// <summary>
    /// Attach this to your Spells HUD Canvas/Panel in the MainGame scene.
    /// It dynamically finds the local player's kart and links the UI elements to the kart's SpellSystem.
    /// </summary>
    public class SpellsHUDManager : MonoBehaviour
    {
        [Header("HUD - Slot 1 (Q)")]
        public Image slot1CooldownFill;
        public TextMeshProUGUI slot1KeyLabel;

        [Header("HUD - Slot 2 (E)")]
        public Image slot2CooldownFill;
        public TextMeshProUGUI slot2KeyLabel;

        [Header("HUD - Slot 3 (R)")]
        public Image slot3CooldownFill;
        public TextMeshProUGUI slot3KeyLabel;

        [Header("HUD - Slot 4 (F)")]
        public Image slot4CooldownFill;
        public TextMeshProUGUI slot4KeyLabel;

        private SpellSystem localSpellSystem;

        private void Update()
        {
            // Keep trying to find the local player's spell system if we haven't yet
            if (localSpellSystem == null)
            {
                FindLocalSpellSystem();
            }
        }

        private void FindLocalSpellSystem()
        {
            var allKarts = FindObjectsOfType<ArcadeKart>();
            foreach (var kart in allKarts)
            {
                // Make sure this kart actually HAS a SpellSystem before we try to link it!
                // Otherwise it might get stuck trying to link to the ML Agent over and over.
                var spellSys = kart.GetComponent<SpellSystem>();
                if (spellSys == null) continue;

                var netObj = kart.GetComponent<NetworkObject>();
                // In Multiplayer: find the one we own
                if (netObj != null && netObj.IsOwner)
                {
                    LinkSpellSystem(spellSys);
                    return;
                }
                // In Singleplayer Fallback (no NetworkObject, but has Player tag)
                if (netObj == null && kart.CompareTag("Player"))
                {
                    LinkSpellSystem(spellSys);
                    return;
                }
            }
        }

        private void LinkSpellSystem(SpellSystem spellSys)
        {
            localSpellSystem = spellSys;
            if (localSpellSystem != null)
            {
                // Assign our global UI references to the local kart's spell system
                localSpellSystem.slot1CooldownFill = slot1CooldownFill;
                localSpellSystem.slot1KeyLabel = slot1KeyLabel;

                localSpellSystem.slot2CooldownFill = slot2CooldownFill;
                localSpellSystem.slot2KeyLabel = slot2KeyLabel;

                localSpellSystem.slot3CooldownFill = slot3CooldownFill;
                localSpellSystem.slot3KeyLabel = slot3KeyLabel;

                localSpellSystem.slot4CooldownFill = slot4CooldownFill;
                localSpellSystem.slot4KeyLabel = slot4KeyLabel;

                Debug.Log("Spells HUD successfully linked to the local player!");
            }
        }
    }
}
