using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public class EnvTile : Node {

        public EnvTile(bool isTraversable, Vector3 worldPos, Vector3Int gridIndex) : base(isTraversable, worldPos, gridIndex) {
        }
    }
}
