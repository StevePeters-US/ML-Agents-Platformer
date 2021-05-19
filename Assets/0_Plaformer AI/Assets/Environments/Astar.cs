using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public static class Astar {
        public static List<Vector3Int> GeneratePath(Grid grid, Node startNode, Node goalNode) {
            List<Vector3Int> pathIndices = new List<Vector3Int>();

            int numEnvNodes = grid.GridSize.x * grid.GridSize.y * grid.GridSize.z;
            Heap<Node> openSet = new Heap<Node>(numEnvNodes);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == goalNode) {
                    //  RetracePath(startNode, goalNode);
                    return pathIndices; ;
                }

                foreach (Node neighbor in GetNeighborNodes(grid, currentNode)) {
                    if (!neighbor.isTraversable || closedSet.Contains(neighbor))
                        continue;

                    //int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    int newMovementCostToNeighbor = currentNode.gCost + GetDistanceManhattan(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, goalNode);
                        neighbor.parentNode = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }
                return pathIndices;
        }

        /*     private void RetracePath(Node startNode, Node endNode) {
                 List<Node> newPath = new List<Node>();
                 Node currentNode = endNode;

                 while (currentNode != startNode) {
                     newPath.Add(currentNode);
                     currentNode = currentNode.parentNode;
                 }

                 newPath.Reverse();

                 foreach (Node node in newPath) {
                     if (node.gridIndex != goalIndex) {
                         gridNodes[node.gridIndex.x, node.gridIndex.y, node.gridIndex.z].NodeType = NodeType.Path;
                         pathIndices.Add(node.gridIndex);
                     }
                 }
             }*/

        public static List<Node> GetNeighborNodes(Grid grid, Node node) {
            List<Node> neighbors = new List<Node>();

            foreach (Vector3Int neighborIndex in node.neighborIndices) {
                neighbors.Add(grid.GridNodes[neighborIndex.x, neighborIndex.y, neighborIndex.z]);
            }

            return neighbors;
        }

        private static int GetDistance(Node nodeA, Node nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distZ = Mathf.Abs(nodeA.gridIndex.z - nodeB.gridIndex.z);

            if (distX > distZ)
                return 14 * distZ + 10 * (distX - distZ);

            return 14 * distX + 10 * (distZ - distX);
        }

        private static int GetDistanceManhattan(Node nodeA, Node nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distY = Mathf.Abs(nodeA.gridIndex.y - nodeB.gridIndex.y);

            return distX + distY;
        }
    }
}
