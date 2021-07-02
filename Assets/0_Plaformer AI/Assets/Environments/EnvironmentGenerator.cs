using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using UnityEngine.Pool;


namespace APG.Environment {
    // Given a grid, the environment generator instantiates all required objects.
    [RequireComponent(typeof(EnvironmentManager))]
    [RequireComponent(typeof(Grid3D_Platformer))]

    public class EnvironmentGenerator : MonoBehaviour {
        [SerializeField] private GameObject floorTile;
        [SerializeField] private GameObject pathTile;
        [SerializeField] private EnvGoal goalTile;
        [SerializeField] private EnvSpawn spawnTile;
        [SerializeField] private GameObject failedTile;

        [SerializeField] private Material tileMat;
        [SerializeField] private Material pathMat;

        ObjectPool<GameObject> tilePool;
        private List<GameObject> instantiatedTiles = new List<GameObject>();

        [SerializeField, Tooltip("Rounded to nearest int")] private Vector3Int envSize;

        private Grid3D_Platformer grid;

        public EnvGoal GoalRef { get => goalRef; }
        private EnvGoal goalRef = null;

        public EnvSpawn SpawnRef { get => spawnRef; }
        private EnvSpawn spawnRef = null;

        private void Awake() {
            grid = GetComponent<Grid3D_Platformer>();

            goalRef = Instantiate<EnvGoal>(goalTile);
            spawnRef = Instantiate<EnvSpawn>(spawnTile);
            tilePool = new ObjectPool<GameObject>(() => Instantiate(floorTile), OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 25, 250);
        }

        #region pooling
        // Called when an item is returned to the pool using Release
        void OnReturnedToPool(GameObject go) {
            go.SetActive(false);
        }

        // Called when an item is taken from the pool using Get
        void OnTakeFromPool(GameObject go) {
            go.SetActive(true);
        }

        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        void OnDestroyPoolObject(GameObject go) {
            Destroy(go);
        }
        #endregion

        public void GenerateEnvironment() {
            grid.GenerateNewGrid();
            InstantiateNodePrefabs();
        }

        public void UpdateGridNodeType(Vector3Int nodeIndex, NodeType newNodeType) {
            grid.UpdateGridNodeType(nodeIndex, newNodeType);
        }

        public void InstantiateNodePrefabs() {
            ClearNodePrefabs();

            for (int x = 0; x < grid.GetGridSize().x; x++) {
                for (int y = 0; y < grid.GetGridSize().y; y++) {
                    for (int z = 0; z < grid.GetGridSize().z; z++) {
                        if (grid.GetGridNode(x, y, z).NodeType == NodeType.Start) {
                            spawnRef.gameObject.SetActive(true);
                            spawnRef.transform.position = grid.GetGridNode(x, y, z).worldPos;
                            spawnRef.transform.rotation = transform.rotation;
                        }

                        else if (grid.GetGridNode(x, y, z).NodeType == NodeType.Goal) {
                            goalRef.gameObject.SetActive(true);
                            goalRef.transform.position = grid.GetGridNode(x, y, z).worldPos;
                            goalRef.transform.rotation = transform.rotation;
                        }

                        else if (grid.GetGridNode(x, y, z).NodeType == NodeType.Tile) {
                            GameObject newTile = tilePool.Get();
                            newTile.gameObject.SetActive(true);
                            newTile.transform.position = grid.GetGridNode(x, y, z).worldPos;
                            newTile.transform.rotation = transform.rotation;
                            instantiatedTiles.Add(newTile);
                        }
                    }
                }
            }
        }

        public void ClearNodePrefabs() {
            goalRef.gameObject.SetActive(false);
            spawnRef.gameObject.SetActive(false);

            foreach (GameObject gameObject in instantiatedTiles) {
                tilePool.Release(gameObject);
            }
            instantiatedTiles.Clear();
        }
    }
}
