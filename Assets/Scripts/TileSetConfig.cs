using System.Collections.Generic;
using UnityEngine;

namespace BoardNumbers
{
    /// <summary>
    /// Configuration for tile deck composition and distribution
    /// </summary>
    [CreateAssetMenu(fileName = "TileSetConfig", menuName = "Board Numbers/Tile Set Config")]
    public class TileSetConfig : ScriptableObject
    {
        [Header("Deck Distribution")]
        [Tooltip("Dictionary mapping tile values to their quantities in the deck")]
        public SerializableDictionary<int, int> tileDistribution = new SerializableDictionary<int, int>
        {
            { 1, 8 },   // value 1: 8 tiles
            { 2, 5 },   // values 2-6: 5 each
            { 3, 5 },
            { 4, 5 },
            { 5, 5 },
            { 6, 5 },
            { 7, 4 },   // values 7-11: 4 each
            { 8, 4 },
            { 9, 4 },
            { 10, 4 },
            { 11, 4 }
        };

        /// <summary>
        /// Get total number of tiles in deck
        /// </summary>
        public int TotalTileCount
        {
            get
            {
                int total = 0;
                foreach (var kvp in tileDistribution)
                {
                    total += kvp.Value;
                }
                return total;
            }
        }

        /// <summary>
        /// Validate the tile distribution configuration
        /// </summary>
        public bool IsValid()
        {
            foreach (var kvp in tileDistribution)
            {
                if (kvp.Key < 1 || kvp.Key > 11 || kvp.Value < 0)
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Serializable dictionary for Unity Inspector
    /// </summary>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            this.Clear();
            if (keys.Count != values.Count)
                throw new System.Exception($"Keys count {keys.Count} != Values count {values.Count}");

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }
}