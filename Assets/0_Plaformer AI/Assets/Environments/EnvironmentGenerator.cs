using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using UnityEngine.Pool;


namespace APG.Environment {
    // Given a grid, the environment generator instantiates all required objects.
    [RequireComponent(typeof(EnvironmentManager))]
    [RequireComponent(typeof(Grid3D_Platformer))]

    public class EnvironmentGenerator : MonoBehaviour {
        [SerializeField] private GameObject floorTile;
        [SerializeField] private GameObject pathTile;
        [SerializeField] private EnvGoal goalTile;
        [SerializeField] private EnvSpawn spawnTile;
        [SerializeField] private GameObject failedTile;

        [SerializeField] private Material tileMat;
        [SerializeField] private Material pathMat;

        ObjectPool<GameObject> tilePool;
        private List<GameObject> instantiatedTiles = new List<GameObject>();

        [SerializeField] private bool usePath = false;
        [SerializeField] private bool useManhattanNeighbors = true;

        [SerializeField, Tooltip("Rounded to nearest int")] private Vector3Int envSize;

        private Grid3D_Platformer grid;

        public EnvGoal GoalRef { get => goalRef; }
        private EnvGoal goalRef = null;

        public EnvSpawn SpawnRef { get => spawnRef; }
        private EnvSpawn spawnRef = null;

        private List<GameObject> instantiatedEnvironmentObjects = new List<GameObject>();

       /* private List<Vector3Int> availableIndices = new List<Vector3Int>();*/

        private List<Vector3Int> pathIndices = new List<Vector3Int>();
        private EnvironmentManager envManager;

        [SerializeField] private Vector3 tileSize = Vector3.one;

        private PathRenderer pathRenderer;

        private void Awake() {
            envManager = GetComponent<EnvironmentManager>();
            grid = GetComponent<Grid3D_Platformer>();

            goalRef = Instantiate<EnvGoal>(goalTile);
            spawnRef = Instantiate<EnvSpawn>(spawnTile);
            tilePool = new ObjectPool<GameObject>(() => Instantiate(floorTile), OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 25, 250);
            pathRenderer = GetComponent<PathRenderer>();
        }

        #region pooling
        // Called when an item is returned to the pool using Release
        void OnReturnedToPool(GameObject go) {
            go.SetActive(false);
        }

        // Called when an item is taken from the pool using Get
        void OnTakeFromPool(GameObject go) {
            go.SetActive(true);
        }

        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        void OnDestroyPoolObject(GameObject go) {
            Destroy(go);
        }
        #endregion

        public void GenerateEnvironment() {
            grid.GenerateNewGrid();
        }

        public void GenerateGridEnvironment() {
            /*      ClearEnvironment_DEP();

                  Vector3 tileSize = floorTile.GetComponent<MeshRenderer>().bounds.size;

                  Vector3Int startIndex = GetRandomIndex();
                  availableIndices.Remove(startIndex);

                  Vector3Int goalIndex = GetRandomIndex();
                  availableIndices.Remove(goalIndex);

                  grid = new EnvGrid(envSize, transform.position, goalIndex, startIndex, tileSize);
                  grid.CreateGrid(useManhattanNeighbors);

                  if (usePath) {
                      pathIndices = Astar.GeneratePath(grid, useManhattanNeighbors, true);
                      Astar.ExpandPath(grid, grid.GetStartNode(), grid.GetGoalNode());
                  }

                  else
                      grid.FillGridWithTiles();

                  InstantiateNodePrefabsDep();

                  if (goalRef)
                      envManager.SubscribeToGoal(goalRef);
                  else {
                      Debug.Log(" Generation failed, try again : Start Index : " + startIndex + " Goal Index : " + goalIndex);
                      GenerateGridEnvironment();
                  }*/
        }

        public void UpdateGridNodeType(Vector3Int nodeIndex, NodeGridType newNodeType) {
            grid.UpdateGridNodeType(nodeIndex, newNodeType);
        }

        

        private void InstantiateNodePrefabsDep() {

            for (int x = 0; x < envSize.x; x++) {
                for (int y = 0; y < envSize.y; y++) {
                    for (int z = 0; z < envSize.z; z++) {
                        if (grid.GetGridNode(x, y, z).NodeType == NodeGridType.Start) {
                            spawnRef = Instantiate<EnvSpawn>(spawnTile, grid.GetGridNode(x, y, z).worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(spawnRef.gameObject);
                        }

                        else if (grid.GetGridNode(x, y, z).NodeType == NodeGridType.Goal) {
                            goalRef = Instantiate<EnvGoal>(goalTile, grid.GetGridNode(x, y, z).worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(goalRef.gameObject);
                        }

                        /* else if (grid.GetGridNode(x, y, z).NodeType == NodeGridType.Path) {
                             GameObject newTile = Instantiate<GameObject>(pathTile, grid.GetGridNode(x, y, z).worldPos, transform.rotation);
                             instantiatedEnvironmentObjects.Add(newTile);
                         }*/

                        else if (grid.GetGridNode(x, y, z).NodeType == NodeGridType.Tile) {
                            GameObject newTile = Instantiate<GameObject>(floorTile, grid.GetGridNode(x, y, z).worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(newTile);
                        }
                    }
                }
            }
        }

        public void InstantiateNodePrefabs() {
            for (int x = 0; x < grid.GetGridSize().x; x++) {
                for (int y = 0; y < grid.GetGridSize().y; y++) {
                    for (int z = 0; z < grid.GetGridSize().z; z++) {
                        if (grid.GetGridNode(x, y, z).NodeType == NodeGridType.Start) {
                            spawnRef.gameObject.SetActive(true);
                            spawnRef.transform.position = grid.GetGridNode(x, y, z).worldPos;
                            spawnRef.transform.rotation = transform.rotation;
                        }

                        else if (grid.GetGridNode(x, y, z).NodeType == NodeGridType.Goal) {
                            goalRef.gameObject.SetActive(true);
                            goalRef.transform.position = grid.GetGridNode(x, y, z).worldPos;
                            goalRef.transform.rotation = transform.rotation;
                        }

                        else if (grid.GetGridNode(x, y, z).NodeType == NodeGridType.Tile) {
                            GameObject newTile = tilePool.Get();
                            newTile.gameObject.SetActive(true);
                            newTile.transform.position = grid.GetGridNode(x, y, z).worldPos;
                            newTile.transform.rotation = transform.rotation;
                            instantiatedTiles.Add(newTile);
                        }
                    }
                }
            }
        }


        public void ClearEnvironment() {
            goalRef.gameObject.SetActive(false);
            spawnRef.gameObject.SetActive(false);

            foreach (GameObject gameObject in instantiatedTiles) {
                tilePool.Release(gameObject);
            }
            instantiatedTiles.Clear();
        }

       /* private void ClearEnvironment_DEP() {
            UpdateFromLessonPlan();

            goalRef = null;
            spawnRef = null;

            foreach (GameObject gameObject in instantiatedEnvironmentObjects) {
                Destroy(gameObject);
            }

            instantiatedEnvironmentObjects.Clear();

            availableIndices.Clear();
            for (int x = 0; x < envSize.x; x++) {
                for (int y = 0; y < envSize.y; y++) {
                    for (int z = 0; z < envSize.z; z++) {
                        availableIndices.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }*/

        private void UpdateFromLessonPlan() {
            // Get random range values from lesson plan
            LessonPlan_PlayerAgent.Instance.UpdateLessonIndex();
            envSize = LessonPlan_PlayerAgent.Instance.GetRandomBoardSize();
            usePath = LessonPlan_PlayerAgent.Instance.runtimeUsePath;
        }
    }
}
