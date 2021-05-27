using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG {
    [RequireComponent(typeof(EnvGenAgent))]
    public class EnvAgentDebugGUI : MonoBehaviour {

        private EnvGenAgent agent;

        private void Awake() {
            agent = GetComponent<EnvGenAgent>();
            DebugGUI.SetGraphProperties("currentTickReward", "Current Tick Reward", -2, 2, 1, new Color(0, 1, 1), false);
            DebugGUI.SetGraphProperties("currentPathLength", "Current Path Length", 0, agent.DesiredPathLength * 2, 2, new Color(1, 0.5f, 1), false);
            DebugGUI.SetGraphProperties("pathLengthReward", "Path Length Reward", 0, 1, 3, new Color(1, 1, 0), true);
        }

        private void Update() {

            DebugGUI.Graph("currentTickReward", agent.currentTickReward);
            DebugGUI.Graph("currentPathLength", agent.CurrentPathLength);
            DebugGUI.Graph("pathLengthReward", agent.PathLengthReward);
        }

        void OnDestroy() {
            // Clean up our logs and graphs when this object is destroyed
            DebugGUI.RemoveGraph("currentTickReward");
            DebugGUI.RemoveGraph("currentPathLength");
            DebugGUI.RemoveGraph("pathLengthReward");
        }
    }
}
