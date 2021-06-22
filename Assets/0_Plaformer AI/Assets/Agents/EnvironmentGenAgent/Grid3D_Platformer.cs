using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Debug = UnityEngine.Debug;

namespace APG.Environment {

    public class Grid3D_Platformer : Grid3D_Abstract {
        [SerializeField] private Vector3Int Grid3DSize = new Vector3Int(10, 1, 10);


        public int MinRows;
        public int MaxRows;

        public int MinColumns;
        public int MaxColumns;

        public int NumCellTypes;
        public int NumSpecialTypes;

        public const int k_EmptyCell = -1;
        [Tooltip("Points earned for clearing a basic cell (cube)")]
        public int BasicCellPoints = 1;

        [Tooltip("Points earned for clearing a special cell (sphere)")]
        public int SpecialCell1Points = 2;

        [Tooltip("Points earned for clearing an extra special cell (plus)")]
        public int SpecialCell2Points = 3;

        /// <summary>
        /// Seed to initialize the <see cref="System.Random"/> object.
        /// </summary>
        public int RandomSeed;

        (int CellType, int SpecialType)[,] m_Cells;
        bool[,] m_Matched;



        System.Random m_Random;

        void Awake() {
            m_Cells = new (int, int)[MaxColumns, MaxRows];
            m_Matched = new bool[MaxColumns, MaxRows];

            // Start using the max rows and columns, but we'll update the current size at the start of each episode.
            m_CurrentGrid3DData = new Grid3DData {
                GridSize = Grid3DSize,
                GridNodes = new Node_3D[Grid3DSize.x, Grid3DSize.y, Grid3DSize.z],//  GetNewNodes(Grid3DSize),

                //Rows = MaxRows,
                //Columns = MaxColumns,
                NumCellTypes = NumCellTypes,
                NumSpecialTypes = NumSpecialTypes
            };

            for (int x = 0; x < Grid3DSize.x; x++) {
                for (int y = 0; y < Grid3DSize.y; y++) {
                    for (int z = 0; z < Grid3DSize.z; z++) {
                        Vector3Int gridIndex = new Vector3Int(x, y, z);
                        Vector3 tileOffset = m_CurrentGrid3DData.tileSize * 0.5f;
                        Vector3 worldPos = transform.position + m_CurrentGrid3DData.tileSize.MultiplyInt(gridIndex) + tileOffset;

                        m_CurrentGrid3DData.GridNodes[x, y, z] = new Node_3D(worldPos, gridIndex, NodeGridType.Empty);
                        m_CurrentGrid3DData.GridNodes[x, y, z].SetNeighborIndices(m_CurrentGrid3DData.GridSize, m_CurrentGrid3DData.useManhattanNeighbors);

                        // m_CurrentGrid3DData.GridNodes[x, y, z] = new Node_3D(Vector3.zero, new Vector3Int(x, y, z), NodeGridType.Tile);
                    }
                }
            }
        }

        void Start() {
            m_Random = new System.Random(RandomSeed == -1 ? gameObject.GetInstanceID() : RandomSeed);
            InitRandom();
        }

        public void GenerateNewGrid() {
            Debug.Log("Generating new 3D grid");

            // Clear old grid data
            ClearGridNodes();

            // Define start and goal
            Vector3Int startIndex = GetRandomIndex();
            m_CurrentGrid3DData.availableIndices.Remove(startIndex);
            m_CurrentGrid3DData.StartIndex = startIndex;
            m_CurrentGrid3DData.GridNodes[startIndex.x, startIndex.y, startIndex.z].NodeType = NodeGridType.Start;
            m_CurrentGrid3DData.GridNodes[startIndex.x, startIndex.y, startIndex.z].locked = true;

            Vector3Int goalIndex = GetRandomIndex();
            m_CurrentGrid3DData.availableIndices.Remove(goalIndex);
            m_CurrentGrid3DData.GoalIndex = goalIndex;
            m_CurrentGrid3DData.GridNodes[goalIndex.x, goalIndex.y, goalIndex.z].NodeType = NodeGridType.Goal;
            m_CurrentGrid3DData.GridNodes[goalIndex.x, goalIndex.y, goalIndex.z].locked = true;

            // Fill in other tiles

            /*  Vector3 gridOffset = new Vector3(-(gridSize.x / 2) * tileSize.x, 0, -(gridSize.z / 2) * tileSize.z);
              grid = new Grid_3D(gridSize, gridOffset + transform.position, tileSize);
              grid.CreateGrid(true);
              grid.FillGridWithRandomTiles(randomTileChance);*/
        }

        public override Grid3DData GetMaxBoardSize() {
            return new Grid3DData {
                //Rows = MaxRows,
                //Columns = MaxColumns,
                NumCellTypes = NumCellTypes,
                NumSpecialTypes = NumSpecialTypes
            };
        }

        public override Grid3DData GetCurrentBoardSize() {
            return m_CurrentGrid3DData;
        }

        /// <summary>
        /// Change the board size to a random size between the min and max rows and columns. This is
        /// cached so that the size is consistent until it is updated.
        /// This is just for an example; you can change your board size however you want.
        /// </summary>
     /*   public void UpdateCurrentBoardSize() {
            var newRows = m_Random.Next(MinRows, MaxRows + 1);
            var newCols = m_Random.Next(MinColumns, MaxColumns + 1);
            m_CurrentGrid3DData.Rows = newRows;
            m_CurrentGrid3DData.Columns = newCols;
        }*/



        public override int GetCellType(int row, int col) {
            if (row >= m_CurrentGrid3DData.GridSize.z || col >= m_CurrentGrid3DData.GridSize.x) {
                throw new IndexOutOfRangeException();
            }
            return m_Cells[col, row].CellType;
        }

        public override int GetSpecialType(int row, int col) {
            if (row >= m_CurrentGrid3DData.GridSize.z || col >= m_CurrentGrid3DData.GridSize.x) {
                throw new IndexOutOfRangeException();
            }
            return m_Cells[col, row].SpecialType;
        }

        public (int, int)[,] Cells {
            get { return m_Cells; }
        }

        public bool[,] Matched {
            get { return m_Matched; }
        }

        // Initialize the board to random values.
        public void InitRandom() {
            for (var i = 0; i < MaxRows; i++) {
                for (var j = 0; j < MaxColumns; j++) {
                    m_Cells[j, i] = (GetRandomCellType(), GetRandomSpecialType());
                }
            }
        }



        void ClearMarked() {
            for (var i = 0; i < MaxRows; i++) {
                for (var j = 0; j < MaxColumns; j++) {
                    m_Matched[j, i] = false;
                }
            }
        }

        int GetRandomCellType() {
            return m_Random.Next(0, NumCellTypes);
        }

        int GetRandomSpecialType() {
            // 1 in N chance to get a type-2 special
            // 2 in N chance to get a type-1 special
            // otherwise 0 (boring)
            var N = 10;
            var val = m_Random.Next(0, N);
            if (val == 0) {
                return 2;
            }

            if (val <= 2) {
                return 1;
            }

            return 0;
        }

        public override int GetMinPathLength() {
            return Astar.GetDistanceManhattan(GetStartNode(), GetGoalNode());
        }

        public override int GetMaxPathLength() {
            Debug.LogWarning("Get Max Path Length not properly implemented in grid3d_platformer", this);
            return 40;
        }

        // Path length not including start and goal tiles
        public override int GetCurrentPathLength() {
            return Mathf.Max(0, pathIndices.Count - 2);
        }

        public override void UpdateRelativeEmptySpaceValue() {
            int numEmpty = 0;

            for (int x = 0; x < GetGridSize().x; x++) {
                for (int y = 0; y < GetGridSize().y; y++) {
                    for (int z = 0; z < GetGridSize().z; z++) {
                        if (GetGridNode(x, y, z).NodeType == NodeGridType.Empty)
                            numEmpty += 1;
                    }
                }
            }

            m_CurrentGrid3DData.relativeEmptySpace = (float)numEmpty / (float)CurrentGrid3DData.GridCount;
        }

        public override void UpdateCohesionValues() {
            float totalCohesionValue = 0;

            for (int x = 0; x < GetGridSize().x; x++) {
                for (int y = 0; y < GetGridSize().y; y++) {
                    for (int z = 0; z < GetGridSize().z; z++) {
                        float cohesiveValue = 0;
                        for (int i = 0; i < GetGridNode(x, y, z).allNeighborIndices.Count; i++) {
                            Vector3Int nodeIndex = GetGridNode(x, y, z).allNeighborIndices[i];
                            if (GetGridNode(x, y, z).NodeType == GetGridNode(nodeIndex.x, nodeIndex.y, nodeIndex.z).NodeType)
                                cohesiveValue += 1f / GetGridNode(x, y, z).allNeighborIndices.Count;
                        }

                        GetGridNode(x, y, z).cohesiveValue = cohesiveValue;

                        totalCohesionValue += cohesiveValue;
                    }
                }
            }
            m_CurrentGrid3DData.avgCohesion = totalCohesionValue / (m_CurrentGrid3DData.GridCount);
        }
    }
}
