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
        private bool GoalActive { get => goalRef.gameObject.activeInHierarchy; }

        public EnvSpawn SpawnRef { get => spawnRef; }
        private EnvSpawn spawnRef = null;
        private bool SpawnActive { get => spawnRef.gameObject.activeInHierarchy; }


        //   private List<GameObject> instantiatedEnvironmentObjects = new List<GameObject>();

        [SerializeField] private bool drawGizmos = false;
        [SerializeField] private bool drawPath = false;

        private Vector3Int currentIndex;

        private bool usePath = true;
        [SerializeField] private float targetPathLength = 12f;
        public float TargetPathLength { get => targetPathLength; }
        public bool useRandomTargetPathLength = false;
        private int currentPathLength;
        public int CurrentPathLength { get => currentPathLength; }
        // Path length reward gets tighter to target value the closer we get to the end of this episode
        public float PathLengthReward { get => MLAgentsExtensions.GetGaussianReward(CurrentPathLength, targetPathLength, Mathf.Lerp(-1f, 1f, EnvTime)); }
        public float PathLengthSlope { get => Mathf.Clamp(MLAgentsExtensions.GetGaussianSlope(CurrentPathLength, targetPathLength, 0.25f, 10f), -1, 1); } // Use wider std dev to help guide the agent to the target
        private PathRenderer pathRenderer;

        const int ACTIONS_BRANCH = 3;
        [SerializeField] private bool maskActions = true;
        private bool[] validActions;
        //const int doNothing = 0;
        const int EMPTY = 0;
        const int TILE = 1;
        const int GOAL = 2;
        const int START = 3;
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

            // Initialize total number of possible discrete actions
            ActionSpec tempSpec = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.ActionSpec;
            tempSpec.BranchSizes = new int[100];
            for (int i = 0; i < tempSpec.BranchSizes.Length; i++) {
                tempSpec.BranchSizes[i] = 4;
            }
            GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.ActionSpec = tempSpec;
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


            if (useRandomTargetPathLength)
                targetPathLength = Random.Range(3, 20);

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
                        //sensor.AddObservation(new Vector3Int(x, y, x).NormalizeVector3Int(GridSize));

                        // And a one hot encoded representation of the cell type
                        float[] cellTypeBuffer = grid.GetOneHotCellData(new Vector3Int(x, y, x));
                        foreach (float cellType in cellTypeBuffer)
                            sensor.AddObservation(cellType);

                        // Debug observations
                    }
                }
            }



            // 2 observations
            sensor.AddObservation(PathLengthReward);
            sensor.AddObservation(PathLengthSlope);

            // 1 observation
            sensor.AddObservation(EnvTime);
        }

        public override void Heuristic(in ActionBuffers actionsOut) {
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut.Clear();

            //discreteActionsOut[0] = Random.Range(0, gridSize.x * gridSize.z);
            /*for (int i = 0; i < 8; i++) {
                discreteActionsOut[i] = Random.Range(0, 2);
            }*/

            /*         discreteActionsOut[0] = Random.Range(0, 10);
                     discreteActionsOut[1] = 0;
                     discreteActionsOut[2] = Random.Range(0, 10);



                     List<int> validActionsList = new List<int>();
                     for (int i = 0; i < validActions.Length; i++) {
                         if (validActions[i] == true)
                             validActionsList.Add(i);
                     }
                     int randListIdx = Random.Range(0, validActionsList.Count);
                     discreteActionsOut[ACTIONS_BRANCH] = validActionsList[randListIdx];*/

            for (int i = 0; i < gridSize.x * gridSize.y * gridSize.z; i++) {
                discreteActionsOut[i] = Random.Range(0, 4);
            }
        }

        private void UpdateValidActionsArray() {
            //  validActions[doNothing] = true;
            validActions[EMPTY] = SpawnActive && GoalActive;
            validActions[TILE] = SpawnActive && GoalActive;
            validActions[GOAL] = !GoalActive;
            validActions[START] = !SpawnActive;
        }


        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
            // Mask the necessary actions if selected by the user.
            // Prevent spawning of duplicate goal and spawn tiles
            if (maskActions) {
                UpdateValidActionsArray();

                for (int i = 0; i < validActions.Length; i++) {
                    actionMask.SetActionEnabled(ACTIONS_BRANCH, i, validActions[i]);
                }
            }
        }

        // Convert output from model into usable variables that can be used to pilot the agent.
        public override void OnActionReceived(ActionBuffers actionBuffers) {
            // Using single action buffer
            //int gridNum = actionBuffers.DiscreteActions[0];
            //currentIndex = new Vector3Int(gridNum % gridSize.x, 0, gridNum / gridSize.x);

            /*     // Using 8 action buffers as binary byte
                // Our indices are stored as a binary byte
                 int flattenedIndex = 0;
                 for (int i = 0; i < 8; i++) {
                     flattenedIndex += (int)Mathf.Pow(2, i) * actionBuffers.DiscreteActions[i];
                 }

                 // If we're trying to manipulate an index outside of our grid, small penalty.
                 // We're doing this because action masking only works on the next frame
                 if(flattenedIndex > (gridSize.x * gridSize.y * gridSize.z - 1)) {
                     Debug.Log("flattened index out of range");
                     return;
                 }

                 currentIndex = new Vector3Int(flattenedIndex % gridSize.x, 0, flattenedIndex / gridSize.x);*/

            /* // Using 3 action buffers for x,y,z
             int x = actionBuffers.DiscreteActions[0];
             int y = actionBuffers.DiscreteActions[1];
             int z = actionBuffers.DiscreteActions[2];
             currentIndex = new Vector3Int(x, y, z);
             //Debug.Log(currentIndex);

             NodeType newNodeType;
             if (actionBuffers.DiscreteActions[ACTIONS_BRANCH] == empty)
                 newNodeType = NodeType.Empty;
             else if (actionBuffers.DiscreteActions[ACTIONS_BRANCH] == tile)
                 newNodeType = NodeType.Tile;
             else if (actionBuffers.DiscreteActions[ACTIONS_BRANCH] == goal) {
                 newNodeType = NodeType.Goal;
                 grid.GoalIndex = currentIndex;
             }
             else if (actionBuffers.DiscreteActions[ACTIONS_BRANCH] == start) {
                 newNodeType = NodeType.Start;
                 grid.StartIndex = currentIndex;
             }
             else
                 return;

             grid.GridNodes[currentIndex.x, currentIndex.y, currentIndex.z].NodeType = newNodeType;*/

            // Using 1 action space per tile
            for (int i = 0; i < actionBuffers.DiscreteActions.Length; i++) {

                // Check if node is locked
                // Check for multiple goals
                // Check for multiple starts
                NodeType newNodeType;
                if (actionBuffers.DiscreteActions[i] == EMPTY)
                    newNodeType = NodeType.Empty;
                else if (actionBuffers.DiscreteActions[i] == TILE)
                    newNodeType = NodeType.Tile;
                else if (actionBuffers.DiscreteActions[i] == GOAL) {
                    newNodeType = NodeType.Goal;
                    grid.GoalIndex = currentIndex;
                }
                else if (actionBuffers.DiscreteActions[i] == START) {
                    newNodeType = NodeType.Start;
                    grid.StartIndex = currentIndex;
                }
                else
                    return;

                currentIndex = new Vector3Int(i % gridSize.x, 0, i / gridSize.x);
                grid.GridNodes[currentIndex.x, currentIndex.y, currentIndex.z].NodeType = newNodeType;
            }


            ClearEnvironment();
            InstantiateNodePrefabs();
        }

        private void Update() {
            if (pathRenderer && drawPath) {
                pathRenderer.UpdatePath(grid.path, PathLengthReward);
            }
        }

        private void FixedUpdate() {
            float tickReward = 0;

            void AddTickReward(float reward) {
                float normalizedReward = reward / MaxStep;
                AddReward(normalizedReward);
                tickReward += normalizedReward;
            }

            // Is there a valid path from start to goal?
            if (usePath && grid != null) {
                Astar.GeneratePath(grid, true, false);

                if (!SpawnActive || !GoalActive)
                    grid.path.Clear();

                currentPathLength = grid.path.Count;
            }

            /*         if (currentPathLength == TargetPathLength) {
                         AddReward(1);
                         EndEpisode();
                     }

                     else
                         AddReward(-0.01f);*/
            // Evaluate level and assign rewards



            // Assign reward based on target path length
            if (CurrentPathLength == 0)
                AddTickReward(-0.1f);

            else
                AddTickReward(PathLengthReward);

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