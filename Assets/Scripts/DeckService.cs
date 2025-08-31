using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BoardNumbers
{
    /// <summary>
    /// Service for managing the tile deck - building, shuffling, and drawing
    /// </summary>
    public class DeckService
    {
        private List<int> deck;
        private TileSetConfig tileConfig;
        private System.Random random;

        public int RemainingTiles => deck?.Count ?? 0;
        public bool IsEmpty => RemainingTiles == 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public DeckService(TileSetConfig config, int? seed = null)
        {
            tileConfig = config;
            random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
            BuildDeck();
        }

        /// <summary>
        /// Build the deck from the tile configuration
        /// </summary>
        public void BuildDeck()
        {
            if (tileConfig == null)
            {
                Debug.LogError("TileSetConfig is null. Cannot build deck.");
                return;
            }

            deck = new List<int>();

            // Add tiles according to configuration
            foreach (var kvp in tileConfig.tileDistribution)
            {
                int tileValue = kvp.Key;
                int quantity = kvp.Value;

                for (int i = 0; i < quantity; i++)
                {
                    deck.Add(tileValue);
                }
            }

            Debug.Log($"Built deck with {deck.Count} tiles");
        }

        /// <summary>
        /// Shuffle the deck using Fisher-Yates algorithm
        /// </summary>
        public void ShuffleDeck()
        {
            if (deck == null || deck.Count <= 1)
                return;

            for (int i = deck.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(0, i + 1);
                int temp = deck[i];
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }

            Debug.Log($"Shuffled deck with {deck.Count} tiles");
        }

        /// <summary>
        /// Draw a single tile from the top of the deck
        /// </summary>
        public int DrawTile()
        {
            if (deck == null || deck.Count == 0)
            {
                Debug.LogWarning("Cannot draw tile: deck is empty");
                return -1; // Invalid tile value
            }

            int drawnTile = deck[0];
            deck.RemoveAt(0);
            
            Debug.Log($"Drew tile: {drawnTile}. Remaining tiles: {deck.Count}");
            return drawnTile;
        }

        /// <summary>
        /// Draw multiple tiles from the deck
        /// </summary>
        public List<int> DrawTiles(int count)
        {
            List<int> drawnTiles = new List<int>();

            for (int i = 0; i < count && !IsEmpty; i++)
            {
                int tile = DrawTile();
                if (tile != -1)
                {
                    drawnTiles.Add(tile);
                }
            }

            return drawnTiles;
        }

        /// <summary>
        /// Peek at the top tile without drawing it
        /// </summary>
        public int PeekTopTile()
        {
            if (deck == null || deck.Count == 0)
                return -1;

            return deck[0];
        }

        /// <summary>
        /// Get the current deck composition for debugging
        /// </summary>
        public Dictionary<int, int> GetDeckComposition()
        {
            if (deck == null)
                return new Dictionary<int, int>();

            return deck.GroupBy(tile => tile)
                      .ToDictionary(group => group.Key, group => group.Count());
        }

        /// <summary>
        /// Reset and rebuild the deck
        /// </summary>
        public void ResetDeck()
        {
            BuildDeck();
            ShuffleDeck();
        }

        /// <summary>
        /// Add a tile back to the deck (for testing or special rules)
        /// </summary>
        public void AddTileBack(int tileValue)
        {
            if (deck == null)
                deck = new List<int>();

            deck.Add(tileValue);
        }

        /// <summary>
        /// Check if we can draw the requested number of tiles
        /// </summary>
        public bool CanDraw(int count)
        {
            return RemainingTiles >= count;
        }

        public override string ToString()
        {
            var composition = GetDeckComposition();
            var compositionStr = string.Join(", ", composition.Select(kvp => $"{kvp.Key}x{kvp.Value}"));
            return $"Deck: {RemainingTiles} tiles ({compositionStr})";
        }
    }
}