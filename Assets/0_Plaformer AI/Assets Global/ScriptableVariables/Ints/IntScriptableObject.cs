using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Int Scriptable Object", menuName = "Scriptable Objects/Variables/Int")]
    public class IntScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        public int InitialValue;

        [System.NonSerialized] public int runtimeValue;

        public void OnAfterDeserialize()
        {
            runtimeValue = InitialValue;
        }

        public void OnBeforeSerialize()
        {
        }
    }
}
