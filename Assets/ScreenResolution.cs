using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    public class ScreenResolution : MonoBehaviour
    {
        private void Awake() {
            Screen.SetResolution(640, 480, false);
        }
    }
}
