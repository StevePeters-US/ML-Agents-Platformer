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

        private void InstantiateCohesionDebugTexts(EnvGrid grid, Vector3Int newGridSize) {
            cohesionDebugTexts = new CohesionDebugText[newGridSize.x, newGridSize.y, newGridSize.z];
            gridSize = newGridSize;

            if (cohesionDebugTextPrefab != null) {
                for (int x = 0; x < newGridSize.x; x++) {
                    for (int y = 0; y < newGridSize.y; y++) {
                        for (int z = 0; z < newGridSize.z; z++) {
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
