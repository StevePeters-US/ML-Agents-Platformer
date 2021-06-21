using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {

/*    public enum NodeType {
        Empty,
        Start,
        Goal,
        Path,
        Tile//,
            // Obstacle
    }*/

    public enum NodeGridType {
        Empty,
        Start,
        Goal,
        Waypoint,
        Tile
    }

    public class Node_3D : IHeapItem<Node_3D> {

        public Vector3Int gridIndex;

        // private bool isTraversable;
        public bool IsTraversable { get => nodeType != NodeGridType.Empty; }


        public bool locked = false; // Can this node be modified by the agent?

        public Vector3 worldPos;

        public Node_3D parentNode;

        public int gCost;
        public int hCost; // Heuristics
        public int fCost { get { return gCost + hCost; } }

        private int heapIndex;
        public int HeapIndex { get => heapIndex; set => heapIndex = value; }

        public GameObject nodePrefab;

        public List<Vector3Int> neighborIndices = new List<Vector3Int>();
        public List<Vector3Int> allNeighborIndices = new List<Vector3Int>();

        private NodeGridType nodeType;
        public NodeGridType NodeType { get => nodeType; set => nodeType = locked ? nodeType : value; }

        public bool isPath = false;
        public float cohesiveValue; // 0-1 how similar are neighboring tiles

        public Node_3D(Vector3 worldPos, Vector3Int gridIndex, NodeGridType nodeType = NodeGridType.Empty) {
            this.worldPos = worldPos;
            this.gridIndex = gridIndex;
            this.nodeType = nodeType;
        }

        public int CompareTo(Node_3D nodeToCompare) {
            int compare = fCost.CompareTo(nodeToCompare.fCost);

            if (compare == 0)
                compare = hCost.CompareTo(nodeToCompare.hCost);

            return -compare;
        }

        public bool IsPathNode() {
            return isPath;
        }

        public void SetNeighborIndices(Vector3Int envSize, bool useManhattanNeighbors) {
            allNeighborIndices.Clear();
            neighborIndices.Clear();

            for (int x = -1; x <= 1; x++) {
                for (int z = -1; z <= 1; z++) {
                    bool skipManhattanNeighbor = false;
                    // Check 8 directions on plane
                    if (x == 0 && z == 0)
                        continue;

                    // Only get nsew neighbors
                    if (useManhattanNeighbors && ((x == 0 && z == 0) || (x != 0 && z != 0)))
                        skipManhattanNeighbor = true;

                    int checkX = gridIndex.x + x;
                    int checkZ = gridIndex.z + z;

                    bool IsInBounds(int x, int y, int z) => x >= 0 && x < envSize.x && y >= 0 && y < envSize.y && z >= 0 && z < envSize.z;

                    if (IsInBounds(checkX, 0, checkZ)) {
                        allNeighborIndices.Add(new Vector3Int(checkX, 0, checkZ));
                        if (!skipManhattanNeighbor)
                            neighborIndices.Add(new Vector3Int(checkX, 0, checkZ));
                    }
                }
            }
        }
    }
}
