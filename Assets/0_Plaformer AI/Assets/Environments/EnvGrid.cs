using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    // Based on Sebastion Lague https://www.youtube.com/watch?v=nhiFx28e7JY
    // https://pastebin.com/vCVCpsQc
    public class EnvGrid {

        public Vector3Int GridSize { get => gridSize; set => gridSize = value; }
        private Vector3Int gridSize;

        private Vector3 gridPos;

        public Node[,,] GridNodes { get => gridNodes; }
        private Node[,,] gridNodes;

        public Vector3Int SpawnIndex { get => startIndex; set => startIndex = value; }
        Vector3Int startIndex;

        Vector3Int goalIndex;
        public Vector3Int GoalIndex { get => goalIndex; set => goalIndex = value; }

        Vector3 tileSize = Vector3.one;
        public Vector3 TileSize { get => tileSize; set => tileSize = value; }



        public EnvGrid(Vector3Int gridSize, Vector3 gridPos, Vector3Int goalIndex, Vector3Int spawnIndex, Vector3 tileSize) {
            this.gridSize = gridSize;
            this.gridPos = gridPos;
            this.goalIndex = goalIndex;
            this.startIndex = spawnIndex;
            this.tileSize = tileSize;
        }

        public EnvGrid(Vector3Int gridSize, Vector3 gridPos, Vector3 tileSize) {
            this.gridSize = gridSize;
            this.gridPos = gridPos;
            this.goalIndex = new Vector3Int(GridSize.x - 1, 0, GridSize.z - 1);
            this.startIndex = new Vector3Int(0, 0, 0);
            this.tileSize = tileSize;
        }


        public LayerMask nonTraversableMask;
        public Vector2 gridWorldSize;
        public float cellSize = 1;
        public float gapSize = 0.1f;

        public Node tilePrefab;
        public List<Node> finalPath;


        public bool onlyDrawPathGizmos = false;


        public void CreateGrid(bool useManhattanNeighbors) {
            gridNodes = new Node[gridSize.x, gridSize.y, gridSize.z];
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        Vector3Int gridIndex = new Vector3Int(x, y, z);
                        Vector3 tileOffset = tileSize * 0.5f;
                        Vector3 worldPos = gridPos.Multiply(tileSize) + tileSize.MultiplyInt(gridIndex) + tileOffset;
                        gridNodes[x, y, z] = new Node(true, worldPos, gridIndex, NodeType.Empty);
                        gridNodes[x, y, z].SetNeighborIndices(gridSize, useManhattanNeighbors);
                    }
                }
            }

            gridNodes[goalIndex.x, goalIndex.y, goalIndex.z].NodeType = NodeType.Goal;
            gridNodes[startIndex.x, startIndex.y, startIndex.z].NodeType = NodeType.Start;
        }

        // Overwrites all empty nodes with tile nodes
        public void FillGridWithTiles() {
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        if (gridNodes[x, y, z].NodeType == NodeType.Empty)
                            gridNodes[x, y, z].NodeType = NodeType.Tile;
                    }
                }
            }
        }

        public Node GetStartNode() {
            return gridNodes[startIndex.x, startIndex.y, startIndex.z];
        }

        public Node GetGoalNode() {
            return gridNodes[goalIndex.x, goalIndex.y, goalIndex.z];
        }

        public List<Node> path = new List<Node>();
        public void DrawGrid() {
            Gizmos.DrawWireCube(gridPos, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (onlyDrawPathGizmos && path != null) {
                foreach (Node node in path) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(node.worldPos, Vector3.one * (cellSize - gapSize));
                }
            }


            else if (gridNodes != null) {
                foreach (Node node in gridNodes) {
                    Gizmos.color = node.isTraversable ? Color.white : Color.yellow;

                    if (path != null && path.Contains(node))
                        Gizmos.color = Color.green;

                    Gizmos.DrawCube(node.worldPos, Vector3.one * (cellSize - gapSize));
                }
            }
        }

        private int GetDistance(Node nodeA, Node nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distZ = Mathf.Abs(nodeA.gridIndex.z - nodeB.gridIndex.z);

            if (distX > distZ)
                return 14 * distZ + 10 * (distX - distZ);

            return 14 * distX + 10 * (distZ - distX);
        }

        private int GetDistanceManhattan(Node nodeA, Node nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distY = Mathf.Abs(nodeA.gridIndex.y - nodeB.gridIndex.y);

            return distX + distY;
        }
    }
}
