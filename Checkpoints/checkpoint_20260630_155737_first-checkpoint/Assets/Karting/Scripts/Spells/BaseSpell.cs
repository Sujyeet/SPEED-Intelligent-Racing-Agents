using UnityEngine;

namespace KartGame.KartSystems
{
    public abstract class BaseSpell : MonoBehaviour
    {
        [Header("Spell Settings")]
        public string spellName = "Spell";
        public float cooldown = 5f;

        private float lastUsedTime = -999f;

        public bool IsReady()
        {
            return Time.time >= lastUsedTime + cooldown;
        }

        public float GetCooldownPercent()
        {
            float elapsed = Time.time - lastUsedTime;
            return Mathf.Clamp01(elapsed / cooldown);
        }

        public void Cast(ArcadeKart kart)
        {
            lastUsedTime = Time.time;
            OnCast(kart);
        }

        // Each spell overrides this
        protected abstract void OnCast(ArcadeKart kart);
    }
}