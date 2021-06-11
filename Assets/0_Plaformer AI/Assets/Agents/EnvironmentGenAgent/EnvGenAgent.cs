using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using APG.Environment;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace APG {
    public class EnvGenAgent : Agent {

        public bool isTraining = true;
        public bool canStep = false;

        public Action<EnvGrid, Vector3Int> OnActionCompleted { get; set; }
        public Action<EnvGrid, Vector3Int> OnEpisodeBegan;
        public Action<EnvGrid> OnSuccessfulBuild;

        private EnvGrid grid;
        public EnvGrid Grid { get => grid; set => grid = value; }

        [SerializeField] private Vector3Int gridSize = new Vector3Int(20, 1, 20);
        public Vector3Int GridSize { get => gridSize; }
        public int GridCount { get => gridSize.x * gridSize.y * gridSize.z; }
        [SerializeField] private Vector3 tileSize = Vector3.one;


        [SerializeField, Range(0, 1)] private float randomTileChance = 0.5f;

        [SerializeField] private bool drawGizmos = false;


        private Vector3Int currentIndex;

        private bool usePath = true;
        private int maxPathLength = 40; // #Todo This needs to be calculated
       [Range(0,1)] private float pathLengthSlider;
        public void OnPathLengthSliderChanged(float newSliderValue) {
            pathLengthSlider = newSliderValue;
        }

        private float targetPathLength = 1f;
        public float TargetPathLength { get => targetPathLength; }
        public int CurrentPathLength { get => grid.GetPathLength; }
        // Path length reward gets tighter to target value the closer we get to the end of this episode
        public float PathLengthReward { get => Mathf.Lerp(0, MLAgentsExtensions.GetGaussianReward(CurrentPathLength, targetPathLength, Mathf.Lerp(0, 1f, EnvTime)), NormalizedPathLengthInfluence); }
        public float PathLengthSlope { get => CurrentPathLength == targetPathLength ? 0 : (CurrentPathLength > targetPathLength ? -1 : 1); }

        public float PathFailedPunishment { get => Mathf.Lerp(0, -1, EnvTime); }

        const int ACTIONS_BRANCH = 1;
        [SerializeField] private bool maskActions = true;
        private bool[] validActions;
        //const int doNothing = 0;
        const int EMPTY = 0;
        const int TILE = 1;
        //const int GOAL = 2;
        //const int START = 3;
        //const int obstacle = 5;

        public float currentTickReward;

        // 0 - 1 based on how much time is left in the episode
        public float EnvTime { get => (float)StepCount / (float)MaxStep; }


        private StatsRecorder stats;
        private bool previousRunSuccessful = false;
        public bool PreviousRunSuccessful { get => previousRunSuccessful; }

        [Range(0, 1)] public float targetCohesionValue = 0.75f;
        public float avgCohesionValue;
        public float CohesionReward { get => Mathf.Lerp(0, MLAgentsExtensions.GetGaussianReward(avgCohesionValue, targetCohesionValue, Mathf.Lerp(2, 5f, EnvTime)), NormalizedCohesionInfluence); }


        public float gridEmptySpace; // 0 -1 tile types composition
        [Range(0, 1)] public float targetGridEmptySpace = 0.5f;
        public float GridEmptySpaceReward { get => Mathf.Lerp(0, MLAgentsExtensions.GetGaussianReward(gridEmptySpace, targetGridEmptySpace, Mathf.Lerp(2, 5f, EnvTime)), NormalizedGridEmptySpaceInfluence); }


        [Range(0, 1)] public float pathLengthInfluence = 1;
        [Range(0, 1)] public float cohesionInfluence = 1;
        [Range(0, 1)] public float gridEmptySpaceInfluence = 1;

        private float TotalInfluence { get => pathLengthInfluence + cohesionInfluence + gridEmptySpaceInfluence; }
        private float NormalizedPathLengthInfluence { get => pathLengthInfluence / TotalInfluence; }
        private float NormalizedCohesionInfluence { get => cohesionInfluence / TotalInfluence; }
        private float NormalizedGridEmptySpaceInfluence { get => gridEmptySpaceInfluence / TotalInfluence; }

        #region initialize
        public override void Initialize() {
            stats = Academy.Instance.StatsRecorder;

        }
        private void Awake() {
            Academy.Instance.AgentPreStep += RequestDecision;

            validActions = new bool[2];

            // Set the number of action choices to grid size
            ActionSpec actionSpec = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.ActionSpec;
            actionSpec.BranchSizes = new int[2];
            actionSpec.BranchSizes[0] = gridSize.x * gridSize.y * gridSize.z;
            actionSpec.BranchSizes[ACTIONS_BRANCH] = 2;
            GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.ActionSpec = actionSpec;
        }


        void OnDestroy() {
            if (Academy.IsInitialized) {
                Academy.Instance.AgentPreStep -= RequestDecision;
            }
        }
        void RequestDecision(int academyStepCount) {
            if (isTraining || canStep)
                RequestDecision();
        }

        #endregion

        #region GUI
        private void OnDrawGizmos() {

            if (drawGizmos && grid != null)
                grid.DrawGrid();
        }
        #endregion

        #region recurring
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

            if (OnActionCompleted != null)
                OnActionCompleted(grid, GridSize);
        }

        #endregion

        public override void OnEpisodeBegin() {
            stats.Add("Previous Run Successful", System.Convert.ToInt32(previousRunSuccessful));

            Vector3 gridOffset = new Vector3(-(gridSize.x / 2) * tileSize.x, 0, -(gridSize.z / 2) * tileSize.z);
            grid = new EnvGrid(gridSize, gridOffset + transform.position, tileSize);
            grid.CreateGrid(true);
            grid.FillGridWithRandomTiles(randomTileChance);

            UpdateFromLessonPlan();

            if (OnEpisodeBegan != null)
                OnEpisodeBegan(grid, GridSize);
        }


        private void UpdateFromLessonPlan() {
            // Get random range values from lesson plan
            LessonPlan_Environment.Instance.UpdateLessonIndex();

            pathLengthInfluence = LessonPlan_Environment.Instance.GetPathInfluence();
            cohesionInfluence = LessonPlan_Environment.Instance.GetCohesionInfluence();
            gridEmptySpaceInfluence = LessonPlan_Environment.Instance.GetGridEmptySpaceInfluence();

            int minPathLength = Astar.GetDistanceManhattan(grid.GridNodes[grid.StartIndex.x, grid.StartIndex.y, grid.StartIndex.z], grid.GridNodes[grid.GoalIndex.x, grid.GoalIndex.y, grid.GoalIndex.z]);
            targetPathLength = Mathf.Lerp( minPathLength, maxPathLength, pathLengthSlider);
            targetCohesionValue = LessonPlan_Environment.Instance.GetTargetCohesion();
            targetGridEmptySpace = LessonPlan_Environment.Instance.GetTargetGridEmptySpace();
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

            // 1 observation
            sensor.AddObservation(EnvTime);

            // 2 observations
            sensor.AddObservation(PathLengthReward);
            sensor.AddObservation(PathLengthSlope);
            sensor.AddObservation(NormalizedPathLengthInfluence);

            sensor.AddObservation(CohesionReward);
            sensor.AddObservation(avgCohesionValue);
            sensor.AddObservation(targetCohesionValue);
            sensor.AddObservation(NormalizedCohesionInfluence);

            sensor.AddObservation(GridEmptySpaceReward);
            sensor.AddObservation(gridEmptySpace);
            sensor.AddObservation(targetGridEmptySpace);
            sensor.AddObservation(NormalizedGridEmptySpaceInfluence);
        }

        public override void Heuristic(in ActionBuffers actionsOut) {
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut.Clear();

            discreteActionsOut[0] = Random.Range(0, gridSize.x * gridSize.z);
            discreteActionsOut[1] = Random.Range(0, 2);
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
        private void EvaluateEnvironment() {
            float tickReward = 0;

            void AddTickReward(float reward) {
                float normalizedReward = reward / (float)MaxStep;
                AddReward(normalizedReward);
                tickReward += normalizedReward;
            }

            // Is there a valid path from start to goal?
            if (usePath && grid != null) {
                Astar.GeneratePath(grid, true, false);

                if (CurrentPathLength == 0)
                    AddTickReward(PathFailedPunishment);

                // Assign reward based on target path length
                else
                    AddTickReward(PathLengthReward);
            }

            UpdateCohesionValues();
            AddTickReward(CohesionReward);    // Maximize cohesion This should have a 0-1 influence range

            UpdateGridEmptySpaceCompositionVal();
            AddTickReward(GridEmptySpaceReward);

            currentTickReward = tickReward;

            // If we're on our last step, ensure that the environment is traversable and save grid
            if (StepCount == MaxStep) {
                if (CurrentPathLength == 0) {
                    AddReward(-10);
                    previousRunSuccessful = false;
                }

                else {
                    previousRunSuccessful = true;
                    if (!isTraining) {
                        OnSuccessfulBuild(grid);
                        canStep = false;
                    }
                }
            }
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

                        totalCohesionValue += cohesiveValue;
                    }
                }
            }
            avgCohesionValue = totalCohesionValue / (GridCount);
        }
    }
}