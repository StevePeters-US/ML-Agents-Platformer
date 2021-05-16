using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

namespace APG
{
    [CreateAssetMenu(fileName = "Lesson Plan", menuName = "Scriptable Objects/Lessons/Lesson Plan")]
    public class LessonPlan : SingletonScriptableObject<LessonPlan>, ISerializationCallbackReceiver
    {
        public List<Lesson> lessons = new List<Lesson>();

        public int InitialLessonIdx;
        [System.NonSerialized] public int runtimeLessonIdx;

        public bool overWriteAcademy = false;

        public void OnAfterDeserialize()
        {
            runtimeLessonIdx = InitialLessonIdx;
        }

        public void OnBeforeSerialize()
        {
        }

        public Vector3Int GetRandomBoardSize()
        {
            UpdateLessonIndex();

            if (lessons[runtimeLessonIdx] != null)
            {
                int randXSize = Random.Range(lessons[runtimeLessonIdx].minXTiles, lessons[runtimeLessonIdx].maxXTiles + 1);
                int randYSize = Random.Range(lessons[runtimeLessonIdx].minYTiles, lessons[runtimeLessonIdx].maxYTiles + 1);
                int randZSize = Random.Range(lessons[runtimeLessonIdx].minZTiles, lessons[runtimeLessonIdx].maxZTiles + 1);

                return new Vector3Int(randXSize, randYSize, randZSize);
            }

            else
                return new Vector3Int(2, 1, 2);
        }

        private void UpdateLessonIndex()
        {
            if (!overWriteAcademy)
            {
            runtimeLessonIdx = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("lesson_index", 0);
            Debug.Log("current Lesson index = " + runtimeLessonIdx);
            }
        }
    }
}
