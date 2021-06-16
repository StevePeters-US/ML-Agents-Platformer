using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG {
   // [RequireComponent(typeof(EnvGenAgent))]
    public class EnvAgentDebugGUI : MonoBehaviour {

        private EnvGenAgent agent;

        private void Awake() {
            agent = FindObjectOfType<EnvGenAgent>();
            DebugGUI.SetGraphProperties("currentTickReward", "Current Tick Reward", -.05f, .05f, 1, new Color(0, 1, 1), false);
            DebugGUI.SetGraphProperties("previousRunSuccessful", "previous Run Successful", 0f, 1f, 1, Color.green, false);
            DebugGUI.SetGraphProperties("envTime", "Environment Time", 0, 1, 1, Color.white, true);

            DebugGUI.SetGraphProperties("currentPathLength", "Current Path Length", 0, 22, 2, Color.white, false);
            DebugGUI.SetGraphProperties("targetPathLength", "Target Path Length", 0, 22, 2, Color.yellow, false);

            DebugGUI.SetGraphProperties("pathLengthReward", "Path Length Reward", -1, 1, 3, Color.green, true);
            DebugGUI.SetGraphProperties("pathFailedPunishment", "Path Failed Punishment", -10, 1, 3, Color.red, true);
            DebugGUI.SetGraphProperties("pathLengthSlope", "Path Length Slope", -1, 1, 3, Color.gray, true);

            DebugGUI.SetGraphProperties("avgCohesion", "Average Cohesion", 0, 1, 4, Color.white, true);
            DebugGUI.SetGraphProperties("cohesionReward", "Cohesion Reward", 0, 1, 4, Color.green, true);

            DebugGUI.SetGraphProperties("gridEmptySpace", "Grid Empty Space", 0, 1, 5, Color.white, true);
            DebugGUI.SetGraphProperties("gridEmptySpaceReward", "Grid Empty Space Reward", 0, 1, 5, Color.green, true);
        }

        private void Update() {

            if (agent.Grid != null) {
            DebugGUI.Graph("currentTickReward", agent.currentTickReward);
            DebugGUI.Graph("previousRunSuccessful", System.Convert.ToInt32(agent.PreviousRunSuccessful));
            DebugGUI.Graph("envTime", agent.EnvTime);

            DebugGUI.Graph("currentPathLength", agent.currentPathLength);
            DebugGUI.Graph("targetPathLength", agent.TargetPathLength);

            DebugGUI.Graph("pathLengthReward", agent.PathLengthReward);
            DebugGUI.Graph("pathFailedPunishment", agent.PathFailedPunishment);
            DebugGUI.Graph("pathLengthSlope", agent.PathLengthSlope);

            DebugGUI.Graph("avgCohesion", agent.avgCohesionValue);
            DebugGUI.Graph("cohesionReward", agent.CohesionReward);

            DebugGUI.Graph("gridEmptySpace", agent.gridEmptySpace);
            DebugGUI.Graph("gridEmptySpaceReward", agent.GridEmptySpaceReward);
            }
        }

        void OnDestroy() {
            // Clean up our logs and graphs when this object is destroyed
            DebugGUI.RemoveGraph("currentTickReward");
            DebugGUI.RemoveGraph("currentPathLength");
            DebugGUI.RemoveGraph("pathLengthReward");
            DebugGUI.RemoveGraph("avgCohesion");
            DebugGUI.RemoveGraph("gridEmptySpace");
        }
    }
}
