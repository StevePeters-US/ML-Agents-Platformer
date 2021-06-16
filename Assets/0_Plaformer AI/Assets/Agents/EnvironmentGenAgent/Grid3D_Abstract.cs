using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public class Grid3DData {

        public Vector3Int GridSize { get => gridSize; set => gridSize = value; }
        private Vector3Int gridSize;

        public int GridCount { get => gridSize.x * gridSize.y * gridSize.z; }

        private Vector3 gridPos;

        public Node_3D[,,] GridNodes { get => gridNodes; }
        private Node_3D[,,] gridNodes;

        public Vector3Int StartIndex { get => startIndex; set => startIndex = value; }
        Vector3Int startIndex;

        Vector3Int goalIndex;
        public Vector3Int GoalIndex { get => goalIndex; set => goalIndex = value; }

        Vector3 tileSize;//= Vector3.one;
        public Vector3 TileSize { get => tileSize; set => tileSize = value; }

        public List<Node_3D> path = new List<Node_3D>();
        // Path length not including start and goal tiles
        public int GetPathLength { get => Mathf.Max(0, path.Count - 2); }

        /// Number of rows on the board
        public int Rows;

        /// Number of columns on the board
        public int Columns;

        /// Maximum number of different types of cells (colors, pieces, etc).
        public int NumCellTypes;

        /// Maximum number of special types. This can be zero, in which case
        /// all cells of the same type are assumed to be equivalent.
        public int NumSpecialTypes;

        [Range(0, 1)] public float relativeEmptySpace;
        [Range(0, 1)] public float avgCohesion;

        public override string ToString() {
            return
                $"Rows: {Rows}, Columns: {Columns}, NumCellTypes: {NumCellTypes}, NumSpecialTypes: {NumSpecialTypes}";
        }
    }

 /*   public struct Grid3DPropertiesStruct {

        public Vector3Int GridSize { get => gridSize; set => gridSize = value; }
        private Vector3Int gridSize;

        public int GridCount { get => gridSize.x * gridSize.y * gridSize.z; }

        private Vector3 gridPos;

        public Node_3D[,,] GridNodes { get => gridNodes; }
        private Node_3D[,,] gridNodes;

        public Vector3Int StartIndex { get => startIndex; set => startIndex = value; }
        Vector3Int startIndex;

        Vector3Int goalIndex;
        public Vector3Int GoalIndex { get => goalIndex; set => goalIndex = value; }

        Vector3 tileSize;//= Vector3.one;
        public Vector3 TileSize { get => tileSize; set => tileSize = value; }

        public List<Node_3D> path;// = new List<Node_3D>();
        // Path length not including start and goal tiles
        public int GetPathLength { get => Mathf.Max(0, path.Count - 2); }

        /// Number of rows on the board
        public int Rows;

        /// Number of columns on the board
        public int Columns;

        /// Maximum number of different types of cells (colors, pieces, etc).
        public int NumCellTypes;

        /// Maximum number of special types. This can be zero, in which case
        /// all cells of the same type are assumed to be equivalent.
        public int NumSpecialTypes;

        [Range(0,1)] public float relativeEmptySpace;
        [Range(0, 1)] public float avgCohesion;

        /// Check that all fields of the left-hand BoardSize are less than or equal to the field of the right-hand BoardSize
        public static bool operator <=(Grid3DPropertiesStruct lhs, Grid3DPropertiesStruct rhs) {
            return lhs.Rows <= rhs.Rows && lhs.Columns <= rhs.Columns && lhs.NumCellTypes <= rhs.NumCellTypes &&
                   lhs.NumSpecialTypes <= rhs.NumSpecialTypes;
        }

        /// Check that all fields of the left-hand BoardSize are greater than or equal to the field of the right-hand BoardSize
        public static bool operator >=(Grid3DPropertiesStruct lhs, Grid3DPropertiesStruct rhs) {
            return lhs.Rows >= rhs.Rows && lhs.Columns >= rhs.Columns && lhs.NumCellTypes >= rhs.NumCellTypes &&
                   lhs.NumSpecialTypes >= rhs.NumSpecialTypes;
        }

        public override string ToString() {
            return
                $"Rows: {Rows}, Columns: {Columns}, NumCellTypes: {NumCellTypes}, NumSpecialTypes: {NumSpecialTypes}";
        }
    }*/
 
    public abstract class Grid3D_Abstract : MonoBehaviour {

        protected Grid3DData m_CurrentGrid3DData;
        public Grid3DData CurrentGrid3DProperties { get => m_CurrentGrid3DData; }

        /// Return the maximum size of the board. This is used to determine the size of observations and actions,
        /// so the returned values must not change.
        /// 
        public abstract Grid3DData GetMaxBoardSize();

        /// Return the current size of the board. The values must less than or equal to the values returned from
        /// GetMaxBoardSize().
        /// By default, this will return GetMaxBoardSize(); if your board doesn't change size, you don't need to override it.
        public virtual Grid3DData GetCurrentBoardSize() {
            return GetMaxBoardSize();
        }

        /// Returns the "color" of the piece at the given row and column.
        /// This should be between 0 and NumCellTypes-1 (inclusive).
        /// The actual order of the values doesn't matter.
        public abstract int GetCellType(int row, int col);
        public abstract int GetMinPathLength();
        public abstract int GetMaxPathLength();

        public abstract int GetCurrentPathLength();

        // 0 - 1 num tiles / num empty tiles
        public abstract void UpdateRelativeEmptySpaceValue();
        public abstract void UpdateCohesionValues();

        /// Returns the special type of the piece at the given row and column.
        /// This should be between 0 and NumSpecialTypes (inclusive).
        /// The actual order of the values doesn't matter.
        public abstract int GetSpecialType(int row, int col);

        public Vector3Int GetGridSize() {
            return m_CurrentGrid3DData.GridSize;
        }

        public Node_3D GetGridNode(int x, int y, int z) {
            return m_CurrentGrid3DData.GridNodes[x, y, z];
        }

        public Vector3Int GetStartPosition() {
            return m_CurrentGrid3DData.StartIndex;
        }

        public Node_3D GetStartNode() {
            return m_CurrentGrid3DData.GridNodes[GetStartPosition().x, GetStartPosition().y, GetStartPosition().z];
        }

        public Vector3Int GetGoalPosition() {
            return m_CurrentGrid3DData.GoalIndex;
        }

        public Node_3D GetGoalNode() {
            return m_CurrentGrid3DData.GridNodes[GetGoalPosition().x, GetGoalPosition().y, GetGoalPosition().z];
        }


        /// Make sure that the current BoardSize isn't larger than the original value of GetMaxBoardSize().
        /// If it is, log a warning.
       // [System.Diagnostics.Conditional("DEBUG")]
       /* internal void CheckBoardSizes(Grid3DData originalMaxBoardSize) {
            var currentBoardSize = GetCurrentBoardSize();
            if (!(currentBoardSize <= originalMaxBoardSize)) {
                Debug.LogWarning(
                    "Current BoardSize is larger than maximum board size was on initialization. This may cause unexpected results.\n" +
                    $"Original GetMaxBoardSize() result: {originalMaxBoardSize}\n" +
                    $"GetCurrentBoardSize() result: {currentBoardSize}"
                );
            }
        }*/
    }
}
