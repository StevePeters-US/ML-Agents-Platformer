using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

namespace APG.Environment
{
    // Creates a grid type environment at runtime for training
    [RequireComponent(typeof(EnvironmentManager))]
    public class EnvironmentGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject floorTile;
        [SerializeField] private GameObject pathTile;
        [SerializeField] private EnvGoal goalTile;
        [SerializeField] private EnvSpawn spawnTile;

        [SerializeField, Tooltip("Rounded to nearest int")] private Vector3 gridSize;

        private Vector3Int envBoardSize;

        public EnvGoal GoalRef { get => goalRef; }
        private EnvGoal goalRef = null;

        public EnvSpawn SpawnRef { get => spawnRef; }
        private EnvSpawn spawnRef = null;

        private List<GameObject> instantiatedEnvironmentObjects = new List<GameObject>();
        private List<Vector3Int> excludedPositions = new List<Vector3Int>();
        private List<Vector3Int> pathPositions = new List<Vector3Int>();
        private EnvironmentManager envManager;

        private EnvironmentNode[,,] envNodes;

        private void Awake()
        {
            envManager = GetComponent<EnvironmentManager>();
        }

        public void GenerateGridEnvironment()
        {
            DestroyEnvObjects();
            envNodes = new EnvironmentNode[envBoardSize.x, envBoardSize.y, envBoardSize.z];

            SetRandomEnvTileSize();

            Vector3 tileSize = floorTile.GetComponent<MeshRenderer>().bounds.size;

            // Spawn goal tile
            Vector3Int goalPos = GetRandomPosition();
            goalRef = Instantiate<EnvGoal>(goalTile, transform.position + tileSize.MultInt(goalPos), transform.rotation);
            envManager.SubscribeToGoal(goalRef);
            instantiatedEnvironmentObjects.Add(goalRef.gameObject);
            excludedPositions.Add(goalPos);

            // Spawn spawn tile
            Vector3Int spawnPos = GetRandomPosition();
            spawnRef = Instantiate<EnvSpawn>(spawnTile, transform.position + tileSize.MultInt(spawnPos), transform.rotation);
            instantiatedEnvironmentObjects.Add(spawnRef.gameObject);

            GeneratePath(spawnPos, goalPos);
            SpawnPath(tileSize);

            for (int i = 0; i < envBoardSize.x; i++)
            {
                for (int j = 0; j < envBoardSize.z; j++)
                {
                    Vector3Int newTilePos = new Vector3Int(i, 0, j);
                    GameObject newTile;

                    if (!IsPositionExcluded(newTilePos))
                    {
                        newTile = Instantiate<GameObject>(floorTile, transform.position + tileSize.MultInt(newTilePos), transform.rotation);
                        instantiatedEnvironmentObjects.Add(newTile);
                    }
                }
            }
        }

        private bool GeneratePath(Vector3Int spawnPos, Vector3Int goalPos)
        {
            pathPositions.Clear();
            // Path from spawn to goal

            Vector3Int testPos = spawnPos;

            bool northIsValid = true;
            bool southIsValid = true;
            bool eastIsValid = true;
            bool westIsValid = true;

            do
            {
                // Get random direction from current tile position
                int randDirection = Random.Range(0, 4);
                if (randDirection == 0 && northIsValid) // North
                {
                    testPos.z += 1;
                    northIsValid = false;
                }

                else if (randDirection == 1 && southIsValid) //South
                {
                    testPos.z -= 1;
                    southIsValid = false;
                }

                else if (randDirection == 2 && eastIsValid) // East
                {
                    testPos.x += 1;
                    eastIsValid = false;
                }

                else if (westIsValid) // West
                {
                    testPos.x -= 1;
                    westIsValid = false;
                }

                else
                    return false;

            } while (!IsPositionExcluded(testPos));

            excludedPositions.Add(testPos);
            pathPositions.Add(testPos);
            return true;
        }

        private void SpawnPath(Vector3 tileSize)
        {
            foreach (Vector3Int pathPosition in pathPositions)
            {
                GameObject newTile;

                newTile = Instantiate<GameObject>(pathTile, transform.position + tileSize.MultInt(pathPosition), transform.rotation);
                instantiatedEnvironmentObjects.Add(newTile);
            }
        }

        private Vector3Int GetRandomPosition()
        {
            Vector3Int outPos = Vector3Int.zero;
            int i = 0;
            while (i < 100 || IsPositionExcluded(outPos))
            {
                i++;
                outPos = new Vector3Int(Random.Range(0, envBoardSize.x), 0, Random.Range(0, envBoardSize.z));
            }

            return outPos;
        }

        private bool IsPositionExcluded(Vector3Int positionToCheck)
        {
            return excludedPositions.Contains(positionToCheck);
        }

        private void DestroyEnvObjects()
        {
            goalRef = null;
            spawnRef = null;

            foreach (GameObject gameObject in instantiatedEnvironmentObjects)
            {
                Destroy(gameObject);
            }

            instantiatedEnvironmentObjects.Clear();
            excludedPositions.Clear();
        }

        private void SetRandomEnvTileSize()
        {
            // Get random range values from lesson plan
            envBoardSize = LessonPlan.Instance.GetRandomBoardSize();
        }
    }
}
