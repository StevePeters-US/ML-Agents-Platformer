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
        //public List<Node> path = new List<Node>();

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


            // Spawn goal tile
            goalIndex = GetRandomIndex();
            envNodes[goalIndex.x, goalIndex.y, goalIndex.z].NodeType = NodeType.Goal;
            excludedIndices.Add(goalIndex);

            spawnIndex = GetRandomIndex();
            envNodes[spawnIndex.x, spawnIndex.y, spawnIndex.z].NodeType = NodeType.Start;
            excludedIndices.Add(spawnIndex);


            // goalRef = new Node(true, transform.position + tileSize.MultInt(goalIndex), goalIndex, NodeType.Goal);
            //goalRef = Instantiate<EnvGoal>(goalTile, transform.position + tileSize.MultInt(goalIndex), transform.rotation);
            //goalRef.gridIndex = goalIndex;
            //goalRef.SetNeighborIndices(envSize);

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // envManager.SubscribeToGoal(goalRef);


            // instantiatedEnvironmentObjects.Add(goalRef.gameObject);

            // Spawn spawn tile
            // spawnRef = new Node(true, transform.position + tileSize.MultInt(spawnIndex), spawnIndex, NodeType.Start);
            //spawnRef = Instantiate<EnvSpawn>(spawnTile, transform.position + tileSize.MultInt(spawnIndex), transform.rotation);
            //spawnRef.gridIndex = spawnIndex;
            //spawnRef.SetNeighborIndices(envSize);
            //instantiatedEnvironmentObjects.Add(spawnRef.gameObject);

            GeneratePath(envNodes[spawnIndex.x, spawnIndex.y, spawnIndex.z], envNodes[goalIndex.x, goalIndex.y, goalIndex.z]);
            // GeneratePathNodes(tileSize);

            /* for (int i = 0; i < envSize.x; i++) {
                 for (int j = 0; j < envSize.z; j++) {
                     Vector3Int newTilePos = new Vector3Int(i, 0, j);
                     GameObject newTile;

                     if (!IsIndexExcluded(newTilePos)) {
                         newTile = Instantiate<GameObject>(floorTile, transform.position + tileSize.MultInt(newTilePos), transform.rotation);
                         
                     }
                 }
             }*/

            InstantiateNodePrefabs();
        }

        private void GeneratePath(Node startNode, Node goalNode) {
            pathIndices.Clear();

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

                foreach (Node neighbor in GetNeighborNodes(currentNode)) {
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
            //path = newPath;

            foreach (Node node in newPath) {
                pathIndices.Add(node.gridIndex);
            }
        }

        /*      private void GeneratePathNodes(Vector3 tileSize) {
                  foreach (Vector3Int pathIndex in pathIndices) {
                      Node newTile;

                     // newTile = Instantiate<Node>(pathTile, transform.position + tileSize.MultInt(pathIndex), transform.rotation);
                      newTile = new Node(true, transform.position + tileSize.MultInt(pathIndex), pathIndex);

                      //newTile.gridIndex = pathIndex;
                      newTile.SetNeighborIndices(envSize);
                      //instantiatedEnvironmentObjects.Add(newTile.gameObject);
                      excludedIndices.Add(pathIndex);
                  }
              }*/

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

        public List<Node> GetNeighborNodes(Node node) {
            List<Node> neighbors = new List<Node>();


            foreach (Vector3Int neighborIndex in node.neighborIndices) {
                neighbors.Add(envNodes[neighborIndex.x, neighborIndex.y, neighborIndex.z]);
            }
            /*for (int x = -1; x <= 1; x++) {
                for (int z = -1; z <= 1; z++) {
                    if (x == 0 && z == 0)
                        continue;

                    int checkX = node.gridIndex.x + x;
                    int checkZ = node.gridIndex.z + z;

                    if (IsInBounds(checkX, 0, checkZ))
                        neighbors.Add(envNodes[checkX, 0, checkZ]);
                }
            }*/

            return neighbors;
        }

        bool IsInBounds(int x, int y, int z) => x >= 0 && x < envSize.x && y >= 0 && y < envSize.y && z >= 0 && z < envSize.z;

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

                        else if (envNodes[x, y, z].NodeType == NodeType.Tile) {
                            GameObject newTile = Instantiate<GameObject>(floorTile, envNodes[x, y, z].worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(newTile);
                        }
                    }
                }
            }
            // Spawn goal tile
            /*     goalRef = new Node(true, transform.position + tileSize.MultInt(goalIndex), goalIndex, NodeType.Goal);
                 goalRef = Instantiate<EnvGoal>(goalTile, transform.position + tileSize.MultInt(goalIndex), transform.rotation);
                 //goalRef.gridIndex = goalIndex;
                 goalRef.SetNeighborIndices(envSize);

                 // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                 // envManager.SubscribeToGoal(goalRef);


                 // instantiatedEnvironmentObjects.Add(goalRef.gameObject);
                 excludedIndices.Add(goalIndex);

                 // Spawn spawn tile
                 Vector3Int spawnIndex = GetRandomIndex();
                 spawnRef = new Node(true, transform.position + tileSize.MultInt(spawnIndex), spawnIndex, NodeType.Start);
                 spawnRef.gridIndex = spawnIndex;
                 spawnRef.SetNeighborIndices(envSize);
                 //instantiatedEnvironmentObjects.Add(spawnRef.gameObject);
                 excludedIndices.Add(spawnIndex);*/
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
