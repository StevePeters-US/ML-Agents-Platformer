using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Bool Scriptable Object", menuName = "Scriptable Objects/Variables/Bool")]
    public class BoolScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        public bool InitialValue;

        [System.NonSerialized] public bool runtimeValue;

        public void OnAfterDeserialize()
        {
            runtimeValue = InitialValue;
        }

        public void OnBeforeSerialize()
        {
        }
    }
}
