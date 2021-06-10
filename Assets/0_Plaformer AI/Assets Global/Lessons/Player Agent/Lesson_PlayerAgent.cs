using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Lesson", menuName = "Scriptable Objects/Lessons/Lesson Player Agent")]
    public class Lesson_PlayerAgent : ScriptableObject
    {
        public string lessonName;

        public int minXTiles = 1;
        public int maxXTiles = 1;
        public int minYTiles = 1;
        public int maxYTiles = 1;
        public int minZTiles = 1;
        public int maxZTiles = 1;

        public bool usePath = false;
    }
}
