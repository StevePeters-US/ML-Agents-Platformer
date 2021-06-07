using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [RequireComponent(typeof(TextMesh))]
    public class CohesionDebugText : MonoBehaviour
    {
        private TextMesh tm;
        private void Awake() {
            tm = GetComponent<TextMesh>();
        }

        public void UpdateText(string newText) {
            tm.text = newText;
        }
    }
}
