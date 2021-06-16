using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Debug = UnityEngine.Debug;

namespace APG.Environment {

    public class Grid3D_Platformer : Grid3D_Abstract {
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
            m_CurrentGrid3DProperties = new Grid3DProperties {
                Rows = MaxRows,
                Columns = MaxColumns,
                NumCellTypes = NumCellTypes,
                NumSpecialTypes = NumSpecialTypes
            };
        }

        void Start() {
            m_Random = new System.Random(RandomSeed == -1 ? gameObject.GetInstanceID() : RandomSeed);
            InitRandom();
        }

        public override Grid3DProperties GetMaxBoardSize() {
            return new Grid3DProperties {
                Rows = MaxRows,
                Columns = MaxColumns,
                NumCellTypes = NumCellTypes,
                NumSpecialTypes = NumSpecialTypes
            };
        }

        public override Grid3DProperties GetCurrentBoardSize() {
            return m_CurrentGrid3DProperties;
        }

        /// <summary>
        /// Change the board size to a random size between the min and max rows and columns. This is
        /// cached so that the size is consistent until it is updated.
        /// This is just for an example; you can change your board size however you want.
        /// </summary>
        public void UpdateCurrentBoardSize() {
            var newRows = m_Random.Next(MinRows, MaxRows + 1);
            var newCols = m_Random.Next(MinColumns, MaxColumns + 1);
            m_CurrentGrid3DProperties.Rows = newRows;
            m_CurrentGrid3DProperties.Columns = newCols;
        }

       

        public override int GetCellType(int row, int col) {
            if (row >= m_CurrentGrid3DProperties.Rows || col >= m_CurrentGrid3DProperties.Columns) {
                throw new IndexOutOfRangeException();
            }
            return m_Cells[col, row].CellType;
        }

        public override int GetSpecialType(int row, int col) {
            if (row >= m_CurrentGrid3DProperties.Rows || col >= m_CurrentGrid3DProperties.Columns) {
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

    }
}
