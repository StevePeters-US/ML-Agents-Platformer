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
        [SerializeField] private Grid3D_Platformer grid;

        private void Update() {
            if (drawPath && grid != null)
                DrawPath(grid.pathIndices);
        }

        public void DrawPath(List<Vector3Int> pathNodes, float pathLengthReward = 1) {
            pathLine.startWidth = 0.1f;
            pathLine.positionCount = pathNodes.Count;

            for (int i = 0; i < pathNodes.Count; i++) {
                Vector3 pos = grid.Grid3DData.GridNodes[pathNodes[i].x, pathNodes[i].y, pathNodes[i].z].worldPos;
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
