using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace KartGame.UI
{
    public class ResultsScreenUI : MonoBehaviour
    {
        [SerializeField] private Text resultText;
        [SerializeField] private Button rematchButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        void Start()
        {
            rematchButton.onClick.AddListener(OnRematchClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        public void SetResult(bool won, int finalPosition)
        {
            resultText.text = won ? "YOU WIN!" : $"Finished: #{finalPosition}";
        }

        private void OnRematchClicked()
        {
            UIManager.Instance.ShowScreen(UIScreen.Lobby);
        }

        private void OnMainMenuClicked()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}