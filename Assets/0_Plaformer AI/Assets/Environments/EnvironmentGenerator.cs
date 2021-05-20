using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

namespace APG.Environment {
    // Creates a grid type environment at runtime for training
    [RequireComponent(typeof(EnvironmentManager))]
    public class EnvironmentGenerator : MonoBehaviour {
        [SerializeField] private GameObject floorTile;
        [SerializeField] private GameObject pathTile;
        [SerializeField] private EnvGoal goalTile;
        [SerializeField] private EnvSpawn spawnTile;

        [SerializeField] private bool usePath = false;
        [SerializeField] private bool useManhattanNeighbors = true;

        [SerializeField, Tooltip("Rounded to nearest int")] private Vector3Int envSize;

        private Grid grid;

        public EnvGoal GoalRef { get => goalRef; }
        private EnvGoal goalRef = null;

        public EnvSpawn SpawnRef { get => spawnRef; }
        private EnvSpawn spawnRef = null;

        private List<GameObject> instantiatedEnvironmentObjects = new List<GameObject>();
        private List<Vector3Int> excludedIndices = new List<Vector3Int>();
        private List<Vector3Int> pathIndices = new List<Vector3Int>();
        private EnvironmentManager envManager;



        private void Awake() {
            envManager = GetComponent<EnvironmentManager>();
        }

        public void GenerateGridEnvironment() {
            DestroyEnvObjects();

            UpdateFromLessonPlan();
            Vector3 tileSize = floorTile.GetComponent<MeshRenderer>().bounds.size;

            grid = new Grid(envSize, transform.position, GetRandomIndex(), GetRandomIndex(), tileSize);
            grid.CreateGrid(useManhattanNeighbors);

            if (usePath) {
                pathIndices = Astar.GeneratePath(grid, grid.GetStartNode(), grid.GetGoalNode(), useManhattanNeighbors);
                Astar.ExpandPath(grid, grid.GetStartNode(), grid.GetGoalNode());
            }

            else
                grid.FillGridWithTiles();

            InstantiateNodePrefabs();

            if (goalRef)
                envManager.SubscribeToGoal(goalRef);
            else {
                Debug.Log(" Generation failed, try again");
                GenerateGridEnvironment();
            }
        }

        private Vector3Int GetRandomIndex() {
            Vector3Int outPos = Vector3Int.zero;
            int i = 0;
            while (i < 100 || IsIndexExcluded(outPos)) {
                i++;
                outPos = new Vector3Int(Random.Range(0, envSize.x), 0, Random.Range(0, envSize.z));
            }

            return outPos;
        }

        private bool IsIndexExcluded(Vector3Int indexToCheck) {
            return excludedIndices.Contains(indexToCheck);
        }

        private void InstantiateNodePrefabs() {

            for (int x = 0; x < envSize.x; x++) {
                for (int y = 0; y < envSize.y; y++) {
                    for (int z = 0; z < envSize.z; z++) {
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

        private void DestroyEnvObjects() {
            goalRef = null;
            spawnRef = null;

            foreach (GameObject gameObject in instantiatedEnvironmentObjects) {
                Destroy(gameObject);
            }

            instantiatedEnvironmentObjects.Clear();
            excludedIndices.Clear();
        }

        private void UpdateFromLessonPlan() {
            // Get random range values from lesson plan
            LessonPlan.Instance.UpdateLessonIndex();
            envSize = LessonPlan.Instance.GetRandomBoardSize();
            usePath = LessonPlan.Instance.runtimeUsePath;
        }
    }
}
