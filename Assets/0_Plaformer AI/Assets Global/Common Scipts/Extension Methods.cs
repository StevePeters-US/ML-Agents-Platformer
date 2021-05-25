using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    public static class ExtensionMethods
    {
        // Extension method, must be static in a static class
        // Extends the base vector3 functionality

        public static Vector3 Round(this Vector3 inVec)
        {
            inVec.x = Mathf.Round(inVec.x);
            inVec.y = Mathf.Round(inVec.y);
            inVec.z = Mathf.Round(inVec.z);
            return inVec;
        }

        public static Vector3 Round(this Vector3 inVec, float scale)
        {
            return Round(inVec / scale) * scale;
        }

        public static Vector3 Multiply(this Vector3 inVec, Vector3 inIntVec) {
            float x = inVec.x * inIntVec.x;
            float y = inVec.y * inIntVec.y;
            float z = inVec.z * inIntVec.z;

            return new Vector3(x, y, z);
        }

        public static Vector3 MultiplyInt(this Vector3 inVec, Vector3Int inIntVec)
        {
            float x = inVec.x * inIntVec.x;
            float y = inVec.y * inIntVec.y;
            float z = inVec.z * inIntVec.z;

            return new Vector3(x, y, z);
        }

        public static float Round(this float inFloat, float scale)
        {
            return Mathf.Round(inFloat / scale) * scale;
        }

        public static float AtLeast(this float v, float min)
        {
            return Mathf.Max(v, min);
        }

        public static int AtLeast(this int v, int min)
        {
            return Mathf.Max(v, min);
        }
    }
}
