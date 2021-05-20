using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_Editor
using UnityEditor;

namespace APG
{
    public class SpawnerTool : EditorWindow
    {
        [MenuItem("Tools/Spawner Tool")]

        public static void OpenTool() => GetWindow<SpawnerTool>();
        SerializedObject so;

        public int numX;
        SerializedProperty propNumX;

        public int numY;
        SerializedProperty propNumY;

        public int spacing;
        SerializedProperty propSpacing;

        public GameObject prefabToInstantiate = null;
        SerializedProperty propPrefabToInstantiate;

        private List<GameObject> spawnedPrefabs = new List<GameObject>();

        private void OnEnable()
        {
            // Every time out selection is changed, refresh the editor window by subscribing to 
            // the selection changed callback. Button enable / disable will not be immediate if we don't do this.
            Selection.selectionChanged += Repaint;

            // Called when the scene is updated
            SceneView.duringSceneGui += DuringSceneGUI;

            so = new SerializedObject(this);
            propPrefabToInstantiate = so.FindProperty("prefabToInstantiate");
            propNumX = so.FindProperty("numX");
            propNumY = so.FindProperty("numY");
            propSpacing = so.FindProperty("spacing");
        }
        private void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        private void OnGUI()
        {
            so.Update();
            EditorGUILayout.PropertyField(propPrefabToInstantiate);
            EditorGUILayout.PropertyField(propNumX);
            propNumX.intValue = propNumX.intValue.AtLeast(1);
            EditorGUILayout.PropertyField(propNumY);
            propNumY.intValue = propNumY.intValue.AtLeast(1);
            EditorGUILayout.PropertyField(propSpacing);
            propSpacing.intValue = propSpacing.intValue.AtLeast(50);

            // By forcing the editor to repaint every time a value changes, it leads to a much smoother 
            // user experience when scrolling through values.
            if (so.ApplyModifiedProperties())
            {
                SceneView.RepaintAll();
            }

            // If you clicked the left mouse button, in the editor window
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                GUI.FocusControl(null); // Select nothing
                Repaint(); // Repaint on the editor window ui
            }
        }

        void DuringSceneGUI(SceneView sceneView)
        {
            Handles.BeginGUI();

            Rect rect = new Rect(40, 8, 150, 25);
            float margin = 2;

            if (GUI.Button(rect, new GUIContent("Spawn Prefabs")))
            {
                SpawnPrefabs();
            }

            rect.y += rect.height + margin;

            if (GUI.Button(rect, new GUIContent("Clear Spawned")))
            {
                ClearStoredPrefabs();
            }

            Handles.EndGUI();
        }

        private void SpawnPrefabs()
        {
            if (prefabToInstantiate == null)
                return;

            ClearStoredPrefabs();

            // Spawn new prefabs in a grid pattern
            for (int i = 0; i < propNumX.intValue; i++)
            {
                for (int j = 0; j < propNumY.intValue; j++)
                {
                    GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefabToInstantiate);

                    Undo.RegisterCreatedObjectUndo(instantiatedPrefab, "Undo Instantiate Prefab");
                    instantiatedPrefab.transform.position = new Vector3( propNumX.intValue * propSpacing.intValue * i, 0, propNumY.intValue * propSpacing.intValue * j);
                    // prefabToInstantiate.transform.rotation = pose.rotation;

                    spawnedPrefabs.Add(instantiatedPrefab);
                }
            }
        }

        private void ClearStoredPrefabs()
        {
            // Destroy all previously spawned prefabs
            for (int i = spawnedPrefabs.Count - 1; i >= 0; i--)
                DestroyImmediate(spawnedPrefabs[i]);

            spawnedPrefabs.Clear();
        }
    }
}
#endif