using UnityEngine;
using UnityEngine.UI;

namespace KartGame.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Button quitButton;

        void Start()
        {
            playButton.onClick.AddListener(OnPlayClicked);
            lobbyButton.onClick.AddListener(OnLobbyClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            UIManager.Instance.ShowScreen(UIScreen.Lobby);
        }

        private void OnLobbyClicked()
        {
            UIManager.Instance.ShowScreen(UIScreen.Lobby);
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }
    }
}