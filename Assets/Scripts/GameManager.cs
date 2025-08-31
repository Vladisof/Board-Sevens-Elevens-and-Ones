using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BoardNumbers
{
    /// <summary>
    /// Main game manager with state machine orchestrating the entire game flow
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private TileSetConfig tileSetConfig;
        [SerializeField] private GameRulesConfig gameRulesConfig;

        [Header("Game State")]
        [SerializeField] private List<PlayerController> players;
        [SerializeField] private int currentPlayerIndex;
        [SerializeField] private GameState currentState;

        // Game services
        private BoardGrid board;
        private DeckService deck;
        private PatternRecognitionService patternService;

        // Game state tracking
        private PlayerController currentPlayer;
        private PlayerController winner;
        private bool gameEnded;

        // Events for UI updates
        public System.Action<GameState> OnStateChanged;
        public System.Action<PlayerController> OnPlayerTurnStart;
        public System.Action<PlayerController, int> OnScoreUpdated;
        public System.Action<PlayerController> OnGameWon;
        public System.Action<Vector2Int, int> OnTilePlaced;
        public System.Action OnBoardUpdated;

        public enum GameState
        {
            Setup,
            PlayerTurn,
            ResolveScore,
            ExtraTurn,
            GameEnd
        }

        // Properties
        public GameState CurrentState => currentState;
        public PlayerController CurrentPlayer => currentPlayer;
        public PlayerController Winner => winner;
        public BoardGrid Board => board;
        public List<PlayerController> Players => players;
        public bool IsGameEnded => gameEnded;

        private void Awake()
        {
            ValidateConfiguration();
            InitializeGame();
        }

        private void Start()
        {
            StartNewGame();
        }

        /// <summary>
        /// Validate that required configurations are set
        /// </summary>
        private void ValidateConfiguration()
        {
            if (tileSetConfig == null)
            {
                Debug.LogError("TileSetConfig is not assigned!");
                return;
            }

            if (gameRulesConfig == null)
            {
                Debug.LogError("GameRulesConfig is not assigned!");
                return;
            }

            if (!tileSetConfig.IsValid())
            {
                Debug.LogError("TileSetConfig is invalid!");
            }

            if (!gameRulesConfig.IsValid())
            {
                Debug.LogError("GameRulesConfig is invalid!");
            }
        }

        /// <summary>
        /// Initialize game systems
        /// </summary>
        private void InitializeGame()
        {
            // Initialize board
            board = new BoardGrid(gameRulesConfig.boardSize);

            // Initialize deck
            deck = new DeckService(tileSetConfig);

            // Initialize pattern recognition
            patternService = new PatternRecognitionService(gameRulesConfig, board);

            // Initialize players
            InitializePlayers(gameRulesConfig.minPlayers); // Default to minimum players for now
        }

        /// <summary>
        /// Initialize players with default colors
        /// </summary>
        private void InitializePlayers(int playerCount)
        {
            players = new List<PlayerController>();
            Color[] playerColors = { Color.red, Color.blue, Color.green, Color.yellow };

            for (int i = 0; i < playerCount; i++)
            {
                Color playerColor = i < playerColors.Length ? playerColors[i] : Random.ColorHSV();
                players.Add(new PlayerController(i, playerColor));
            }

            Debug.Log($"Initialized {playerCount} players");
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartNewGame()
        {
            Debug.Log("Starting new game...");
            
            SetState(GameState.Setup);
            SetupNewGame();
        }

        /// <summary>
        /// Setup phase - prepare for new game
        /// </summary>
        private void SetupNewGame()
        {
            // Reset game state
            gameEnded = false;
            winner = null;
            
            // Clear and shuffle deck
            deck.ResetDeck();
            
            // Clear board
            board.ClearBoard();
            
            // Reset all players
            foreach (var player in players)
            {
                player.ResetForNewGame();
            }
            
            // Deal initial hands
            DealInitialHands();
            
            // Randomize first player
            currentPlayerIndex = Random.Range(0, players.Count);
            currentPlayer = players[currentPlayerIndex];
            
            Debug.Log($"Game setup complete. First player: {currentPlayer.PlayerName}");
            
            SetState(GameState.PlayerTurn);
        }

        /// <summary>
        /// Deal initial hands to all players
        /// </summary>
        private void DealInitialHands()
        {
            foreach (var player in players)
            {
                for (int i = 0; i < gameRulesConfig.handSize; i++)
                {
                    int tile = deck.DrawTile();
                    if (tile != -1)
                    {
                        player.AddTileToHand(tile);
                    }
                }
            }
            
            Debug.Log($"Dealt {gameRulesConfig.handSize} tiles to each player");
        }

        /// <summary>
        /// Handle player tile placement
        /// </summary>
        public bool PlaceTile(Vector2Int position, int tileValue)
        {
            if (currentState != GameState.PlayerTurn && currentState != GameState.ExtraTurn)
            {
                Debug.LogWarning("Cannot place tile: not in player turn state");
                return false;
            }

            if (!currentPlayer.HasTile(tileValue))
            {
                Debug.LogWarning($"Player {currentPlayer.PlayerName} doesn't have tile {tileValue}");
                return false;
            }

            if (!board.PlaceTile(position, tileValue))
            {
                return false; // Board placement failed
            }

            // Remove tile from player's hand
            currentPlayer.RemoveTileFromHand(tileValue);

            // Notify UI
            OnTilePlaced?.Invoke(position, tileValue);
            OnBoardUpdated?.Invoke();

            Debug.Log($"{currentPlayer.PlayerName} placed tile {tileValue} at {position}");

            // Move to scoring resolution
            SetState(GameState.ResolveScore);
            ResolveScoring(position);

            return true;
        }

        /// <summary>
        /// Resolve scoring after tile placement
        /// </summary>
        private void ResolveScoring(Vector2Int placedPosition)
        {
            var scoringResult = patternService.ProcessScoringInOrder(placedPosition);

            // Apply points
            if (scoringResult.totalPoints > 0)
            {
                currentPlayer.AddScore(scoringResult.totalPoints);
                OnScoreUpdated?.Invoke(currentPlayer, scoringResult.totalPoints);
                
                Debug.Log($"{currentPlayer.PlayerName} scored {scoringResult.totalPoints} points! Total: {currentPlayer.Score}");
            }

            // Handle bonus tile draw
            if (scoringResult.drawBonusTile && !deck.IsEmpty)
            {
                int bonusTile = deck.DrawTile();
                if (bonusTile != -1)
                {
                    currentPlayer.AddTileToHand(bonusTile);
                    Debug.Log($"{currentPlayer.PlayerName} drew bonus tile: {bonusTile}");
                }
            }

            // Check for win condition
            if (CheckWinConditions())
            {
                return; // Game ended
            }

            // Handle extra turn
            if (scoringResult.extraTurn)
            {
                currentPlayer.SetExtraTurn(true);
                SetState(GameState.ExtraTurn);
                Debug.Log($"{currentPlayer.PlayerName} gets an extra turn!");
            }
            else
            {
                FinishTurn();
            }
        }

        /// <summary>
        /// Finish the current turn and move to next player
        /// </summary>
        private void FinishTurn()
        {
            // Draw back to hand size
            DrawToHandSize(currentPlayer);

            // Clear extra turn flag
            currentPlayer.SetExtraTurn(false);

            // Move to next player (only if not in extra turn)
            if (currentState != GameState.ExtraTurn)
            {
                NextPlayer();
            }
            else
            {
                // Stay with same player for extra turn
                SetState(GameState.PlayerTurn);
            }
        }

        /// <summary>
        /// Draw tiles to maintain hand size
        /// </summary>
        private void DrawToHandSize(PlayerController player)
        {
            while (player.HandCount < gameRulesConfig.handSize && !deck.IsEmpty)
            {
                int tile = deck.DrawTile();
                if (tile != -1)
                {
                    player.AddTileToHand(tile);
                }
            }
        }

        /// <summary>
        /// Move to the next player
        /// </summary>
        private void NextPlayer()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            currentPlayer = players[currentPlayerIndex];

            OnPlayerTurnStart?.Invoke(currentPlayer);
            SetState(GameState.PlayerTurn);

            Debug.Log($"Next turn: {currentPlayer.PlayerName}");
        }

        /// <summary>
        /// Check win conditions
        /// </summary>
        private bool CheckWinConditions()
        {
            // Check score-based win
            PlayerController scoreWinner = players.FirstOrDefault(p => p.Score >= gameRulesConfig.targetScore);
            if (scoreWinner != null)
            {
                EndGame(scoreWinner);
                return true;
            }

            // Check board full
            if (board.IsBoardFull())
            {
                PlayerController highestScorer = players.OrderByDescending(p => p.Score).First();
                EndGame(highestScorer);
                return true;
            }

            return false;
        }

        /// <summary>
        /// End the game with a winner
        /// </summary>
        private void EndGame(PlayerController gameWinner)
        {
            winner = gameWinner;
            gameEnded = true;
            SetState(GameState.GameEnd);

            OnGameWon?.Invoke(winner);
            Debug.Log($"Game ended! Winner: {winner.PlayerName} with {winner.Score} points");
        }

        /// <summary>
        /// Set the game state and notify listeners
        /// </summary>
        private void SetState(GameState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                OnStateChanged?.Invoke(currentState);
                Debug.Log($"Game state changed to: {currentState}");
            }
        }

        /// <summary>
        /// Get valid moves for the current player
        /// </summary>
        public List<Vector2Int> GetValidMoves()
        {
            return board.GetEmptyPositions();
        }

        /// <summary>
        /// Get scoring preview for a potential move
        /// </summary>
        public PatternRecognitionService.ScoringResult PreviewMove(Vector2Int position, int tileValue)
        {
            return patternService.PreviewPlacement(position, tileValue);
        }

        /// <summary>
        /// Force end game (for testing or quit)
        /// </summary>
        public void ForceEndGame()
        {
            if (!gameEnded)
            {
                PlayerController leader = players.OrderByDescending(p => p.Score).First();
                EndGame(leader);
            }
        }

        #if UNITY_EDITOR
        [ContextMenu("Debug Game State")]
        private void DebugGameState()
        {
            Debug.Log($"=== GAME STATE DEBUG ===");
            Debug.Log($"State: {currentState}");
            Debug.Log($"Current Player: {currentPlayer?.PlayerName} (Score: {currentPlayer?.Score})");
            Debug.Log($"Deck: {deck?.RemainingTiles} tiles remaining");
            Debug.Log($"Board: {board?.GetEmptyCellCount()}/{board?.TotalCells} empty");
            
            foreach (var player in players)
            {
                Debug.Log($"Player {player.PlayerName}: {player.Score} points, {player.HandCount} tiles in hand");
            }
        }
        #endif
    }
}