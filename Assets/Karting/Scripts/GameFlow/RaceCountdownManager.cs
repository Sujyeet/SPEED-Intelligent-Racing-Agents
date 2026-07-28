using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using KartGame.KartSystems;
using TMPro;
using Unity.Netcode;

namespace KartGame.GameFlow
{
    public class RaceCountdownManager : NetworkBehaviour
    {
        [Header("UI References")]
        public Button startRaceButton;
        public TextMeshProUGUI countdownText;
        public CanvasGroup countdownCanvasGroup;

        [Header("Settings")]
        public float countdownTime = 3f;
        public AudioClip countdownTickSound;
        public AudioClip countdownGoSound;
        private AudioSource audioSource;
        
        private bool isRaceStarted = false;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (startRaceButton != null)
            {
                startRaceButton.onClick.AddListener(OnStartRaceClicked);
                // Hide button by default until network spawns
                startRaceButton.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // Continuously lock all karts until the race officially starts
            // This ensures karts spawned over the network AFTER the scene loads are also locked!
            if (!isRaceStarted)
            {
                SetAllKartsMovement(false);
            }
        }

        public override void OnNetworkSpawn()
        {
            // Only the Host (Server) is allowed to click the Start Race button
            if (IsServer && startRaceButton != null)
            {
                startRaceButton.gameObject.SetActive(true);
            }
        }

        private void OnStartRaceClicked()
        {
            if (IsServer)
            {
                // Hide the button so it can't be clicked twice
                if (startRaceButton != null)
                    startRaceButton.gameObject.SetActive(false);
                    
                // Tell all clients (including the Host) to start the visual countdown
                StartCountdownClientRpc();
            }
        }

        [ClientRpc]
        private void StartCountdownClientRpc()
        {
            StartCoroutine(StartCountdownRoutine());
        }

        private IEnumerator StartCountdownRoutine()
        {
            if (countdownCanvasGroup != null)
                countdownCanvasGroup.alpha = 1f;

            float currentTimer = countdownTime;

            while (currentTimer > 0)
            {
                if (countdownText != null)
                {
                    countdownText.text = Mathf.CeilToInt(currentTimer).ToString();
                }

                if (countdownTickSound != null && currentTimer == Mathf.FloorToInt(currentTimer))
                {
                    audioSource.PlayOneShot(countdownTickSound);
                }

                yield return new WaitForSeconds(1f);
                currentTimer -= 1f;
            }

            // GO!
            if (countdownText != null)
            {
                countdownText.text = "GO!";
            }

            if (countdownGoSound != null)
            {
                audioSource.PlayOneShot(countdownGoSound);
            }

            // Mark race as started and unlock all karts!
            isRaceStarted = true;
            SetAllKartsMovement(true);

            // Fade out the UI
            float fadeDuration = 1f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                if (countdownCanvasGroup != null)
                {
                    countdownCanvasGroup.alpha = 1f - (elapsed / fadeDuration);
                }
                yield return null;
            }

            if (countdownText != null)
                countdownText.gameObject.SetActive(false);
        }

        private void SetAllKartsMovement(bool canMove)
        {
            var allKarts = FindObjectsOfType<ArcadeKart>();
            foreach (var kart in allKarts)
            {
                kart.SetCanMove(canMove);
                
                if (kart.Rigidbody != null)
                {
                    kart.Rigidbody.isKinematic = !canMove;
                }
            }
        }
    }
}
