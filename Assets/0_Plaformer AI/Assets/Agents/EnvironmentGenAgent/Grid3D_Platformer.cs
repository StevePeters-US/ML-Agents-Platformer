using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Debug = UnityEngine.Debug;

namespace APG.Environment {

    public class Grid3D_Platformer : Grid3D_Abstract {
        [SerializeField] private Vector3Int Grid3DSize = new Vector3Int(10, 1, 10);

        void Awake() {
            // Start using the max rows and columns, but we'll update the current size at the start of each episode.
            m_Grid3DData = new Grid3DData {
                GridSize = Grid3DSize,
                GridNodes = new Node_3D[Grid3DSize.x, Grid3DSize.y, Grid3DSize.z]
            };

            tex.Resize(Grid3DSize.x, Grid3DSize.z);

            for (int x = 0; x < Grid3DSize.x; x++) {
                for (int y = 0; y < Grid3DSize.y; y++) {
                    for (int z = 0; z < Grid3DSize.z; z++) {
                        Vector3Int gridIndex = new Vector3Int(x, y, z);
                        Vector3 tileOffset = m_Grid3DData.tileSize * 0.5f;
                        Vector3 worldPos = transform.position + m_Grid3DData.tileSize.MultiplyInt(gridIndex) + tileOffset;

                        m_Grid3DData.GridNodes[x, y, z] = new Node_3D(worldPos, gridIndex, NodeType.Empty);
                        m_Grid3DData.GridNodes[x, y, z].SetNeighborIndices(m_Grid3DData.GridSize, m_Grid3DData.useManhattanNeighbors);
                    }
                }
            }
        }

        public void GenerateNewGrid() {
            Debug.Log("Generating new 3D grid");

            // Clear old grid data
            ClearGridNodes();

            // Define start and goal
            Vector3Int startIndex = GetRandomIndex();
            m_Grid3DData.availableIndices.Remove(startIndex);
            m_Grid3DData.StartIndex = startIndex;
            m_Grid3DData.GridNodes[startIndex.x, startIndex.y, startIndex.z].NodeType = NodeType.Start;
            m_Grid3DData.GridNodes[startIndex.x, startIndex.y, startIndex.z].locked = true;

            Vector3Int goalIndex = GetRandomIndex();
            m_Grid3DData.availableIndices.Remove(goalIndex);
            m_Grid3DData.GoalIndex = goalIndex;
            m_Grid3DData.GridNodes[goalIndex.x, goalIndex.y, goalIndex.z].NodeType = NodeType.Goal;
            m_Grid3DData.GridNodes[goalIndex.x, goalIndex.y, goalIndex.z].locked = true;

            // Fill in other tiles
            FillGridWithRandomTiles();
        }     

        public override int GetMinPathLength() {
            return Astar.GetDistanceManhattan(GetStartNode(), GetGoalNode());
        }

        public override int GetMaxPathLength() {
            Debug.LogWarning("Get Max Path Length not properly implemented in grid3d_platformer", this);
            return 40;
        }

        // Path length is number of steps from start to goal, so it does not include the start tile
        public override int GetCurrentPathLength() {
            return Mathf.Max(0, pathIndices.Count - 1);
        }

        public override void UpdateRelativeEmptySpaceValue() {
            int numEmpty = 0;

            for (int x = 0; x < GetGridSize().x; x++) {
                for (int y = 0; y < GetGridSize().y; y++) {
                    for (int z = 0; z < GetGridSize().z; z++) {
                        if (GetGridNode(x, y, z).NodeType == NodeType.Empty)
                            numEmpty += 1;
                    }
                }
            }

            m_Grid3DData.relativeEmptySpace = (float)numEmpty / (float)Grid3DData.GridCount;
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
            m_Grid3DData.avgCohesion = totalCohesionValue / (m_Grid3DData.GridCount);
        }
    }
}
