using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public static class Astar {
        public static List<Vector3Int> GeneratePath(EnvGrid grid, bool useManhattanDistance, bool updatePathNodeTypes) {
            grid.path.Clear();

            Node startNode = grid.GetStartNode();
            Node goalNode = grid.GetGoalNode();

            List<Vector3Int> pathIndices = new List<Vector3Int>();

            int numEnvNodes = grid.GridSize.x * grid.GridSize.y * grid.GridSize.z;
            Heap<Node> openSet = new Heap<Node>(numEnvNodes);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == goalNode) {
                    RetracePath(grid, startNode, goalNode, updatePathNodeTypes);
                    return pathIndices;
                }

                foreach (Node neighbor in GetNeighborNodes(grid, currentNode)) {
                    if (!neighbor.IsTraversable || closedSet.Contains(neighbor))
                        continue;

                    int newMovementCostToNeighbor = useManhattanDistance ? currentNode.gCost + GetDistanceManhattan(currentNode, neighbor) : currentNode.gCost + GetDistance(currentNode, neighbor);
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

        private static void RetracePath(EnvGrid grid, Node startNode, Node endNode, bool updatePathNodeTypes) {
            List<Node> newPath = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode) {
                newPath.Add(currentNode);
                currentNode = currentNode.parentNode;
            }

            newPath.Add(startNode);

            newPath.Reverse();

            grid.ResetPath();

            // if (updatePathNodeTypes) {
            foreach (Node node in newPath) {
                if (node.gridIndex != endNode.gridIndex || node.gridIndex != startNode.gridIndex) {
                    // grid.GridNodes[node.gridIndex.x, node.gridIndex.y, node.gridIndex.z].NodeType = NodeType.Path;
                    grid.GridNodes[node.gridIndex.x, node.gridIndex.y, node.gridIndex.z].isPath = true;

                }
            }
            // }

            grid.path = newPath;
        }


        public static List<Node> GetNeighborNodes(EnvGrid grid, Node node) {
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

        public static int GetDistanceManhattan(Node nodeA, Node nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distZ = Mathf.Abs(nodeA.gridIndex.z - nodeB.gridIndex.z);

            return distX + distZ;
        }

        // Grows a path to include all valid neighbor nodes 
        public static void ExpandPath(EnvGrid grid, Node startNode, Node endNode, bool updatePath = true) {
            List<Node> newPath = new List<Node>();

            foreach (Node node in grid.path) {
                newPath.Add(node);
                UpdateNeighborNodes(node);
            }

            UpdateNeighborNodes(startNode);
            UpdateNeighborNodes(endNode);

            if (updatePath)
                grid.path = newPath;

            void UpdateNeighborNodes(Node node) {
                foreach (Vector3Int neighborIndex in node.neighborIndices) {
                    Node neighborNode = grid.GridNodes[neighborIndex.x, neighborIndex.y, neighborIndex.z];
                    if (neighborNode.NodeType == NodeType.Empty || neighborNode.NodeType == NodeType.Tile)
                        neighborNode.NodeType = NodeType.Path;
                }
            }
        }
    }
}
