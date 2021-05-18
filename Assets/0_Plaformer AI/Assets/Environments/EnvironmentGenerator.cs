using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

namespace APG.Environment {
    // Creates a grid type environment at runtime for training
    [RequireComponent(typeof(EnvironmentManager))]
    public class EnvironmentGenerator : MonoBehaviour {
        [SerializeField] private GameObject floorTile;
        [SerializeField] private GameObject pathTile;
        [SerializeField] private EnvGoal goalTile;
        [SerializeField] private EnvSpawn spawnTile;

        [SerializeField, Tooltip("Rounded to nearest int")] private Vector3Int envSize; //private Vector3 gridSize;

        public EnvGoal GoalRef { get => goalRef; }
        private EnvGoal goalRef = null;

        public EnvSpawn SpawnRef { get => spawnRef; }
        private EnvSpawn spawnRef = null;

        private List<GameObject> instantiatedEnvironmentObjects = new List<GameObject>();
        private List<Vector3Int> excludedPositions = new List<Vector3Int>();
        private List<Vector3Int> pathPositions = new List<Vector3Int>();
        private EnvironmentManager envManager;

        private Node[,,] envNodes;
        public List<Node> path = new List<Node>();

        private void Awake() {
            envManager = GetComponent<EnvironmentManager>();
        }

        public void GenerateGridEnvironment() {
            DestroyEnvObjects();
            envNodes = new Node[envSize.x, envSize.y, envSize.z];

            SetRandomEnvTileSize();

            Vector3 tileSize = floorTile.GetComponent<MeshRenderer>().bounds.size;

            // Spawn goal tile
            Vector3Int goalPos = GetRandomPosition();
            goalRef = Instantiate<EnvGoal>(goalTile, transform.position + tileSize.MultInt(goalPos), transform.rotation);
            envManager.SubscribeToGoal(goalRef);
            instantiatedEnvironmentObjects.Add(goalRef.gameObject);
            excludedPositions.Add(goalPos);

            // Spawn spawn tile
            Vector3Int spawnPos = GetRandomPosition();
            spawnRef = Instantiate<EnvSpawn>(spawnTile, transform.position + tileSize.MultInt(spawnPos), transform.rotation);
            instantiatedEnvironmentObjects.Add(spawnRef.gameObject);

            GeneratePath(spawnRef, goalRef);
            GeneratePathNodes(tileSize);

            for (int i = 0; i < envSize.x; i++) {
                for (int j = 0; j < envSize.z; j++) {
                    Vector3Int newTilePos = new Vector3Int(i, 0, j);
                    GameObject newTile;

                    if (!IsPositionExcluded(newTilePos)) {
                        newTile = Instantiate<GameObject>(floorTile, transform.position + tileSize.MultInt(newTilePos), transform.rotation);
                        instantiatedEnvironmentObjects.Add(newTile);
                    }
                }
            }
        }
        //  private void GeneratePath(Vector3Int startPos, Vector3Int goalPos) {

        private void GeneratePath(Node startNode, Node goalNode) {
            pathPositions.Clear();

            // Node startNode = GetNodeFromWorldPoint(startPos);
            //Node goalNode = GetNodeFromWorldPoint(goalPos);

            //   List<Node> openSet = new List<Node>();
            int numEnvNodes = envSize.x * envSize.y * envSize.z;
            Heap<Node> openSet = new Heap<Node>(numEnvNodes);
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

                if (currentNode == goalNode) {
                    RetracePath(startNode, goalNode);
                    return;
                }

                foreach (Node neighbor in GetNeighbors(currentNode)) {
                    if (!neighbor.isTraversable || closedSet.Contains(neighbor))
                        continue;

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, goalNode);
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

        private void GeneratePathNodes(Vector3 tileSize) {
            foreach (Vector3Int pathPosition in pathPositions) {
                GameObject newTile;

                newTile = Instantiate<GameObject>(pathTile, transform.position + tileSize.MultInt(pathPosition), transform.rotation);
                instantiatedEnvironmentObjects.Add(newTile);
            }
        }

        private int GetDistance(Node nodeA, Node nodeB) {
            int distX = Mathf.Abs(nodeA.gridIndex.x - nodeB.gridIndex.x);
            int distZ = Mathf.Abs(nodeA.gridIndex.z - nodeB.gridIndex.z);

            if (distX > distZ)
                return 14 * distZ + 10 * (distX - distZ);

            return 14 * distX + 10 * (distZ - distX);
        }

        public Node GetNodeFromWorldPoint(Vector3 worldPos) {
            /* float percentX = (worldPos.x + envSize.x / 2) / envSize.x;
             float percentY = (worldPos.z + envSize.y / 2) / envSize.y;
             percentX = Mathf.Clamp01(percentX);
             percentY = Mathf.Clamp01(percentY);

             int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
             int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);*/

            int x = Mathf.RoundToInt(worldPos.x);
            int z = Mathf.RoundToInt(worldPos.z);

            //  if (gridNodes != null && gridNodes.GetLength(0) > x && gridNodes.GetLength(1) > y)
            return envNodes[x, 0, z];

            //else
            //    return null;
        }

        public List<Node> GetNeighbors(Node node) {
            List<Node> neighbors = new List<Node>();

            for (int x = -1; x <= 1; x++) {
                for (int z = -1; z <= 1; z++) {
                    if (x == 0 && z == 0)
                        continue;

                    int checkX = node.gridIndex.x + x;
                    int checkZ = node.gridIndex.z + z;

                    if (IsInBounds(checkX, 0, checkZ))
                        neighbors.Add(envNodes[checkX, 0, checkZ]);
                }
            }

            return neighbors;
        }

        bool IsInBounds(int x, int y, int z) => x >= 0 && x < envSize.x && y >= 0 && y < envSize.y && z >= 0 && z < envSize.z;

        private Vector3Int GetRandomPosition() {
            Vector3Int outPos = Vector3Int.zero;
            int i = 0;
            while (i < 100 || IsPositionExcluded(outPos)) {
                i++;
                outPos = new Vector3Int(Random.Range(0, envSize.x), 0, Random.Range(0, envSize.z));
            }

            return outPos;
        }

        private bool IsPositionExcluded(Vector3Int positionToCheck) {
            return excludedPositions.Contains(positionToCheck);
        }

        private void InstantiateNodePrefabs() {

        }

        private void DestroyEnvObjects() {
            goalRef = null;
            spawnRef = null;

            foreach (GameObject gameObject in instantiatedEnvironmentObjects) {
                Destroy(gameObject);
            }

            instantiatedEnvironmentObjects.Clear();
            excludedPositions.Clear();
        }

        private void SetRandomEnvTileSize() {
            // Get random range values from lesson plan
            envSize = LessonPlan.Instance.GetRandomBoardSize();
        }
    }
}
