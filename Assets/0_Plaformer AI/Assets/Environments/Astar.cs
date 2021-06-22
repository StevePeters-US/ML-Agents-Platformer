using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public static class Astar {
        public static List<Vector3Int> GeneratePath(Grid3D_Abstract grid, bool useManhattanDistance, bool updatePathNodeTypes) {
            grid.pathIndices.Clear();

            Node_3D startNode = grid.GetStartNode();
            Node_3D goalNode = grid.GetGoalNode();

            List<Vector3Int> pathIndices = new List<Vector3Int>();

            int numEnvNodes = grid.CurrentGrid3DData.GridSize.x * grid.CurrentGrid3DData.GridSize.y * grid.CurrentGrid3DData.GridSize.z;
            Heap<Node_3D> openSet = new Heap<Node_3D>(numEnvNodes);
            HashSet<Node_3D> closedSet = new HashSet<Node_3D>();
            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node_3D currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == goalNode) {
                    RetracePath(grid, startNode, goalNode, updatePathNodeTypes);
                    return pathIndices;
                }

                foreach (Node_3D neighbor in GetNeighborNodes(grid, currentNode)) {
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

        private static void RetracePath(Grid3D_Abstract grid, Node_3D startNode, Node_3D endNode, bool updatePathNodeTypes) {
            List<Node_3D> newPath = new List<Node_3D>();
            Node_3D currentNode = endNode;

            while (currentNode != startNode) {
                newPath.Add(currentNode);
                currentNode = currentNode.parentNode;
            }

            newPath.Add(startNode);

            newPath.Reverse();

            grid.ResetPathIndices();

            List<Vector3Int> outPathIndices = new List<Vector3Int>();

            // if (updatePathNodeTypes) {
            foreach (Node_3D node in newPath) {
                if (node.gridIndex != endNode.gridIndex || node.gridIndex != startNode.gridIndex) {
                    // grid.GridNodes[node.gridIndex.x, node.gridIndex.y, node.gridIndex.z].NodeType = NodeType.Path;
                    grid.CurrentGrid3DData.GridNodes[node.gridIndex.x, node.gridIndex.y, node.gridIndex.z].isPath = true;
                    outPathIndices.Add(node.gridIndex);
                }
            }
            // }

            grid.pathIndices = outPathIndices;
        }


        public static List<Node_3D> GetNeighborNodes(Grid3D_Abstract grid, Node_3D node) {
            List<Node_3D> neighbors = new List<Node_3D>();

            foreach (Vector3Int neighborIndex in node.neighborIndices) {
                neighbors.Add(grid.CurrentGrid3DData.GridNodes[neighborIndex.x, neighborIndex.y, neighborIndex.z]);
            }

            return neighbors;
        }

        private static int GetDistance(Node_3D nodeA, Node_3D nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distZ = Mathf.Abs(nodeA.gridIndex.z - nodeB.gridIndex.z);

            if (distX > distZ)
                return 14 * distZ + 10 * (distX - distZ);

            return 14 * distX + 10 * (distZ - distX);
        }

        public static int GetDistanceManhattan(Node_3D nodeA, Node_3D nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distZ = Mathf.Abs(nodeA.gridIndex.z - nodeB.gridIndex.z);

            return distX + distZ;
        }

        // Grows a path to include all valid neighbor nodes 
        public static void ExpandPath(Grid3D_Abstract grid, Node_3D startNode, Node_3D endNode, bool updatePath = true) {
          /*  List<Node_3D> newPath = new List<Node_3D>();

            foreach (Node_3D node in grid.path) {
                newPath.Add(node);
                UpdateNeighborNodes(node);
            }

            UpdateNeighborNodes(startNode);
            UpdateNeighborNodes(endNode);

            if (updatePath)
                grid.path = newPath;

            void UpdateNeighborNodes(Node_3D node) {
                foreach (Vector3Int neighborIndex in node.neighborIndices) {
                    Node_3D neighborNode = grid.CurrentGrid3DData.GridNodes[neighborIndex.x, neighborIndex.y, neighborIndex.z];
                    *//*if (neighborNode.NodeType == NodeGridType.Empty || neighborNode.NodeType == NodeGridType.Tile)
                        neighborNode.NodeType = NodeGridType.Path;*//*
                }
            }*/
        }
    }
}
