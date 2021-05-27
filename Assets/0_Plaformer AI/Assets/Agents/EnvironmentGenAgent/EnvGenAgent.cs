using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using APG.Environment;
using UnityEngine.Pool;

namespace APG {
    public class EnvGenAgent : Agent {

        private EnvGrid grid;
        [SerializeField] private Vector3Int gridSize = new Vector3Int(20, 1, 20);
        public Vector3Int GridSize { get => gridSize; }
        [SerializeField] private Vector3 tileSize = Vector3.one;

        [SerializeField] private GameObject floorTile;
        [SerializeField] private GameObject pathTile;
        [SerializeField] private EnvGoal goalTile;
        [SerializeField] private EnvSpawn spawnTile;

        ObjectPool<GameObject> tilePool;
        private List<GameObject> instantiatedTiles = new List<GameObject>();

        ObjectPool<GameObject> pathPool;
        private List<GameObject> instantiatedPathTiles = new List<GameObject>();

        [SerializeField, Range(0, 1)] private float randomTileChance = 0.5f;

        public EnvGoal GoalRef { get => goalRef; }
        private EnvGoal goalRef = null;

        public EnvSpawn SpawnRef { get => spawnRef; }
        private EnvSpawn spawnRef = null;

        //   private List<GameObject> instantiatedEnvironmentObjects = new List<GameObject>();

        [SerializeField] private bool drawGizmos = false;
        [SerializeField] private bool drawPath = false;

        private Vector3Int currentIndex;

        private bool usePath = true;
        [SerializeField] private float desiredPathLength = 12f;
        public float DesiredPathLength { get => desiredPathLength; }
       // private List<Vector3Int> pathIndices;
        private int currentPathLength;
        public int CurrentPathLength { get => currentPathLength; }
        // Path length reward gets tighter to target value the closer we get to the end of this episode
        public float PathLengthReward { get => MLAgentsExtensions.GetGaussianReward(CurrentPathLength, desiredPathLength, Mathf.Lerp(0.01f, 1f, EnvTime)); }
        private PathRenderer pathRenderer;

        private bool maskActions = true;
        private bool[] validActions;
        const int doNothing = 0;
        const int empty = 1;
        const int tile = 2;
        const int goal = 3;
        const int start = 4;
        //const int obstacle = 5;

        public float currentTickReward;

        // 0 - 1 based on how much time is left in the episode
        public float EnvTime { get => (float)StepCount / (float)MaxStep; }

        private void Awake() {
            validActions = new bool[5];
            goalRef = Instantiate<EnvGoal>(goalTile);
            spawnRef = Instantiate<EnvSpawn>(spawnTile);
            tilePool = new ObjectPool<GameObject>(() => Instantiate(floorTile), OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 25, 250);
            pathPool = new ObjectPool<GameObject>(() => Instantiate(pathTile), OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, 25);
            pathRenderer = GetComponent<PathRenderer>();
        }

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

        public override void OnEpisodeBegin() {
            Debug.Log("Generating New Environment");
            ClearEnvironment();
            Vector3 gridOffset = new Vector3(-(gridSize.x / 2) * tileSize.x, 0, -(gridSize.z / 2) * tileSize.z);
            grid = new EnvGrid(gridSize, gridOffset + transform.position, tileSize);
            grid.CreateGrid(true);
            grid.FillGridWithRandomTiles(randomTileChance);

            InstantiateNodePrefabs();
        }

        private void OnDrawGizmos() {

            if (drawGizmos && grid != null)
                grid.DrawGrid();          
        }

        private void ClearEnvironment() {
            //UpdateFromLessonPlan();

            goalRef.gameObject.SetActive(false);
            spawnRef.gameObject.SetActive(false);

            foreach (GameObject gameObject in instantiatedTiles) {
                tilePool.Release(gameObject);
            }
            instantiatedTiles.Clear();

            foreach (GameObject gameObject in instantiatedPathTiles) {
                pathPool.Release(gameObject);
            }
            instantiatedPathTiles.Clear();
        }

        public override void CollectObservations(VectorSensor sensor) {

            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        // Add a normalized vector for grid cell position data
                        sensor.AddObservation(new Vector3Int(x, y, x).NormalizeVector3Int(GridSize));

                        // And a one hot encoded representation of the cell type
                        float[] cellTypeBuffer = grid.GetOneHotCellData(new Vector3Int(x, y, x));
                        foreach (float cellType in cellTypeBuffer)
                            sensor.AddObservation(cellType);

                        // Debug observations
                    }
                }
            }



            // 1 observation
            sensor.AddObservation(PathLengthReward);

            // 1 observation
            sensor.AddObservation(EnvTime);
        }

        public override void Heuristic(in ActionBuffers actionsOut) {
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut.Clear();

            discreteActionsOut[0] = Random.Range(0, gridSize.x * gridSize.z);

            List<int> validActionsList = new List<int>();
            for (int i = 0; i < validActions.Length; i++) {
                if (validActions[i] == true)
                    validActionsList.Add(i);
            }
            int randListIdx = Random.Range(0, validActionsList.Count);

            discreteActionsOut[1] = validActionsList[randListIdx];
        }

        private void UpdateValidActionsArray() {
            bool spawnActive = spawnRef.gameObject.activeInHierarchy;
            bool goalActive = goalRef.gameObject.activeInHierarchy;

            validActions[doNothing] = true;
            validActions[empty] = spawnActive && goalActive;
            validActions[tile] = spawnActive && goalActive;
            validActions[goal] = !goalActive;
            validActions[start] = !spawnActive;
        }


        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
            // Mask the necessary actions if selected by the user.
            // Prevent spawning of duplicate goal and spawn tiles
            if (maskActions) {
                UpdateValidActionsArray();

                for (int i = 0; i < validActions.Length; i++) {
                    actionMask.SetActionEnabled(1, i, validActions[i]);
                }
            }
        }

        // Convert output from model into usable variables that can be used to pilot the agent.
        public override void OnActionReceived(ActionBuffers actionBuffers) {
            int gridNum = actionBuffers.DiscreteActions[0];

            currentIndex = new Vector3Int(gridNum % gridSize.x, 0, gridNum / gridSize.x);

            NodeType newNodeType;
            if (actionBuffers.DiscreteActions[1] == empty)
                newNodeType = NodeType.Empty;
            else if (actionBuffers.DiscreteActions[1] == tile)
                newNodeType = NodeType.Tile;
            else if (actionBuffers.DiscreteActions[1] == goal) {
                newNodeType = NodeType.Goal;
                grid.GoalIndex = currentIndex;
            }
            else if (actionBuffers.DiscreteActions[1] == start) {
                newNodeType = NodeType.Start;
                grid.StartIndex = currentIndex;
            }
            else
                return;

            grid.GridNodes[currentIndex.x, currentIndex.y, currentIndex.z].NodeType = newNodeType;

            ClearEnvironment();
            InstantiateNodePrefabs();
        }

        private void Update() {
            if (pathRenderer && drawPath) {
                pathRenderer.UpdatePath(grid.path, PathLengthReward);
            }
        }

        private void FixedUpdate() {
            // Evaluate level and assign rewards
            float tickReward = 0;

            void AddTickReward(float reward) {
                float normalizedReward = reward / MaxStep;
                AddReward(normalizedReward);
                tickReward += normalizedReward;
            }

            // Is there a valid path from start to goal?
            if (usePath && grid != null) {
                Astar.GeneratePath(grid, true, false);
                currentPathLength = grid.path.Count;

                // Assign reward based on target path length
                if (CurrentPathLength == 0)
                    AddTickReward(-0.1f);

                else
                    AddTickReward(PathLengthReward);

            }
            currentTickReward = tickReward;
        }

        private void InstantiateNodePrefabs() {

            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        if (grid.GridNodes[x, y, z].NodeType == NodeType.Start) {
                            spawnRef.gameObject.SetActive(true);
                            spawnRef.transform.position = grid.GridNodes[x, y, z].worldPos;
                            spawnRef.transform.rotation = transform.rotation;
                        }

                        else if (grid.GridNodes[x, y, z].NodeType == NodeType.Goal) {
                            goalRef.gameObject.SetActive(true);
                            goalRef.transform.position = grid.GridNodes[x, y, z].worldPos;
                            goalRef.transform.rotation = transform.rotation;
                        }

                        /*   else if (grid.GridNodes[x, y, z].NodeType == NodeType.Path) {
                               GameObject newTile = Instantiate<GameObject>(pathTile, grid.GridNodes[x, y, z].worldPos, transform.rotation);
                               instantiatedEnvironmentObjects.Add(newTile);
                           }*/

                        else if (grid.GridNodes[x, y, z].NodeType == NodeType.Tile) {
                            GameObject newTile = tilePool.Get();
                            newTile.transform.position = grid.GridNodes[x, y, z].worldPos;
                            newTile.transform.rotation = transform.rotation;
                            instantiatedTiles.Add(newTile);
                        }
                    }
                }
            }
        }
    }
}