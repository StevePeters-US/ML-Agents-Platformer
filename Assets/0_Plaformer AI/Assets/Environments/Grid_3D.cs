using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    // Based on Sebastion Lague https://www.youtube.com/watch?v=nhiFx28e7JY
    // https://pastebin.com/vCVCpsQc
    public class Grid_3D {

        public Vector3Int GridSize { get => gridSize; set => gridSize = value; }
        private Vector3Int gridSize;

        private Vector3 gridPos;

        public Node_3D[,,] GridNodes { get => gridNodes; }
        private Node_3D[,,] gridNodes;

        public Vector3Int StartIndex { get => startIndex; set => startIndex = value; }
        Vector3Int startIndex;

        Vector3Int goalIndex;
        public Vector3Int GoalIndex { get => goalIndex; set => goalIndex = value; }

        Vector3 tileSize = Vector3.one;
        public Vector3 TileSize { get => tileSize; set => tileSize = value; }

        public List<Node_3D> path = new List<Node_3D>();
        // Path length not including start and goal tiles
        public int GetPathLength { get => Mathf.Max(0, path.Count - 2); }

        public Grid_3D(Vector3Int gridSize, Vector3 gridPos, Vector3Int goalIndex, Vector3Int spawnIndex, Vector3 tileSize) {
            this.gridSize = gridSize;
            this.gridPos = gridPos;
            this.goalIndex = goalIndex;
            this.startIndex = spawnIndex;
            this.tileSize = tileSize;
        }

        public Grid_3D(Vector3Int gridSize, Vector3 gridPos, Vector3 tileSize, bool randomGoalSpawn = true) {
            this.gridSize = gridSize;
            this.gridPos = gridPos;
            if (randomGoalSpawn) {
                List<Vector3Int> excludedIndices = new List<Vector3Int>();
                this.goalIndex = GetRandomIndex(excludedIndices);
                // exclude goal index and it's neighbors.
                excludedIndices.Add(goalIndex);

                if (goalIndex.x < gridSize.x - 2)
                    excludedIndices.Add(goalIndex + new Vector3Int(1, 0, 0));
                if (goalIndex.x > 0)
                    excludedIndices.Add(goalIndex + new Vector3Int(-1, 0, 0));
                if (goalIndex.z < gridSize.z - 2)
                    excludedIndices.Add(goalIndex + new Vector3Int(0, 0, 1));
                if (goalIndex.z > 0)
                    excludedIndices.Add(goalIndex + new Vector3Int(0, 0, -1));
                this.startIndex = GetRandomIndex(excludedIndices);
            }
            else {
                this.goalIndex = new Vector3Int(GridSize.x - 1, 0, GridSize.z - 1);
                this.startIndex = new Vector3Int(0, 0, 0);
            }
            this.tileSize = tileSize;
        }


        public LayerMask nonTraversableMask;
        public Vector2 gridWorldSize;
        public float cellSize = 1;
        public float gapSize = 0.1f;

        public Node_3D tilePrefab;
        public List<Node_3D> finalPath;


        public bool onlyDrawPathGizmos = false;

        public void CreateGrid(bool useManhattanNeighbors) {

            gridNodes = new Node_3D[gridSize.x, gridSize.y, gridSize.z];
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        Vector3Int gridIndex = new Vector3Int(x, y, z);
                        Vector3 tileOffset = tileSize * 0.5f;
                        Vector3 worldPos = gridPos + tileSize.MultiplyInt(gridIndex) + tileOffset;

                        gridNodes[x, y, z] = new Node_3D(worldPos, gridIndex, NodeGridType.Empty);
                        gridNodes[x, y, z].SetNeighborIndices(gridSize, useManhattanNeighbors);


                    }
                }
            }

            gridNodes[goalIndex.x, goalIndex.y, goalIndex.z].NodeType = NodeGridType.Goal;
            gridNodes[goalIndex.x, goalIndex.y, goalIndex.z].locked = true;
            gridNodes[startIndex.x, startIndex.y, startIndex.z].NodeType = NodeGridType.Start;
            gridNodes[startIndex.x, startIndex.y, startIndex.z].locked = true;
        }

        // Overwrites all empty nodes with tile nodes
        public void FillGridWithTiles() {
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        if (gridNodes[x, y, z].NodeType == NodeGridType.Empty)
                            gridNodes[x, y, z].NodeType = NodeGridType.Tile;
                    }
                }
            }
        }

        public void FillGridWithRandomTiles(float randomChance = 0.5f) {
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        if (gridNodes[x, y, z].NodeType == NodeGridType.Empty && UnityEngine.Random.Range(0f, 1f) < randomChance)
                            gridNodes[x, y, z].NodeType = NodeGridType.Tile;
                    }
                }
            }
        }

        public Node_3D GetStartNode() {
            return gridNodes[startIndex.x, startIndex.y, startIndex.z];
        }

        public Node_3D GetGoalNode() {
            return gridNodes[goalIndex.x, goalIndex.y, goalIndex.z];
        }

        public float[] GetOneHotCellData(Vector3Int cellIndex) {
            float[] cellTypeBuffer = new float[Enum.GetNames(typeof(NodeGridType)).Length];

            for (var i = 0; i < cellTypeBuffer.Length; i++)
                cellTypeBuffer[i] = gridNodes[cellIndex.x, cellIndex.y, cellIndex.z].NodeType == (NodeGridType)i ? 1.0f : 0.0f;

            return cellTypeBuffer;
        }

        // Refactor this
        public float[] GetOneHotGridCellData(Vector3Int cellIndex) {
            float[] cellTypeBuffer = new float[Enum.GetNames(typeof(NodeGridType)).Length];

            NodeGridType currentNodeType = gridNodes[cellIndex.x, cellIndex.y, cellIndex.z].NodeType;
            cellTypeBuffer[0] = currentNodeType == NodeGridType.Empty ? 1.0f : 0.0f;
           // cellTypeBuffer[1] = currentNodeType == NodeGridType.Goal || currentNodeType == NodeGridType.Start ? 1.0f : 0.0f;
           // cellTypeBuffer[2] = currentNodeType == NodeGridType.Tile || currentNodeType == NodeGridType.Path ? 1.0f : 0.0f;

            return cellTypeBuffer;
        }

        public void DrawGrid() {
            Gizmos.DrawWireCube(gridPos, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (onlyDrawPathGizmos && path != null) {
                foreach (Node_3D node in path) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(node.worldPos, Vector3.one * (cellSize - gapSize));
                }
            }


            else if (gridNodes != null) {
                foreach (Node_3D node in gridNodes) {
                    Gizmos.color = node.IsTraversable ? Color.white : Color.yellow;

                    if (path != null && path.Contains(node))
                        Gizmos.color = Color.green;

                    Gizmos.DrawCube(node.worldPos, Vector3.one * (cellSize - gapSize));
                }
            }
        }

        public void ResetPath() {
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        gridNodes[x, y, z].isPath = false;
                    }
                }
            }
        }


        private Vector3Int GetRandomIndex(List<Vector3Int> excludedIndices) {
            List<Vector3Int> validIndices = new List<Vector3Int>();

            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        Vector3Int index = new Vector3Int(x, y, z);
                        if (!excludedIndices.Contains(index))
                            validIndices.Add(index);
                    }
                }
            }

            return validIndices[UnityEngine.Random.Range(0, validIndices.Count)];
        }
    }
}