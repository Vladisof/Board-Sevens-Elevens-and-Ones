using System.Collections.Generic;
using UnityEngine;

namespace BoardNumbers
{
    /// <summary>
    /// Manages the 7x7 game board grid and tile placement
    /// </summary>
    public class BoardGrid
    {
        private int[,] grid;
        private int boardSize;
        private Vector2Int[] eightWayDirections = {
            new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1),
            new Vector2Int(0, -1),                          new Vector2Int(0, 1),
            new Vector2Int(1, -1),  new Vector2Int(1, 0),  new Vector2Int(1, 1)
        };

        public int BoardSize => boardSize;
        public int TotalCells => boardSize * boardSize;

        /// <summary>
        /// Constructor
        /// </summary>
        public BoardGrid(int size = 7)
        {
            boardSize = size;
            grid = new int[boardSize, boardSize];
            ClearBoard();
        }

        /// <summary>
        /// Clear the entire board (set all cells to 0 - empty)
        /// </summary>
        public void ClearBoard()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    grid[x, y] = 0; // 0 represents empty cell
                }
            }
        }

        /// <summary>
        /// Check if a position is valid (within board bounds)
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
        }

        /// <summary>
        /// Check if a position is valid (Vector2Int version)
        /// </summary>
        public bool IsValidPosition(Vector2Int pos)
        {
            return IsValidPosition(pos.x, pos.y);
        }

        /// <summary>
        /// Check if a cell is empty
        /// </summary>
        public bool IsCellEmpty(int x, int y)
        {
            return IsValidPosition(x, y) && grid[x, y] == 0;
        }

        /// <summary>
        /// Check if a cell is empty (Vector2Int version)
        /// </summary>
        public bool IsCellEmpty(Vector2Int pos)
        {
            return IsCellEmpty(pos.x, pos.y);
        }

        /// <summary>
        /// Get the tile value at a position
        /// </summary>
        public int GetTileAt(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return -1; // Invalid position

            return grid[x, y];
        }

        /// <summary>
        /// Get the tile value at a position (Vector2Int version)
        /// </summary>
        public int GetTileAt(Vector2Int pos)
        {
            return GetTileAt(pos.x, pos.y);
        }

        /// <summary>
        /// Place a tile on the board
        /// </summary>
        public bool PlaceTile(int x, int y, int tileValue)
        {
            if (!IsValidPosition(x, y))
            {
                Debug.LogWarning($"Invalid position for tile placement: ({x}, {y})");
                return false;
            }

            if (!IsCellEmpty(x, y))
            {
                Debug.LogWarning($"Cell ({x}, {y}) is not empty. Current value: {grid[x, y]}");
                return false;
            }

            if (tileValue < 1 || tileValue > 11)
            {
                Debug.LogWarning($"Invalid tile value: {tileValue}");
                return false;
            }

            grid[x, y] = tileValue;
            Debug.Log($"Placed tile {tileValue} at ({x}, {y})");
            return true;
        }

        /// <summary>
        /// Place a tile on the board (Vector2Int version)
        /// </summary>
        public bool PlaceTile(Vector2Int pos, int tileValue)
        {
            return PlaceTile(pos.x, pos.y, tileValue);
        }

        /// <summary>
        /// Remove a tile from the board (set to empty)
        /// </summary>
        public bool RemoveTile(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return false;

            grid[x, y] = 0; // 0 represents empty
            return true;
        }

        /// <summary>
        /// Remove a tile from the board (Vector2Int version)
        /// </summary>
        public bool RemoveTile(Vector2Int pos)
        {
            return RemoveTile(pos.x, pos.y);
        }

        /// <summary>
        /// Get all neighboring positions (8-way adjacency)
        /// </summary>
        public List<Vector2Int> GetNeighbors(int x, int y)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            Vector2Int currentPos = new Vector2Int(x, y);

            foreach (Vector2Int direction in eightWayDirections)
            {
                Vector2Int neighborPos = currentPos + direction;
                if (IsValidPosition(neighborPos))
                {
                    neighbors.Add(neighborPos);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Get all neighboring positions (Vector2Int version)
        /// </summary>
        public List<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            return GetNeighbors(pos.x, pos.y);
        }

        /// <summary>
        /// Find all connected groups that include the specified position
        /// </summary>
        public List<List<Vector2Int>> FindConnectedGroups(Vector2Int startPos)
        {
            if (!IsValidPosition(startPos) || IsCellEmpty(startPos))
                return new List<List<Vector2Int>>();

            List<List<Vector2Int>> allGroups = new List<List<Vector2Int>>();
            bool[,] visited = new bool[boardSize, boardSize];

            // Find the connected group containing the start position
            List<Vector2Int> group = new List<Vector2Int>();
            FloodFill(startPos, visited, group);
            
            if (group.Count >= 3) // Only consider groups of 3 or more tiles
            {
                allGroups.Add(group);
            }

            return allGroups;
        }

        /// <summary>
        /// Flood fill algorithm to find connected tiles
        /// </summary>
        private void FloodFill(Vector2Int pos, bool[,] visited, List<Vector2Int> group)
        {
            if (!IsValidPosition(pos) || visited[pos.x, pos.y] || IsCellEmpty(pos))
                return;

            visited[pos.x, pos.y] = true;
            group.Add(pos);

            // Check all 8 neighbors
            foreach (Vector2Int neighbor in GetNeighbors(pos))
            {
                if (!visited[neighbor.x, neighbor.y] && !IsCellEmpty(neighbor))
                {
                    FloodFill(neighbor, visited, group);
                }
            }
        }

        /// <summary>
        /// Calculate the sum of tiles in a group
        /// </summary>
        public int CalculateGroupSum(List<Vector2Int> group)
        {
            int sum = 0;
            foreach (Vector2Int pos in group)
            {
                sum += GetTileAt(pos);
            }
            return sum;
        }

        /// <summary>
        /// Check if the board is full
        /// </summary>
        public bool IsBoardFull()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (grid[x, y] == 0)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get count of empty cells
        /// </summary>
        public int GetEmptyCellCount()
        {
            int count = 0;
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (grid[x, y] == 0)
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Get all empty positions
        /// </summary>
        public List<Vector2Int> GetEmptyPositions()
        {
            List<Vector2Int> emptyPositions = new List<Vector2Int>();
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (grid[x, y] == 0)
                        emptyPositions.Add(new Vector2Int(x, y));
                }
            }
            return emptyPositions;
        }

        /// <summary>
        /// Get a copy of the current board state
        /// </summary>
        public int[,] GetBoardCopy()
        {
            int[,] copy = new int[boardSize, boardSize];
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    copy[x, y] = grid[x, y];
                }
            }
            return copy;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Board {boardSize}x{boardSize}:");
            
            for (int y = boardSize - 1; y >= 0; y--) // Top to bottom display
            {
                for (int x = 0; x < boardSize; x++)
                {
                    sb.Append(grid[x, y].ToString().PadLeft(3));
                }
                sb.AppendLine();
            }
            
            sb.AppendLine($"Empty cells: {GetEmptyCellCount()}/{TotalCells}");
            return sb.ToString();
        }
    }
}