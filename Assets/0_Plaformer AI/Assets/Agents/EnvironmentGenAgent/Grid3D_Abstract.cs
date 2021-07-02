using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public class Grid3DData {

        public Vector3Int GridSize { get => gridSize; set => gridSize = value; }
        private Vector3Int gridSize;

        public int GridCount { get => gridSize.x * gridSize.y * gridSize.z; }

        public Node_3D[,,] GridNodes { get => gridNodes; set => gridNodes = value; }
        private Node_3D[,,] gridNodes;

        public List<Vector3Int> availableIndices = new List<Vector3Int>();

        public Vector3Int StartIndex { get => startIndex; set => startIndex = value; }
        Vector3Int startIndex;

        Vector3Int goalIndex;
        public Vector3Int GoalIndex { get => goalIndex; set => goalIndex = value; }

        public Vector3 tileSize = Vector3.one * 2;
        public Vector3 TileSize { get => tileSize; set => tileSize = value; }

        public bool useManhattanNeighbors = true;

        [Range(0, 1)] public float relativeEmptySpace;
        [Range(0, 1)] public float avgCohesion;       
    }


    public abstract class Grid3D_Abstract : MonoBehaviour {

        protected Grid3DData m_Grid3DData;
        public Grid3DData Grid3DData { get => m_Grid3DData; }

        [SerializeField] protected Texture2D tex;
        public float randomTileChance = 0.5f;

        public List<Vector3Int> pathIndices = new List<Vector3Int>();     

        public abstract int GetMinPathLength();
        public abstract int GetMaxPathLength();

        public abstract int GetCurrentPathLength();

        // 0 - 1 num tiles / num empty tiles
        public abstract void UpdateRelativeEmptySpaceValue();
        public abstract void UpdateCohesionValues();


        protected void Update() {
            // Update path using A*
            pathIndices = Astar.GeneratePath(this, true, false);

            UpdateCohesionValues();
            UpdateRelativeEmptySpaceValue();
            UpdateGridTexture();
        }

        public Vector3Int GetGridSize() {
            return m_Grid3DData.GridSize;
        }

        protected void ClearGridNodes() {
            m_Grid3DData.availableIndices.Clear();

            for (int x = 0; x < m_Grid3DData.GridSize.x; x++) {
                for (int y = 0; y < m_Grid3DData.GridSize.y; y++) {
                    for (int z = 0; z < m_Grid3DData.GridSize.z; z++) {
                        m_Grid3DData.GridNodes[x, y, z].locked = false;
                        m_Grid3DData.GridNodes[x, y, z].NodeType = NodeType.Empty;
                        m_Grid3DData.availableIndices.Add(new Vector3Int(x, y, z));
                    }
                }
            }

            ClearPathIndices();
        }
        public void ClearPathIndices() {
            for (int x = 0; x < m_Grid3DData.GridSize.x; x++) {
                for (int y = 0; y < m_Grid3DData.GridSize.y; y++) {
                    for (int z = 0; z < m_Grid3DData.GridSize.z; z++) {
                        m_Grid3DData.GridNodes[x, y, z].isPath = false;
                    }
                }
            }

            pathIndices.Clear();
        }

        // Overwrites all empty nodes with tile nodes
        public void FillGridWithTiles() {
            for (int x = 0; x < m_Grid3DData.GridSize.x; x++) {
                for (int y = 0; y < m_Grid3DData.GridSize.y; y++) {
                    for (int z = 0; z < m_Grid3DData.GridSize.z; z++) {
                        if (m_Grid3DData.GridNodes[x, y, z].NodeType == NodeType.Empty)
                            m_Grid3DData.GridNodes[x, y, z].NodeType = NodeType.Tile;
                    }
                }
            }
        }

        public void FillGridWithRandomTiles() {
            for (int x = 0; x < m_Grid3DData.GridSize.x; x++) {
                for (int y = 0; y < m_Grid3DData.GridSize.y; y++) {
                    for (int z = 0; z < m_Grid3DData.GridSize.z; z++) {
                        if (m_Grid3DData.GridNodes[x, y, z].NodeType == NodeType.Empty && UnityEngine.Random.Range(0f, 1f) < randomTileChance)
                            m_Grid3DData.GridNodes[x, y, z].NodeType = NodeType.Tile;
                    }
                }
            }
        }

        protected Vector3Int GetRandomIndex() {
            int randomIdx = Random.Range(0, m_Grid3DData.availableIndices.Count);
            return m_Grid3DData.availableIndices[randomIdx];
        }

        public void UpdateGridNodeType(Vector3Int nodeIndex, NodeType newNodeType) {
            m_Grid3DData.GridNodes[nodeIndex.x, nodeIndex.y, nodeIndex.z].NodeType = newNodeType;
        }

        public Node_3D GetGridNode(int x, int y, int z) {
            return m_Grid3DData.GridNodes[x, y, z]; ;
        }

        protected Node_3D[,,] GetNewNodes(Vector3Int gridSize) {
            return new Node_3D[gridSize.x, gridSize.y, gridSize.z];
        }

        public Vector3Int GetStartPosition() {
            return m_Grid3DData.StartIndex;
        }

        public Node_3D GetStartNode() {
            return m_Grid3DData.GridNodes[GetStartPosition().x, GetStartPosition().y, GetStartPosition().z];
        }

        public Vector3Int GetGoalPosition() {
            return m_Grid3DData.GoalIndex;
        }

        public Node_3D GetGoalNode() {
            return m_Grid3DData.GridNodes[GetGoalPosition().x, GetGoalPosition().y, GetGoalPosition().z];
        }

        protected void UpdateGridTexture() {
            for (int x = 0; x < m_Grid3DData.GridSize.x; x++) {
                for (int y = 0; y < m_Grid3DData.GridSize.y; y++) {
                    for (int z = 0; z < m_Grid3DData.GridSize.z; z++) {

                        //Color nodeColor = Color.cyan;
                        Color nodeColor = tex.GetPixel(x, z);
                        NodeType nodeGridType = m_Grid3DData.GridNodes[x, y, z].NodeType;

                        // Update node color path values
                        switch (nodeGridType) {
                            case NodeType.Empty:
                                nodeColor.r = 0;
                                break;
                            case NodeType.Start:
                                nodeColor.r = 1;
                                break;
                            case NodeType.Goal:
                                nodeColor.r = 1;
                                break;
                            case NodeType.Waypoint:
                                nodeColor.r = 1;
                                break;
                            case NodeType.Tile:
                                nodeColor.r = 0.5f;
                                break;
                            default:
                                break;
                        }

                        if (m_Grid3DData.GridNodes[x, y, z].isPath)
                            nodeColor.r = 0.75f;

                        // Update node color cohesion values
                        nodeColor.g = m_Grid3DData.GridNodes[x, y, z].cohesiveValue;

                        tex.SetPixel(x, z, nodeColor);
                    }
                }
            }
            tex.Apply();
        }
    }
}
