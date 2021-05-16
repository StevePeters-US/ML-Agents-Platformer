using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Float Scriptable Object", menuName = "Scriptable Objects/Variables/Float")]
    public class FloatScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        public float InitialValue;

        [System.NonSerialized] public float runtimeValue;

        public void OnAfterDeserialize()
        {
            runtimeValue = InitialValue;
        }

        public void OnBeforeSerialize()
        {
        }
    }
}
