using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace APG.Environment {
    [RequireComponent(typeof(LineRenderer))]
    public class PathRenderer : MonoBehaviour {
        public LineRenderer pathLine;
        [SerializeField] private bool drawPath = false;
        public float yOffset = 1.5f;

        private EnvGenAgent envGenAgent;

        private void Awake() {
            envGenAgent = GetComponent<EnvGenAgent>();
        }

        private void Update() {
            if (drawPath && envGenAgent)
                UpdatePath(envGenAgent.Grid.path);
        }

        public void UpdatePath(List<Node> pathNodes, float pathLengthReward = 1) {
            pathLine.startWidth = 0.1f;
            pathLine.positionCount = pathNodes.Count;

            for (int i = 0; i < pathNodes.Count; i++) {
                Vector3 pos = pathNodes[i].worldPos;
                pos.y += yOffset;
                pathLine.SetPosition(i, pos);
            }

            // Interpolate our color from red to green based on how close our path is to the target length
            float rHue;
            float gHue;
            float outHue;
            float value;
            float saturation;
            Color.RGBToHSV(Color.red, out rHue, out saturation, out value);
            Color.RGBToHSV(Color.green, out gHue, out saturation, out value);
            outHue = Mathf.Lerp(rHue, gHue, pathLengthReward);

            pathLine.material.color = Color.HSVToRGB(outHue, saturation, value);
        }
    }
}
