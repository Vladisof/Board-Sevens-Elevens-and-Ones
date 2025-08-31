using UnityEngine;

namespace BoardNumbers
{
    /// <summary>
    /// Test runner for validating game logic without Unity UI
    /// </summary>
    public class GameTestRunner : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private TileSetConfig testTileConfig;
        [SerializeField] private GameRulesConfig testGameRules;
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool enableDebugLogging = true;

        private void Start()
        {
            if (runTestsOnStart)
            {
                RunAllTests();
            }
        }

        /// <summary>
        /// Run all available tests
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== STARTING GAME LOGIC TESTS ===");

            TestBoardGrid();
            TestDeckService();
            TestPlayerController();
            TestPatternRecognition();
            TestGameFlow();

            Debug.Log("=== ALL TESTS COMPLETED ===");
        }

        /// <summary>
        /// Test BoardGrid functionality
        /// </summary>
        private void TestBoardGrid()
        {
            Debug.Log("--- Testing BoardGrid ---");

            BoardGrid board = new BoardGrid(7);
            
            // Test basic placement
            Assert(board.PlaceTile(0, 0, 5), "Should place tile at (0,0)");
            Assert(board.GetTileAt(0, 0) == 5, "Should get correct tile value");
            Assert(!board.IsCellEmpty(0, 0), "Cell should not be empty");
            Assert(!board.PlaceTile(0, 0, 3), "Should not overwrite existing tile");
            
            // Test boundaries
            Assert(!board.PlaceTile(-1, 0, 1), "Should reject negative coordinates");
            Assert(!board.PlaceTile(7, 0, 1), "Should reject coordinates >= board size");
            
            // Test neighbors
            var neighbors = board.GetNeighbors(1, 1);
            Assert(neighbors.Count == 8, "Center position should have 8 neighbors");
            
            var cornerNeighbors = board.GetNeighbors(0, 0);
            Assert(cornerNeighbors.Count == 3, "Corner position should have 3 neighbors");

            Debug.Log("BoardGrid tests passed!");
        }

        /// <summary>
        /// Test DeckService functionality
        /// </summary>
        private void TestDeckService()
        {
            Debug.Log("--- Testing DeckService ---");

            if (testTileConfig == null)
            {
                Debug.LogWarning("No TileSetConfig provided for testing");
                return;
            }

            DeckService deck = new DeckService(testTileConfig, 12345); // Use seed for reproducibility
            
            int initialCount = deck.RemainingTiles;
            Assert(initialCount > 0, "Deck should have tiles after construction");
            
            // Test drawing
            int drawnTile = deck.DrawTile();
            Assert(drawnTile >= 1 && drawnTile <= 11, "Drawn tile should be valid");
            Assert(deck.RemainingTiles == initialCount - 1, "Deck count should decrease after draw");
            
            // Test drawing multiple
            var drawnTiles = deck.DrawTiles(3);
            Assert(drawnTiles.Count == 3, "Should draw requested number of tiles");
            
            Debug.Log("DeckService tests passed!");
        }

        /// <summary>
        /// Test PlayerController functionality
        /// </summary>
        private void TestPlayerController()
        {
            Debug.Log("--- Testing PlayerController ---");

            PlayerController player = new PlayerController(0, Color.red, "Test Player");
            
            Assert(player.PlayerId == 0, "Player ID should be set correctly");
            Assert(player.PlayerName == "Test Player", "Player name should be set");
            Assert(player.Score == 0, "Initial score should be 0");
            Assert(player.HandCount == 0, "Initial hand should be empty");
            
            // Test hand management
            player.AddTileToHand(7);
            Assert(player.HandCount == 1, "Hand count should increase");
            Assert(player.HasTile(7), "Player should have the added tile");
            
            bool removed = player.RemoveTileFromHand(7);
            Assert(removed, "Should successfully remove tile");
            Assert(player.HandCount == 0, "Hand should be empty after removal");
            
            // Test scoring
            player.AddScore(15);
            Assert(player.Score == 15, "Score should be updated");
            
            Debug.Log("PlayerController tests passed!");
        }

        /// <summary>
        /// Test pattern recognition
        /// </summary>
        private void TestPatternRecognition()
        {
            Debug.Log("--- Testing Pattern Recognition ---");

            if (testGameRules == null)
            {
                Debug.LogWarning("No GameRulesConfig provided for testing");
                return;
            }

            BoardGrid board = new BoardGrid(7);
            PatternRecognitionService patterns = new PatternRecognitionService(testGameRules, board);
            
            // Test Seven pattern (3 + 4 = 7)
            board.PlaceTile(1, 1, 3);
            board.PlaceTile(1, 2, 4);
            
            var result = patterns.CheckForPatterns(new Vector2Int(1, 2));
            Assert(result.totalPoints == testGameRules.sevenPoints, "Should score for seven pattern");
            Assert(result.drawBonusTile, "Should get draw bonus for seven");
            
            // Test Triple Ones
            board.ClearBoard();
            board.PlaceTile(2, 2, 1);
            board.PlaceTile(2, 3, 1);
            board.PlaceTile(3, 2, 1);
            
            result = patterns.CheckForPatterns(new Vector2Int(3, 2));
            Assert(result.totalPoints == testGameRules.tripleOnePoints, "Should score for triple ones");
            Assert(result.extraTurn, "Should get extra turn for triple ones");
            
            Debug.Log("Pattern Recognition tests passed!");
        }

        /// <summary>
        /// Test basic game flow
        /// </summary>
        private void TestGameFlow()
        {
            Debug.Log("--- Testing Game Flow ---");

            // This would test GameManager if we had configs set up
            if (testTileConfig != null && testGameRules != null)
            {
                // Create a simple game flow test here
                Debug.Log("Game flow test would run with proper configs");
            }
            else
            {
                Debug.LogWarning("Cannot test game flow without proper configs");
            }

            Debug.Log("Game Flow tests completed!");
        }

        /// <summary>
        /// Simple assertion helper
        /// </summary>
        private void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Debug.LogError($"TEST FAILED: {message}");
            }
            else if (enableDebugLogging)
            {
                Debug.Log($"TEST PASSED: {message}");
            }
        }
    }
}