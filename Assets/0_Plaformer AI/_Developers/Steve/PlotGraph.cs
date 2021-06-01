using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG {
    [RequireComponent(typeof(LineRenderer))]
    public class PlotGraph : MonoBehaviour {
        public LineRenderer pathLine;
        public float stdDeviation = 0.01f;
        //public EnvGenAgent targetAgent;


        private void Awake() {
            pathLine = GetComponent<LineRenderer>();
        }

        void Update() {
            pathLine.startWidth = 0.1f;
            pathLine.positionCount = 25;


            for (int i = 0; i < 25; i++) {
                float curReward = MLAgentsExtensions.GetGaussianReward(i, 10, stdDeviation);

                Vector3 pos = transform.position;
                pos.x += i;
                pos.y += curReward;
                pathLine.SetPosition(i, pos);
            }
      /*      if (stdDeviation > 2)
                stdDeviation = 0.01f;
            else
            stdDeviation += 0.001f;*/


        }
    }
}
