using System.Collections.Generic;
using UnityEngine;

namespace KartGame.UI
{
    public enum UIScreen
    {
        None,
        MainMenu,
        Lobby,
        HUD,
        Results
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private GameObject mainMenuScreen;
        [SerializeField] private GameObject lobbyScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject resultsScreen;

        private Dictionary<UIScreen, GameObject> screens;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            screens = new Dictionary<UIScreen, GameObject>
            {
                { UIScreen.MainMenu, mainMenuScreen },
                { UIScreen.Lobby, lobbyScreen },
                { UIScreen.HUD, hudScreen },
                { UIScreen.Results, resultsScreen }
            };
        }

        public void ShowScreen(UIScreen screen)
        {
            foreach (var kvp in screens)
            {
                if (kvp.Value != null)
                    kvp.Value.SetActive(kvp.Key == screen);
            }
        }
    }
}