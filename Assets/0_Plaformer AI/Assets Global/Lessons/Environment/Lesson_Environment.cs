using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG {
    [CreateAssetMenu(fileName = "Lesson", menuName = "Scriptable Objects/Lessons/Lesson Environment")]
    public class Lesson_Environment : ScriptableObject {
        public string lessonName;

        public float minPathInfluence = 1;
        public float maxPathInfluence = 1;
        public float GetPathInfluence() {
            return Random.Range(minPathInfluence, maxPathInfluence);
        }

        public float minCohesionInfluence = 0;
        public float maxCohesionInfluence = 0;
        [Range(0, 1)] public float targetCohesion = 0.75f;
        public float GetCohesionInfluence() {
            return Random.Range(minCohesionInfluence, maxCohesionInfluence);
        }

        public float minGridEmptySpaceInfluence = 0;
        public float maxGridEmptySpaceInfluence = 0;
        [Range(0, 1)] public float targetGridEmptySpace = 0.25f;
        public float GetGridEmptySpaceInfluence() {
            return Random.Range(minGridEmptySpaceInfluence, maxGridEmptySpaceInfluence);
        }

    }
}
