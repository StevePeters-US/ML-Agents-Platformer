using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

namespace APG {
    [CreateAssetMenu(fileName = "Lesson Plan Environment", menuName = "Scriptable Objects/Lessons/Lesson Plan Environment")]
    public class LessonPlan_Environment : SingletonScriptableObject<LessonPlan_Environment>, ISerializationCallbackReceiver {
        public List<Lesson_Environment> lessons = new List<Lesson_Environment>();

        public int InitialLessonIdx;
        [System.NonSerialized] public int runtimeLessonIdx;

      /*  public bool InitialUsePath;
        [System.NonSerialized] public bool runtimeUsePath;*/

        public bool overWriteAcademy = false;

        public void OnAfterDeserialize() {
            runtimeLessonIdx = InitialLessonIdx;
        }

        public void OnBeforeSerialize() {
        }

        public float GetPathInfluence() {
           return lessons[runtimeLessonIdx].GetPathInfluence();
        }

        public float GetCohesionInfluence() {
            return lessons[runtimeLessonIdx].GetCohesionInfluence();
        }

        public float GetTargetCohesion() {
            return lessons[runtimeLessonIdx].targetCohesion;
        }

        public float GetGridEmptySpaceInfluence() {
            return lessons[runtimeLessonIdx].GetGridEmptySpaceInfluence();
        }

        public float GetTargetGridEmptySpace() {
            return lessons[runtimeLessonIdx].targetGridEmptySpace;
        }
            


        public void UpdateLessonIndex() {
            if (!overWriteAcademy) {
                runtimeLessonIdx = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("lesson_index", 0);
                Debug.Log("current Lesson index = " + runtimeLessonIdx);
            }

            if (runtimeLessonIdx >= lessons.Count) {
                Debug.LogWarning("Lesson index out of range", this);
                runtimeLessonIdx = lessons.Count - 1;
            }

            /*if (lessons[runtimeLessonIdx] != null) {
                runtimeUsePath = lessons[runtimeLessonIdx].usePath;
            }*/
        }
    }
}
