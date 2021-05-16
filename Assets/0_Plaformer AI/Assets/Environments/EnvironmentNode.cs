using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public class EnvironmentNode : MonoBehaviour,  IHeapItem<EnvironmentNode> {
        public int gridX;
        public int gridY;

        public bool isTraversable;
        public Vector3 worldPos;

        public Node parentNode;

        public int gCost;
        public int hCost; // Heuristics
        public int fCost { get { return gCost + hCost; } }

        private int heapIndex;
        public int HeapIndex { get => heapIndex; set => heapIndex = value; }

        public EnvironmentNode(bool isTraversable, Vector3 worldPos, int gridX, int gridY) {
            this.isTraversable = isTraversable;
            this.worldPos = worldPos;
            this.gridX = gridX;
            this.gridY = gridY;
        }

        public int CompareTo(EnvironmentNode nodeToCompare) {
            int compare = fCost.CompareTo(nodeToCompare.fCost);

            if (compare == 0)
                compare = hCost.CompareTo(nodeToCompare.hCost);

            return -compare;
        }
    }
}
