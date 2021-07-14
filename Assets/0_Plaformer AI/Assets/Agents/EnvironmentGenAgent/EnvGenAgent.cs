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

        private bool isTraining = false;
        public bool canStep = false;

        //public Action<Grid3D_Platformer> OnActionCompleted { get; set; }
        public Action<Vector3Int, NodeType, Vector3Int, NodeType> OnActionCompleted;
        public Action OnEpisodeBegan;
        public Action OnSuccessfulBuild;

        [SerializeField] private Grid3D_Platformer grid;
        public Grid3D_Platformer Grid { get => grid; }

        //[SerializeField] private Vector3Int gridSize = new Vector3Int(20, 1, 20);
        //public Vector3Int GridSize { get => grid.GetGridSize(); }
        //public int GridCount { get => gridSize.x * gridSize.y * gridSize.z; }

        // const int ACTIONS_BRANCH = 1;
        [SerializeField] private bool maskActions = true;

        //  const int EMPTY = 0;
        //  const int TILE = 1;
        //const int GOAL = 2;
        //const int START = 3;
        //const int obstacle = 5;


        // 0 - 1 based on how much time is left in the episode
        public float EnvTime { get => (float)StepCount / (float)MaxStep; }
        public float currentTickReward;

        private bool usePath = true;
        [Range(0, 1)] private float pathLengthInterpolator;
        public void OnPathLengthSliderChanged(float newSliderValue) { pathLengthInterpolator = newSliderValue; }

        //private float pathLengthWeight = 1;
        private float targetPathLength = 1f;
        public float TargetPathLength { get => targetPathLength; }
        public int currentPathLength;
        // Path length reward gets tighter to target value the closer we get to the end of this episode
        public float PathLengthReward { get => Mathf.Lerp(0, MLAgentsExtensions.GetGaussianReward(currentPathLength, targetPathLength, Mathf.Lerp(0, 1f, EnvTime)), pathLengthInfluence); }
        public float PathLengthSlope { get => currentPathLength == targetPathLength ? 0 : (currentPathLength > targetPathLength ? -1 : 1); }
        public float PathFailedPunishment { get => Mathf.Lerp(0, -1, EnvTime); }


        [Range(0, 1)] public float targetCohesion = 0.75f;
        public float avgCohesionValue;
        public float CohesionReward { get => Mathf.Lerp(0, MLAgentsExtensions.GetGaussianReward(avgCohesionValue, targetCohesion, Mathf.Lerp(2, 5f, EnvTime)), cohesionInfluence); }


        //   public float gridEmptySpace; // 0 -1 tile types composition
        //  [Range(0, 1)] public float targetGridEmptySpace = 0.5f;
        //  public float GridEmptySpaceReward { get => Mathf.Lerp(0, MLAgentsExtensions.GetGaussianReward(gridEmptySpace, targetGridEmptySpace, Mathf.Lerp(2, 5f, EnvTime)), gridEmptySpaceInfluence); }

        [Range(0, 1)] public float pathLengthInfluence;
        [Range(0, 1)] public float cohesionInfluence;
        // [Range(0, 1)] public float gridEmptySpaceInfluence;

        //   private float TotalInfluence { get => pathLengthWeight + cohesionInfluence + gridEmptySpaceInfluence; }
        //  private float NormalizedPathLengthInfluence { get => pathLengthWeight / TotalInfluence; }
        //  private float NormalizedCohesionInfluence { get => cohesionInfluence / TotalInfluence; }
        //  private float NormalizedGridEmptySpaceInfluence { get => gridEmptySpaceInfluence / TotalInfluence; }

        private StatsRecorder stats;
        private bool previousRunSuccessful = false;
        public bool PreviousRunSuccessful { get => previousRunSuccessful; }

        const int MOVEFROMBUFFERINDEX = 0;
        const int MOVETOBUFFERINDEX = 1;

        #region initialize
        public override void Initialize() {
            stats = Academy.Instance.StatsRecorder;
        }

        private void Awake() {
            Academy.Instance.AgentPreStep += RequestDecision;

            // Set the number of action choices to grid size
            ActionSpec actionSpec = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.ActionSpec;
            actionSpec.BranchSizes = new int[2];
            actionSpec.BranchSizes[MOVEFROMBUFFERINDEX] = 20 * 20;
            actionSpec.BranchSizes[MOVETOBUFFERINDEX] = 20 * 20;
            GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.ActionSpec = actionSpec;

            GameStateManager gsm = FindObjectOfType<GameStateManager>();
            Debug.Log("Env agent current state : " + gsm.GetStateName(), this);
            isTraining = gsm.GetStateName() == "Game State Training Environment Agent";
            Debug.Log("Agent is training : " + isTraining, this);
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

        #region recurring
        // Convert output from model into usable variables that can be used to pilot the agent.
        public override void OnActionReceived(ActionBuffers actionBuffers) {
            // Using single action buffer
            int moveFromActionIndex = actionBuffers.DiscreteActions[MOVEFROMBUFFERINDEX];
            Vector3Int moveFromIndex = new Vector3Int(moveFromActionIndex % grid.GetGridSize().x, 0, moveFromActionIndex / grid.GetGridSize().x);

            int moveToActionIndex = actionBuffers.DiscreteActions[MOVETOBUFFERINDEX];
            Vector3Int moveToIndex = new Vector3Int(moveToActionIndex % grid.GetGridSize().x, 0, moveToActionIndex / grid.GetGridSize().x);

            NodeType fromNodeType = NodeType.Empty;
            NodeType toNodeType = NodeType.Tile;

            EvaluateEnvironment();

            OnActionCompleted?.Invoke(moveFromIndex, fromNodeType, moveToIndex, toNodeType);
        }

        #endregion

        public override void OnEpisodeBegin() {
            stats.Add("Environment Agent/Previous Run Successful", System.Convert.ToInt32(previousRunSuccessful));

            if (isTraining) {
                UpdateAgentFromLessonPlan();
            }

            OnEpisodeBegan?.Invoke();
        }

        public void UpdateAgentFromLessonPlan() {
            // Get random range values from lesson plan
            LessonPlan_Environment.Instance.UpdateLessonIndex();

            pathLengthInterpolator = LessonPlan_Environment.Instance.GetRandomPathLength();
            targetCohesion = LessonPlan_Environment.Instance.GetRandomCohesion();
            //targetGridEmptySpace = LessonPlan_Environment.Instance.GetRandomGridEmptySpace();

            pathLengthInfluence = LessonPlan_Environment.Instance.GetPathLengthInfluence();
            cohesionInfluence = LessonPlan_Environment.Instance.GetCohesionInfluence();
            //gridEmptySpaceInfluence = LessonPlan_Environment.Instance.GetGridEmptySpaceInfluence();

            //randomTileChance = LessonPlan_Environment.Instance.GetStartingRandomTileChance();

            int minPathLength = Astar.GetDistanceManhattan(grid.GetStartNode(), grid.GetGoalNode());
            targetPathLength = Mathf.Lerp(minPathLength, 40, pathLengthInterpolator);
        }

        public override void CollectObservations(VectorSensor sensor) {

            // 1 observation
            sensor.AddObservation(EnvTime);

            // 2 observations
            sensor.AddObservation(PathLengthReward);
            sensor.AddObservation(PathLengthSlope);
            //sensor.AddObservation(pathLengthInfluence);

            sensor.AddObservation(CohesionReward);
            sensor.AddObservation(avgCohesionValue);
            sensor.AddObservation(targetCohesion);
            //    sensor.AddObservation(cohesionInfluence);

            // sensor.AddObservation(GridEmptySpaceReward);
            // sensor.AddObservation(gridEmptySpace);
            //  sensor.AddObservation(targetGridEmptySpace);
            //  sensor.AddObservation(gridEmptySpaceInfluence);
        }

        public override void Heuristic(in ActionBuffers actionsOut) {
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut.Clear();

            List<int> validFromIndices = new List<int>();
            List<int> validToIndices = new List<int>();

            for (int x = 0; x < grid.Grid3DData.GridNodes.GetLength(0); x++) {
                for (int y = 0; y < grid.Grid3DData.GridNodes.GetLength(1); y++) {
                    for (int z = 0; z < grid.Grid3DData.GridNodes.GetLength(2); z++) {
                        int actionIndex = x + y + (z * grid.Grid3DData.GridNodes.GetLength(0)); // Grid index to branch index - flatten array

                        if (!grid.Grid3DData.GridNodes[x, y, z].locked && !(grid.Grid3DData.GridNodes[x, y, z].NodeType == NodeType.Empty)) {
                            validFromIndices.Add(actionIndex);
                        }

                        else if (!grid.Grid3DData.GridNodes[x, y, z].locked && (grid.Grid3DData.GridNodes[x, y, z].NodeType == NodeType.Empty)) {
                            validToIndices.Add(actionIndex);
                        }
                    }
                }
            }

            discreteActionsOut[MOVEFROMBUFFERINDEX] = validFromIndices[Random.Range(0, validFromIndices.Count)];
            discreteActionsOut[MOVETOBUFFERINDEX] = validToIndices[Random.Range(0, validToIndices.Count)];
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
            if (maskActions) {
                for (int x = 0; x < grid.Grid3DData.GridNodes.GetLength(0); x++) {
                    for (int y = 0; y < grid.Grid3DData.GridNodes.GetLength(1); y++) {
                        for (int z = 0; z < grid.Grid3DData.GridNodes.GetLength(2); z++) {

                            int actionIndex = x + y + (z * grid.Grid3DData.GridNodes.GetLength(0)); // Grid index to branch index - flatten array

                            // Mask all locked indices, which should include start and goal indices  
                            actionMask.SetActionEnabled(MOVEFROMBUFFERINDEX, actionIndex, !grid.Grid3DData.GridNodes[x, y, z].locked);
                            actionMask.SetActionEnabled(MOVETOBUFFERINDEX, actionIndex, !grid.Grid3DData.GridNodes[x, y, z].locked);

                            // Can't move from empty. can't move to occupied tile
                            actionMask.SetActionEnabled(MOVEFROMBUFFERINDEX, actionIndex, grid.Grid3DData.GridNodes[x, y, z].NodeType == NodeType.Empty);
                            actionMask.SetActionEnabled(MOVETOBUFFERINDEX, actionIndex, grid.Grid3DData.GridNodes[x, y, z].NodeType == NodeType.Empty);
                        }
                    }
                }
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
                currentPathLength = grid.GetCurrentPathLength();

                if (currentPathLength == 0)
                    AddTickReward(PathFailedPunishment);

                // Assign reward based on target path length
                else
                    AddTickReward(PathLengthReward);
            }

            AddTickReward(CohesionReward);
            // AddTickReward(GridEmptySpaceReward);

            currentTickReward = tickReward;

            // If we're on our last step, ensure that the environment is traversable and save grid
            if (StepCount == MaxStep) {
                if (currentPathLength == 0) {
                    AddReward(-10);
                    previousRunSuccessful = false;
                }

                else {
                    previousRunSuccessful = true;
                    if (!isTraining) {
                        OnSuccessfulBuild?.Invoke();
                        canStep = false;
                    }
                }
            }

            // Update env stats on tensorboard
            stats.Add("Environment Agent/Path Length Reward", System.Convert.ToInt32(PathLengthReward));
            stats.Add("Environment Agent/Cohesion Reward", System.Convert.ToInt32(CohesionReward));
            // stats.Add("Environment Agent/Empty Space Reward", System.Convert.ToInt32(GridEmptySpaceReward));
        }
    }
}