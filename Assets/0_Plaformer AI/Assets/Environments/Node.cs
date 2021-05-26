using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {

    public enum NodeType {
        Empty,
        Start, 
        Goal,
        Path, 
        Tile,
        Obstacle
    }

    public class Node : IHeapItem<Node> {

        public Vector3Int gridIndex;

       // private bool isTraversable;
        public bool IsTraversable { get => nodeType != NodeType.Empty; }

        public Vector3 worldPos;

        public Node parentNode;

        public int gCost;
        public int hCost; // Heuristics
        public int fCost { get { return gCost + hCost; } }

        private int heapIndex;
        public int HeapIndex { get => heapIndex; set => heapIndex = value; }

        public GameObject nodePrefab;

        public List<Vector3Int> neighborIndices = new List<Vector3Int>();

        private NodeType nodeType;
        public NodeType NodeType { get => nodeType; set => nodeType = value; }

        public Node(Vector3 worldPos, Vector3Int gridIndex, NodeType nodeType = NodeType.Empty) {
            this.worldPos = worldPos;
            this.gridIndex = gridIndex;
            this.NodeType = nodeType;
        }

        public int CompareTo(Node nodeToCompare) {
            int compare = fCost.CompareTo(nodeToCompare.fCost);

            if (compare == 0)
                compare = hCost.CompareTo(nodeToCompare.hCost);

            return -compare;
        }

        public void SetNeighborIndices(Vector3Int envSize, bool useManhattanNeighbors) {
            neighborIndices.Clear();

            for (int x = -1; x <= 1; x++) {
                for (int z = -1; z <= 1; z++) {
                    // Check 8 directions on plane
                    if (!useManhattanNeighbors && ( x == 0 && z == 0))
                        continue;

                    // Only get nsew neighbors
                    if (useManhattanNeighbors && ((x == 0 && z == 0) ||(x != 0 && z != 0)))
                        continue;

                    int checkX = gridIndex.x + x;
                    int checkZ = gridIndex.z + z;

                    bool IsInBounds(int x, int y, int z) => x >= 0 && x < envSize.x && y >= 0 && y < envSize.y && z >= 0 && z < envSize.z;

                    if (IsInBounds(checkX, 0, checkZ))
                        neighborIndices.Add(new Vector3Int( checkX, 0, checkZ));
                }
            }
        }
    }
}
