using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    // Based on Sebastion Lague https://www.youtube.com/watch?v=nhiFx28e7JY
    // https://pastebin.com/vCVCpsQc
    public class Grid : MonoBehaviour {

        public LayerMask nonTraversableMask;
        public Vector2 gridWorldSize;
        public float cellSize = 1;
        public float gapSize = 0.1f;

        public Node tilePrefab;
        Node[,] gridNodes;
        public List<Node> finalPath;


        public bool onlyDrawPathGizmos = false;

        int gridSizeX, gridSizeY;

        public int MaxSize { get { return gridSizeX * gridSizeY; } }

        void Start() {
            cellSize = Mathf.Max(cellSize, 0.1f);
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / cellSize);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / cellSize);
            CreateGrid();
        }

        private void CreateGrid() {
            gridNodes = new Node[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position - ((Vector3.right * gridWorldSize.x) / 2) - ((Vector3.forward * gridWorldSize.y) / 2);

            Vector3 cellCenter = new Vector3(cellSize, cellSize, cellSize) * 0.5f;

            for (int x = 0; x < gridSizeX; x++) {
                for (int y = 0; y < gridSizeY; y++) {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * cellSize + cellCenter.x) + Vector3.forward * (y * cellSize + cellCenter.z);
                    bool traversable = !(Physics.CheckBox(worldPoint, cellCenter, transform.rotation, nonTraversableMask));

                    // gridNodes[x, y] = new EnvironmentNode(traversable, worldPoint, x, y);
                   // gridNodes[x, y] = GameObject.Instantiate<Node>(tilePrefab, worldPoint, transform.rotation, transform);
                }
            }
        }

        public List<Node> path = new List<Node>();
        private void OnDrawGizmos() {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (onlyDrawPathGizmos && path != null) {
                foreach (Node node in path) {
                    Gizmos.color = Color.green;
                    //Gizmos.DrawCube(node.worldPos, Vector3.one * (nodeDiameter - .1f));
                    Gizmos.DrawCube(node.worldPos, Vector3.one * (cellSize - gapSize));
                }
            }


            else if (gridNodes != null) {
                foreach (Node node in gridNodes) {
                    Gizmos.color = node.isTraversable ? Color.white : Color.yellow;

                    if (path != null && path.Contains(node))
                        Gizmos.color = Color.green;

                    //Gizmos.DrawCube(node.worldPos, Vector3.one * (nodeDiameter - .1f));
                    Gizmos.DrawCube(node.worldPos, Vector3.one * (cellSize - gapSize));
                }
            }
        }

        public Node GetNodeFromWorldPoint(Vector3 worldPos) {
            float percentX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPos.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

            //  if (gridNodes != null && gridNodes.GetLength(0) > x && gridNodes.GetLength(1) > y)
            return gridNodes[x, y];

            //else
            //    return null;
        }

        public List<Node> GetNeighbors(Node node) {
            List<Node> neighbors = new List<Node>();

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.gridIndex.x + x;
                    int checkY = node.gridIndex.y + y;

                    if (IsInBounds(checkX, checkY))
                        neighbors.Add(gridNodes[checkX, checkY]);
                }
            }

            return neighbors;
        }

        bool IsInBounds(int x, int y) => x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY;

        private void GetPath(Vector3 startPos, Vector3 targetPos) {

            Node startNode = GetNodeFromWorldPoint(startPos);
            Node targetNode = GetNodeFromWorldPoint(targetPos);

            //   List<Node> openSet = new List<Node>();
            Heap<Node> openSet = new Heap<Node>(MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node currentNode = openSet.RemoveFirst();
                /*Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++) {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) {
                        currentNode = openSet[i];
                    }
                }*/

                //openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode) {
                    RetracePath(startNode, targetNode);
                    return;
                }

                foreach (Node neighbor in GetNeighbors(currentNode)) {
                    if (!neighbor.isTraversable || closedSet.Contains(neighbor))
                        continue;

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.parentNode = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }
        }

        private void RetracePath(Node startNode, Node endNode) {
            List<Node> newPath = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode) {
                newPath.Add(currentNode);
                currentNode = currentNode.parentNode;
            }

            newPath.Reverse();
            path = newPath;
        }

        private int GetDistance(Node nodeA, Node nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distY = Mathf.Abs(nodeA.gridIndex.y - nodeB.gridIndex.y);

            if (distX > distY)
                return 14 * distY + 10 * (distX - distY);

            return 14 * distX + 10 * (distY - distX);
        }

        private int GetDistanceManhattan(Node nodeA, Node nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distY = Mathf.Abs(nodeA.gridIndex.y - nodeB.gridIndex.y);

            return distX + distY;
        }
    }
}
