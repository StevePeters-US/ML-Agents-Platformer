using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public class EnvironmentManager : MonoBehaviour {
        public bool envCompleted = false;

        private EnvironmentGenerator envGenerator;

        private EnvGoal envGoal;
        private EnvSpawn envSpawn;
        private PlayerAgent playerAgent;

        private void OnEnable() {
            EnvironmentalManagers.Instance.Add(this);

            playerAgent = GetComponentInChildren<PlayerAgent>();

            envGenerator = GetComponent<EnvironmentGenerator>();
        }

        private void OnDisable() {
            EnvironmentalManagers.Instance.Remove(this);
            UnsubscribeFromGoal();
        }

        public void SubscribeToGoal(EnvGoal newEnvGoal) {
            envGoal = newEnvGoal;
            envGoal.OnGoalTriggered += GoalTriggered;
        }

        private void UnsubscribeFromGoal() {
            envGoal.OnGoalTriggered -= GoalTriggered;
            envGoal = null;
        }

        // A player agent has entered the goal, ask the game state what to do
        public void GoalTriggered() {
            envCompleted = true;

            // This behavior should be decided by the game state
            playerAgent.AgentReachedGoal();
        }

        public void ResetEnvironment() {
            envGenerator.GenerateGridEnvironment();

            envSpawn = envGenerator.SpawnRef;

            if (envSpawn && playerAgent)
                envSpawn.ResetSpawnedAgent(playerAgent);
        }
    }
}
