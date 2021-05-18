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

        Vector3Int goalIndex;
        Vector3Int spawnIndex;

        [SerializeField, Tooltip("Rounded to nearest int")] private Vector3Int envSize; //private Vector3 gridSize;

        public EnvGoal GoalRef { get => goalRef; }
        private EnvGoal goalRef = null;

        public EnvSpawn SpawnRef { get => spawnRef; }
        private EnvSpawn spawnRef = null;

        private List<GameObject> instantiatedEnvironmentObjects = new List<GameObject>();
        private List<Vector3Int> excludedIndices = new List<Vector3Int>();
        private List<Vector3Int> pathIndices = new List<Vector3Int>();
        private EnvironmentManager envManager;

        private Node[,,] envNodes;

        private void Awake() {
            envManager = GetComponent<EnvironmentManager>();
        }

        public void GenerateGridEnvironment() {
            DestroyEnvObjects();

            SetRandomEnvTileSize();
            Vector3 tileSize = floorTile.GetComponent<MeshRenderer>().bounds.size;

            envNodes = new Node[envSize.x, envSize.y, envSize.z];
            for (int x = 0; x < envSize.x; x++) {
                for (int y = 0; y < envSize.y; y++) {
                    for (int z = 0; z < envSize.z; z++) {
                        Vector3Int gridIndex = new Vector3Int(x, y, z);
                        Vector3 worldPos = transform.position + tileSize.MultInt(gridIndex);
                        envNodes[x, y, z] = new Node(true, worldPos, gridIndex, NodeType.Tile);
                        envNodes[x, y, z].SetNeighborIndices(envSize);
                    }
                }
            }

            goalIndex = GetRandomIndex();
            envNodes[goalIndex.x, goalIndex.y, goalIndex.z].NodeType = NodeType.Goal;
            excludedIndices.Add(goalIndex);

            spawnIndex = GetRandomIndex();
            envNodes[spawnIndex.x, spawnIndex.y, spawnIndex.z].NodeType = NodeType.Start;
            excludedIndices.Add(spawnIndex);

            GeneratePath(envNodes[spawnIndex.x, spawnIndex.y, spawnIndex.z], envNodes[goalIndex.x, goalIndex.y, goalIndex.z]);
            InstantiateNodePrefabs();

            envManager.SubscribeToGoal(goalRef);
        }

        private void GeneratePath(Node startNode, Node goalNode) {
            pathIndices.Clear();

            int numEnvNodes = envSize.x * envSize.y * envSize.z;
            Heap<Node> openSet = new Heap<Node>(numEnvNodes);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == goalNode) {
                    RetracePath(startNode, goalNode);
                    return;
                }

                foreach (Node neighbor in GetNeighborNodes(currentNode)) {
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
        }

        private void RetracePath(Node startNode, Node endNode) {
            List<Node> newPath = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode) {
                newPath.Add(currentNode);
                currentNode = currentNode.parentNode;
            }

            newPath.Reverse();

            foreach (Node node in newPath) {
                if (node.gridIndex != goalIndex) {
                    envNodes[node.gridIndex.x, node.gridIndex.y, node.gridIndex.z].NodeType = NodeType.Path;
                    pathIndices.Add(node.gridIndex);
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

        public List<Node> GetNeighborNodes(Node node) {
            List<Node> neighbors = new List<Node>();


            foreach (Vector3Int neighborIndex in node.neighborIndices) {
                neighbors.Add(envNodes[neighborIndex.x, neighborIndex.y, neighborIndex.z]);
            }

            return neighbors;
        }

        private Vector3Int GetRandomIndex() {
            Vector3Int outPos = Vector3Int.zero;
            int i = 0;
            while (i < 100 || IsIndexExcluded(outPos)) {
                i++;
                outPos = new Vector3Int(Random.Range(0, envSize.x), 0, Random.Range(0, envSize.z));
            }

            return outPos;
        }

        private bool IsIndexExcluded(Vector3Int indexToCheck) {
            return excludedIndices.Contains(indexToCheck);
        }

        private void InstantiateNodePrefabs() {

            for (int x = 0; x < envSize.x; x++) {
                for (int y = 0; y < envSize.y; y++) {
                    for (int z = 0; z < envSize.z; z++) {
                        if (envNodes[x, y, z].NodeType == NodeType.Start) {
                            spawnRef = Instantiate<EnvSpawn>(spawnTile, envNodes[x, y, z].worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(spawnRef.gameObject);
                        }

                        else if (envNodes[x, y, z].NodeType == NodeType.Goal) {
                            goalRef = Instantiate<EnvGoal>(goalTile, envNodes[x, y, z].worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(goalRef.gameObject);
                        }

                        else if (envNodes[x, y, z].NodeType == NodeType.Path) {
                            GameObject newTile = Instantiate<GameObject>(pathTile, envNodes[x, y, z].worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(newTile);
                        }

                        else if (envNodes[x, y, z].NodeType == NodeType.Tile) {
                            GameObject newTile = Instantiate<GameObject>(floorTile, envNodes[x, y, z].worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(newTile);
                        }
                    }
                }
            }
        }

        private void DestroyEnvObjects() {
            goalRef = null;
            spawnRef = null;

            foreach (GameObject gameObject in instantiatedEnvironmentObjects) {
                Destroy(gameObject);
            }

            instantiatedEnvironmentObjects.Clear();
            excludedIndices.Clear();
        }

        private void SetRandomEnvTileSize() {
            // Get random range values from lesson plan
            envSize = LessonPlan.Instance.GetRandomBoardSize();
        }
    }
}
