using APG.Environment;
using UnityEngine;

namespace APG {
    public class CohesionDebugger : MonoBehaviour {
        private EnvGenAgent envAgent;

        [SerializeField] private CohesionDebugText cohesionDebugTextPrefab;
        private CohesionDebugText[,,] cohesionDebugTexts;

        private Vector3Int gridSize;

        private void Awake() {
            envAgent = FindObjectOfType<EnvGenAgent>();
            envAgent.OnEpisodeBegan += InstantiateCohesionDebugTexts;
        }

        private void InstantiateCohesionDebugTexts(Grid_3D grid) {
            cohesionDebugTexts = new CohesionDebugText[grid.GridSize.x, grid.GridSize.y, grid.GridSize.z];
            gridSize = grid.GridSize;

            if (cohesionDebugTextPrefab != null) {
                for (int x = 0; x < grid.GridSize.x; x++) {
                    for (int y = 0; y < grid.GridSize.y; y++) {
                        for (int z = 0; z < grid.GridSize.z; z++) {
                            CohesionDebugText text = Instantiate(cohesionDebugTextPrefab, this.transform);
                            text.transform.position = grid.GridNodes[x, y, z].worldPos + new Vector3(0, 1.5f, 0);
                            cohesionDebugTexts[x, y, z] = text;
                        }
                    }
                }
            }
        }

        private void Update() {
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        cohesionDebugTexts[x, y, z].UpdateText(envAgent.Grid.GridNodes[x, y, z].cohesiveValue.ToString("#.00"));
                    }
                }
            }
        }

    }
}
