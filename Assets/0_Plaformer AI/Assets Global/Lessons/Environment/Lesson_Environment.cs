using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG {
    [CreateAssetMenu(fileName = "Lesson", menuName = "Scriptable Objects/Lessons/Lesson Environment")]
    public class Lesson_Environment : ScriptableObject {
        public string lessonName;

        [Range(0, 1)] public float minTargetPathLength = 1;
        [Range(0, 1)] public float maxTargetPathLength = 1;
        [Range(0, 1)] public float pathLengthInfluence = 0;

        public float GetRandomPathLength() {
            return Random.Range(minTargetPathLength, maxTargetPathLength);
        }

        [Range(0, 1)] public float minCohesion = 0;
        [Range(0, 1)] public float maxCohesion = 0;
        [Range(0, 1)] public float cohesionInfluence = 0;

        //[Range(0, 1)] public float targetCohesion = 0.75f;
        public float GetRandomCohesion() {
            return Random.Range(minCohesion, maxCohesion);
        }

        [Range(0, 1)] public float minGridEmptySpace = 0;
        [Range(0, 1)] public float maxGridEmptySpace = 0;
        [Range(0, 1)] public float gridEmptySpaceInfluence = 0;
        //[Range(0, 1)] public float targetGridEmptySpace = 0.25f;
        public float GetRandomGridEmptySpace() {
            return Random.Range(minGridEmptySpace, maxGridEmptySpace);
        }

    }
}
