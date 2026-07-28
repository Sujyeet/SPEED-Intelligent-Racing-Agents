using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using KartGame.KartSystems;
using KartGame.GameFlow; // Added namespace

namespace KartGame.Multiplayer
{
    public class MultiplayerRaceManager : NetworkBehaviour
    {
        public static MultiplayerRaceManager Instance { get; private set; }

        [Header("Race Config")]
        public int lapsToComplete = 3;

        [Header("UI References")]
        [Tooltip("The HUD UI screen to deactivate when the race ends.")]
        public GameObject hudCanvas;
        [Tooltip("The results/end-game screen to activate.")]
        public GameObject resultsCanvas;
        [Tooltip("Text element on the results screen to display ranking.")]
        public Text resultsText;
        [Tooltip("Button to return to the Lobby.")]
        public Button mainMenuButton;

        private int localCurrentLap = 1;
        private int localCheckpointsPassed = 0;
        private bool localFinished = false;

        // Server-side list of finished client IDs in order
        private readonly List<ulong> finishOrder = new List<ulong>();

        // Standings: Key = ClientId, Value = (Laps, Checkpoints, LastTime)
        private struct PlayerProgress
        {
            public ulong clientId;
            public int lap;
            public int checkpoints;
            public float lastUpdateTime;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (resultsCanvas != null)
            {
                resultsCanvas.SetActive(false);
            }

            // Bind to the local lap/checkpoint update events if they exist
            // Or we will trigger it directly from LapObject/Checkpoints
        }

        private void OnEnable()
        {
            GameModeManager.OnAgentFinishedRace += RegisterAgentFinish;
        }

        private void OnDisable()
        {
            GameModeManager.OnAgentFinishedRace -= RegisterAgentFinish;
        }

        public override void OnNetworkSpawn()
        {
            localCurrentLap = 1;
            localCheckpointsPassed = 0;
            localFinished = false;

            if (IsServer)
            {
                finishOrder.Clear();
            }

            // Handle ML Agents presence
            if (!GameModeManager.IncludeMLAgents)
            {
                var allScripts = FindObjectsOfType<MonoBehaviour>();
                foreach (var script in allScripts)
                {
                    if (script.GetType().Name == "KartAgent")
                    {
                        Destroy(script.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the local player completes a lap or checkpoint.
        /// </summary>
        public void OnLocalPlayerProgress(int lap, int checkpoints)
        {
            if (localFinished) return;

            localCurrentLap = lap;
            localCheckpointsPassed = checkpoints;

            // Notify server of progress
            if (IsSpawned)
            {
                UpdateProgressServerRpc(NetworkManager.Singleton.LocalClientId, lap, checkpoints);
            }

            // Check win condition
            if (lap >= lapsToComplete)
            {
                localFinished = true;
                FinishRace();
            }
        }

        /// <summary>
        /// Registers finish for AI/ML Agent in single player mode.
        /// </summary>
        public void RegisterAgentFinish(Component agent)
        {
            if (!GameModeManager.IsSinglePlayer)
                return;

            if (agent != null)
            {
                var kart = agent.GetComponent<ArcadeKart>();
                if (kart != null)
                {
                    kart.SetCanMove(false);
                }

                ulong agentVirtualId = (ulong)(1000 + Mathf.Abs(agent.GetInstanceID()));
                if (!finishOrder.Contains(agentVirtualId))
                {
                    finishOrder.Add(agentVirtualId);
                    int placement = finishOrder.Count;
                    Debug.Log($"[SinglePlayer] ML Agent '{agent.name}' finished the race in position #{placement}!");
                }
            }
        }

        private void FinishRace()
        {
            Debug.Log("Local player finished the race!");
            
            // Disable kart movement
            var localKart = FindLocalPlayerKart();
            if (localKart != null)
            {
                localKart.SetCanMove(false);
            }

            if (IsSpawned)
            {
                // Notify server of finish
                RegisterFinishServerRpc(NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                // Single-player fallback
                ShowResults(1);
            }
        }

        private ArcadeKart FindLocalPlayerKart()
        {
            var karts = FindObjectsOfType<ArcadeKart>();
            foreach (var k in karts)
            {
                var netObj = k.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsOwner)
                {
                    return k;
                }
                // Fallback for singleplayer
                if (netObj == null && k.CompareTag("Player"))
                {
                    return k;
                }
            }
            return null;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateProgressServerRpc(ulong clientId, int lap, int checkpoints)
        {
            // Can be used to sync leaderboard/rankings in real-time
        }

        [ServerRpc(RequireOwnership = false)]
        private void RegisterFinishServerRpc(ulong clientId)
        {
            if (!finishOrder.Contains(clientId))
            {
                finishOrder.Add(clientId);
                int placement = finishOrder.Count;
                NotifyFinishClientRpc(clientId, placement);
            }
        }

        [ClientRpc]
        private void NotifyFinishClientRpc(ulong clientId, int placement)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                ShowResults(placement);
            }
            else
            {
                Debug.Log($"Player {clientId} finished in position #{placement}!");
            }
        }

        private void ShowResults(int placement)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (hudCanvas != null) hudCanvas.SetActive(false);
            if (resultsCanvas != null) resultsCanvas.SetActive(true);

            if (resultsText != null)
            {
                string ordinal = GetOrdinal(placement);
                resultsText.text = $"YOU FINISHED\n{placement}{ordinal} PLACE!";
            }
        }

        private string GetOrdinal(int num)
        {
            if (num <= 0) return "";
            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return "th";
            }
            switch (num % 10)
            {
                case 1: return "st";
                case 2: return "nd";
                case 3: return "rd";
                default: return "th";
            }
        }

        private void ReturnToMainMenu()
        {
            if (IsSpawned)
            {
                NetworkManager.Singleton.Shutdown();
            }
            SceneManager.LoadScene("MainMenu");
        }
    }
}
