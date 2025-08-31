using UnityEngine;

namespace BoardNumbers
{
    /// <summary>
    /// Configuration for game rules, scoring, and board parameters
    /// </summary>
    [CreateAssetMenu(fileName = "GameRulesConfig", menuName = "Board Numbers/Game Rules Config")]
    public class GameRulesConfig : ScriptableObject
    {
        [Header("Scoring Points")]
        [Tooltip("Points awarded for forming a connected group that sums to 7")]
        public int sevenPoints = 3;

        [Tooltip("Points awarded for forming a connected group that sums to 11")]
        public int elevenPoints = 5;

        [Tooltip("Points awarded for forming exactly three connected 1s")]
        public int tripleOnePoints = 7;

        [Header("Game Parameters")]
        [Tooltip("Target score to win the game")]
        public int targetScore = 77;

        [Tooltip("Size of the game board (creates a square grid)")]
        public int boardSize = 7;

        [Tooltip("Number of tiles each player holds in hand")]
        public int handSize = 3;

        [Header("Adjacency Rules")]
        [Tooltip("Use 8-way adjacency (orthogonal + diagonal) for connected groups")]
        public bool eightWayAdjacency = true;

        [Header("Bonus Rules")]
        [Tooltip("Drawing a tile when forming a group of 7")]
        public bool sevenBonusDrawTile = true;

        [Tooltip("Taking an extra turn when forming a group of 11")]
        public bool elevenBonusExtraTurn = true;

        [Tooltip("Taking an extra turn when forming exactly three connected 1s")]
        public bool tripleOneBonusExtraTurn = true;

        [Header("Player Settings")]
        [Tooltip("Minimum number of players")]
        public int minPlayers = 2;

        [Tooltip("Maximum number of players")]
        public int maxPlayers = 4;

        /// <summary>
        /// Validate the game rules configuration
        /// </summary>
        public bool IsValid()
        {
            return targetScore > 0 && 
                   boardSize > 0 && 
                   handSize > 0 && 
                   minPlayers >= 2 && 
                   maxPlayers >= minPlayers &&
                   sevenPoints >= 0 &&
                   elevenPoints >= 0 &&
                   tripleOnePoints >= 0;
        }

        /// <summary>
        /// Get the total number of board cells
        /// </summary>
        public int TotalBoardCells => boardSize * boardSize;
    }
}