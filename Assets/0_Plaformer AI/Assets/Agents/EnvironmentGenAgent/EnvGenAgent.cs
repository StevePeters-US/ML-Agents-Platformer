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
        public EnvGrid Grid { get => grid; set => grid = value; }

        [SerializeField] private Vector3Int gridSize = new Vector3Int(20, 1, 20);
        public Vector3Int GridSize { get => gridSize; }
        public int GridCount { get => gridSize.x * gridSize.y * gridSize.z; }
        [SerializeField] private Vector3 tileSize = Vector3.one;

        [SerializeField] private GameObject floorTile;
        [SerializeField] private GameObject pathTile;
        [SerializeField] private EnvGoal goalTile;
        [SerializeField] private EnvSpawn spawnTile;
        [SerializeField] private GameObject failedTile;

        [SerializeField] private Material tileMat;
        [SerializeField] private Material pathMat;

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

        public GameObject FailedRef { get => failedRef; }
        private GameObject failedRef = null;

        [SerializeField] private bool drawGizmos = false;


        private Vector3Int currentIndex;

        private bool usePath = true;
        [SerializeField] private float targetPathLength = 12f;
        public float TargetPathLength { get => targetPathLength; }
        public bool useRandomTargetPathLength = false;

        public int CurrentPathLength { get => grid.GetPathLength; }
        // Path length reward gets tighter to target value the closer we get to the end of this episode
        public float PathLengthReward { get => MLAgentsExtensions.GetGaussianReward(CurrentPathLength, targetPathLength, Mathf.Lerp(0, 1f, EnvTime)); }
        // public float PathLengthSlope { get => Mathf.Clamp(MLAgentsExtensions.GetGaussianSlope(CurrentPathLength, targetPathLength, 0.25f, 10f), -1, 1); } // Use wider std dev to help guide the agent to the target
        public float PathLengthSlope { get => CurrentPathLength > targetPathLength ? -1 : 1; }

        public float PathFailedPunishment { get => Mathf.Lerp(0, -1, EnvTime); }

        private PathRenderer pathRenderer;

        const int ACTIONS_BRANCH = 1;
        [SerializeField] private bool maskActions = true;
        private bool[] validActions;
        //const int doNothing = 0;
        const int EMPTY = 0;
        const int TILE = 1;
        //const int GOAL = 2;
        //const int START = 3;
        //const int obstacle = 5;

        public float CompletionReward { get => Mathf.Lerp(10f, 0f, EnvTime); }
        public float currentTickReward;

        // 0 - 1 based on how much time is left in the episode
        public float EnvTime { get => (float)StepCount / (float)MaxStep; }

        private ActionSpec actionSpec;

        private StatsRecorder stats;
        private bool previousRunSuccessful = false;
        public bool PreviousRunSuccessful { get => previousRunSuccessful; }
        private int numEpsiodesRun;
        private int numEpisodesSuccessful;

        private Queue<bool> runSuccessQueue = new Queue<bool>();

        public bool debugCohesion = false;
        [SerializeField] private CohesionDebugText cohesionDebugTextPrefab;
        private CohesionDebugText[,,] cohesionDebugTexts;
        public float avgCohesionValue;

        public float gridEmptySpace; // 0 -1 tile types composition
        public float targetGridEmptySpace = 0.5f;
        public float GridEmptySpaceReward { get => MLAgentsExtensions.GetGaussianReward(gridEmptySpace, targetGridEmptySpace, Mathf.Lerp(2, 5f, EnvTime)); }

        public override void Initialize() {
            stats = Academy.Instance.StatsRecorder;
        }


        private void Awake() {
            validActions = new bool[2];
            goalRef = Instantiate<EnvGoal>(goalTile);
            spawnRef = Instantiate<EnvSpawn>(spawnTile);
            failedRef = Instantiate<GameObject>(failedTile);
            tilePool = new ObjectPool<GameObject>(() => Instantiate(floorTile), OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 25, 250);
            pathPool = new ObjectPool<GameObject>(() => Instantiate(pathTile), OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, 25);
            pathRenderer = GetComponent<PathRenderer>();

            // Can't modify in place
            actionSpec = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.ActionSpec;
            /*actionSpec.BranchSizes = new int[100];
            for (int i = 0; i < actionSpec.BranchSizes.Length; i++) {
                actionSpec.BranchSizes[i] = 2;
            }*/
            actionSpec.BranchSizes = new int[2];
            actionSpec.BranchSizes[0] = gridSize.x * gridSize.y * gridSize.z;
            actionSpec.BranchSizes[ACTIONS_BRANCH] = 2;
            GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.ActionSpec = actionSpec;
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

            /*if (!completed) {
                stats.Add("Completed", System.Convert.ToInt32(completed));
            }
            completed = false;*/
            numEpsiodesRun += 1;
            stats.Add("Success Ratio", (float)numEpisodesSuccessful / (float)numEpsiodesRun);


            /*  runSuccessQueue.Enqueue(previousRunSuccessful);
              if (runSuccessQueue.Count > 10)
                  runSuccessQueue.Dequeue();

              int numSuccess = 0;
              foreach (bool runSuccess in runSuccessQueue) {
                  numSuccess += 1;
              }*/
            stats.Add("Previous Run Successful", System.Convert.ToInt32(previousRunSuccessful));



            Vector3 gridOffset = new Vector3(-(gridSize.x / 2) * tileSize.x, 0, -(gridSize.z / 2) * tileSize.z);
            grid = new EnvGrid(gridSize, gridOffset + transform.position, tileSize);
            grid.CreateGrid(true);
            grid.FillGridWithRandomTiles(randomTileChance);

            int minPathLength = Astar.GetDistanceManhattan(grid.GridNodes[grid.StartIndex.x, grid.StartIndex.y, grid.StartIndex.z], grid.GridNodes[grid.GoalIndex.x, grid.GoalIndex.y, grid.GoalIndex.z]);

            if (useRandomTargetPathLength)
                targetPathLength = Random.Range(minPathLength, 20);

            else
                targetPathLength = minPathLength;

            // if (previousRunSuccessful)
            InstantiateNodePrefabs();
            if (debugCohesion)
                InstantiateCohesionDebugTexts();
            //else
            //      InstantiateFailedPrefab();

            //previousRunSuccessful = false;
            // InstantiateNodePrefabs();
        }

        private void OnDrawGizmos() {

            if (drawGizmos && grid != null)
                grid.DrawGrid();
        }

        private void ClearEnvironment() {
            //UpdateFromLessonPlan();

            goalRef.gameObject.SetActive(false);
            spawnRef.gameObject.SetActive(false);
            FailedRef.SetActive(false);

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
                        sensor.AddObservation(grid.GridNodes[x, y, z].locked ? 1 : 0);
                        sensor.AddObservation(grid.GridNodes[x, y, z].IsPathNode() ? 1 : 0);

                        // And a one hot encoded representation of the cell type
                        //float[] cellTypeBuffer = grid.GetOneHotCellData(new Vector3Int(x, y, x));
                        float[] cellTypeBuffer = grid.GetOneHotGridCellData(new Vector3Int(x, y, x));

                        foreach (float cellType in cellTypeBuffer)
                            sensor.AddObservation(cellType);


                        sensor.AddObservation(grid.GridNodes[x, y, z].cohesiveValue);
                        // Debug observations
                    }
                }
            }



            // 2 observations
            sensor.AddObservation(PathLengthReward);
            sensor.AddObservation(PathLengthSlope);

            // 1 observation
            sensor.AddObservation(EnvTime);

            sensor.AddObservation(avgCohesionValue);

            sensor.AddObservation(targetGridEmptySpace);
            sensor.AddObservation(gridEmptySpace);
            sensor.AddObservation(GridEmptySpaceReward);
        }

        public override void Heuristic(in ActionBuffers actionsOut) {
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut.Clear();

            discreteActionsOut[0] = Random.Range(0, gridSize.x * gridSize.z);
            discreteActionsOut[1] = Random.Range(0, 2);
            /*for (int i = 0; i < 8; i++) {
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

            /* for (int i = 0; i < gridSize.x * gridSize.y * gridSize.z; i++) {
                 discreteActionsOut[i] = Random.Range(0, 2);
             }*/
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {

            if (maskActions) {

                // Mask start and goal positions

                /*   for (int x = 0; x < grid.GridNodes.GetLength(0); x++) {
                       for (int y = 0; y < grid.GridNodes.GetLength(1); y++) {
                           for (int z = 0; z < grid.GridNodes.GetLength(2); z++) {
                               for (int i = 0; i < actionSpec.BranchSizes[x + y + z]; i++) {
                                   int branchIndex = x + y + (z * grid.GridNodes.GetLength(0));
                                   actionMask.SetActionEnabled(branchIndex, 1, !grid.GridNodes[x, y, z].locked);
                               }
                           }
                       }
                   }*/
            }
        }

        // Convert output from model into usable variables that can be used to pilot the agent.
        public override void OnActionReceived(ActionBuffers actionBuffers) {
            // Using single action buffer
            int gridNum = actionBuffers.DiscreteActions[0];
            currentIndex = new Vector3Int(gridNum % gridSize.x, 0, gridNum / gridSize.x);

            NodeType newNodeType;
            if (actionBuffers.DiscreteActions[ACTIONS_BRANCH] == EMPTY)
                newNodeType = NodeType.Empty;
            else if (actionBuffers.DiscreteActions[ACTIONS_BRANCH] == TILE)
                newNodeType = NodeType.Tile;
            else
                return;

            grid.GridNodes[currentIndex.x, currentIndex.y, currentIndex.z].NodeType = newNodeType;

            EvaluateEnvironment();

            ClearEnvironment();
            InstantiateNodePrefabs();
        }

        private void EvaluateEnvironment() {


            float tickReward = 0;

            void AddTickReward(float reward) {
                float normalizedReward = reward / MaxStep;
                AddReward(normalizedReward);
                tickReward += normalizedReward;
            }

            // Is there a valid path from start to goal?
            if (usePath && grid != null) {
                Astar.GeneratePath(grid, true, false);

                // if (!SpawnActive || !GoalActive)
                //   grid.path.Clear();
            }

            // Evaluate level and assign rewards
            /* if (CurrentPathLength == TargetPathLength) {
                 *//* AddReward(CompletionReward);
                  previousRunSuccessful = true;
                  numEpisodesSuccessful += 1;
                  EndEpisode();*//*
             }*/

            else if (CurrentPathLength == 0)
                AddTickReward(PathFailedPunishment);

            // Assign reward based on target path length
            else
                AddTickReward(PathLengthReward);

            UpdateCohesionValues();
            AddTickReward(avgCohesionValue);    // Maximize cohesion This should have a 0-1 influence range

            UpdateGridEmptySpaceCompositionVal();
            AddTickReward(GridEmptySpaceReward);

            currentTickReward = tickReward;

            // If we're on our last step, ensure that the environment is traversable and save grid
            if (StepCount == MaxStep) {
                if (CurrentPathLength == 0) {
                    AddReward(-10);
                    previousRunSuccessful = false;
                }

                else
                    previousRunSuccessful = true;
            }

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

        private void InstantiateFailedPrefab() {
            failedRef.SetActive(true);
            failedRef.transform.position = grid.GridNodes[0, 0, 0].worldPos;
            failedRef.transform.rotation = transform.rotation;
        }

        private void InstantiateCohesionDebugTexts() {
            cohesionDebugTexts = new CohesionDebugText[gridSize.x, gridSize.y, gridSize.z];

            if (cohesionDebugTextPrefab != null) {
                for (int x = 0; x < gridSize.x; x++) {
                    for (int y = 0; y < gridSize.y; y++) {
                        for (int z = 0; z < gridSize.z; z++) {


                            CohesionDebugText text = Instantiate(cohesionDebugTextPrefab, this.transform);
                            text.transform.position = grid.GridNodes[x, y, z].worldPos + new Vector3(0, 1.5f, 0);
                            cohesionDebugTexts[x, y, z] = text;

                        }
                    }
                }

            }
        }

        public void UpdateCohesionValues() {
            float totalCohesionValue = 0;

            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        float cohesiveValue = 0;
                        for (int i = 0; i < grid.GridNodes[x, y, z].allNeighborIndices.Count; i++) {
                            Vector3Int nodeIndex = grid.GridNodes[x, y, z].allNeighborIndices[i];
                            if (grid.GridNodes[x, y, z].NodeType == grid.GridNodes[nodeIndex.x, nodeIndex.y, nodeIndex.z].NodeType)
                                cohesiveValue += 1f / grid.GridNodes[x, y, z].allNeighborIndices.Count;
                        }

                        grid.GridNodes[x, y, z].cohesiveValue = cohesiveValue;
                        if (debugCohesion)
                            cohesionDebugTexts[x, y, z].UpdateText(cohesiveValue.ToString("#.00"));

                        totalCohesionValue += cohesiveValue;
                    }
                }
            }
            avgCohesionValue = totalCohesionValue / (GridCount);
        }

        public void UpdateGridEmptySpaceCompositionVal() {
            int numEmpty = 0;

            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        if (grid.GridNodes[x, y, z].NodeType == NodeType.Empty)
                            numEmpty += 1;
                    }
                }
            }

            gridEmptySpace = (float)numEmpty / (float)GridCount;
        }
    }
}