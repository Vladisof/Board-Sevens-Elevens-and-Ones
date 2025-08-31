using System.Collections.Generic;
using UnityEngine;

namespace BoardNumbers
{
    /// <summary>
    /// Player data container - POCO for player state management
    /// </summary>
    [System.Serializable]
    public class PlayerController
    {
        [Header("Player Identity")]
        [SerializeField] private int playerId;
        [SerializeField] private Color playerColor;
        [SerializeField] private string playerName;

        [Header("Game State")]
        [SerializeField] private List<int> hand;
        [SerializeField] private int score;
        [SerializeField] private bool hasExtraTurn;

        // Properties for easy access
        public int PlayerId => playerId;
        public Color PlayerColor => playerColor;
        public string PlayerName => playerName;
        public List<int> Hand => hand;
        public int Score => score;
        public bool HasExtraTurn => hasExtraTurn;
        public int HandCount => hand?.Count ?? 0;

        /// <summary>
        /// Constructor for creating a new player
        /// </summary>
        public PlayerController(int id, Color color, string name = null)
        {
            playerId = id;
            playerColor = color;
            playerName = string.IsNullOrEmpty(name) ? $"Player {id + 1}" : name;
            hand = new List<int>();
            score = 0;
            hasExtraTurn = false;
        }

        /// <summary>
        /// Add a tile to the player's hand
        /// </summary>
        public void AddTileToHand(int tileValue)
        {
            hand.Add(tileValue);
        }

        /// <summary>
        /// Remove a tile from the player's hand
        /// </summary>
        public bool RemoveTileFromHand(int tileValue)
        {
            return hand.Remove(tileValue);
        }

        /// <summary>
        /// Remove a tile from hand by index
        /// </summary>
        public int RemoveTileFromHandAt(int index)
        {
            if (index >= 0 && index < hand.Count)
            {
                int tile = hand[index];
                hand.RemoveAt(index);
                return tile;
            }
            return -1; // Invalid index
        }

        /// <summary>
        /// Check if player has a specific tile in hand
        /// </summary>
        public bool HasTile(int tileValue)
        {
            return hand.Contains(tileValue);
        }

        /// <summary>
        /// Add points to the player's score
        /// </summary>
        public void AddScore(int points)
        {
            score += points;
        }

        /// <summary>
        /// Set extra turn flag
        /// </summary>
        public void SetExtraTurn(bool extraTurn)
        {
            hasExtraTurn = extraTurn;
        }

        /// <summary>
        /// Clear the player's hand
        /// </summary>
        public void ClearHand()
        {
            hand.Clear();
        }

        /// <summary>
        /// Get a copy of the hand for safe iteration
        /// </summary>
        public List<int> GetHandCopy()
        {
            return new List<int>(hand);
        }

        /// <summary>
        /// Reset player state for new game
        /// </summary>
        public void ResetForNewGame()
        {
            hand.Clear();
            score = 0;
            hasExtraTurn = false;
        }

        /// <summary>
        /// Check if player has won based on target score
        /// </summary>
        public bool HasWon(int targetScore)
        {
            return score >= targetScore;
        }

        public override string ToString()
        {
            return $"{playerName} (ID: {playerId}, Score: {score}, Hand: {HandCount} tiles)";
        }
    }
}