using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace BoardNumbers
{
    /// <summary>
    /// Simple main menu controller
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI versionText;

        private void Start()
        {
            InitializeMenu();
        }

        /// <summary>
        /// Initialize the main menu
        /// </summary>
        private void InitializeMenu()
        {
            if (titleText != null)
            {
                titleText.text = "Board Numbers:\nSevens, Elevens, and Ones";
            }

            if (versionText != null)
            {
                versionText.text = $"Unity {Application.unityVersion}";
            }
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartNewGame()
        {
            Debug.Log("Starting new game...");
            SceneManager.LoadScene("Game");
        }

        /// <summary>
        /// Start new game with specific player count
        /// </summary>
        public void StartGameWithPlayers(int playerCount)
        {
            Debug.Log($"Starting game with {playerCount} players...");
            
            // Store player count for the game scene
            PlayerPrefs.SetInt("PlayerCount", playerCount);
            PlayerPrefs.Save();
            
            SceneManager.LoadScene("Game");
        }

        /// <summary>
        /// Quit the application
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Load scene by name
        /// </summary>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}