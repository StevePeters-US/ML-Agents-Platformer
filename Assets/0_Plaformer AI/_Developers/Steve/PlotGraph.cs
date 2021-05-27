using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG {
    [RequireComponent(typeof(LineRenderer))]
    public class PlotGraph : MonoBehaviour {
        public LineRenderer pathLine;
        public float stdDeviation = 1;
        public EnvGenAgent targetAgent;

        private void Awake() {
            pathLine = GetComponent<LineRenderer>();
        }

        void Update() {
            pathLine.startWidth = 0.1f;
            pathLine.positionCount = 25;

            if (targetAgent)
                stdDeviation = Mathf.Lerp(0.01f, 1f, targetAgent.EnvTime);

            for (int i = 0; i < 25; i++) {
                float curReward = MLAgentsExtensions.GetGaussianReward(i, 10, stdDeviation);

                Vector3 pos = transform.position;
                pos.x += i;
                pos.y += curReward;
                pathLine.SetPosition(i, pos);
            }
        }
    }
}
