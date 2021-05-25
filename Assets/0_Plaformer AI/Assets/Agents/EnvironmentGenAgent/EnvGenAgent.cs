using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using APG.Environment;

namespace APG {
    public class EnvGenAgent : Agent {

        private EnvGrid grid;
        [SerializeField] private Vector3Int gridSize = new Vector3Int(20,1,20);
        [SerializeField] private Vector3 tileSize = Vector3.one;

        [SerializeField] private GameObject floorTile;
        [SerializeField] private GameObject pathTile;
        [SerializeField] private EnvGoal goalTile;
        [SerializeField] private EnvSpawn spawnTile;

        public EnvGoal GoalRef { get => goalRef; }
        private EnvGoal goalRef = null;

        public EnvSpawn SpawnRef { get => spawnRef; }
        private EnvSpawn spawnRef = null;

        private List<GameObject> instantiatedEnvironmentObjects = new List<GameObject>();


        public override void OnEpisodeBegin() {
            Debug.Log("Generating New Environment");
            ClearEnvironment();

            grid = new EnvGrid(gridSize, new Vector3(-10, 0, -10), tileSize);
            grid.CreateGrid(true);

            InstantiateNodePrefabs();
            // Generate Random environment
            // MLAgentsExtensions.WriteOneHot()
        }

        private void OnDrawGizmos() {
            if (grid != null)
                grid.DrawGrid();
        }

        private void ClearEnvironment() {
            //UpdateFromLessonPlan();

            goalRef = null;
            spawnRef = null;

            foreach (GameObject gameObject in instantiatedEnvironmentObjects) {
                Destroy(gameObject);
            }

            instantiatedEnvironmentObjects.Clear();

          /*  availableIndices.Clear();
            for (int x = 0; x < envSize.x; x++) {
                for (int y = 0; y < envSize.y; y++) {
                    for (int z = 0; z < envSize.z; z++) {
                        availableIndices.Add(new Vector3Int(x, y, z));
                    }
                }
            }*/
        }

        private void InstantiateNodePrefabs() {

            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    for (int z = 0; z < gridSize.z; z++) {
                        if (grid.GridNodes[x, y, z].NodeType == NodeType.Start) {
                            spawnRef = Instantiate<EnvSpawn>(spawnTile, grid.GridNodes[x, y, z].worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(spawnRef.gameObject);
                        }

                        else if (grid.GridNodes[x, y, z].NodeType == NodeType.Goal) {
                            goalRef = Instantiate<EnvGoal>(goalTile, grid.GridNodes[x, y, z].worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(goalRef.gameObject);
                        }

                        else if (grid.GridNodes[x, y, z].NodeType == NodeType.Path) {
                            GameObject newTile = Instantiate<GameObject>(pathTile, grid.GridNodes[x, y, z].worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(newTile);
                        }

                        else if (grid.GridNodes[x, y, z].NodeType == NodeType.Tile) {
                            GameObject newTile = Instantiate<GameObject>(floorTile, grid.GridNodes[x, y, z].worldPos, transform.rotation);
                            instantiatedEnvironmentObjects.Add(newTile);
                        }
                    }
                }
            }
        }
    }
}