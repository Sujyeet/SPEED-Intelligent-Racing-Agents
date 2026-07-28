using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using KartGame.GameFlow;

namespace KartGame.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [Header("Single Player")]
        [SerializeField] private Button singlePlayerButton;

        [Header("Multiplayer")]
        [SerializeField] private Button multiplayerHostButton;
        [SerializeField] private Button multiplayerJoinButton;
        [SerializeField] private Toggle includeMLAgentsToggle;

        [Header("Relay Settings")]
        [SerializeField] private RelayManager relayManager;
        [SerializeField] private TMPro.TMP_InputField joinCodeInput;
        [SerializeField] private TMPro.TMP_Text joinCodeDisplay;

        [Header("Other UI")]
        [SerializeField] private Button backButton;
        [SerializeField] private Text playerCountText;
        [SerializeField] private string gameSceneName = "GameScene";

        void Start()
        {
            if (singlePlayerButton != null) singlePlayerButton.onClick.AddListener(OnSinglePlayerClicked);
            if (multiplayerHostButton != null) multiplayerHostButton.onClick.AddListener(OnMultiplayerHostClicked);
            if (multiplayerJoinButton != null) multiplayerJoinButton.onClick.AddListener(OnMultiplayerJoinClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

            if (relayManager == null)
            {
                relayManager = FindObjectOfType<RelayManager>();
            }

            // Update UI initial state based on network role
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                if (NetworkManager.Singleton.IsServer)
                {
                    UpdatePlayerCount(NetworkManager.Singleton.ConnectedClients.Count);
                }
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            UpdatePlayerCount(NetworkManager.Singleton.ConnectedClients.Count);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            UpdatePlayerCount(NetworkManager.Singleton.ConnectedClients.Count);
        }

        public void UpdatePlayerCount(int count)
        {
            if (playerCountText != null)
                playerCountText.text = $"Players: {count}";
        }

        private void OnSinglePlayerClicked()
        {
            GameModeManager.IsSinglePlayer = true;
            GameModeManager.IncludeMLAgents = true; // Always true for single player

            if (NetworkManager.Singleton != null)
            {
                // Start as host so local logic still works if it depends on Netcode
                NetworkManager.Singleton.StartHost();
                NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            }
            else
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }

        private async void OnMultiplayerHostClicked()
        {
            GameModeManager.IsSinglePlayer = false;
            if (includeMLAgentsToggle != null)
            {
                GameModeManager.IncludeMLAgents = includeMLAgentsToggle.isOn;
            }

            if (relayManager != null)
            {
                string code = await relayManager.CreateRelay();
                if (!string.IsNullOrEmpty(code))
                {
                    if (joinCodeDisplay != null)
                    {
                        joinCodeDisplay.text = $"JOIN CODE: {code}";
                        joinCodeDisplay.gameObject.SetActive(true);
                    }
                    NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
                }
            }
            else if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartHost();
                NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            }
        }

        private async void OnMultiplayerJoinClicked()
        {
            GameModeManager.IsSinglePlayer = false;

            if (relayManager != null && joinCodeInput != null && !string.IsNullOrEmpty(joinCodeInput.text))
            {
                string code = joinCodeInput.text.Trim().ToUpper();
                bool success = await relayManager.JoinRelay(code);
                
                if (success)
                {
                    Debug.Log("Successfully joined the game via Relay!");
                }
                else
                {
                    Debug.LogError("Failed to join! Check the code and try again.");
                }
            }
            else if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        private void OnBackClicked()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }
            if (UIManager.Instance != null)
                UIManager.Instance.ShowScreen(UIScreen.MainMenu);
        }
    }
}