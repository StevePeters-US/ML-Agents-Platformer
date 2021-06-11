using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    public class ScreenResolution : MonoBehaviour
    {
        public Vector2Int widthHeight = new Vector2Int(640, 480);
        private void Awake() {
            Screen.SetResolution(widthHeight.x, widthHeight.y, false);
        }
    }
}
