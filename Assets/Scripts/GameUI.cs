using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BoardNumbers
{
    /// <summary>
    /// UI controller for the main game interface
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI currentPlayerText;
        [SerializeField] private TextMeshProUGUI currentScoreText;
        [SerializeField] private TextMeshProUGUI gameStatusText;
        [SerializeField] private Transform handContainer;
        [SerializeField] private Transform boardContainer;
        [SerializeField] private Button[] handTileButtons;
        [SerializeField] private GameObject handTilePrefab;
        [SerializeField] private GameObject boardTilePrefab;

        [Header("Game State")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private int selectedTileValue = -1;

        private void Start()
        {
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();

            SubscribeToEvents();
            UpdateUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Subscribe to game manager events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnStateChanged += OnGameStateChanged;
                gameManager.OnPlayerTurnStart += OnPlayerTurnStart;
                gameManager.OnScoreUpdated += OnScoreUpdated;
                gameManager.OnGameWon += OnGameWon;
                gameManager.OnTilePlaced += OnTilePlaced;
                gameManager.OnBoardUpdated += OnBoardUpdated;
            }
        }

        /// <summary>
        /// Unsubscribe from game manager events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnStateChanged -= OnGameStateChanged;
                gameManager.OnPlayerTurnStart -= OnPlayerTurnStart;
                gameManager.OnScoreUpdated -= OnScoreUpdated;
                gameManager.OnGameWon -= OnGameWon;
                gameManager.OnTilePlaced -= OnTilePlaced;
                gameManager.OnBoardUpdated -= OnBoardUpdated;
            }
        }

        /// <summary>
        /// Update the entire UI
        /// </summary>
        public void UpdateUI()
        {
            UpdatePlayerInfo();
            UpdateHandDisplay();
            UpdateBoardDisplay();
            UpdateGameStatus();
        }

        /// <summary>
        /// Update current player information
        /// </summary>
        private void UpdatePlayerInfo()
        {
            if (gameManager?.CurrentPlayer != null)
            {
                var player = gameManager.CurrentPlayer;
                
                if (currentPlayerText != null)
                    currentPlayerText.text = $"Current Player: {player.PlayerName}";
                
                if (currentScoreText != null)
                    currentScoreText.text = $"Score: {player.Score}";
                
                // Update text color to match player color
                if (currentPlayerText != null)
                    currentPlayerText.color = player.PlayerColor;
            }
        }

        /// <summary>
        /// Update hand tile display
        /// </summary>
        private void UpdateHandDisplay()
        {
            if (gameManager?.CurrentPlayer == null || handContainer == null)
                return;

            // Clear existing hand tiles
            foreach (Transform child in handContainer)
            {
                if (child.gameObject != handTilePrefab) // Don't destroy the prefab
                    Destroy(child.gameObject);
            }

            // Create new hand tile buttons
            var hand = gameManager.CurrentPlayer.Hand;
            for (int i = 0; i < hand.Count; i++)
            {
                CreateHandTile(hand[i], i);
            }
        }

        /// <summary>
        /// Create a hand tile button
        /// </summary>
        private void CreateHandTile(int tileValue, int index)
        {
            if (handTilePrefab == null) return;

            GameObject tileObj = Instantiate(handTilePrefab, handContainer);
            Button button = tileObj.GetComponent<Button>();
            TextMeshProUGUI text = tileObj.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
                text.text = tileValue.ToString();

            if (button != null)
            {
                button.onClick.AddListener(() => SelectHandTile(tileValue));
                
                // Highlight selected tile
                var colors = button.colors;
                if (selectedTileValue == tileValue)
                {
                    colors.normalColor = Color.yellow;
                    colors.selectedColor = Color.yellow;
                }
                button.colors = colors;
            }
        }

        /// <summary>
        /// Update board display
        /// </summary>
        private void UpdateBoardDisplay()
        {
            if (gameManager?.Board == null || boardContainer == null)
                return;

            // Clear existing board tiles
            foreach (Transform child in boardContainer)
            {
                if (child.gameObject != boardTilePrefab) // Don't destroy the prefab
                    Destroy(child.gameObject);
            }

            // Create board grid
            var board = gameManager.Board;
            for (int x = 0; x < board.BoardSize; x++)
            {
                for (int y = 0; y < board.BoardSize; y++)
                {
                    CreateBoardTile(x, y);
                }
            }
        }

        /// <summary>
        /// Create a board tile
        /// </summary>
        private void CreateBoardTile(int x, int y)
        {
            if (boardTilePrefab == null) return;

            GameObject tileObj = Instantiate(boardTilePrefab, boardContainer);
            Button button = tileObj.GetComponent<Button>();
            TextMeshProUGUI text = tileObj.GetComponentInChildren<TextMeshProUGUI>();

            // Position the tile
            RectTransform rectTransform = tileObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float tileSize = 60f; // Adjust as needed
                rectTransform.anchoredPosition = new Vector2(x * tileSize, y * tileSize);
            }

            var board = gameManager.Board;
            int tileValue = board.GetTileAt(x, y);

            // Set tile display
            if (text != null)
            {
                text.text = tileValue == 0 ? "" : tileValue.ToString();
            }

            // Set tile interaction
            if (button != null)
            {
                if (tileValue == 0) // Empty tile
                {
                    button.onClick.AddListener(() => PlaceTileOnBoard(x, y));
                    button.interactable = selectedTileValue != -1;
                }
                else // Occupied tile
                {
                    button.interactable = false;
                    var colors = button.colors;
                    colors.disabledColor = Color.gray;
                    button.colors = colors;
                }
            }
        }

        /// <summary>
        /// Update game status text
        /// </summary>
        private void UpdateGameStatus()
        {
            if (gameStatusText == null || gameManager == null)
                return;

            string status = "";
            switch (gameManager.CurrentState)
            {
                case GameManager.GameState.Setup:
                    status = "Setting up game...";
                    break;
                case GameManager.GameState.PlayerTurn:
                    status = "Select a tile from your hand, then click on the board to place it";
                    break;
                case GameManager.GameState.ExtraTurn:
                    status = "Extra Turn! Select a tile from your hand";
                    break;
                case GameManager.GameState.ResolveScore:
                    status = "Resolving scoring...";
                    break;
                case GameManager.GameState.GameEnd:
                    if (gameManager.Winner != null)
                        status = $"Game Over! {gameManager.Winner.PlayerName} wins with {gameManager.Winner.Score} points!";
                    else
                        status = "Game Over!";
                    break;
            }

            gameStatusText.text = status;
        }

        /// <summary>
        /// Handle hand tile selection
        /// </summary>
        private void SelectHandTile(int tileValue)
        {
            selectedTileValue = tileValue;
            UpdateHandDisplay(); // Refresh to show selection
            Debug.Log($"Selected tile: {tileValue}");
        }

        /// <summary>
        /// Handle board tile placement
        /// </summary>
        private void PlaceTileOnBoard(int x, int y)
        {
            if (selectedTileValue == -1)
            {
                Debug.LogWarning("No tile selected!");
                return;
            }

            Vector2Int position = new Vector2Int(x, y);
            bool success = gameManager.PlaceTile(position, selectedTileValue);
            
            if (success)
            {
                selectedTileValue = -1; // Clear selection
                UpdateUI(); // Refresh UI
            }
        }

        /// <summary>
        /// Event handlers
        /// </summary>
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            UpdateGameStatus();
        }

        private void OnPlayerTurnStart(PlayerController player)
        {
            selectedTileValue = -1; // Clear selection for new player
            UpdateUI();
        }

        private void OnScoreUpdated(PlayerController player, int pointsAdded)
        {
            UpdatePlayerInfo();
            
            // Show score animation/feedback here if desired
            Debug.Log($"{player.PlayerName} scored {pointsAdded} points!");
        }

        private void OnGameWon(PlayerController winner)
        {
            UpdateGameStatus();
            
            // Show victory screen/animation here if desired
            Debug.Log($"Victory! {winner.PlayerName} wins!");
        }

        private void OnTilePlaced(Vector2Int position, int tileValue)
        {
            // Tile placement feedback can be added here
            Debug.Log($"Tile {tileValue} placed at {position}");
        }

        private void OnBoardUpdated()
        {
            UpdateBoardDisplay();
        }

        /// <summary>
        /// Public methods for UI buttons
        /// </summary>
        public void NewGameButton()
        {
            selectedTileValue = -1;
            gameManager?.StartNewGame();
        }

        public void QuitGameButton()
        {
            Application.Quit();
        }
    }
}