using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public class Node : MonoBehaviour, IHeapItem<Node> {

        public Vector3Int gridIndex;

        public bool isTraversable;
        public Vector3 worldPos;

        public Node parentNode;

        public int gCost;
        public int hCost; // Heuristics
        public int fCost { get { return gCost + hCost; } }

        private int heapIndex;
        public int HeapIndex { get => heapIndex; set => heapIndex = value; }

        public GameObject nodePrefab;

        public List<Node> neighbors;

        public Node(bool isTraversable, Vector3 worldPos, Vector3Int gridIndex) {
            this.isTraversable = isTraversable;
            this.worldPos = worldPos;
            this.gridIndex = gridIndex;
        }

        public int CompareTo(Node nodeToCompare) {
            int compare = fCost.CompareTo(nodeToCompare.fCost);

            if (compare == 0)
                compare = hCost.CompareTo(nodeToCompare.hCost);

            return -compare;
        }
    }
}
