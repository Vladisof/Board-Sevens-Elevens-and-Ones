using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BoardNumbers
{
    /// <summary>
    /// Service for recognizing scoring patterns and calculating bonuses
    /// </summary>
    public class PatternRecognitionService
    {
        private GameRulesConfig gameRules;
        private BoardGrid board;

        /// <summary>
        /// Result of pattern recognition for a placed tile
        /// </summary>
        public struct ScoringResult
        {
            public int totalPoints;
            public bool drawBonusTile;
            public bool extraTurn;
            public List<PatternMatch> matches;

            public ScoringResult(int points = 0, bool drawTile = false, bool bonus = false)
            {
                totalPoints = points;
                drawBonusTile = drawTile;
                extraTurn = bonus;
                matches = new List<PatternMatch>();
            }
        }

        /// <summary>
        /// Details of a single pattern match
        /// </summary>
        public struct PatternMatch
        {
            public PatternType type;
            public List<Vector2Int> positions;
            public int sum;
            public int points;

            public PatternMatch(PatternType patternType, List<Vector2Int> matchPositions, int groupSum, int pointsAwarded)
            {
                type = patternType;
                positions = new List<Vector2Int>(matchPositions);
                sum = groupSum;
                points = pointsAwarded;
            }
        }

        public enum PatternType
        {
            Seven,
            Eleven,
            TripleOne
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PatternRecognitionService(GameRulesConfig rules, BoardGrid gameBoard)
        {
            gameRules = rules;
            board = gameBoard;
        }

        /// <summary>
        /// Check for scoring patterns after placing a tile
        /// </summary>
        public ScoringResult CheckForPatterns(Vector2Int placedTilePosition)
        {
            ScoringResult result = new ScoringResult();
            
            if (!board.IsValidPosition(placedTilePosition) || board.IsCellEmpty(placedTilePosition))
            {
                Debug.LogWarning($"Invalid tile position for pattern check: {placedTilePosition}");
                return result;
            }

            // Find all connected groups that include the placed tile
            List<List<Vector2Int>> connectedGroups = board.FindConnectedGroups(placedTilePosition);
            
            foreach (var group in connectedGroups)
            {
                // Only check groups of 3 or more tiles
                if (group.Count < 3)
                    continue;

                // Check for different pattern types
                CheckTripleOnes(group, ref result);
                CheckElevens(group, ref result);
                CheckSevens(group, ref result);
            }

            Debug.Log($"Pattern check complete. Points: {result.totalPoints}, Draw: {result.drawBonusTile}, Extra turn: {result.extraTurn}");
            return result;
        }

        /// <summary>
        /// Check for triple ones pattern (exactly three connected 1s)
        /// </summary>
        private void CheckTripleOnes(List<Vector2Int> group, ref ScoringResult result)
        {
            if (group.Count != 3)
                return;

            // Check if all tiles are 1s
            bool allOnes = true;
            foreach (var pos in group)
            {
                if (board.GetTileAt(pos) != 1)
                {
                    allOnes = false;
                    break;
                }
            }

            if (allOnes)
            {
                PatternMatch match = new PatternMatch(
                    PatternType.TripleOne,
                    group,
                    3, // Sum of three 1s
                    gameRules.tripleOnePoints
                );

                result.matches.Add(match);
                result.totalPoints += gameRules.tripleOnePoints;
                
                if (gameRules.tripleOneBonusExtraTurn)
                {
                    result.extraTurn = true;
                }

                Debug.Log($"Triple ones pattern found! Points: {gameRules.tripleOnePoints}");
            }
        }

        /// <summary>
        /// Check for elevens pattern (connected group summing to 11)
        /// </summary>
        private void CheckElevens(List<Vector2Int> group, ref ScoringResult result)
        {
            int sum = board.CalculateGroupSum(group);
            
            if (sum == 11)
            {
                PatternMatch match = new PatternMatch(
                    PatternType.Eleven,
                    group,
                    sum,
                    gameRules.elevenPoints
                );

                result.matches.Add(match);
                result.totalPoints += gameRules.elevenPoints;
                
                if (gameRules.elevenBonusExtraTurn)
                {
                    result.extraTurn = true;
                }

                Debug.Log($"Elevens pattern found! Points: {gameRules.elevenPoints}");
            }
        }

        /// <summary>
        /// Check for sevens pattern (connected group summing to 7)
        /// </summary>
        private void CheckSevens(List<Vector2Int> group, ref ScoringResult result)
        {
            int sum = board.CalculateGroupSum(group);
            
            if (sum == 7)
            {
                PatternMatch match = new PatternMatch(
                    PatternType.Seven,
                    group,
                    sum,
                    gameRules.sevenPoints
                );

                result.matches.Add(match);
                result.totalPoints += gameRules.sevenPoints;
                
                if (gameRules.sevenBonusDrawTile)
                {
                    result.drawBonusTile = true;
                }

                Debug.Log($"Sevens pattern found! Points: {gameRules.sevenPoints}");
            }
        }

        /// <summary>
        /// Process scoring results in the correct order: Triple One → Eleven → Seven
        /// </summary>
        public ScoringResult ProcessScoringInOrder(Vector2Int placedTilePosition)
        {
            ScoringResult result = CheckForPatterns(placedTilePosition);
            
            // Sort matches by priority: TripleOne first, then Eleven, then Seven
            result.matches.Sort((a, b) => {
                int priorityA = GetPatternPriority(a.type);
                int priorityB = GetPatternPriority(b.type);
                return priorityA.CompareTo(priorityB);
            });

            return result;
        }

        /// <summary>
        /// Get pattern priority for sorting (lower number = higher priority)
        /// </summary>
        private int GetPatternPriority(PatternType type)
        {
            switch (type)
            {
                case PatternType.TripleOne: return 1;
                case PatternType.Eleven: return 2;
                case PatternType.Seven: return 3;
                default: return 999;
            }
        }

        /// <summary>
        /// Check if a tile placement would create any scoring patterns (for AI/hints)
        /// </summary>
        public ScoringResult PreviewPlacement(Vector2Int position, int tileValue)
        {
            // Temporarily place the tile
            bool wasEmpty = board.IsCellEmpty(position);
            if (!wasEmpty)
                return new ScoringResult(); // Can't place on occupied cell

            board.PlaceTile(position, tileValue);
            
            // Check patterns
            ScoringResult result = CheckForPatterns(position);
            
            // Remove the tile (undo the temporary placement)
            board.RemoveTile(position);
            
            return result;
        }

        /// <summary>
        /// Get all possible scoring moves for the current board state
        /// </summary>
        public List<(Vector2Int position, int tileValue, ScoringResult result)> GetAllScoringMoves(List<int> availableTiles)
        {
            var scoringMoves = new List<(Vector2Int, int, ScoringResult)>();
            var emptyPositions = board.GetEmptyPositions();

            foreach (var position in emptyPositions)
            {
                foreach (int tile in availableTiles)
                {
                    var result = PreviewPlacement(position, tile);
                    if (result.totalPoints > 0 || result.drawBonusTile || result.extraTurn)
                    {
                        scoringMoves.Add((position, tile, result));
                    }
                }
            }

            return scoringMoves;
        }
    }
}