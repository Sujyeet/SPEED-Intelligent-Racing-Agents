using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

namespace KartGame.KartSystems
{
    public class SpellSystem : MonoBehaviour
    {
        [Header("Spell Slots")]
        public BaseSpell spellSlot1; // Q
        public BaseSpell spellSlot2; // E
        public BaseSpell spellSlot3; // R
        public BaseSpell spellSlot4; // F

        [Header("HUD - Slot 1 (I)")]
        public Image slot1CooldownFill;
        public TextMeshProUGUI slot1KeyLabel;

        [Header("HUD - Slot 2 (J)")]
        public Image slot2CooldownFill;
        public TextMeshProUGUI slot2KeyLabel;

        [Header("HUD - Slot 3 (K)")]
        public Image slot3CooldownFill;
        public TextMeshProUGUI slot3KeyLabel;

        [Header("HUD - Slot 4 (L)")]
        public Image slot4CooldownFill;
        public TextMeshProUGUI slot4KeyLabel;

        private ArcadeKart kart;
        private NetworkObject networkObject;

        void Awake()
        {
            kart = GetComponent<ArcadeKart>();
            networkObject = GetComponent<NetworkObject>();
        }

        void Start()
        {
            if (slot1KeyLabel) slot1KeyLabel.text = "I";
            if (slot2KeyLabel) slot2KeyLabel.text = "J";
            if (slot3KeyLabel) slot3KeyLabel.text = "K";
            if (slot4KeyLabel) slot4KeyLabel.text = "L";
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

            if (networkObject != null && networkObject.IsSpawned && !networkObject.IsOwner)
                return;

            if (Input.GetKeyDown(KeyCode.I))
                TryCastSpell(spellSlot1);

            if (Input.GetKeyDown(KeyCode.J))
                TryCastSpell(spellSlot2);

            if (Input.GetKeyDown(KeyCode.K))
                TryCastSpell(spellSlot3);

            if (Input.GetKeyDown(KeyCode.L))
                TryCastSpell(spellSlot4);

            UpdateCooldownUI(spellSlot1, slot1CooldownFill);
            UpdateCooldownUI(spellSlot2, slot2CooldownFill);
            UpdateCooldownUI(spellSlot3, slot3CooldownFill);
            UpdateCooldownUI(spellSlot4, slot4CooldownFill);
        }

        void TryCastSpell(BaseSpell spell)
        {
            if (spell == null) return;
            if (!spell.IsReady()) return;

            spell.Cast(kart);
        }

        void UpdateCooldownUI(BaseSpell spell, Image fillImage)
        {
            if (fillImage == null) return;
            
            if (spell == null)
            {
                fillImage.fillAmount = 0f; // Force clear if no spell assigned
                return;
            }

            fillImage.fillAmount = 1f - spell.GetCooldownPercent();
        }
    }
}