using System.Collections;
using UnityEngine;

namespace KartGame.KartSystems
{
    public class TurboSurgeSpell : BaseSpell
    {
        [Header("Turbo Surge Settings")]
        public float speedBoostAmount = 15f;  // added on top of TopSpeed
        public float duration = 3f;

        protected override void OnCast(ArcadeKart kart)
        {
            StartCoroutine(ApplyTurboSurge(kart));
        }

        IEnumerator ApplyTurboSurge(ArcadeKart kart)
        {
            // Add a temporary stat powerup to the kart
            var boost = new ArcadeKart.StatPowerups
            {
                PowerUpID = "TurboSurge",
                MaxTime = duration,
                ElapsedTime = 0f,
                modifiers = new ArcadeKart.Stats
                {
                    TopSpeed = speedBoostAmount,
                    Acceleration = 5f
                }
            };

            kart.AddPowerup(boost);
            Debug.Log("Turbo Surge activated!");

            yield return new WaitForSeconds(duration);
            Debug.Log("Turbo Surge ended.");
        }
    }
}