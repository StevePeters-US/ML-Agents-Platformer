using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG {
    [RequireComponent(typeof(LineRenderer))]
    public class SlopeGraph : MonoBehaviour {
        public LineRenderer pathLine;
        public float x = 5;
        public float stdDeviation = 0.01f;
        public float currentSlope;
        public float halfLineLength = 1f;

        private void Awake() {
            pathLine = GetComponent<LineRenderer>();
        }

        void Update() {
            pathLine.startWidth = 0.1f;
            pathLine.positionCount = 2;

            currentSlope = MLAgentsExtensions.GetGaussianSlope(x, 10, stdDeviation, 0.25f);
            float yOffset = MLAgentsExtensions.GetGaussianReward(x, 10, stdDeviation);


            Vector3 pos = transform.position;
            pos.x += x + halfLineLength;
            pos.y += (yOffset + (currentSlope * halfLineLength));
            pathLine.SetPosition(0, pos);

            pos = transform.position;
            pos.x += x - halfLineLength;
            pos.y += (yOffset - (currentSlope * halfLineLength));
            pathLine.SetPosition(1, pos);
        }
    }
}
