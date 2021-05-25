using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    public interface IEnvironmentObject {
        EnvironmentManager envManager { get; }
        bool UpdateEnvironmentManager();
    }
}
