using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public class EnvironmentManager : MonoBehaviour {
        public bool envCompleted = false;

        private EnvironmentGenerator envGenerator;
        [SerializeField] private EnvGenAgent envAgent;

        private EnvGoal envGoal;
        private EnvSpawn envSpawn;
        private PlayerAgent playerAgent;

        private Grid_3D grid;

        private float defaultTimeScale;

        private void OnEnable() {
            EnvironmentalManagers.Instance.Add(this);

            envGenerator = FindObjectOfType<EnvironmentGenerator>();
            if (envAgent == null)
                envAgent = FindObjectOfType<EnvGenAgent>();

            envAgent.OnActionCompleted += OnActionTaken;
            envAgent.OnEpisodeBegan += OnEpisodeBegin;
            envAgent.OnSuccessfulBuild += OnSuccessfulBuild;

            playerAgent = GetComponentInChildren<PlayerAgent>();

            defaultTimeScale = Time.timeScale;
        }

        private void OnDestroy() {
            envAgent.OnActionCompleted -= OnActionTaken;
            envAgent.OnEpisodeBegan -= OnEpisodeBegin;
            envAgent.OnSuccessfulBuild -= OnSuccessfulBuild;
        }

        private void OnActionTaken() {
            if (envGenerator != null) {
                envGenerator.ClearEnvironment();
                envGenerator.InstantiateNodePrefabs();
            }
        }

        private void OnEpisodeBegin() {
            UpdateFromLessonPlan();

            envGenerator.GenerateEnvironment();



            // int minPathLength = Astar.GetDistanceManhattan(grid.GetStartNode(), grid.GetGoalNode());
            // targetPathLength = Mathf.Lerp(minPathLength, maxPathLength, pathLengthInterpolator);

        }

        private void UpdateFromLessonPlan() {
            // Update grid

            // Update agent
        }

        private void OnSuccessfulBuild() {
            Time.timeScale = defaultTimeScale;

            if (envGenerator != null) {
                envGenerator.ClearEnvironment();
                envGenerator.InstantiateNodePrefabs();
            }
        }

        private void OnDisable() {
            EnvironmentalManagers.Instance.Remove(this);
            // UnsubscribeFromGoal();
        }

        public void SubscribeToGoal(EnvGoal newEnvGoal) {
            if (envGoal)
                envGoal.OnGoalTriggered -= GoalTriggered;

            envGoal = newEnvGoal;
            envGoal.OnGoalTriggered += GoalTriggered;
            //envGoal.OnGoalDestroyed += GoalDestroyed;
        }

        /* public void UnsubscribeFromGoal() {
             if (envGoal) {
             }
         }*/

        // A player agent has entered the goal, ask the game state what to do
        public void GoalTriggered() {
            envCompleted = true;

            // This behavior should be decided by the game state
            playerAgent.AgentReachedGoal();
        }

        /*  public void GoalDestroyed() {
              if (envGoal != null) {
              envGoal.OnGoalTriggered -= GoalTriggered;
              envGoal.OnGoalDestroyed -= GoalDestroyed;
              envGoal = null;
              }
          }*/

        public void ResetEnvironment() {
            envGenerator.GenerateGridEnvironment();

            envSpawn = envGenerator.SpawnRef;

            if (envSpawn != null && playerAgent)
                envSpawn.ResetSpawnedAgent(playerAgent);
        }

        public void GenerateEnvironment() {
            if (envAgent != null) {
                envAgent.GenerateNewGrid();
                envAgent.canStep = true;
                Time.timeScale = 50;
            }
        }
    }
}
